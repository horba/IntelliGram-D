using InstaBotPrototype.Services.Instagram;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace InstaBotPrototype
{
    public class InstagramService : IInstagramService
    {
        const string clientId = "937fa7572cb244e9885382f8cedba3c8";
        const string standartToken = "5543216871.937fa75.4bf238c6f78b459bb7d92c0f5716cf85";
        // Adress of the IntelliGram application
        const string redirectUri = "http://localhost:58687";
        const int postsAmount = 10;

        // All users, registered in the sandbox.
        // To add new user, you need to add him to the sandbox first, then you
        // need to use authorize and use GetAllPermissions() method, to get an access token
        static Dictionary<string, string> accessTokens = new Dictionary<string, string>()
        {
            {"5543216871" , "5543216871.937fa75.4bf238c6f78b459bb7d92c0f5716cf85" },
            {"4307857850" , "4307857850.937fa75.493f293385ea406b920a4b2e16da8d11" },
            {"6661469241" , "6661469241.937fa75.e8eb4ac54a374d2a942bc99eea5f0035" }
        };

        public string GetUserId(string username)
        {
            var getUserInfo = "https://api.instagram.com/v1/users/search?q=" + username + "&access_token=" + standartToken;
            var response = GetResponse(getUserInfo);
            var user = JsonConvert.DeserializeObject<UsersInfo>(response);
            return user.Data[0].Id;
        }

        public IEnumerable<ImageData> GetRecentUserPosts(string userId)
        {
            var getRecentMedia = "https://api.instagram.com/v1/users/" + userId + "/media/recent?access_token=" + accessTokens[userId];
            var response = GetResponse(getRecentMedia);
            var posts = JsonConvert.DeserializeObject<Post>(response);
            return posts.Data;

        }

        public IEnumerable<string> GetLatestPosts(string username)
        {
            var userId = GetUserId(username);
            var posts = new List<ImageData>();
            var followers = GetFollowers(userId);
            foreach (var user in followers.Data)
            {
                posts.AddRange(GetRecentUserPosts(user.Id));
            }
            posts.Sort(new ImageComparer());
            var latestPosts = new List<string>();
            for (var i = 0; i < posts.Count && i < postsAmount; i++)
            {
                latestPosts.Add(posts[i].Images.Standard_resolution.Url);
            }
            return latestPosts;
        }

        public UsersInfo GetFollowers(string userId)
        {
            var getFollowers = "https://api.instagram.com/v1/users/" + userId + "/follows?access_token=" + accessTokens[userId];
            var response = GetResponse(getFollowers);
            var followers = JsonConvert.DeserializeObject<UsersInfo>(response);
            return followers;
        }

        public void GetAllPermissions()
        {
            var authorization = "https://api.instagram.com/oauth/authorize?client_id=" + clientId + "&redirect_uri=" + redirectUri + "&scope=basic+public_content+comments+follower_list&response_type=token";
            var webRequest = WebRequest.Create(authorization);
            var webResponse = webRequest.GetResponse();
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

        public int Login(string username, string password) => throw new NotImplementedException();

        #region ClassesForDeserialization
        public class User
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("full_name")]
            public string Full_name { get; set; }
            [JsonProperty("profile_picture")]
            public string Profile_picture { get; set; }
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
            public string Created_time { get; set; }

        }

        public class ImageComparer : IComparer<ImageData>
        {
            //Descending sorting
            public int Compare(ImageData x, ImageData y) => y.Created_time.CompareTo(x.Created_time);
        }

        public class Images
        {
            [JsonProperty("standard_resolution")]
            public StandartResolution Standard_resolution { get; set; }
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
            public List<ImageData> Data { get; set; }
        }

        public class UsersInfo
        {
            [JsonProperty("data")]
            public List<User> Data { get; set; }
        }
        #endregion
    }
}