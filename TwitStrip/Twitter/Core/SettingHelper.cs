using System.Configuration;

namespace Core
{
    // Wrapper class to get/store user settings
    public static class SettingHelper
    {
        public static string UserName {
            get { return Properties.Settings.Default.UserName; }
            set { Properties.Settings.Default.UserName = value; }
        }

        public static string Password {
            get { return Properties.Settings.Default.PassWord; }
            set { Properties.Settings.Default.PassWord = value; }
        }

        public static string MessageInvalidUserSettings {
            get { return "Unable to validate user settings"; }
        }

        public static string MessageNewTweets {
            get { return "New tweets have arrived"; }
        }

        public static string TweetyIconUriString {
            get { return "pack://application:,,,/Tweety;component/Resources/Peace Dove.ico"; }
        }

        public static void Save() {
            Properties.Settings.Default.Save();
        }
    }
}
