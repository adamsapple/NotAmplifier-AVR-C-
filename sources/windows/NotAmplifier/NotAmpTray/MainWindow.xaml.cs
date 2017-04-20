using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NotAmpTray
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var setting          = Properties.Settings.Default;
            TrayIcon.IconSource  = this.Icon;
            TrayIcon.ToolTipText = setting.tray_tooltip;

            TrayIcon.TrayMouseDoubleClick += (_o, _e) => {
                var target = this;

                // オプション画面の表示/非表示を変更
                if (target.IsVisible)
                {
                    target.Hide();
                }
                else
                {
                    target.Show();
                    target.Activate();   // ダブルクリックするとTrayにFocusを持ってかれるので、窓にFocusを再度与える(強制)
                }
            };

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void MenuItem_Config_Click(object sender, RoutedEventArgs e)
        {
            Show();
        }

        /// <summary>
        /// ハイパーリンククリック時のイベント
        /// ブラウザで所定URL(e.Uri.AbsoluteUri)に遷移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        /// <summary>
        /// 選択Tabが変更になった際のイベント。
        /// 「状態」では、ピークメータ更新用タイマーを動作させ、それ以外の時は止める。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ti = (sender as TabControl).SelectedItem as TabItem;

            if (ti == tiStatus)
            {
                //viewmodel.StartPeakMeter();
            }
            else
            {
                //viewmodel.StopPeakMeter();
            }
        }
    }
}
