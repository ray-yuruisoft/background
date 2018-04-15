using log4net;
using log4net.Config;
using log4net.Repository;
using System.IO;

namespace background.Tools
{
    public class LogHelper
    {
        //加载log4日志组件
        public static readonly ILoggerRepository repository = LogManager.CreateRepository("NETCoreRepository");
        public static void Init()
        {
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
        }
    }
}
