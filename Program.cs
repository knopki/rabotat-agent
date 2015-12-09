using RabotatAgent.Collector;
using RabotatAgent.GUI;
using RabotatAgent.Sender;
using RabotatAgent.Types;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace RabotatAgent
{
    class Program
    {
        static private ConcurrentQueue<ActiveWindow> queue = null;
        static private Mutex mutex;

        [STAThread]
        static void Main(string[] args)
        {
            // наши настройки
            var settings = new MySettings();
            
            // перевариваем аргументы коммандной строки
            for (int i = 0; i <= args.Length - 2; i = i + 2)
            {
                switch (args[i])
                {
                    case "--secret":
                        settings.Secret = args[i + 1];
                        break;
                    case "--submit":
                        settings.SubmitUrl = args[i + 1];
                        break;
                    case "--step":
                        settings.StepDelay = Int32.Parse(args[i + 1]) * 1000;
                        break;
                    case "--idle":
                        settings.IdleDelay = Int32.Parse(args[i + 1]);
                        break;
                }
            }
            if (settings.Secret == null || settings.SubmitUrl == null)
            {
                MessageBox.Show("RabotatAgent: Необходимы аргументы --submit и --secret");
                Environment.Exit(1);
            }

            // проверка на single instance
            bool isFirstInstance;
            String mutexName = string.Format(
                CultureInfo.InvariantCulture,
                "RabotatAgent~{0}~{1}~{2}~1ce0cd05-a6eb-4c9a-8562-64bb8ffe3838",
                Environment.UserDomainName,
                Environment.UserName,
                (settings.Secret + settings.SubmitUrl).GetHashCode());
            mutex = new Mutex(true, mutexName, out isFirstInstance);
            if (!isFirstInstance) {
                Environment.Exit(2);
            }

            // инициализировали очередь
            queue = new ConcurrentQueue<ActiveWindow>();

            // запустили сбор данных в фоне
            DataCollector dc = new DataCollector(settings, ref queue);
            var collectTask = dc.StartTask();

            // запуск отправки данных в фоне
            DataSender ds = new DataSender(settings, ref queue);
            var sendTask = ds.StartTask();

            // tray icon
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TaskTrayApplicationContext());
        }
    }
}
