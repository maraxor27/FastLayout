using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using TEST2.Properties;

namespace TEST2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Thread t = new Thread(FormThread);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            Application.Run(new TasktrayApplication());
        }
        private static void FormThread()
        {
            Application.Run(new Form1());
        }
    }
    public class TasktrayApplication : ApplicationContext
    {
        private NotifyIcon trayIcon;
        
        public TasktrayApplication()
        {
            
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Icon.FromHandle(Resources.Icon_32x32.GetHicon()),
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Exit", Exit)
            }),
                Visible = true
            };
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;
            Process.GetCurrentProcess().Kill();
        }
    }
}
