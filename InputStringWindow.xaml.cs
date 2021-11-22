using System.Windows;
using System.Windows.Input;

namespace WallHaven
{
    /// <summary>
    /// InputStringWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InputStringWindow : Window
    {
        public string InputString = string.Empty;
        
        public InputStringWindow(string title = "", string subtitle = "", bool showCancel = true)
        {
            InitializeComponent();
            WindowTitle.Text = title;
            SubTitle.Content = subtitle;
            CancelButton.Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed;
            if (subtitle.StartsWith("http"))
            {
                SubTitle.Cursor = Cursors.Hand;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            InputString = InputStringTextBox.Text;
            DialogResult = true;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SubTitle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(SubTitle.Cursor == Cursors.Hand)
            {
                string uri = SubTitle.Content.ToString();
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
}
