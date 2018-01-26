using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
namespace InstaBotPrototype.Services.Instagram
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
            string getUserInfo = "https://api.instagram.com/v1/users/search?q=" + username + "&access_token=" + standartToken;
            string response = GetResponse(getUserInfo);
            UsersInfo info = JsonConvert.DeserializeObject<UsersInfo>(response);
            return info.Users[0].Id;
        }

        public IEnumerable<ImageData> GetRecentUserPosts(string userId)
        {
            string getRecentMedia = "https://api.instagram.com/v1/users/" + userId + "/media/recent?access_token=" + accessTokens[userId];
            string response = GetResponse(getRecentMedia);
            Post posts = JsonConvert.DeserializeObject<Post>(response);
            return posts.Images;
        }

        public IEnumerable<ImageData> GetLatestPosts(string username)
        {
            string userId = GetUserId(username);
            List<ImageData> posts = new List<ImageData>();
            foreach (var user in GetFollowers(userId).Users)
            {
                posts.AddRange(GetRecentUserPosts(user.Id));
            }
            return posts.OrderBy(x => x.CreatedTime).Take(postsAmount);
        }

        public UsersInfo GetFollowers(string userId)
        {
            string getFollowers = "https://api.instagram.com/v1/users/" + userId + "/follows?access_token=" + accessTokens[userId];
            string response = GetResponse(getFollowers);
            UsersInfo followers = JsonConvert.DeserializeObject<UsersInfo>(response);
            return followers;
        }
        public void GetAllPermissions()
        {
            string authorization = "https://api.instagram.com/oauth/authorize?client_id=" + clientId + "&redirect_uri=" + redirectUri + "&scope=basic+public_content+comments+follower_list&response_type=token";
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
        #endregion
    }
}

