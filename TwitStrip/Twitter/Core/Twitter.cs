using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Core {
    public class StatusAlert {
        public String ScreenName;
        public String Name;
        public String Text;
    }

    public class Result {
        public String ID;
        public String Text;
        public String ProfileImageUrl;
        public String DateUpdated;
        public String CreatedAt;
    }

    public static class Twitter {
        private const int MAXCHARACTERS = 149;
        private const string TWITTER_URL = "http://twitter.com/";
        private const string PATH_FRIENDS_STATUS = "statuses/friends/";
        private const string PATH_FRIENDS_TIMELINE = "statuses/friends_timeline";
        private const string PATH_STATUS_UPDATE = "statuses/update.xml?source=tweety&status=";
        private const string PATH_USERS_SHOW = "users/show/";

        //--------------------------------------------------------------
        // Public methods
        //--------------------------------------------------------------

        /// <summary> Update a specified user's status </summary>
        public static String UpdateStatus(string sMessage, string sUserName, string sPassword) {
            Stream ResponseStream = WebHelper.GetWebResponse(TWITTER_URL + PATH_STATUS_UPDATE + sMessage, WebHelper.HTTPPOST, sUserName, sPassword);
            StreamReader reader = new StreamReader(ResponseStream);
            string returnValue = reader.ReadToEnd();
            reader.Close();

            return returnValue;
        }

        /// <summary> Get a specified user's details </summary>
        public static Result GetUserInfo(string sUserName) {
            Stream ResponseStream = WebHelper.GetWebResponse(TWITTER_URL + PATH_USERS_SHOW + sUserName + ".xml", WebHelper.HTTPGET);
            XmlDocument xml = new XmlDocument();
            xml.Load(ResponseStream);
            return GetUserInfoFromNode(xml.DocumentElement);
        }

        /// <summary> Get friends timeline </summary>
        public static List<Result> GetFriendsTimeline(string sUserName, string sPassword) {
            string NumberOfTweets = "?count=30";
            Stream ResponseStream = WebHelper.GetWebResponse(TWITTER_URL + PATH_FRIENDS_TIMELINE + ".xml" + NumberOfTweets,
                                                             WebHelper.HTTPGET, 
                                                             sUserName, 
                                                             sPassword);
            XmlDocument xml = new XmlDocument();
            xml.Load(ResponseStream);
            return GetStatusList(xml);
        }

        //--------------------------------------------------------------
        // Private Methods
        //--------------------------------------------------------------

        private static List<Result> GetStatusList(XmlDocument xml) {
            List<Result> StatusList = new List<Result>();

            foreach (XmlNode StatusNode in xml.GetElementsByTagName("status")) {
                Result StatusInfo = new Result();
                string StatusText = WebHelper.UrlDecode(StatusNode["text"].InnerText);
                StatusInfo.Text = StatusText;
                StatusInfo.ID = StatusNode["id"].InnerText;
                StatusInfo.CreatedAt = ConvertTwitterDate(StatusNode["created_at"].InnerText);

                Result UserInfo = GetUserInfoFromNode(StatusNode.SelectSingleNode("user"));
                StatusInfo.ProfileImageUrl = UserInfo.ProfileImageUrl;

                StatusList.Add(StatusInfo);
            }

            return StatusList;
        }

        /// <summary> Parse twitter date into user friendly display date/time. </summary>
        /// <param name="TwitterDate">DateTime as returned by twitter. e.g. Sun Dec 20 15:16:16 +0000 2009</param>
        private static string ConvertTwitterDate(string TwitterDate) {
            string[] Elements = TwitterDate.Split(' ');

            string DayElement = Convert.ToInt32(Elements[2]) == DateTime.Now.Day ? "Today" : Elements[0];
            
            string TimeElement = Elements[3];
            TimeElement = TimeElement.Substring(0, TimeElement.LastIndexOf(':'));

            return string.Concat(DayElement, " ", TimeElement);
        }

        private static Result GetUserInfoFromNode(XmlNode UserNode) {
            Result UserInfo = new Result();

            // Get the user's latest status
            XmlNode UserStatusNode = UserNode.SelectSingleNode("status");

            // This info may not exist on all user nodes
            if (UserStatusNode != null)
                UserInfo.Text = UserStatusNode["text"].InnerText;

            UserInfo.ProfileImageUrl = UserNode["profile_image_url"].InnerText;

            return UserInfo;
        }
    }
}
