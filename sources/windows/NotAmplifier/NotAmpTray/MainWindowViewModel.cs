using NotAmpTray.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moral.Util;

using NotAmplifier;
using NotAmplifier.Protocol;

namespace NotAmpTray
{
    class MainWindowViewModel: Moral.Model.SimpleModel
    {
        public AppCloseCommand AppCloseCommand { get; private set; } = new AppCloseCommand();

        public string GitURL
        {
            get { return Properties.Settings.Default.GitURL; }
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
                    //notampConnector.Open();
                }else
                {
                    //notampConnector.Close();
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
            IsDeviceEnable = true;

            notampConnector = new NotAmpConnector();
            notampConnector.OnNotAmpDataRecieved += NotAmpDataRecievedHandler;
        }
    }
}
