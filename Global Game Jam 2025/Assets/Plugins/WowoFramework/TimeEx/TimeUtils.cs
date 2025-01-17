namespace WowoFramework.TimeEx
{
    public static class TimeUtils
    {
        public static string ConvertSecondsToHHMMSS(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            if (hours == 0)
            {
                return $"{minutes:D2}:{seconds:D2}";
            }
            
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }
}
