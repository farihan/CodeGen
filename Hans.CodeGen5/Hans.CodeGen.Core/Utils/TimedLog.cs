using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hans.CodeGen.Core.Utils
{
    public class TimedLog : IDisposable
    {
        private string message;
        private long startTime;

        public TimedLog(string message)
        {
            var currentDateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            this.message = currentDateTime + "\t" + message;
            this.startTime = DateTime.Now.Ticks;
        }

        public void Dispose()
        {
            var timeTaken = TimeSpan.FromTicks(DateTime.Now.Ticks - this.startTime)
                .TotalSeconds;
            var msg = this.message + "\t" + timeTaken + " seconds";
            //EntLibHelper.PerformanceLog(msg);
            //System.Diagnostics.Debug.WriteLine(msg);
            Console.WriteLine();
            Console.WriteLine(msg);
        }
    }
}
