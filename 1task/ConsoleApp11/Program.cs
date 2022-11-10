using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace ConsoleApp11
{
    class Program
    {
        private static List<long> Func()
        {
            Stopwatch time;
            time = Stopwatch.StartNew();

            var list = new List<long>();
            for (var i = 0; i < 1000000; i++)
            {
                list.Add(time.ElapsedMilliseconds);
            }

            time.Stop();

            var result = new List<long>();
            for (var i = 1; i < 1000000; i++)
            {
                var dif = list[i] - list[i - 1];
                if (dif > 1)
                    result.Add(dif);
            }

            return result;
        }

        static void Main(string[] args)
        {
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)(1 << 1);
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            var res1 = new List<long>();
            var res2 = new List<long>();

            var thread1 = new Thread(() => {res1 = Func(); });
            var thread2 = new Thread(() => {res2 = Func(); });

            thread1.Start();
            thread2.Start();

            Thread.Sleep(5000);

            var answer = (res1.Sum() + res2.Sum()) / (res1.Count + res2.Count);

            Console.WriteLine(answer);

            // в среднем около 32
        }
    }
}
