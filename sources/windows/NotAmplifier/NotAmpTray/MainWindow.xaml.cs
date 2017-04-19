using System;
using System.Collections.Generic;
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
    }
}
