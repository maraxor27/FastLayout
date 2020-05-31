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
            Application.Run(new TasktrayApplication());
        }
    }
    public class TasktrayApplication : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private Thread appThread;
        private bool[] msg;

        public TasktrayApplication()
        {
            msg = new bool[] { false };
            Start();
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Icon.FromHandle(Resources.Icon_32x32.GetHicon()),
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Reset", Reset), new MenuItem("Exit", Exit)
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
        void Reset(object sender, EventArgs e)
        {
            msg[0] = true;
        }
        void Start()
        {
            Console.WriteLine("Starting Form1");
            appThread = new Thread(FormThread);
            appThread.SetApartmentState(ApartmentState.STA);
            appThread.Start();
        }
        private void FormThread()
        {
            Application.Run(new Form1(msg));
        }
    }
}
