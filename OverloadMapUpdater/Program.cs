using OverloadServerTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverloadMapUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(String.Format("OVERLOAD MAP UPDATED - Created by Maestro"));
            Console.WriteLine();
            Console.WriteLine("--- Checking for updated Overload maps on maps.overload.com - please wait ---");
            Console.WriteLine();

            OverloadMapManager mapManager = new OverloadMapManager();
            mapManager.SetLogger(Console.WriteLine);
            mapManager.Update();

            Console.WriteLine();
            Console.WriteLine("--- Map check complete ---");

            for (int i = 0; i < 10; i++)
            {
                if (Console.KeyAvailable)
                {
                    while (Console.KeyAvailable) Console.ReadKey(true);
                    return;
                }
                Thread.Sleep(500);
            }
        }
    }
}
