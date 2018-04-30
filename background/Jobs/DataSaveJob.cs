using System.Threading.Tasks;
using background.Tools;
using log4net;
using Quartz;

namespace background.Jobs
{
    public class DataSaveJob : Quartz.IJob
    {

        //  private static readonly ILog log = LogManager.GetLogger(LogHelper.repository.Name, typeof(DataSaveJob));
        private static readonly Logger log = new Logger("DataSaveJob");
        public static void ExecuteFn()
        {
            
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        public Task Execute(IJobExecutionContext context)
        {
            log.Info("DataSaveJob开始工作:");
            ExecuteFn();
            return Task.CompletedTask;
        }
    }
}
