using background.Jobs;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace background.Tools
{
    /// <summary>
    /// 任务调度中心
    /// </summary>
    public class SchedulerHelper
    {
        public static StdSchedulerFactory factory;
        public static IScheduler scheduler;
        public static async Task Init()
        {
            try
            {
                // 从工厂中获取调度程序实例
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                factory = new StdSchedulerFactory(props);
                scheduler = await factory.GetScheduler();

                // 开启调度器
                await scheduler.Start();

                // 定义这个工作，并将其绑定到我们的IJob实现类
                IJobDetail job = JobBuilder.Create<DataSaveJob>()
                    .WithIdentity("job1", "group1")
                    .Build();

                // 触发作业立即运行，然后每10秒重复一次，无限循环
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        // .WithIntervalInMinutes(2)
                        .WithIntervalInSeconds(30) //测试使用
                       .RepeatForever())
                    .Build();

                // 告诉Quartz使用我们的触发器来安排作业
                await scheduler.ScheduleJob(job, trigger);

                // 定义这个工作，并将其绑定到我们的IJob实现类
                IJobDetail job2 = JobBuilder.Create<CacheClearJob>()
                    .WithIdentity("job2", "group2")
                    .Build();

                ITrigger trigger2 = TriggerBuilder.Create()
                   .WithIdentity("job2", "group2")
                   .WithCronSchedule("0 15 2 * * ?")//使用Cron表达式
                   .ForJob("job2", "group2")
                   .Build();

                // 告诉Quartz使用我们的触发器来安排作业
                await scheduler.ScheduleJob(job2, trigger2);

                // 等待60秒
                // await Task.Delay(TimeSpan.FromSeconds(60));

                // 关闭调度程序
                // await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }
        }
    }
}
//在特定的时间内建立触发器，无需重复，代码如下：

//// 触发器构建器默认创建一个简单的触发器，实际上返回一个ITrigger
//ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
//    .WithIdentity("trigger1", "group1")
//    .StartAt(DateTime.Now) //指定开始时间为当前系统时间
//    .ForJob("job1", "group1") //通过JobKey识别作业
//    .Build();
//在特定的时间建立触发器，然后每十秒钟重复十次：

//// 触发器构建器默认创建一个简单的触发器，实际上返回一个ITrigger
//ITrigger trigger = trigger = TriggerBuilder.Create()
//   .WithIdentity("trigger2", "group2")
//   .StartAt(DateTime.Now) // 指定开始时间
//   .WithSimpleSchedule(x => x
//   .WithIntervalInSeconds(10)
//   .WithRepeatCount(10)) // 请注意，重复10次将总共重复11次
//   .ForJob("job2", "group2") //通过JobKey识别作业                   
//   .Build();
//构建一个触发器，将在未来五分钟内触发一次：

//// 触发器构建器默认创建一个简单的触发器，实际上返回一个ITrigger
//ITrigger trigger = trigger = (ISimpleTrigger)TriggerBuilder.Create()
//   .WithIdentity("trigger3", "group3")
//   .StartAt(DateBuilder.FutureDate(5, IntervalUnit.Minute)) //使用DateBuilder将来创建一个时间日期
//   .ForJob("job3", "group3") //通过JobKey识别作业
//   .Build();
//建立一个现在立即触发的触发器，然后每隔五分钟重复一次，直到22:00：

//// 触发器构建器默认创建一个简单的触发器，实际上返回一个ITrigger
//ITrigger trigger = trigger = TriggerBuilder.Create()
//   .WithIdentity("trigger4", "group4")
//   .WithSimpleSchedule(x => x
//        .WithIntervalInMinutes(5)//每5秒执行一次
//        .RepeatForever())
//   .EndAt(DateBuilder.DateOf(22, 0, 0))//晚上22点结束
//   .Build();
//建立一个触发器，在一个小时后触发，然后每2小时重复一次：

//// 触发器构建器默认创建一个简单的触发器，实际上返回一个ITrigger
//ITrigger trigger = TriggerBuilder.Create()
//   .WithIdentity("trigger5") // 由于未指定组，因此“trigger5”将位于默认分组中
//   .StartAt(DateBuilder.EvenHourDate(null)) // 获取下个一小时时间                 
//   .WithSimpleSchedule(x => x
//        .WithIntervalInHours(2)//执行间隔2小时
//        .RepeatForever())
//   .Build();

//每天早上8点到下午5点建立一个触发器，每隔一分钟就会触发一次：

//// 触发器构建器默认创建一个简单的触发器，实际上返回一个ITrigger
//ITrigger trigger = TriggerBuilder.Create()
//   .WithIdentity("Job1", "group1")
//   .WithCronSchedule("0 0/2 8-17 * * ?")//使用Cron表达式
//   .ForJob("Job1", "group1")
//   .Build();
//建立一个触发器，每天在上午10:42开始执行：

//// 触发器构建器默认创建一个简单的触发器，实际上返回一个ITrigger
//ITrigger trigger = TriggerBuilder.Create()
//   .WithIdentity("Job2", "group2")
//   .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(10, 42)) // 在这里使用CronScheduleBuilder的静态辅助方法
//   .ForJob("Job2", "group2")
//   .Build();
//构建一个触发器，将在星期三上午10:42在除系统默认值之外的TimeZone中触发：

//// 触发器构建器默认创建一个简单的触发器，实际上返回一个ITrigger
//ITrigger trigger = TriggerBuilder.Create()
//   .WithIdentity("Job3", "group3")
//   .WithCronSchedule("0 42 10 ? * WED", x => x
//   .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time")))
//   .ForJob("Job3", "group3")
//   .Build();