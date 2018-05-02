using System;
using System.IO;
using System.Text;
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

        private static Logger log = new Logger("Program");
        public static IServiceProvider serviceProvider;
        static void Main(string[] args)
        {
            Console.WriteLine("程序开始运行");

            Init();

            ThreadPool.QueueUserWorkItem(state =>{
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                DotnetSpider.Core.Startup.Run("-s:MultiSupplementSpider", "-tid:1", "-i:guid");
            });

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

            //加载配置文件
            ConfigHelper.Init();
            log.Info("配置文件加载完成".ToLine('='));

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
                ThreadPool.QueueUserWorkItem(state =>{host.Run();});
            }

            #endregion

            //作业调度器
            SchedulerHelper.Init();
            log.Info("作业调度器加载完成".ToLine('='));

        }

        #endregion

    }

}
