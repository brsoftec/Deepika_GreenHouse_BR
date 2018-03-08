using GH.Util;
using System.Threading;
using System.Threading.Tasks;

namespace GH.Core.TaskServices
{
    public abstract class ASubTask
    {
        public int MinutesToSleep { set; get; }
        public ASubTask(string nameMinutesToSleep)
        {
            this.MinutesToSleep = ConfigHelp.GetIntValue(nameMinutesToSleep);
        }

        public void Start()
        {
            if (this.MinutesToSleep <= 0)
                return;
            Task taskA = new Task(() =>
            {
                try
                {
                    while (true)
                    {
                        //Thread.Sleep(this.MinutesToSleep * 60 * 1000);
                        Thread.Sleep(this.MinutesToSleep * 15 * 1000);
                        try
                        {
                            ExecuteMethod();
                        }
                        catch {
                        }
                    }
                }
                catch
                {
                }
            }
           );
            taskA.Start();
        }
        public abstract void ExecuteMethod();
    }
}