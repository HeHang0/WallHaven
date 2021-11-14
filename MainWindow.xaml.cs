using Microsoft.Win32;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WallHaven.WallHavenClient;

namespace WallHaven
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
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
        private WallHavenRequest? wallHaven;
        private WallHavenResponse? wallHavenResult;
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (setting.WindowHeight > 0 && setting.WindowWidth > 0)
            {
                Height = setting.WindowHeight;
                Width = setting.WindowWidth;
            }
            SetMenuSort(setting.Sort);
            InitHaven();
        }

        private void InitHaven()
        {
            while (string.IsNullOrWhiteSpace(setting.APIKey))
            {
                InputStringWindow inputStringWindow = new InputStringWindow("WallHaven APIKey", "https://wallhaven.cc/settings/account", false);
                if (inputStringWindow.ShowDialog() == true)
                {
                    setting.APIKey = inputStringWindow.InputString;
                }
            }

            Config config = new Config()
            {
                BaseUrl = setting.BaseUrl,
                APIKey = setting.APIKey
            };
            wallHaven = new WallHavenRequest(new HttpClient(), config);
            MenuGeneral.IsChecked = setting.General;
            MenuAnime.IsChecked = setting.Anime;
            MenuPeople.IsChecked = setting.People;
            MenuSFW.IsChecked = setting.SFW;
            MenuSketchy.IsChecked = setting.Sketchy;
            MenuNSFW.IsChecked = setting.NSFW;
            RefreshPicList();
        }

        private async void RefreshPicList()
        {
            if (wallHaven == null) return;
            ChangeLoading(true);
            MenuRefresh.IsEnabled = false;
            string _searchParams = new SearchParamsBuilder()
                  //.WithMinimumResolution(3440, 1440)
                  .IncludeGeneral(MenuGeneral.IsChecked)
                  .IncludeAnime(MenuAnime.IsChecked)
                  .IncludePeople(MenuPeople.IsChecked)
                  .IncludeSafe(MenuSFW.IsChecked)
                  .IncludeSketchy(MenuSketchy.IsChecked)
                  .IncludeNSFW(MenuNSFW.IsChecked)
                  .OrderBy(OrderBy.desc)
                  .SortBy(GetSorting())
                  .Build();
            try
            {
                wallHavenResult = await wallHaven.Search(_searchParams);
                currentIndex = 0;
                MenuRefresh.IsEnabled = true;
                ShowImage();
            }
            catch (Exception)
            {
                TipsPos.Content = "加载失败";
            }
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

        private void MenuRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshPicList();
        }

        private void ShowImage()
        {
            if (wallHavenResult == null || currentIndex >= wallHavenResult.Data.Count - 1 || currentIndex < 0)
            {
                ChangeLoading(true);
                TipsPos.Content = "加载失败";
                return;
            }
            string imageUrl = wallHavenResult.Data[currentIndex].Path;
            if(!string.IsNullOrWhiteSpace(imageUrl)) SetImage(imageUrl);
            MenuLast.IsEnabled = currentIndex > 0;
            MenuNext.IsEnabled = currentIndex < wallHavenResult.Data.Count - 1;
        }

        private void SetImage(string imageUri)
        {
            ChangeLoading(true);
            BitmapImage imageBitmap = new BitmapImage(new Uri(imageUri));
            imageBitmap.DownloadCompleted += ImageBitmap_DownloadCompleted;
            ImageDisplay.Source = imageBitmap;
            ImageDisplay.RenderTransform = new MatrixTransform();
        }

        private void ChangeLoading(bool show)
        {
            TipsPos.Content = show ? "加载中..." : "";
            TipsPos.Visibility = show ? Visibility.Visible : Visibility.Hidden;
        }

        private void ImageBitmap_DownloadCompleted(object sender, EventArgs e)
        {
            ChangeLoading(false);
            if (sender == null || !(sender is BitmapSource)) return;
            ((BitmapSource)sender).DownloadCompleted -= ImageBitmap_DownloadCompleted;
            MenuSaveAs.IsEnabled = true;
            MenuWallpaper.IsEnabled = true;
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

        private bool mouseMoved = false;
        private void ImageDisplay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _ = Mouse.Capture(null);
            imageMouseDown = false;
            if (mouseMoved)
            {
                var mousePos = e.GetPosition(ImageBackgroud);
                if(mousePos.X < ImageBackgroud.ActualWidth / 2)
                {
                    MenuLast_Click(ImageBackgroud, new RoutedEventArgs());
                }
                else
                {
                    MenuNext_Click(ImageBackgroud, new RoutedEventArgs());
                }
            }
            mouseMoved = false;
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "图片 (*.jpg)|*.jpg",
                FilterIndex = 1,
                RestoreDirectory = true
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
                if (e.OriginalSource != null && e.OriginalSource is System.Windows.Controls.MenuItem menuStyle && menuStyle.Tag != null)
                {
                    (int, int) wpStyle = WallPaperStyle((string)menuStyle.Tag);
                    if (wpStyle.Item1 != -1 && wpStyle.Item2 != -1)
                    {
                        Registry.CurrentUser.OpenSubKey("Control Panel", true)?.OpenSubKey("Desktop", true)?.SetValue("TileWallpaper", wpStyle.Item1.ToString());
                        Registry.CurrentUser.OpenSubKey("Control Panel", true)?.OpenSubKey("Desktop", true)?.SetValue("WallpaperStyle", wpStyle.Item2.ToString());
                    }
                }
                SystemParametersInfo(20, 0, Settings.AppCacheImagePath, 0x01 | 0x02);
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
            }
            return (-1, -1);
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
            setting.General = MenuGeneral.IsChecked;
            setting.Anime = MenuAnime.IsChecked;
            setting.People = MenuPeople.IsChecked;
            setting.SFW = MenuSFW.IsChecked;
            setting.Sketchy = MenuSketchy.IsChecked;
            setting.NSFW = MenuNSFW.IsChecked;
            setting.WindowHeight = ActualHeight;
            setting.WindowWidth = ActualWidth;
            Settings.SaveSetting(setting);
        }



        private void MenuSorting_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == null || ReferenceEquals(sender, e.OriginalSource)) return;
            System.Windows.Controls.MenuItem menu = (System.Windows.Controls.MenuItem)e.OriginalSource;
            if (menu.Tag != null) SetMenuSort(StrToSorting(menu.Tag.ToString()));
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
                default: return Sorting.random;
            }
        }

        private void SetMenuSort(Sorting sorting)
        {
            MenuDateAdded.IsChecked = MenuDateAdded.Tag == null ? false : (StrToSorting(MenuDateAdded.Tag.ToString()) == sorting);
            MenuRelevance.IsChecked = MenuRelevance.Tag == null ? false : (StrToSorting(MenuRelevance.Tag.ToString()) == sorting);
            MenuRandom.IsChecked = MenuRandom.Tag == null ? false : (StrToSorting(MenuRandom.Tag.ToString()) == sorting);
            MenuViews.IsChecked = MenuViews.Tag == null ? false : (StrToSorting(MenuViews.Tag.ToString()) == sorting);
            MenuFavourites.IsChecked = MenuFavourites.Tag == null ? false : (StrToSorting(MenuFavourites.Tag.ToString()) == sorting);
            MenuToplist.IsChecked = MenuToplist.Tag == null ? false : (StrToSorting(MenuToplist.Tag.ToString()) == sorting);
        }

        private Sorting GetSorting()
        {
            if (MenuDateAdded.IsChecked) return Sorting.date_added;
            if (MenuRelevance.IsChecked) return Sorting.relevance;
            if (MenuRandom.IsChecked) return Sorting.random;
            if (MenuViews.IsChecked) return Sorting.views;
            if (MenuFavourites.IsChecked) return Sorting.favourites;
            if (MenuToplist.IsChecked) return Sorting.toplist;
            return Sorting.random;
        }
    }
}
