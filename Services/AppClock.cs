namespace MedicalCenterWeb.Services
{
    public static class AppClock
    {
        private static readonly TimeZoneInfo Zone = GetClinicZone();

        private static TimeZoneInfo GetClinicZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Minsk");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.Local;
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.Local;
            }
        }

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);

        public static DateTime Today => Now.Date;
    }
}
