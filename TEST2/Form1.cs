﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;


namespace TEST2
{
    public partial class Form1 : Form
    {
        private static string[] blacklist = { "SystemSettings", "Video.UI", "WWAHost", "NVIDIA Share",
            "WindowsInternal.ComposableShell.Experiences.TextInput.InputApp", "WinStore.App", "explorer", "Idle","StartMenuExperienceHost" };
        private Queue<IntPtr> lastOpenWindowHandle;
        private Thread listener;
        private Process currentProcess;
        private RECT[] monitors;
        private Image[][] hImages;
        private Image[][] vImages;
        private const double visibility = 1.0;
        private static bool isRunning = true;
        public Form1()
        {
            // make the app unaffected by screen scaling
            SetProcessDPIAware();
            InitializeComponent();
            this.Opacity = 0.0;
            // start thread to wait for key listener
            listener = new Thread(keylistener);
            listener.SetApartmentState(ApartmentState.STA);
            lastOpenWindowHandle = new Queue<IntPtr>(10);
            currentProcess = Process.GetCurrentProcess();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            GetMontiorSizes();
            LoadImages();
            listener.Start();
            this.TransparencyKey = this.BackColor = Color.Gray;
        }
        private void GetMontiorSizes()
        {
            Screen[] allScreens = Screen.AllScreens;
            monitors = new RECT[allScreens.Length];
            int count = 0;
            foreach (Screen screen in allScreens)
            {
                Graphics g = new Control().CreateGraphics();
                monitors[count].left = screen.Bounds.Location.X;
                monitors[count].top = screen.Bounds.Location.Y;
                monitors[count].right = screen.Bounds.X + screen.Bounds.Size.Width;
                monitors[count].bottom = screen.Bounds.Y + screen.Bounds.Size.Height;
                printRECT(monitors[count]);
                count++;
            }
        }
        private void LoadImages()
        {
            hImages = new Image[4][];
            hImages[0] = new Image[] { global::TEST2.Properties.Resources.h_layout_1_T };
            hImages[1] = new Image[] { global::TEST2.Properties.Resources.h_layout_2_t };
            hImages[2] = new Image[] { global::TEST2.Properties.Resources.h_layout_3_t, global::TEST2.Properties.Resources.h_layout_3_2_t };
            hImages[3] = new Image[] { global::TEST2.Properties.Resources.h_layout_4_t, global::TEST2.Properties.Resources.h_layout_4_2_t };
            vImages = new Image[3][];
            vImages[0] = new Image[] { global::TEST2.Properties.Resources.h_layout_1_T };
            vImages[1] = new Image[] { global::TEST2.Properties.Resources.v_layout_2_t };
            vImages[2] = new Image[] { global::TEST2.Properties.Resources.v_layout_3_t };
        }

        // Coordinate system of RECT structure
        //            |
        //  (-x,-y)   |   (x,-y) 
        //            |
        //-------------------------
        //            |
        //  (-x, y)   |   (x, y) 
        //            |
        // Origine at the top left corner of main display
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        private void ShowUI()
        {
            // Invoke new action to change form value from a thread that didn't initialize it 
            Invoke(new Action(() =>
            {
                this.Opacity = visibility;
                this.TopMost = true;
            }));
        }
        private void HideUI()
        {
            // Invoke new action to change form value from a thread that didn't initialize it 
            Invoke(new Action(() =>
            {
                this.Opacity = 0.0;
                this.TopMost = false;
            }));
        }
        private void MoveForm(int x, int y)
        {
            // Invoke new action to change form value from a thread that didn't initialize it 
            Invoke(new Action(() =>
            {
                // middle of the form at (x, y)
                this.Top = y - this.Height / 2;
                this.Left = x - this.Width / 2;
            }));
        }

        // c++ function 
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);
        //GetWindowRect(IntPtr hWnd, ref RECT Rect)

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        //GetForegroundWindow() return the handle for the last click window

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        //ShowWindow(hWnd, 9); makes it possible to modify maximized window

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("User32.dll")]
        static extern uint GetProcessIdOfThread(IntPtr Thread);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll")]
        static extern bool SetProcessDPIAware();

        [DllImport("User32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, ref RECT Rect);
        private static void printRECT(RECT r)
        {
            Console.WriteLine("top left : [{0},{1}]\nbottom right : [{2},{3}]\n", r.left, r.top, r.right, r.bottom);
        }
        static void PrintProcesses(Process[] processList)
        {
            Console.WriteLine("List of the current processes");
            foreach (Process process in processList)
            {
                // If the process appears on the Taskbar (if has a title)
                // print the information of the process
                try
                {
                    if (process != null && !String.IsNullOrEmpty(process.MainWindowTitle))
                    {
                        Console.WriteLine("\nProcess: {0}", process.ProcessName);
                        Console.WriteLine("    ID   : {0}", process.Id);
                        Console.WriteLine("    Title: {0} \n", process.MainWindowTitle);
                    }
                }
                catch (System.NullReferenceException) { }
            }
        }
        private void keylistener()
        {
            int layoutCounter = 0, cMonitor, lastMonitor = getMonitor(Control.MousePosition.X, Control.MousePosition.Y);
            bool wasCTRLDown, ImageNeedsUpdate = false;
            RECT cm;
            while (true)
            {
                // keep the process low CPU usage
                Thread.Sleep(40);

                wasCTRLDown = false;

                // Wait for CRTLleft + WINleft for activation 
                if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LWin))
                {
                    wasCTRLDown = true;
                    // Checks to make sure WINleft is still clicked 
                    while (Keyboard.IsKeyDown(Key.LWin))
                    {
                        // Makes the layoutImage appear when clicking a window 
                        if (this.Opacity.Equals(0) && lastOpenWindowHandle.Count > 0)
                        {
                            ShowUI();
                            ImageNeedsUpdate = true;
                        }
                            
                        // LayoutImage changes monitors with the cursor
                        cMonitor = getMonitor(Control.MousePosition.X, Control.MousePosition.Y);
                        cm = monitors[cMonitor];
                        if (lastMonitor != cMonitor)
                        {
                            MoveForm((cm.left + cm.right)/2, (cm.top + cm.bottom) / 2);
                            lastMonitor = cMonitor;
                            ImageNeedsUpdate = true;
                        }

                        // update the image 
                        if (this.Opacity == visibility && lastOpenWindowHandle.Count > 0 && ImageNeedsUpdate)
                        {
                            UpdateLayoutImage(cm, lastOpenWindowHandle.Count - 1, layoutCounter);
                            ImageNeedsUpdate = false;
                        }
                        
                        // detects click of the LeftCtrl button
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) && !wasCTRLDown)
                        {
                            Console.WriteLine("changeLayout");
                            layoutCounter++;
                            wasCTRLDown = true;
                            ImageNeedsUpdate = true;
                        }
                        else if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            wasCTRLDown = false;
                        } 
                        
                        // detects left mouse button activation to add last click window to queue
                        if (Control.MouseButtons == MouseButtons.Left)
                        {
                            Console.WriteLine("---   click   ---");
                            try
                            {
                                IntPtr handle = GetForegroundWindow();
                                Process process = GetProcessByHWND(handle);
                                Console.WriteLine("Process clicked - name: {0}", process.ProcessName);

                                if (process != null  && FilterProcess(process) && !lastOpenWindowHandle.Contains(handle))
                                {
                                    Console.WriteLine("Process added to queue");
                                    PrintProcesses(new Process[] { process });
                                    Console.WriteLine("border thickness = {0}", GetBorderThickness(handle));
                                    lastOpenWindowHandle.Enqueue(handle);
                                    lastOpenWindowHandle.TrimExcess();
                                    layoutCounter = 0;
                                    ImageNeedsUpdate = true;
                                }
                            }
                            catch (System.ArgumentException)
                            {
                                Console.WriteLine("Invalide process ID");
                            }
                        }

                        // keep the process low CPU usage
                        Thread.Sleep(30);
                    }
                    HideUI();
                    if (lastOpenWindowHandle.Count != 0)
                    {
                        ApplyDynamicLayoutWithBorder(lastOpenWindowHandle, layoutCounter);
                        lastOpenWindowHandle.Clear();
                    }
                    Thread.Sleep(500);
                }
            }
        }
        private void UpdateLayoutImage(RECT monitor, int index1, int index2)
        {
            Image nextImage;
            if (isHorizontalMonitor(monitor))
            {
                if (index1 > 3)
                    index1 = 3;
                Console.WriteLine("index 1 = {0}, index 2 = {1}", index1, index2);
                nextImage = hImages[index1][index2 % hImages[index1].Length];
            } 
            else
            {
                if (index1 > 2)
                    index1 = 2;
                nextImage = vImages[index1][0];
            }
            this.BackgroundImage = nextImage;
        }
        // Get process from a windows handle
        private static Process GetProcessByHWND(IntPtr hWnd)
        {
            uint processID = 0;
            GetWindowThreadProcessId(hWnd, out processID);
            return Process.GetProcessById(Convert.ToInt32(processID));
        }

        private void ApplyDynamicLayoutWithBorder(Queue<IntPtr> handleQueue, int index)
        {
            IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", null);
            RECT rect, taskbar = new RECT();
            GetWindowRect(taskbarHandle, ref taskbar);
            int borderThickness, taskBarHeight = Math.Abs(taskbar.bottom - taskbar.top);
            int monitor = getMonitor(Control.MousePosition.X, Control.MousePosition.Y);
            Console.WriteLine("Mouse position: ({0},{1}) => Monitor: {2}", Control.MousePosition.X, Control.MousePosition.Y, monitor);

            double scaling = getScalingFactor(Process.GetCurrentProcess().MainWindowHandle);
            Console.WriteLine("Display scaling is: {0}", scaling);
            if (monitor != -1)
            {
                IntPtr currentHandle;
                RECT currentMonitor = monitors[monitor];
                int size = handleQueue.Count;
                int fullUsableHeight = currentMonitor.bottom - currentMonitor.top - taskBarHeight;
                int maxSize = size;
                if (maxSize > 3)
                    maxSize = 3;
                if (isHorizontalMonitor(currentMonitor))
                {
                    //             |             |             |
                    //             |             |             |
                    //             |             |             |
                    //             |             |             |
                    //             |             |             |
                    //             |             |             |
                    //             |             |             |
                    //             |             |             |
                    if (size > 0 && size < 5 && index % hImages[maxSize].Length == 0)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            rect = new RECT();
                            currentHandle = handleQueue.Dequeue();
                            bool result = ShowWindow(currentHandle, 9);
                            if (GetWindowRect(currentHandle, ref rect))
                            {
                                borderThickness = GetBorderThickness(currentHandle);
                                Console.WriteLine("result of minimization: {0}", result);
                                MoveWindowsRelativeToDisplay(currentHandle, currentMonitor, (int)((currentMonitor.right - currentMonitor.left) * i / size - borderThickness * scaling),
                                    0, (int)((currentMonitor.right - currentMonitor.left) / size + borderThickness * 2 * scaling),
                                    (int)(currentMonitor.bottom - currentMonitor.top - taskBarHeight + borderThickness * scaling), true);
                            }
                            if (i == 0)
                            {
                                try
                                {
                                    SetForegroundWindow(currentHandle);
                                }
                                catch (System.InvalidOperationException) { }
                            }
                        }
                    }
                    //             |             |
                    //             |             |
                    //             |             |
                    //             |             | __ __ __ __ __ 
                    //             |             |
                    //             |             |
                    //             |             |
                    //             |             |
                    else if (size > 2 && size < 5 && index % hImages[maxSize].Length == 1)
                    {
                        for (int i = 0; i < size - 2; i++)
                        {
                            rect = new RECT();
                            currentHandle = handleQueue.Dequeue();
                            bool result = ShowWindow(currentHandle, 9);
                            if (GetWindowRect(currentHandle, ref rect))
                            {
                                borderThickness = GetBorderThickness(currentHandle);
                                Console.WriteLine("result of minimization: {0}", result);
                                MoveWindowsRelativeToDisplay(currentHandle, currentMonitor, (int)((currentMonitor.right - currentMonitor.left) * i / (size - 1) - borderThickness * scaling),
                                    0, (int)((currentMonitor.right - currentMonitor.left) / (size - 1) + borderThickness * 2 * scaling),
                                    (int)(currentMonitor.bottom - currentMonitor.top - taskBarHeight + borderThickness * scaling), true);
                            }
                            if (i == 0)
                            {
                                try
                                {
                                    SetForegroundWindow(currentHandle);
                                }
                                catch (System.InvalidOperationException) { }
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            rect = new RECT();
                            currentHandle = handleQueue.Dequeue();
                            bool result = ShowWindow(currentHandle, 9);
                            if (GetWindowRect(currentHandle, ref rect))
                            {
                                Console.WriteLine("result of minimization: {0}", result);
                                borderThickness = GetBorderThickness(currentHandle);
                                MoveWindowsRelativeToDisplay(currentHandle, currentMonitor, (int)((currentMonitor.right - currentMonitor.left) * (size - 2) / (size - 1) - borderThickness * scaling),
                                    currentMonitor.top + fullUsableHeight * i / 2,
                                    (int)((currentMonitor.right - currentMonitor.left) / (size - 1) + borderThickness * 2 * scaling),
                                    (int)(fullUsableHeight / 2 + borderThickness * scaling), true);
                            }
                            if (i == 0)
                            {
                                try
                                {
                                    SetForegroundWindow(currentHandle);
                                }
                                catch (System.InvalidOperationException) { }
                            }
                        }
                    }
                }
                else
                {
                    //__ __ __ __ __ __ __ __ __
                    //
                    //
                    //
                    //
                    //__ __ __ __ __ __ __ __ __
                    //
                    //
                    //
                    //
                    //__ __ __ __ __ __ __ __ __
                    //
                    //
                    //
                    //
                    //__ __ __ __ __ __ __ __ __
                    if (size > 0 && size < 4)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            rect = new RECT();
                            currentHandle = handleQueue.Dequeue();
                            ShowWindow(currentHandle, 9);
                            if (GetWindowRect(currentHandle, ref rect))
                            {
                                borderThickness = GetBorderThickness(currentHandle);
                                MoveWindowsRelativeToDisplay(currentHandle, currentMonitor, (int)(-borderThickness * scaling),
                                    fullUsableHeight * i / size,
                                    (int)(currentMonitor.right - currentMonitor.left + borderThickness * 2 * scaling),
                                    (int)(fullUsableHeight / size + borderThickness * scaling), true);
                            }
                            if (i == 0)
                            {
                                try
                                {
                                    SetForegroundWindow(currentHandle);
                                }
                                catch (System.InvalidOperationException) { }
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Monitor not found!!!");
            }
        }
        private int getMonitor(int x, int y)
        {
            // Get monitor that contains the coordinates
            RECT monitor;
            for (int i = 0; i < monitors.Length; i++)
            {
                monitor = monitors[i];
                if (y <= monitor.bottom && y >= monitor.top)
                    if (x <= monitor.right && x >= monitor.left)
                    {
                        return i;
                    }  
            }
            return -1;
        }
        private static bool isHorizontalMonitor(RECT monitor)
        {
            return ((monitor.right - monitor.left) >= (monitor.bottom - monitor.top));
        }
        private bool FilterProcess(Process process)
        {
            // Prevent some processes such as the task bar from being affected by the program
            int counter = 0;
            while (counter < blacklist.Length)
            {
                if (process.ProcessName.Equals(blacklist[counter++]))
                    return false;
                else if (process.ProcessName.Equals(currentProcess.ProcessName))
                    return false;               
            }
            return true;        
        }
        private float getScalingFactor(IntPtr hWnd)
        {
            // Gets the scaling factor of the screen in which the handle is positioned
            Graphics g = Graphics.FromHwnd(hWnd);
            float dpiX = g.DpiX;
            return dpiX / 96;
        }
        private int GetBorderThickness(IntPtr hWnd)
        {
            // Gets the thickness of the invisible border of some program window
            RECT rcClient = new RECT();
            RECT rcWind = new RECT();
            GetClientRect(hWnd, ref rcClient);
            GetWindowRect(hWnd, ref rcWind);
            return ((rcWind.right - rcWind.left) - rcClient.right) / 2;
        }
        private void MoveWindowsRelativeToDisplay(IntPtr hWnd, RECT monitor, int x, int y, int width, int height, bool bRepaint) 
        {
            // Places a windows to the position (x, y) relative to the top left corner of the choosen monitor
            MoveWindow(hWnd, monitor.left + x, monitor.top + y, width, height, bRepaint);
        }
    }
}
