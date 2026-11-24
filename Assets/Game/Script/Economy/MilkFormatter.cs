using System;
using System.Globalization;

namespace MilkMaker.Economy
{
    public static class MilkFormatter
    {
        private static readonly string[] Suffixes =
        {
            "",
            "K",
            "M",
            "B",
            "T",
            "Qa",
            "Qi"
        };

        public static string Format(double value)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                return "-";
            }

            int index = 0;

            while (Math.Abs(value) >= 1000d &&
                   index < Suffixes.Length - 1)
            {
                value /= 1000d;
                index++;
            }

            value = Math.Round(
                value,
                2,
                MidpointRounding.AwayFromZero);

            if (Math.Abs(value) >= 1000d &&
                index < Suffixes.Length - 1)
            {
                value /= 1000d;
                index++;
            }

            string format = index == 0 ? "0.##" : "0.00";

            string result = value.ToString(
                format,
                CultureInfo.InvariantCulture);

            return index == 0
                ? result
                : result + " " + Suffixes[index];
        }
    }
}