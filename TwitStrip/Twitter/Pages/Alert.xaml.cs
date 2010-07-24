using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using Core;

namespace Pages {
    public partial class Alert : Window {
        private System.Windows.Forms.Timer CloseTimer = new System.Windows.Forms.Timer();
        private event Action LinkClicked = delegate { };

        public Alert(string sMessage, string sImageUri, Action LinkClickEvent) {
            InitializeComponent();

            imgProfile.Source = new BitmapImage(new Uri(sImageUri));

            LinkClicked = null;
            LinkClicked += LinkClickEvent;
            HyperlinkMessage.Inlines.Add(sMessage);
            HyperlinkMessage.Click += HyperlinkMessage_Click;

            // Display form in bottom right of screen above taskbar
            System.Drawing.Rectangle rect = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Left = rect.Width - this.Width;
            this.Top = rect.Height - this.Height;

            // Setup timer to close form
            CloseTimer.Interval = 1000 * 10;
            CloseTimer.Tick += new EventHandler(Timer_Tick);
            CloseTimer.Start();

            this.Topmost = true;
            // Cancel topmost as soon as the form is loaded
            this.Loaded += (o, e) => this.Topmost = false;
            this.Show();
        }

        private void HyperlinkMessage_Click(object sender, System.Windows.RoutedEventArgs e) {
            CloseForm();
            LinkClicked();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            CloseForm();
        }

        private void Timer_Tick(object o, EventArgs e) {
            var sb = (System.Windows.Media.Animation.Storyboard)this.FindResource("StoryboardFadeOut");
            sb.Begin();
            CloseForm();
        }

        private void CloseForm() {
            CloseTimer.Dispose();
            this.Close();
        }
   
    }
}
