using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using InstaBotPrototype;
using System.IO;
using System.Linq;
using System.Net;
using System.Data.Common;
using System.Net.Http;

namespace InstaBotPrototype.Services.Instagram
{
    public class InstagramService : IInstagramService
    {
        string clientId = AppSettingsProvider.Config["clientId"];
        string standartToken = AppSettingsProvider.Config["standartToken"];
        // Adress of the IntelliGram application
        const string redirectUri = "http://localhost:58687";
        const int postsAmount = 10;
        const string baseUri = "https://api.instagram.com/";
        const string userUri = baseUri + "v1/users/";
        const string authUri = baseUri + "oauth/authorize?client_id=";
        const string mediaUri = baseUri + "v1/media/";
        string currentUserId = "";
        private string connectionString = AppSettingsProvider.Config["connectionString"];
        private DbProviderFactory factory = DbProviderFactories.GetFactoryByProvider(AppSettingsProvider.Config["dataProvider"]);

        // To add new user, you need to add him to the sandbox first, then you
        // need authorize and use GetAllPermissions() method, to get an access token

        public string GetUserId(string username)
        {
            string getUserInfo = userUri + "search?q=" + username + "&access_token=" + standartToken;
            string response = GetResponse(getUserInfo);
            UsersInfo info = JsonConvert.DeserializeObject<UsersInfo>(response);
            currentUserId = info.Users[0].Id;
            return currentUserId;
        }
        public string GetUsername(string token)
        {
            string getUserInfo = userUri + "self/?&access_token=" + token;
            string response = GetResponse(getUserInfo);
            UserInfo info = JsonConvert.DeserializeObject<UserInfo>(response);
            return info.User.Username;
        }
        private string GetAccessTokenUrlParam(string userId)
        {
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                var getUsersToken = factory.CreateCommand();
                getUsersToken.Connection = connection;
                getUsersToken.CommandText = @"SELECT AccessToken FROM InstagramIntegration
                                              WHERE InstagramId = @InstagramId";
                var idParam = factory.CreateParameter();
                idParam.ParameterName = "InstagramId";
                idParam.Value = userId;
                getUsersToken.Parameters.Add(idParam);
                var reader = getUsersToken.ExecuteReader();
                string accessToken;
                if (reader.HasRows)
                {
                    reader.Read();
                    accessToken = reader.GetString(0);
                    return accessToken;
                }
                else
                {
                    throw new Exception("User was not found");
                }
            }




        }
        public IEnumerable<ImageData> GetRecentUserPosts(string userId)
        {
            string getRecentMedia = userUri + userId + "/media/recent?access_token=" + GetAccessTokenUrlParam(currentUserId);
            string response = GetResponse(getRecentMedia);
            Post posts = JsonConvert.DeserializeObject<Post>(response);
            return posts.Images;
        }

        public IEnumerable<ImageData> GetLatestPosts(string username)
        {
            string userId = GetUserId(username);
            var posts = new List<ImageData>();
            foreach (var user in GetFollowers(userId).Users)
            {
                posts.AddRange(GetRecentUserPosts(user.Id));
            }
            return posts.OrderBy(x => x.CreatedTime).Take(postsAmount);
        }

        public UsersInfo GetFollowers(string userId)
        {
            string getFollowers = userUri + userId + "/follows?access_token=" + GetAccessTokenUrlParam(currentUserId);
            string response = GetResponse(getFollowers);
            UsersInfo followers = JsonConvert.DeserializeObject<UsersInfo>(response);
            return followers;
        }

        public async void Like(string mediaId)
        {
            using (var client = new HttpClient())
            {
                var pars = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("access_token", GetAccessTokenUrlParam(currentUserId)) });
                var resp = await client.PostAsync(mediaUri + mediaId + "/likes", pars);
                Console.WriteLine(resp.StatusCode);
            }
        }


        public async void Comment(string mediaId, string commentText = "Comment from bot :)")
        {
            using (var client = new HttpClient())
            {
                var pars = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("access_token", GetAccessTokenUrlParam(currentUserId)), new KeyValuePair<string, string>("text", commentText) });
                var resp = await client.PostAsync(mediaUri + mediaId + "/comments", pars);
                Console.WriteLine(resp.StatusCode);
            }
        }

        public WebResponse GetAllPermissions()
        {
            string authorization = authUri + clientId + "&redirect_uri=" + redirectUri + "&scope=basic+public_content+comments+likes+follower_list&response_type=token";
            var webRequest = WebRequest.Create(authorization);
            return webRequest.GetResponse();
        }

        private string GetResponse(string request)
        {
            var webRequest = WebRequest.Create(request);
            var webResponse = webRequest.GetResponse();
            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        public int Login(string username, string password)
        {
            throw new NotImplementedException();
        }
        #region ClassesForDeserialization
        public class User
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("full_name")]
            public string Fullname { get; set; }
            [JsonProperty("profile_picture")]
            public string ProfilePicture { get; set; }
            [JsonProperty("username")]
            public string Username { get; set; }
        }

        public class ImageData
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("user")]
            public User User { get; set; }
            [JsonProperty("images")]
            public Images Images { get; set; }
            [JsonProperty("created_time")]
            public string CreatedTime { get; set; }
            [JsonProperty("tags")]
            public List<String> Tags { get; set; }
        }
        public class Images
        {
            [JsonProperty("standard_resolution")]
            public StandartResolution StandartResolution { get; set; }
        }
        public class StandartResolution
        {
            [JsonProperty("width")]
            public int Width { get; set; }
            [JsonProperty("height")]
            public int Height { get; set; }
            [JsonProperty("url")]
            public string Url { get; set; }
        }
        public class Post
        {
            [JsonProperty("data")]
            public List<ImageData> Images { get; set; }
        }

        public class UsersInfo
        {
            [JsonProperty("data")]
            public List<User> Users { get; set; }
        }
        public class UserInfo
        {
            [JsonProperty("data")]
            public User User { get; set; }
        }
        #endregion
    }
}