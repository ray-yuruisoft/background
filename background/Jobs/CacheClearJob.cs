using background.Tools;
using log4net;
using Quartz;
using System.Threading.Tasks;

namespace background.Jobs
{
    public class CacheClearJob : Quartz.IJob
    {

        private static readonly ILog log = LogManager.GetLogger(LogHelper.repository.Name, typeof(CacheClearJob));
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        public Task Execute(IJobExecutionContext context)
        {
            log.Info("CacheClearJob开始工作:");

            //清空缓存



            return Task.CompletedTask;
        }
    }
}
