using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ApexReportTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 08/03/2019
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                Hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL | HotkeyModifiers.MOD_ALT, Key.P, () =>
                {
                    this.WindowState = WindowState.Normal;
                    this.Activate();
                    GetPlayerId();
                });
                this.Title += " (Ctrl+Alt+P 快速举报)";
            }
            catch (Exception)
            {
                this.Title += " (快速举报已禁用)";
            }
            
        }

        [Serializable]
        class Status
        {
            public string PlayerId, FirstName, Email, Details;
            public bool IsWallHack, IsAimbot;
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
            SaveStatus();
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
            Dispatcher.Invoke(new Action(() =>
            {
                ImageBox.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }));
        }

        private void PlayerIdParser_PlayerIdFound(Bitmap bitmap)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                MonoImageBox.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }));
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

            ReportDetails reportDetails = new ReportDetails(HeakerIdBox.Text, WallHackCkb.IsChecked == true, AimbotCkb.IsChecked == true, DetailsBox.Text);
            try
            {
                ApexEac.Submit(PlayerIdBox.Text, FirstNameBox.Text, "", EmailBox.Text, reportDetails.ToString());
            }
            catch (ApexEac.GetVerificationException)
            {
                MessageBoxEx.Show(this, "连接失败", "提交失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (ApexEac.InvalidParameterException ex)
            {
                MessageBoxEx.Show(this, ex.Message, "提交被驳回", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBoxEx.Show(this, "提交成功", "提交成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
