using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NotAmpTray
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var app = System.Windows.Application.Current;
            // Windowをクローズしてもアプリケーションが終了しないように設定しておく。
            app.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;  // Exitを明示的にCallすることでアプリケーションが終了するように設定

        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
