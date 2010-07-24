using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Core;

namespace Pages
{
    public partial class MainWindow : Window, IDisposable
    {
        BackgroundWorker _bgwFriendsTimeLine = new BackgroundWorker();
        BackgroundWorker _bgwMyStatus = new BackgroundWorker();
        System.Windows.Forms.NotifyIcon _NotifyIcon = new System.Windows.Forms.NotifyIcon();
        System.Windows.Forms.Timer _StatusTimer;

        // Keep track of the last tweet id to check to new tweets
        Int64 _lLastId;

        public MainWindow() {
            InitializeComponent();

            SetupNotifyIcon();

            // Setup global error handler
            App.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
        }

        #region Events

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            // Display and animate the error message button
            string sMessage;

            if (e.Exception.InnerException == null)
                sMessage = e.Exception.Message;
            else
                sMessage = e.Exception.InnerException.Message;

            btnError.Content = sMessage;
            btnError.ToolTip = sMessage;

            if (btnError.Height == 0) {
                var sb = (System.Windows.Media.Animation.Storyboard)this.FindResource("DisplayError");
                sb.Begin();
            }

            e.Handled = true;
        }

        private void btnError_Click(object sender, RoutedEventArgs e) {
            var sb = (System.Windows.Media.Animation.Storyboard)this.FindResource("HideError");
            sb.Begin();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) {
            bgwFriendsTimeLine_Start();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            Settings SettingsForm = new Settings();
            SettingsForm.ShowDialog();
        }

        private void txtStatus_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateStatusButtons(true);
        }

        private void btnUpdateStatus_Click(object sender, RoutedEventArgs e) {
            Twitter.UpdateStatus(txtStatus.Text, SettingHelper.UserName, SettingHelper.Password);
            UpdateStatusButtons(false);
        }

        private void btnCancelUpdate_Click(object sender, RoutedEventArgs e) {
            UpdateStatusButtons(false);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.RoutedEventArgs e) {
            using (System.Diagnostics.Process p = new System.Diagnostics.Process()) {
                Hyperlink link = (Hyperlink)sender;
                p.StartInfo.FileName = link.NavigateUri.AbsoluteUri;
                p.Start();
            }
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            // Check for user settings before settings up form
            if (String.IsNullOrEmpty(SettingHelper.UserName)) {
                Settings SettingsForm = new Settings();
                SettingsForm.ShowDialog();

                if (String.IsNullOrEmpty(SettingHelper.UserName))
                    Close();
                else
                    Setup();
            } else
                Setup();
        }

        private void window_Closing(object sender, CancelEventArgs e) {
            this.Dispose();
        }

        private void window_StateChanged(object sender, EventArgs e) {
            if (this.WindowState == WindowState.Minimized)
                this.Hide();
        }

        void StatusTimer_Tick(object sender, EventArgs e) {
            bgwFriendsTimeLine_Start();
            bgwMyStatus_Start();
        }

        #endregion

        #region BackgroundWorker Methods

        // Friends Timeline worker methods

        void bgwFriendsTimeLine_Start() {
            if (!_bgwFriendsTimeLine.IsBusy) {
                this.Title = "Tweety - Looking for tweets...";
                _bgwFriendsTimeLine.RunWorkerAsync();
            }
        }

        void bgwFriendsTimeLine_DoWork(object sender, DoWorkEventArgs e) {
            e.Result = Twitter.GetFriendsTimeline(SettingHelper.UserName, SettingHelper.Password);
        }

        void bgwFriendsTimeLine_Completed(object sender, RunWorkerCompletedEventArgs e) {
            this.Title = "Tweety";

            if (e.Result != null) {
                HandleResults((List<Result>)e.Result);

                if (btnError.Height > 0) {
                    var sb = (System.Windows.Media.Animation.Storyboard)this.FindResource("HideError");
                    sb.Begin();
                }
            }
        }

        // My Status worker methods
        void bgwMyStatus_Start() {
            if (!_bgwMyStatus.IsBusy)
                _bgwMyStatus.RunWorkerAsync();
        }

        void bgwMyStatus_DoWork(object sender, DoWorkEventArgs e) {
            e.Result = Twitter.GetUserInfo(SettingHelper.UserName);
        }

        void bgwMyStatus_Completed(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Result != null) {
                Result MyInfo = (Result)e.Result;
                txtStatus.TextChanged -= txtStatus_TextChanged;
                txtStatus.Text = MyInfo.Text;
                txtStatus.TextChanged += txtStatus_TextChanged;
                imgProfile.Source = new BitmapImage(new Uri(MyInfo.ProfileImageUrl));
            }
        }

        #endregion

        #region Support Methods

        /// <summary> Display list of tweets inside the Grid control </summary>
        private void AddResultsToGrid(List<Result> ResultList) {
            //grdTweets.RowDefinitions.Clear();
            grdTweets.Children.Clear();

            foreach (Result Status in ResultList) {
                
                //grdTweets.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                
                // Create the text for grid
                TextBlock TextBlock = new TextBlock();
                TextBlock.Margin = new Thickness(4);
                TextBlock.TextWrapping = TextWrapping.Wrap;
               
                TextBlock.Inlines.AddRange(WPFHelper.CreateInlineTextWithLinks(Status.Text, Hyperlink_RequestNavigate));
                TextBlock.Inlines.Add(new Italic(new Run(Environment.NewLine + Status.CreatedAt)));
                Grid.SetColumn(TextBlock, 1);
                //Grid.SetRow(TextBlock, grdTweets.RowDefinitions.Count - 1);
                grdTweets.Children.Add(TextBlock);

                // Create the profile image for grid
                Image ProfileImage = new Image();
                ProfileImage.Source = new BitmapImage(new Uri(Status.ProfileImageUrl));
                Grid.SetColumn(ProfileImage, 0);
                //Grid.SetRow(ProfileImage, grdTweets.RowDefinitions.Count - 1);
                grdTweets.Children.Add(ProfileImage);
            }
        }

        /// <summary> Setup for the page to get tweets</summary>
        private void Setup() {
            // Setup timer to get friends timeline
            _StatusTimer = new System.Windows.Forms.Timer();
            _StatusTimer.Tick += new EventHandler(StatusTimer_Tick);
            _StatusTimer.Interval = 1000 * 120;
            _StatusTimer.Start();

            // Get friends timeline
            _bgwFriendsTimeLine.DoWork += new DoWorkEventHandler(bgwFriendsTimeLine_DoWork);
            _bgwFriendsTimeLine.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwFriendsTimeLine_Completed);
            bgwFriendsTimeLine_Start();

            // Get my details
            _bgwMyStatus.DoWork += new DoWorkEventHandler(bgwMyStatus_DoWork);
            _bgwMyStatus.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwMyStatus_Completed);
            bgwMyStatus_Start();
        }

        /// <summary> Check for new tweets, and display if there are any </summary>
        void HandleResults(List<Result> ResultList) {
            // Check to see if there are new tweets
            Int64 lLastId = Convert.ToInt64(ResultList[0].ID);

            if (_lLastId != lLastId) {
                AddResultsToGrid(ResultList);

                // New tweets have been found display the alert form
                if (_lLastId != 0) {
                    Alert Alert = new Alert(SettingHelper.MessageNewTweets, SettingHelper.TweetyIconUriString, RestoreWindow);
                }
            }

            _lLastId = lLastId;
        }

        private void RestoreWindow() {
            if (this.WindowState == WindowState.Minimized) {
                this.Show();
                this.WindowState = WindowState.Normal;
            } else {
                this.WindowState = WindowState.Normal;
                NativeMethods.ShowWindowTopMost(window);
            }
        }

        private void SetupNotifyIcon() {
            _NotifyIcon.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Uri TweetyUri = new Uri(SettingHelper.TweetyIconUriString);
            System.IO.Stream IconStream = Application.GetResourceStream(TweetyUri).Stream;
            _NotifyIcon.Icon = new System.Drawing.Icon(IconStream);
            _NotifyIcon.Click += new EventHandler((o, e) => RestoreWindow());
            _NotifyIcon.Visible = true;
        }

        private void UpdateStatusButtons(bool SetVisible) {
            if (SetVisible) {
                btnUpdateStatus.Visibility = Visibility.Visible;
                btnCancelUpdate.Visibility = Visibility.Visible;
                btnSettings.Visibility = Visibility.Hidden;
            } else {
                btnUpdateStatus.Visibility = Visibility.Hidden;
                btnCancelUpdate.Visibility = Visibility.Hidden;
                btnSettings.Visibility = Visibility.Visible;
            }
        }

        public void Dispose() {
            _StatusTimer.Dispose();
            _NotifyIcon.Dispose();
            _bgwMyStatus.Dispose();
            _bgwFriendsTimeLine.Dispose();
        }

        #endregion

        private void cmdPost_Click(object sender, RoutedEventArgs e)
        {
            if (marScroll.Visibility == System.Windows.Visibility.Visible)
            {
                marScroll.Visibility = System.Windows.Visibility.Hidden;
                txtStatus.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                marScroll.Visibility = System.Windows.Visibility.Visible;
                txtStatus.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
