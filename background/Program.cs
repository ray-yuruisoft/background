using System;
using System.IO;
using System.Threading;
using background.Data;
using background.InversionOfControl;
using background.Jobs;
using background.Tools;
using EasyCaching.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace background
{
    class Program
    {

        private static log4net.ILog log = log4net.LogManager.GetLogger(LogHelper.repository.Name, typeof(Program));
        public static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            Console.WriteLine("程序开始运行");
            Init();
            string key = "";
            var run = true;
            while (run)
            {
                key = Console.ReadLine();
                switch (key)
                {
                    case "exit":
                        run = false;
                        break;
                }
            }
        }

        private static void CloseProgram()
        {
            Console.WriteLine("-- CTRL_CLOSE_EVENT --");
            DataSaveJob.ExecuteFn();
        }

        #region Init

        private static void Init()
        {

            #region //CloseProgram Event

            //Ctrl+C or Ctrl+Break
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                CloseProgram();
            };
            //进程退出事件
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) =>
            {
                CloseProgram();
            };

            #endregion

            //容器初始化
            ProgramContainer.Init();

            //加载log4日志组件
            LogHelper.Init();
            log.Info("日志加载完成".ToSameLength(40, '-'));

            //加载配置文件
            ConfigHelper.Init();
            log.Info("配置文件加载完成".ToSameLength(40, '-'));

            #region //WebHost

            var url = ConfigHelper.GetAppSettings("WebHost");
            if (!string.IsNullOrWhiteSpace(url))
            {
                var host = new WebHostBuilder()
                       .UseKestrel()
                       .UseUrls(url)
                       .UseContentRoot(Directory.GetCurrentDirectory())
                       .UseIISIntegration()
                       .UseStartup<Startup>()
                       .Build();
                ThreadPool.QueueUserWorkItem(
                            state =>
                            {
                                host.Run();
                            });
            }

            #endregion

            //作业调度器
            SchedulerHelper.Init();
            log.Info("作业调度器开始加载".ToSameLength(40, '-'));

        }

        #endregion

    }

}
