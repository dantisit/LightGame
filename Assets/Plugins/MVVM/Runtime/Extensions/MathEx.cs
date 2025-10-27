using System;

namespace MVVM
{
    public static class MathEx
    {
        public static int ClampCyclic(int value, int min, int max)
        {
            if (min > max)
                throw new ArgumentException("min must be <= max");
            int num1 = max - min + 1;
            int num2 = (value - min) % num1;
            if (num2 < 0)
                num2 += num1;
            return min + num2;
        }
    }
}