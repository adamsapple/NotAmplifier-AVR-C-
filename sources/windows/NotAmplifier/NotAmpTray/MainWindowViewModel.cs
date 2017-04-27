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
using System.Collections.ObjectModel;

namespace NotAmpTray
{
    class MainWindowViewModel: Moral.Model.SimpleModel, IDisposable
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
                if(ComPorts.IndexOf(value) >= 0)
                {
                    SetProperty(ref _SerialPortName, value);

                    notampConnector.PortName = _SerialPortName;
                }
            }
        }

        public ObservableCollection<string> ComPorts { get; private set; }        

        private bool _IsDeviceEnable;
        public bool IsDeviceEnable
        {
            get
            {
                return _IsDeviceEnable;
            }
            set
            {
                if (value && _SerialPortName==null)
                {
                    IsDeviceEnable = false;
                    return;
                }

                SetProperty(ref _IsDeviceEnable, value);

                if ( value )
                {
                    notampConnector.Open();
                }
                else
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
            switch (e.Type)
            {
                case MessageType.MSG_OP_ID_MPW:

                    break;
            }
        }

        public MainWindowViewModel()
        {
            //! セッティング情報読み込み
            var setting = Properties.Settings.Default;

            if (!setting.IsUpgrade)
            {
                setting.Upgrade();                      //!< Upgradeを実行する
                setting.IsUpgrade = true;               //!<「Upgradeを実行した」という情報を設定する
                setting.Save();                         //!< 現行バージョンの設定を保存する
            }

            //! NotAmpコネクタの初期化
            notampConnector = new NotAmpConnector();
            notampConnector.OnNotAmpDataRecieved += NotAmpDataRecievedHandler;

            //! ComPorts一覧初期化
            var ComPorts = new ObservableCollection<string>();
            Array.ForEach<string>(SerialPort.GetPortNames(), (str)=>ComPorts.Add(str));

            this.ComPorts   = ComPorts;
            GitURL          = setting.GitURL;
            SerialPortName  = setting.SerialPortName;

            
            if (0 > ComPorts.IndexOf(SerialPortName))
            {
                SerialPortName = null;
            }

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

        public void Dispose()
        {
            notampConnector.Close();
        }
    }
}
