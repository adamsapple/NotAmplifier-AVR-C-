using System.Windows;

namespace NotAmpTray.Command
{
    class AppCloseCommand : Moral.Command.SimpleCommand
    {
        override public void Execute(object parameter)
        {
            Application.Current.Shutdown();
        }
    }
}
