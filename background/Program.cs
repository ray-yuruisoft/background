using System;
using System.Runtime.Loader;
using System.Threading.Tasks;
using background.Tools;
using log4net;

namespace background
{
    class Program
    {
        private static ILog log = LogManager.GetLogger(LogHelper.repository.Name, typeof(Program));
        private static bool alreadydone = false;
        static void Main(string[] args)
        {
            try
            {
                MainAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                Console.ReadKey();
            }
        }
        public static async Task MainAsync(string[] args)
        {
            Console.WriteLine("程序开始运行");
            //捕获Ctrl+C事件
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                CloseProgram(alreadydone);
            };
            //进程退出事件
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) =>
            {
                CloseProgram(alreadydone);
            };
            ////卸载事件
            //AssemblyLoadContext.Default.Unloading += state => { 
            //    CloseProgram(alreadydone);
            //};

            //加载log4日志组件
            LogHelper.Init();
            log.Info("日志加载完成".ToSameLength(40, '-'));
            //加载配置文件
            ConfigHelper.Init();
            log.Info("配置文件加载完成".ToSameLength(40, '-'));
            //作业调度器
            await SchedulerHelper.Init();
            log.Info("作业调度器加载完成".ToSameLength(40, '-'));


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
        private static void CloseProgram(bool alreadydone)
        {
            if (!alreadydone)
            {
                Console.WriteLine("-- CTRL_CLOSE_EVENT --");
                alreadydone = true;
                Console.ReadKey();
            }
        }
    }
}
