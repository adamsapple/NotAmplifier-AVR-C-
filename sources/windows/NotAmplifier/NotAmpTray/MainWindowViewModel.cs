using NotAmpTray.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moral.Util;

using NotAmplifier;
using NotAmplifier.Protocol;
using System.IO.Ports;

namespace NotAmpTray
{
    class MainWindowViewModel: Moral.Model.SimpleModel
    {
        public AppCloseCommand AppCloseCommand { get; private set; } = new AppCloseCommand();

        public string GitURL { get; private set; } = null;
        public string _SerialPortName;
        public string SerialPortName {
            get
            {
                return _SerialPortName;
            }
            set
            {
                if(Array.IndexOf(SerialPort.GetPortNames(), value) >= 0)
                {
                    _SerialPortName = value;

                    notampConnector.PortName = _SerialPortName;
                }
            }
        }

        private bool _IsDeviceEnable;
        public bool IsDeviceEnable
        {
            get
            {
                return _IsDeviceEnable;
            }
            set
            {
                SetProperty(ref _IsDeviceEnable, value);

                if ( value )
                {
                    notampConnector.Open();
                }else
                {
                    notampConnector.Close();
                }
            }
        }

        private bool _IsStartUp;
        public bool IsStartUp
        {
            get { return (_IsStartUp = WindowsUtil.ExistsStartUp_CurrentUserRun()); }
            set { SetProperty(ref _IsStartUp, value);
                if (value)
                {
                    WindowsUtil.RegiserStartUp_CurrentUserRun();
                }
                else
                {
                    WindowsUtil.UnregiserStartUp_CurrentUserRun();
                }
            }
        }

        private NotAmpConnector notampConnector;

        void NotAmpDataRecievedHandler(object sender, NotAmpDataRecievedEventArgs e)
        {

        }

        public MainWindowViewModel()
        {
            //! セッティング情報読み込み
            var setting = Properties.Settings.Default;

            //! Configに書き込まれたAssemblyVersionを比較し、
            //!  今回起動したApplicationを差異があれば、コンフィグのUpgradeを行う。
            var assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (!assemblyVersion.Equals(setting.Version))
            {
                setting.Upgrade();
                setting.Version = assemblyVersion;
                setting.Save();
            }

            GitURL         = setting.GitURL;
            SerialPortName = setting.SerialPortName;
            
            if(0 > Array.IndexOf(SerialPort.GetPortNames(), SerialPortName))
            {
                SerialPortName = null;
            }

            notampConnector = new NotAmpConnector();
            notampConnector.OnNotAmpDataRecieved += NotAmpDataRecievedHandler;

            if (SerialPortName != null)
            {
                notampConnector.PortName = SerialPortName;
            }

            IsDeviceEnable = setting.IsDeviceEnable;
        }

        public void SaveSettings()
        {
            var setting = Properties.Settings.Default;

            setting.SerialPortName = SerialPortName;
            setting.IsDeviceEnable = IsDeviceEnable;

            setting.Save();
        }
    }
}
