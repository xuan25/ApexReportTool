using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Tesseract;

namespace ApexReportTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
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
            string fileName = "Status.dat";
            Stream stream = new FileStream(fileDirectory + fileName, FileMode.Create, FileAccess.ReadWrite);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, status);
            stream.Close();
        }

        public bool LoadStatus()
        {
            string path = Environment.CurrentDirectory + "\\Status.dat";
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

                Bitmap bitmap = Screenshot.GetImg("Apex Legends");

                if (bitmap.Width < 2 || bitmap.Height < 2)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        HeakerIdBox.Text = "";
                        HeakerIdBox.IsEnabled = true;
                    }));
                    return;
                }

                ApexLayout.ApexNameTagPosition tagPosition = new ApexLayout.ApexNameTagPosition(bitmap);

                Rectangle area = tagPosition.GetArea();
                if (area.Width <= 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        HeakerIdBox.Text = "";
                        HeakerIdBox.IsEnabled = true;
                    }));
                    return;
                }

                Bitmap newbitmap = Screenshot.CropImage(bitmap, tagPosition.GetStartPoint(), area);

                Dispatcher.Invoke(new Action(() =>
                {
                    BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(newbitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    ImageBox.Source = bitmapSource;
                }));

                Screenshot.Monochrome(newbitmap);

                Dispatcher.Invoke(new Action(() =>
                {
                    BitmapSource monoBitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(newbitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    MonoImageBox.Source = monoBitmapSource;
                }));

                TesseractEngine tesseractEngine = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndCube);
                Page page = tesseractEngine.Process(newbitmap);
                Dispatcher.Invoke(new Action(() =>
                {
                    HeakerIdBox.Text = page.GetText().Trim();
                    HeakerIdBox.IsEnabled = true;
                }));

            });
            getPlayerIdThread.Start();
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
