using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using background.InversionOfControl;
using background.Jobs;
using background.Tools;
using DotnetSpider.Core;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace background
{
    class Program
    {

        private static Logger log = new Logger("Program");
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

            #region spider exit

            foreach (var item in DotnetSpider.Core.Startup.spiders)
            {
                var temp = item as Spider;
                if(temp.Stat == Status.Init 
                   || temp.Stat == Status.Paused
                   || temp.Stat == Status.Running
                  ){
                    temp.Exit();
                }
            }

            #endregion
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

            //加载配置文件
            ConfigHelper.Init();
            log.Info("配置文件加载完成".ToLine('=', 26));

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
                ThreadPool.QueueUserWorkItem(state => { host.Run(); });
            }

            #endregion

            //作业调度器
            // SchedulerHelper.Init();
            // log.Info("作业调度器加载完成".ToLine('=', 26));

            #region //Spider

            var spiders = ConfigHelper.GetAppSettingsArray("SpiderStartup");
            if(spiders.Length != 0)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    foreach (var item in spiders)
                    {
                        DotnetSpider.Core.Startup.Run(item.Split(";"));
                    }
                });
            }

            #endregion

        }

        #endregion

    }

}
