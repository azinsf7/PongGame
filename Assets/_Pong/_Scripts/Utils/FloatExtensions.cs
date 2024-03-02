namespace NoMossStudios.Utilities
{
    public static class FloatExtensions
    {
        public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        //format a float (in seconds) to a readable string
        public static string FormatTime(this float timeInSeconds, bool lessDecimals = false)
        {
            var minutes = (int)timeInSeconds / 60;
            var seconds = (int)timeInSeconds - 60 * minutes;
            var milliseconds = (int)(1000 * (timeInSeconds - minutes * 60 - seconds));

            if (lessDecimals)
                milliseconds /= 100;

            return $"{minutes:0}:{seconds:00}.{milliseconds:0}";
        }
    }
}
