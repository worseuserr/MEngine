using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEngine
{
    public static class Util
    {
        static public double Lerp(double start, double end, double by)
        {
            return start * (1d - by) + end * by;
        }
        static public Vector2 Lerp(Vector2 start, Vector2 end, double by)
        {
            return start * (1d - by) + end * by;
        }
    }
}
