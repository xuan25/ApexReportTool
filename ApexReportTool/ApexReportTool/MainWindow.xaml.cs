using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace ApexReportTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 14/03/2019
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeContextMenu();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            InitializeHotkeys();
        }

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private void InitializeContextMenu()
        {
            Uri uri = new Uri("/icon.ico", UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(uri);
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = this.Title;
            notifyIcon.Icon = new Icon(info.Stream);
            notifyIcon.Visible = true;
            notifyIcon.Click += notifyIcon_Click;

            System.Windows.Forms.MenuItem exitMenuItem = new System.Windows.Forms.MenuItem("退出", delegate (object sender, EventArgs args)
            {
                this.Close();
            });
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] { exitMenuItem });
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void InitializeHotkeys()
        {
            try
            {
                Hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL | HotkeyModifiers.MOD_ALT, Key.P, () =>
                {
                    ShowWindow();
                    GetPlayerId();
                });
                this.Title += " (Ctrl+Alt+P 快速举报)";
            }
            catch (Exception)
            {
                this.Title += " (快速举报已禁用)";
            }
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        [Serializable]
        class Status
        {
            public string PlayerId, FirstName, Email, Details;
            public bool IsWallHack, IsAimbot, IsSpeedHacked, IsDamageHacked;
            public bool IsSaveImg;
            public bool IsLoggedIn;
        }

        public void SaveStatus()
        {
            Status status = new Status()
            {
                PlayerId = PlayerIdBox.Text,
                FirstName = FirstNameBox.Text,
                Email = EmailBox.Text,
                Details = DetailsBox.Text,
                IsWallHack = WallHackCkb.IsChecked == true,
                IsAimbot = AimbotCkb.IsChecked == true,
                IsSpeedHacked = SpeedHackedCkb.IsChecked == true,
                IsDamageHacked = DamageHackedCkb.IsChecked == true,
                IsSaveImg = SaveImgCkb.IsChecked == true,
                IsLoggedIn = ea != null
            };

            string fileDirectory = Environment.CurrentDirectory + "\\";
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);
            string fileName = "Config.dat";
            Stream stream = new FileStream(fileDirectory + fileName, FileMode.Create, FileAccess.ReadWrite);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, status);
            stream.Close();
        }

        public bool LoadStatus()
        {
            string path = Environment.CurrentDirectory + "\\Config.dat";
            if (!File.Exists(path))
            {
                return false;
            }
            try
            {
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                Status status = (Status)binaryFormatter.Deserialize(stream);
                stream.Close();

                PlayerIdBox.Text = status.PlayerId;
                FirstNameBox.Text = status.FirstName;
                EmailBox.Text = status.Email;
                DetailsBox.Text = status.Details;
                WallHackCkb.IsChecked = status.IsWallHack;
                AimbotCkb.IsChecked = status.IsAimbot;
                SpeedHackedCkb.IsChecked = status.IsSpeedHacked;
                DamageHackedCkb.IsChecked = status.IsDamageHacked;
                SaveImgCkb.IsChecked = status.IsSaveImg;

                if (status.IsLoggedIn)
                    LoginBtn_Click(null, null);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Dispose();
            SaveStatus();
            if (loginWindow != null)
                loginWindow.Close();
        }

        Thread getPlayerIdThread;
        private void GetPlayerId()
        {
            if (getPlayerIdThread != null)
                getPlayerIdThread.Abort();
            getPlayerIdThread = new Thread(delegate ()
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    HeakerIdBox.Text = "处理中...";
                    HeakerIdBox.IsEnabled = false;
                    ImageBox.Source = null;
                    MonoImageBox.Source = null;
                }));

                PlayerIdParser playerIdParser = new PlayerIdParser();
                playerIdParser.PlayerIdFound += PlayerIdParser_PlayerIdFound;
                playerIdParser.PlayerIdMonochrome += PlayerIdParser_PlayerIdMonochrome;
                string id = playerIdParser.Parse();

                Dispatcher.Invoke(new Action(() =>
                {
                    if (id != null)
                        HeakerIdBox.Text = id;
                    else
                        HeakerIdBox.Text = "";
                    HeakerIdBox.IsEnabled = true;
                }));
            });
            getPlayerIdThread.Start();
        }

        private void PlayerIdParser_PlayerIdMonochrome(Bitmap bitmap)
        {
            bool isSave;
            Dispatcher.Invoke(new Action(() =>
            {
                ImageBox.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                isSave = SaveImgCkb.IsChecked == true;
            }));
            SaveImage(bitmap, "orig");
        }

        private void PlayerIdParser_PlayerIdFound(Bitmap bitmap)
        {
            bool isSave;
            Dispatcher.Invoke(new Action(() =>
            {
                MonoImageBox.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                isSave = SaveImgCkb.IsChecked == true;
            }));
            SaveImage(bitmap, "mono");
        }

        private void SaveImage(Bitmap bitmap, string tag)
        {
            string directory = Environment.CurrentDirectory + "\\img\\";
            string filename = tag + "_" + DateTime.Now.ToFileTime().ToString() + ".png";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            bitmap.Save(directory + filename);
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            GetPlayerId();
        }

        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (EmailBox.Text.Trim() == "")
            {
                MessageBoxEx.Show(this, "请填写电子邮箱", "提交失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(FirstNameBox.Text.Trim() == "")
            {
                MessageBoxEx.Show(this, "请填写称呼", "提交失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (HeakerIdBox.Text.Trim() == "")
            {
                MessageBoxEx.Show(this, "请填写被举报者的Id", "提交失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ReportDetails reportDetails = new ReportDetails(HeakerIdBox.Text, 
                WallHackCkb.IsChecked == true, 
                AimbotCkb.IsChecked == true, 
                SpeedHackedCkb.IsChecked == true, 
                DamageHackedCkb.IsChecked == true, 
                DetailsBox.Text);
            SubmitReports(reportDetails);
        }

        private void SubmitReports(ReportDetails reportDetails)
        {
            if (ea != null)
            {
                try
                {
                    if (ea.ReportCheat(reportDetails.HackerName, reportDetails.ToString()))
                    {
                        MessageBoxEx.Show(this, "举报信息已成功提交给EA", "[EA]提交成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (EA.TokenExpiredException)
                {
                    MessageBoxEx.Show(this, "EA登录已过期，请重新登录", "[EA]提交失败",  MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (EA.PlayerNotFoundException)
                {
                    MessageBoxEx.Show(this, "未能找到被举报玩家，请检查举报信息是否正确", "[EA]提交失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch (EA.ReportFaildException ex)
                {
                    MessageBoxEx.Show(this, ex.Message, "[EA]提交失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            try
            {
                if (ApexEac.Submit(PlayerIdBox.Text, FirstNameBox.Text, "", EmailBox.Text, reportDetails.ToString()))
                {
                    MessageBoxEx.Show(this, "举报信息已成功提交给EAC", "[EAC]提交成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (ApexEac.GetVerificationException)
            {
                MessageBoxEx.Show(this, "连接失败", "[EAC]提交失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ApexEac.InvalidParameterException ex)
            {
                MessageBoxEx.Show(this, ex.Message, "[EAC]提交被驳回", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }


        private LoginWindow loginWindow;
        private EA ea;

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (loginWindow == null)
            {
                loginWindow = new LoginWindow();
                loginWindow.LoggedIn += LoginWindow_LoggedIn;
                loginWindow.LoggedOut += LoginWindow_LoggedOut;
            }
                
            if (ea == null)
            {
                loginWindow.Login();
                LoginBtn.IsEnabled = false;
                LoginBtn.Content = "正在登录...";
            }
            else
            {
                loginWindow.Logout();
                LoginBtn.IsEnabled = false;
                LoginBtn.Content = "正在注销...";
            }
            
        }

        private void LoginWindow_LoggedIn(string json)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                LoginBtn.IsEnabled = true;
                LoginBtn.Content = "注销登录";
            }));
            string token = Regex.Match(json, "{\"access_token\":\"(?<Token>.+)\",\"token_type\":\".+\",\"expires_in\":\".+\"}").Groups["Token"].Value;
            ea = new EA(token);
        }

        private void LoginWindow_LoggedOut()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                LoginBtn.IsEnabled = true;
                LoginBtn.Content = "登录EA";
            }));
            ea = null;
        }
        
    }
}
