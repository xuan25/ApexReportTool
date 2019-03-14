using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ApexReportTool
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 14/03/2019
    /// </summary>
    public partial class LoginWindow : Window
    {
        public delegate void LoggedInDel(string json);
        public event LoggedInDel LoggedIn;
        public delegate void LoggedOutDel();
        public event LoggedOutDel LoggedOut;
        public delegate void LoginCanceledDel();
        public event LoginCanceledDel LoginCanceled;

        public LoginWindow()
        {
            InitializeComponent();

            this.ShowInTaskbar = false;
            this.WindowState = WindowState.Minimized;
            this.Show();
        }

        public void Login()
        {
            LoginBroswer.Navigate("https://signin.ea.com/p/web2/login?execution=e526258876s1&initref=https%3A%2F%2Faccounts.ea.com%3A443%2Fconnect%2Fauth%3Fredirect_uri%3Dhttps%253A%252F%252Fwww.origin.com%252Foauth%252Flogin%253Flpu%253Dtrue%2526ru%253D%252Fzh-hk%252Fstore%252F%253Flogin%253DWyJsb2dpbj1XeUlpWFElM0QlM0QiXQ%253D%253D%26locale%3Dzh_HK%26display%3Dweb2%252Flogin%26response_type%3Dcode%26client_id%3Dlive.origin.com");
        }

        public void Logout()
        {
            LoginBroswer.Navigate("https://accounts.ea.com/connect/logout?client_id=live.origin.com&locale=en_IE&redirect_uri=https%3A%2F%2Fwww.origin.com%2Foauth%2Flogout%3Fru%3D%2Fen-ie%2Fstore%2F%3Flogin%3DWyIiXQ%3D%3D");
        }

        private void LoginBroswer_LoadCompleted(object sender, NavigationEventArgs e)
        {
            WebBrowser webBrowser = (WebBrowser)sender;
            if (e.Uri.ToString().StartsWith("https://signin.ea.com/p/web2/login"))
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.ShowInTaskbar = true;
            }
            else if (Regex.Match(e.Uri.ToString(), "https://www.origin.com/[a-z]+/[a-z-]+/oauth/login").Success)
            {
                this.Hide();
                webBrowser.Navigate("https://accounts.ea.com/connect/auth?client_id=ORIGIN_JS_SDK&response_type=token&redirect_uri=nucleus:rest&prompt=none&release_type=prod");
            }
            else if (e.Uri.ToString().StartsWith("https://accounts.ea.com/connect/auth"))
            {
                //this.Hide();
                dynamic document = webBrowser.Document;
                string json = document.documentElement.innerText;
                LoggedIn?.Invoke(json);
            }
            else if (Regex.Match(e.Uri.ToString(), "https://www.origin.com/[a-z]+/[a-z-]+/oauth/logout").Success)
            {
                this.Hide();
                LoggedOut?.Invoke();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = (SystemParameters.WorkArea.Height - this.ActualHeight) / 2;
            this.Left = (SystemParameters.WorkArea.Width - this.ActualWidth) / 2;
            this.Hide();
        }

        private bool IsClose = false;

        public new void Close()
        {
            IsClose = true;
            base.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsClose)
            {
                e.Cancel = true;
                this.Hide();
                LoginCanceled?.Invoke();
            }
        }
    }
}
