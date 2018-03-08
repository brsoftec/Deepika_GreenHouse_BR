using Quartz;
using Quartz.Impl;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            IJobDetail job = JobBuilder.Create<CheckJob>().Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(12, 30))
                  )
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}