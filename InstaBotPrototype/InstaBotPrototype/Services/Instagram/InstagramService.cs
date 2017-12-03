using InstaBotPrototype.Services.Instagram;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace InstaBotPrototype
{
    public class InstagramService:IInstagramService
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
            UsersInfo user = JsonConvert.DeserializeObject<UsersInfo>(response);
            return user.data[0].id;
        }

        public IEnumerable<ImageData> GetRecentUserPosts(string userId)
        {
            string getRecentMedia = "https://api.instagram.com/v1/users/" + userId + "/media/recent?access_token=" + accessTokens[userId];
            string response = GetResponse(getRecentMedia);
            Post posts = JsonConvert.DeserializeObject<Post>(response);
            return posts.data;

        }

        public  IEnumerable<string> GetLatestPosts(string username)
        {
            string userId = GetUserId(username);
            List<ImageData> posts = new List<ImageData>();
            UsersInfo followers = GetFollowers(userId);
            foreach (var user in followers.data)
            {
                posts.AddRange(GetRecentUserPosts(user.id));
            }
            posts.Sort(new ImageComparer());
            List<string> latestPosts = new List<string>();
            for (int i = 0; i < posts.Count && i < postsAmount; i++)
            {
                latestPosts.Add(posts[i].images.standard_resolution.url);
            }
            return latestPosts;
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
            public string id { get; set; }
            public string full_name { get; set; }
            public string profile_picture { get; set; }
            public string username { get; set; }
        }

        public class ImageData
        {
            public string id { get; set; }
            public User user { get; set; }
            public Images images { get; set; }
            public string created_time { get; set; }

        }

        public class ImageComparer : IComparer<ImageData>
        {
            //Descending sorting
            public int Compare(ImageData x, ImageData y)
            {
                return y.created_time.CompareTo(x.created_time);
            }
        }

        public class Images
        {
            public StandartResolution standard_resolution { get; set; }
        }
        public class StandartResolution
        {
            public int width { get; set; }
            public int height { get; set; }
            public string url { get; set; }
        }
        public class Post
        {
            public List<ImageData> data { get; set; }
        }

        public class UsersInfo
        {
            public List<User> data { get; set; }
        }
        #endregion
    }
}
