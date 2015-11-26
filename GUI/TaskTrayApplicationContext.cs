using System.Windows.Forms;

namespace RabotatAgent.GUI
{
    public class TaskTrayApplicationContext : ApplicationContext
    {
        NotifyIcon notifyIcon = new NotifyIcon();

        public TaskTrayApplicationContext()
        {
            notifyIcon.Icon = Properties.Resources.AppIcon;
            notifyIcon.Visible = true;
        }
    }
}
