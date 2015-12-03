using RabotatAgent.Types;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RabotatAgent.Collector
{
    class DataCollector
    {
        private MySettings Settings;
        private ConcurrentQueue<ActiveWindow> Queue;

        public DataCollector(MySettings settings, ref ConcurrentQueue<ActiveWindow> q) {
            Settings = settings;
            Queue = q;
        }

        private ActiveWindow GetActiveWindowInfo()
        {
            var activeWindow = new ActiveWindow();
            IntPtr hWnd = MyUser32.GetForegroundWindow();
            uint procId = 0;
            MyUser32.GetWindowThreadProcessId(hWnd, out procId);

            try
            {
                var proc = Process.GetProcessById((int)procId);
                activeWindow.From = DateTime.Now.ToUniversalTime();
                activeWindow.To = DateTime.Now.ToUniversalTime();
                activeWindow.ProcessName = proc.ProcessName;
                activeWindow.WindowTitle = proc.MainWindowTitle;
                activeWindow.ModuleName = proc.MainModule.ModuleName;
                activeWindow.CompanyName = proc.MainModule.FileVersionInfo.CompanyName;
                activeWindow.Description = proc.MainModule.FileVersionInfo.FileDescription;
                activeWindow.FileName = proc.MainModule.FileVersionInfo.FileName;
                activeWindow.ProductName = proc.MainModule.FileVersionInfo.ProductName;
            }
            catch (Win32Exception e)
            {
                Debug.WriteLine("Win32 exception: " + e.Message);
            }

            activeWindow.Url = DataCollectorBrowser.GetBrowserUrl(activeWindow, hWnd);
            if (activeWindow.Url != null && activeWindow.Url != "")
            {
                Debug.WriteLine(activeWindow.Url);
            }

            return activeWindow;
        }

        private bool IsEqualWindowInfo(ActiveWindow a, ActiveWindow b)
        {
            if (a.ProcessName != b.ProcessName) return false;
            if (a.ModuleName != b.ModuleName) return false;
            if (a.WindowTitle != b.WindowTitle) return false;
            if (a.CompanyName != b.CompanyName) return false;
            if (a.Description != b.Description) return false;
            if (a.FileName != b.FileName) return false;
            if (a.ProductName != b.ProductName) return false;
            if (a.Url != b.Url) return false;
            return true;
        }

        public void CollectData()
        {
            ActiveWindow curWindow = null;
            ActiveWindow prevWindow = null;
            for (;;)
            {
                Debug.WriteLine(" --== STEP ==-- ");

                var idle = MyUser32.IdleTime > (new TimeSpan(0, 0, Settings.IdleDelay));
                if (idle) { Debug.WriteLine("Idle"); }
                curWindow = GetActiveWindowInfo();

                // если наступило бездействие, то закидываем предыдущее значение в очередь и очищаем его
                if (idle && prevWindow != null)
                {
                    Debug.WriteLine("Enqueue previous because of idle");
                    Queue.Enqueue(prevWindow);
                    prevWindow = null;
                }
                // если предыдущего значения нет, то просто сохраняем его и идём дальше
                else if (!idle && prevWindow == null)
                {
                    Debug.WriteLine("Save current to previous");
                    prevWindow = curWindow;
                }
                // если предыдущее значение есть, то сравниваем значения
                else if (!idle && prevWindow != null)
                {
                    // если совпадают, то просто обновляем время
                    if (IsEqualWindowInfo(prevWindow, curWindow))
                    {
                        Debug.WriteLine("Update previous");
                        prevWindow.To = curWindow.To;
                    }
                    // если не совпадают, то предыдущее в очередь, текущее в предыдущее
                    else
                    {
                        Debug.WriteLine("Enqueue previous and save current to previous");
                        Queue.Enqueue(prevWindow);
                        prevWindow = curWindow;
                    }
                }

                Thread.Sleep(Settings.StepDelay);
            }
        }

        public Task StartTask() {
            return Task.Factory.StartNew(CollectData);
        }
    }
}
