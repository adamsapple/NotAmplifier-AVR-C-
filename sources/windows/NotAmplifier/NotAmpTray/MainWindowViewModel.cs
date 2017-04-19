using NotAmpTray.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moral.Util;

namespace NotAmpTray
{
    class MainWindowViewModel: Moral.Model.SimpleModel
    {
        public AppCloseCommand AppCloseCommand { get; private set; } = new AppCloseCommand();

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
    }
}
