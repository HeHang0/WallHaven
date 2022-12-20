using Microsoft.Win32;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WallHaven.Theme;
using WallHaven.WallHavenClient;
using Windows.Globalization;

namespace WallHaven
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double zoomMin = 0.5;
        private const double zoomMax = 5;
        private const double zoomStep = 1.25;

        private Point mousePosition;
        private bool imageMouseDown;
        private Point imagePosition;
        private Point currentPosition;
        private WallHavenRequest wallHaven;
        private WallHavenResponse wallHavenResult;
        private readonly Settings setting = Settings.ReadSetting();

        public MainWindow()
        {
            try
            {
                this.UpdateStyleAttributes();
            }
            catch (Exception)
            {
                InitializeComponent();
            }
            SourceInitialized += MainWindow_SourceInitialized;
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr hwnd;
            if ((hwnd = new WindowInteropHelper(sender as Window).Handle) == IntPtr.Zero)
                throw new InvalidOperationException("Could not get window handle.");

            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }

        const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x320;
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_DWMCOLORIZATIONCOLORCHANGED:
                    /* 
                     * Update gradient brushes with new color information from
                     * NativeMethods.DwmGetColorizationParams() or the registry.
                     */
                    InitModalBackground();
                    return IntPtr.Zero;
                default:
                    return IntPtr.Zero;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (setting.WindowHeight > 0 && setting.WindowWidth > 0)
            {
                Height = setting.WindowHeight;
                Width = setting.WindowWidth;
            }
            BackGround.Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.BlueGradients.GetHbitmap(),
                IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            InitHaven();
        }

        private void InitHaven()
        {
            //if (string.IsNullOrWhiteSpace(setting.APIKey))
            //{
            //    InputStringWindow inputStringWindow = new InputStringWindow("WallHaven APIKey", "https://wallhaven.cc/settings/account", false);
            //    if (inputStringWindow.ShowDialog() == true)
            //    {
            //        setting.APIKey = inputStringWindow.InputString;
            //    }
            //}

            Config config = new Config()
            {
                BaseUrl = setting.BaseUrl,
                APIKey = setting.APIKey
            };

            wallHaven = new WallHavenRequest(config);
            InitMenuCheck();
            RefreshPicList();
            InitModalBackground();
        }

        private void InitModalBackground()
        {
            WindowsTheme wt = ThemeHelper.GetWindowsTheme();
            SolidColorBrush scb;
            if (wt == WindowsTheme.Dark)
            {
                scb = new SolidColorBrush(Color.FromArgb(0x66, 0, 0, 0));
            }
            else
            {
                scb = new SolidColorBrush(Color.FromArgb(0x66, 0xff, 0xff, 0xff));
            }
            BorderDisplayCtrlBack.Background = scb;
            BorderImages.Background = scb;
            BoderSettingCtrl.Background = scb;
            BorderSetting.Background = scb;
        }

        private void InitMenuCheck()
        {
            MenuGeneral.IsChecked = setting.General;
            MenuAnime.IsChecked = setting.Anime;
            MenuPeople.IsChecked = setting.People;
            MenuSFW.IsChecked = setting.SFW;
            MenuSketchy.IsChecked = setting.Sketchy;
            MenuNSFW.IsChecked = setting.NSFW;
            MenuWide.IsChecked = setting.Wide;
            MenuUltraWide.IsChecked = setting.UltraWide;
            MenuPortrait.IsChecked = setting.Portrait;
            MenuSquare.IsChecked = setting.Square;
            SetMenuSort(setting.Sort);
        }

        private async void RefreshPicList()
        {
            if (wallHaven == null) return;
            ChangeLoading(true);
            MenuRefresh.IsEnabled = false;
            SearchParamsBuilder _searchParamsBuilder = new SearchParamsBuilder()
                  .WithRatios(GetRatio())
                  .IncludeGeneral(MenuGeneral.IsChecked)
                  .IncludeAnime(MenuAnime.IsChecked)
                  .IncludePeople(MenuPeople.IsChecked)
                  .IncludeSafe(MenuSFW.IsChecked)
                  .IncludeSketchy(MenuSketchy.IsChecked)
                  .IncludeNSFW(MenuNSFW.IsChecked)
                  .OrderBy(OrderBy.desc)
                  .SortBy(GetSorting());
            string _searchParams = _searchParamsBuilder.Build();
            try
            {
                wallHavenResult = await wallHaven.Search(_searchParams, setting.BaseUrl, setting.APIKey);
                currentIndex = 0;
                if (wallHavenResult != null)
                {
                    ImagesDisplay.ItemsSource = wallHavenResult.Data;
                    ShowImage();
                }
                else
                {
                    TipsPos.Content = "加载失败";
                }
            }
            catch (Exception)
            {
                TipsPos.Content = "加载失败";
            }
            MenuRefresh.IsEnabled = true;
        }

        private List<string> GetRatio()
        {
            List<string> list = new List<string>();
            if (MenuWide.IsChecked) list.Add(Ratio.Wide);
            if (MenuUltraWide.IsChecked) list.Add(Ratio.UltraWide);
            if (MenuPortrait.IsChecked) list.Add(Ratio.Portrait);
            if (MenuSquare.IsChecked) list.Add(Ratio.Square);
            return list;
        }

        private int currentIndex;
        private void MenuLast_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex == 0) return;
            currentIndex -= 1;
            ShowImage();
        }

        private void MenuNext_Click(object sender, RoutedEventArgs e)
        {
            if (wallHavenResult == null || currentIndex >= wallHavenResult.Data.Count - 1) return;
            currentIndex += 1;
            ShowImage();
        }

        private void ImagesDisplay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement image = (FrameworkElement)sender;
            if (wallHavenResult != null && image.Tag is string)
            {
                int index = wallHavenResult.Data.FindIndex(o => o.Id == image.Tag.ToString());
                if (index >= 0)
                {
                    currentIndex = index;
                    ShowImage();
                    e.Handled = true;
                }
            }
        }

        private void MenuRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshPicList();
        }

        private void MenuCopy_Click(object sender, RoutedEventArgs e)
        {
            if (ImageDisplay.Source != null && ImageDisplay.Source is BitmapSource source)
            {
                BitmapSource bitmapImage = source;
                Clipboard.SetImage(bitmapImage);
            }
        }

        private string currentName = "";
        private void ShowImage()
        {
            if (wallHavenResult == null || currentIndex >= wallHavenResult.Data.Count - 1 || currentIndex < 0)
            {
                ChangeLoading(true);
                TipsPos.Content = languageRD != null ? languageRD["loadingFail"] : "";
                return;
            }
            string imageUrl = wallHavenResult.Data[currentIndex].Path;
            currentName = wallHavenResult.Data[currentIndex].Tags?.FirstOrDefault()?.Name ?? "";
            if (!string.IsNullOrWhiteSpace(imageUrl)) SetImage(imageUrl);
            MenuLast.IsEnabled = currentIndex > 0;
            MenuNext.IsEnabled = currentIndex < wallHavenResult.Data.Count - 1;
        }

        ResourceDictionary languageRD;
        private void SetImage(string imageUri)
        {
            BitmapImage imageBitmap = new BitmapImage(new Uri(imageUri));
            if (imageBitmap.IsDownloading)
            {
                ChangeLoading(true);
                MenuSaveAs.IsEnabled = false;
                MenuWallpaper.IsEnabled = false;
                MenuCopy.IsEnabled = false;
                imageBitmap.DownloadProgress += ImageBitmap_DownloadProgress;
                imageBitmap.DownloadCompleted += ImageBitmap_DownloadCompleted;
            }
            ImageDisplay.Source = imageBitmap;
            ImageDisplay.RenderTransform = new MatrixTransform();
        }

        private void ChangeLoading(bool show)
        {
            ResourceManager currentResource = new ResourceManager("Language.Properties.Resources", typeof(Properties.Resources).Assembly);
            var languages = Windows.System.UserProfile.GlobalizationPreferences.Languages.ToList();
            string language = "en-us";
            if (languages != null && languages.Count > 0)
            {
                if (languages[0].Contains("zh") || languages[0].Contains("cn")) language = "zh-cn";
            }
            if(languageRD == null)
            {
                languageRD = Application.Current.Resources.MergedDictionaries.FirstOrDefault(m => m.Source != null && m.Source.OriginalString.Equals($@"Resources\{language}.xaml"));
            }
            TipsPos.Content = show && languageRD != null ? $"{languageRD["loading"]}..." : "";
            TipsPos.Visibility = show ? Visibility.Visible : Visibility.Hidden;
        }

        private void ImageBitmap_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            Console.WriteLine();
            int progress = e.Progress;
            Dispatcher.Invoke(() => { TipsPos.Content = languageRD != null ? $"{languageRD["loading"]}...[{e.Progress}]" : ""; });
        }

        private void ImageBitmap_DownloadCompleted(object sender, EventArgs e)
        {
            ChangeLoading(false);
            if (sender == null || !(sender is BitmapSource)) return;
            ((BitmapSource)sender).DownloadCompleted -= ImageBitmap_DownloadCompleted;
            ((BitmapSource)sender).DownloadProgress += ImageBitmap_DownloadProgress;
            MenuSaveAs.IsEnabled = true;
            MenuWallpaper.IsEnabled = true;
            MenuCopy.IsEnabled = true;
        }

        private void Zoom(double delta, Point position)
        {
            Matrix matrix = ImageDisplay.RenderTransform.Value;
            double scale = delta > 0 ? zoomStep : (1.0 / zoomStep); //determine which way to zoom
            matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);

            if (matrix.M11 > zoomMin && matrix.M11 < zoomMax) //restrict zoom in and out amounts
            {
                if (matrix.M11 <= 1 || matrix.M22 <= 1 ||
                    ImageDisplay.ActualWidth * matrix.M11 + matrix.OffsetX < ImageBackgroud.ActualWidth ||
                    ImageDisplay.ActualHeight * matrix.M22 + matrix.OffsetY < ImageBackgroud.ActualHeight)
                {
                    matrix.OffsetX = ImageDisplay.ActualWidth * (1 - matrix.M11) / 2;
                    matrix.OffsetY = ImageDisplay.ActualHeight * (1 - matrix.M22) / 2;
                }
                ImageDisplay.RenderTransform = new MatrixTransform(matrix);
            }
        }

        private void ImageBackgroud_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ImageDisplay.Source == null) return;
            Zoom(e.Delta, e.GetPosition(ImageDisplay));
        }

        private void ImageDisplay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ImageDisplay.Source == null || !ImageDisplay.CaptureMouse())
            {
                return;
            }
            imageMouseDown = true;

            mousePosition = e.GetPosition(ImageBackgroud);
            imagePosition.X = ImageDisplay.RenderTransform.Value.OffsetX;
            imagePosition.Y = ImageDisplay.RenderTransform.Value.OffsetY;
        }

        private void ImageDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (!imageMouseDown)
            {
                return;
            }

            currentPosition = e.GetPosition(ImageBackgroud);
            Matrix matrix = ImageDisplay.RenderTransform.Value;
            matrix.OffsetX = imagePosition.X + currentPosition.X - mousePosition.X;
            matrix.OffsetY = imagePosition.Y + currentPosition.Y - mousePosition.Y;
            ImageDisplay.RenderTransform = new MatrixTransform(matrix);
            mouseMoved = true;
        }

        private void ImageDisplay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _ = Mouse.Capture(null);
            imageMouseDown = false;
        }

        private bool mouseMoved = false;
        private void ImageBackgroud_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!mouseMoved)
            {
                var mousePos = e.GetPosition(ImageBackgroud);
                if (mousePos.X < ImageBackgroud.ActualWidth / 2)
                {
                    MenuLast_Click(ImageBackgroud, new RoutedEventArgs());
                }
                else
                {
                    MenuNext_Click(ImageBackgroud, new RoutedEventArgs());
                }
                mouseMoved = false;
            }
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "图片 (*.jpg)|*.jpg",
                FilterIndex = 1,
                RestoreDirectory = true,
                FileName = currentName
            };
            //sfd.FileName = Title;
            if (sfd.ShowDialog() == true)
            {
                SaveImage(sfd.FileName.ToString());
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        private void MenuWallpaper_Click(object sender, RoutedEventArgs e)
        {
            if (SaveImage(Settings.AppCacheImagePath))
            {
                MenuWallpaper.IsEnabled = false;
                (int, int) wpStyle = (-1, -1);
                if (e.OriginalSource != null && e.OriginalSource is System.Windows.Controls.MenuItem menuStyle && menuStyle.Tag != null)
                {
                    wpStyle = WallPaperStyle((string)menuStyle.Tag);
                }
                Task.Run(() => {
                    if (wpStyle.Item1 != -1 && wpStyle.Item2 != -1)
                    {
                        Registry.CurrentUser.OpenSubKey("Control Panel", true)?.OpenSubKey("Desktop", true)?.SetValue("TileWallpaper", wpStyle.Item1.ToString());
                        Registry.CurrentUser.OpenSubKey("Control Panel", true)?.OpenSubKey("Desktop", true)?.SetValue("WallpaperStyle", wpStyle.Item2.ToString());
                    }
                    SystemParametersInfo(20, 0, Settings.AppCacheImagePath, 0x01 | 0x02);
                    Dispatcher.Invoke(new Action(() => {
                        MenuWallpaper.IsEnabled = true;
                    }));
                });
            }
        }

        private (int, int) WallPaperStyle(string type)
        {
            switch (type)
            {
                case "1": return (0, 10);
                case "2": return (0, 2);
                case "3": return (1, 0);
                case "4": return (0, 0);
                case "5": return (0, 22);
                default: return (-1, -1);
            }
        }

        private bool SaveImage(string filePath)
        {
            if (ImageDisplay.Source != null && ImageDisplay.Source is BitmapSource source)
            {
                BitmapSource bitmapImage = source;
                if (bitmapImage.IsDownloading) return false;
                try
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                    using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate))
                    {
                        encoder.Save(fileStream);
                    }
                    return true;
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            setting.WindowHeight = ActualHeight;
            setting.WindowWidth = ActualWidth;

            Settings.SaveSetting(setting);
        }

        private void MenuRatio_Click(object sender, RoutedEventArgs e)
        {
            setting.Wide = MenuWide.IsChecked;
            setting.UltraWide = MenuUltraWide.IsChecked;
            setting.Portrait = MenuPortrait.IsChecked;
            setting.Square = MenuSquare.IsChecked;
        }

        private void MenuInclude_Click(object sender, RoutedEventArgs e)
        {
            setting.General = MenuGeneral.IsChecked;
            setting.Anime = MenuAnime.IsChecked;
            setting.People = MenuPeople.IsChecked;
            setting.SFW = MenuSFW.IsChecked;
            setting.Sketchy = MenuSketchy.IsChecked;
            setting.NSFW = MenuNSFW.IsChecked;
        }

        private void MenuSorting_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == null || ReferenceEquals(sender, e.OriginalSource)) return;
            System.Windows.Controls.MenuItem menu = (System.Windows.Controls.MenuItem)e.OriginalSource;
            if (menu.Tag != null) SetMenuSort(StrToSorting(menu.Tag.ToString()));
            setting.Sort = GetSorting();
        }

        private Sorting StrToSorting(string sortStr)
        {
            switch (sortStr)
            {
                case "1": return Sorting.date_added;
                case "2": return Sorting.relevance;
                case "3": return Sorting.random;
                case "4": return Sorting.views;
                case "5": return Sorting.favourites;
                case "6": return Sorting.toplist;
                case "7": return Sorting.hot;
                default: return Sorting.random;
            }
        }

        private void SetMenuSort(Sorting sorting)
        {
            MenuDateAdded.IsChecked = MenuDateAdded.Tag != null && (StrToSorting(MenuDateAdded.Tag.ToString()) == sorting);
            MenuRelevance.IsChecked = MenuRelevance.Tag != null && (StrToSorting(MenuRelevance.Tag.ToString()) == sorting);
            MenuRandom.IsChecked = MenuRandom.Tag != null && (StrToSorting(MenuRandom.Tag.ToString()) == sorting);
            MenuViews.IsChecked = MenuViews.Tag != null && (StrToSorting(MenuViews.Tag.ToString()) == sorting);
            MenuFavourites.IsChecked = MenuFavourites.Tag != null && (StrToSorting(MenuFavourites.Tag.ToString()) == sorting);
            MenuToplist.IsChecked = MenuToplist.Tag != null && (StrToSorting(MenuToplist.Tag.ToString()) == sorting);
            MenuHot.IsChecked = MenuHot.Tag != null && (StrToSorting(MenuHot.Tag.ToString()) == sorting);
        }

        private Sorting GetSorting()
        {
            if (MenuDateAdded.IsChecked) return Sorting.date_added;
            if (MenuRelevance.IsChecked) return Sorting.relevance;
            if (MenuRandom.IsChecked) return Sorting.random;
            if (MenuViews.IsChecked) return Sorting.views;
            if (MenuFavourites.IsChecked) return Sorting.favourites;
            if (MenuToplist.IsChecked) return Sorting.toplist;
            if (MenuHot.IsChecked) return Sorting.hot;
            return Sorting.random;
        }

        private void SetCtrlButtonAnimation(object sender, double scale)
        {
            var animation = new DoubleAnimation()
            {
                To = scale,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            BorderCloseAnimation.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            BorderMinAnimation.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            BorderMaxAnimation.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            //if (Equals(BorderClose, sender)) BorderCloseAnimation.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            //else if (Equals(BorderMin, sender)) BorderMinAnimation.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            //else if (Equals(BorderMax, sender)) BorderMaxAnimation.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            SetCtrlButtonAnimation(sender, 1);
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            SetCtrlButtonAnimation(sender, 1.5);
        }

        private void BoderSettingCtrlParent_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isBorderSettingShow) return;
            SetSettingCtrlButtonAnimation(0);
        }

        private void BoderSettingCtrlParent_MouseLeave(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(BoderSettingCtrl);
            if(pt.Y > 0) SetSettingCtrlButtonAnimation(-55);
        }

        private void SetSettingCtrlButtonAnimation(int y)
        {
            var animation = new DoubleAnimation()
            {
                To = y,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            BoderSettingCtrl.RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        private bool isBorderSettingShow = false;
        private void ShowBorderSetting(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            SetBorderSetting();
            isBorderSettingShow = true;
            SetSettingBorderAnimation(0);
        }

        private void HideBorderSetting(object sender, RoutedEventArgs e)
        {
            isBorderSettingShow = false;
            SetSettingBorderAnimation(-380);
            GetBorderSetting();
            InitMenuCheck();
        }

        private void SetSettingBorderAnimation(int y)
        {
            var animation = new DoubleAnimation()
            {
                To = y,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            BorderSetting.RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        private void GetBorderSetting()
        {
            setting.Sort = GetSettingSorting();
            setting.APIKey = SettingsAPIToken.Text;
            setting.General = SettingsIncludeGeneral.IsOn;
            setting.SFW = SettingsIncludeSFW.IsOn;
            setting.Anime = SettingsIncludeAnime.IsOn;
            setting.Sketchy = SettingsIncludeSketchy.IsOn;
            setting.People = SettingsIncludePeople.IsOn;
            setting.NSFW = SettingsIncludeNSFW.IsOn;
            setting.Wide = SettingsRatioWide.IsOn;
            setting.UltraWide = SettingsRatioUltraWide.IsOn;
            setting.Portrait = SettingsRatioPortrait.IsOn;
            setting.Square = SettingsRatioSquare.IsOn;
        }

        private void SetBorderSetting()
        {
            SettingsAPIToken.Text = setting.APIKey;
            SettingsIncludeGeneral.IsOn = setting.General;
            SettingsIncludeSFW.IsOn = setting.SFW;
            SettingsIncludeAnime.IsOn = setting.Anime;
            SettingsIncludeSketchy.IsOn = setting.Sketchy;
            SettingsIncludePeople.IsOn = setting.People;
            SettingsIncludeNSFW.IsOn = setting.NSFW;
            SettingsRatioWide.IsOn = setting.Wide;
            SettingsRatioUltraWide.IsOn = setting.UltraWide;
            SettingsRatioPortrait.IsOn = setting.Portrait;
            SettingsRatioSquare.IsOn = setting.Square;
            SetSettingSorting(setting.Sort);
        }

        private void SetSettingSorting(Sorting sorting)
        {
            if ((sorting == Sorting.date_added && !SettingsSortingDateAdded.IsOn) || (sorting != Sorting.date_added && SettingsSortingDateAdded.IsOn))
            {
                SettingsSortingDateAdded.IsOn = SettingsSortingDateAdded.Tag != null && (StrToSorting(SettingsSortingDateAdded.Tag.ToString()) == sorting);
            }
            if ((sorting == Sorting.relevance && !SettingsSortingRelevance.IsOn) || (sorting != Sorting.relevance && SettingsSortingRelevance.IsOn))
            {
                SettingsSortingRelevance.IsOn = SettingsSortingRelevance.Tag != null && (StrToSorting(SettingsSortingRelevance.Tag.ToString()) == sorting);
            }
            if ((sorting == Sorting.random && !SettingsSortingRandom.IsOn) || (sorting != Sorting.random && SettingsSortingRandom.IsOn))
            {
                SettingsSortingRandom.IsOn = SettingsSortingRandom.Tag != null && (StrToSorting(SettingsSortingRandom.Tag.ToString()) == sorting);
            }
            if ((sorting == Sorting.views && !SettingsSortingViews.IsOn) || (sorting != Sorting.views && SettingsSortingViews.IsOn))
            {
                SettingsSortingViews.IsOn = SettingsSortingViews.Tag != null && (StrToSorting(SettingsSortingViews.Tag.ToString()) == sorting);
            }
            if ((sorting == Sorting.favourites && !SettingsSortingFavourites.IsOn) || (sorting != Sorting.favourites && SettingsSortingFavourites.IsOn))
            {
                SettingsSortingFavourites.IsOn = SettingsSortingFavourites.Tag != null && (StrToSorting(SettingsSortingFavourites.Tag.ToString()) == sorting);
            }
            if ((sorting == Sorting.toplist && !SettingsSortingToplist.IsOn) || (sorting != Sorting.toplist && SettingsSortingToplist.IsOn))
            {
                SettingsSortingToplist.IsOn = SettingsSortingToplist.Tag != null && (StrToSorting(SettingsSortingToplist.Tag.ToString()) == sorting);
            }
        }

        private Sorting GetSettingSorting()
        {
            if (SettingsSortingDateAdded.IsOn) return Sorting.date_added;
            if (SettingsSortingRelevance.IsOn) return Sorting.relevance;
            if (SettingsSortingRandom.IsOn) return Sorting.random;
            if (SettingsSortingViews.IsOn) return Sorting.views;
            if (SettingsSortingFavourites.IsOn) return Sorting.favourites;
            if (SettingsSortingToplist.IsOn) return Sorting.toplist;
            return Sorting.random;
        }

        private void SettingsSorting_Indeterminate(object sender, RoutedEventArgs e)
        {
            ToggleSwitch menu = (ToggleSwitch)sender;
            if (menu.IsOn)
            {
                SetSettingSorting(StrToSorting(menu.Tag.ToString()));
            }
            else if (!menu.IsOn)
            {

            }
        }

        private void BoderDisplayParent_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isDisplayShow) return;
            SetDisplayButtonAnimation(10);
        }

        private void BoderDisplayParent_MouseLeave(object sender, MouseEventArgs e)
        {
            SetDisplayButtonAnimation(-40);
        }

        private void SetDisplayButtonAnimation(int y)
        {
            var animation = new DoubleAnimation()
            {
                To = y,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            BoderDisplayCtrl.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void SetDisplayBorderAnimation(int x)
        {
            var animation = new DoubleAnimation()
            {
                To = x,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            BorderImages.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private bool isDisplayShow = false;
        private void ShowImageDisplay(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            isDisplayShow = true;
            SetDisplayBorderAnimation(0);
        }

        private void HideImageDisplay(object sender, RoutedEventArgs e)
        {
            isDisplayShow = false;
            SetDisplayBorderAnimation(-240);
        }

        private void APIToken_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string uri = SettingsAPITokenLabel.ToolTip.ToString();
            if (string.IsNullOrWhiteSpace(uri)) return;
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = uri
            };
            System.Diagnostics.Process.Start(psi);
        }
    }
}
