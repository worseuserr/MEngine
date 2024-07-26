using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MEngine
{
    public static class _debug
    {
        public static bool Variable = false;
        public static string Output = "";
        public static void RuntimeTick(bool bFirstInOrder)
        {

            if (bFirstInOrder) // first in order
            {

            }

            // last in order

        }
        public static void WindowTick()
        {

        }
        public static void RuntimeInit()
        {

        }
        public static void WindowInit()
        {

        }
        public static void RenderText()
        {
            Console.WriteLine(Output);
        }
    }
}
