namespace MedicalCenterWeb.Models
{
    public static class AppointmentDisplay
    {
        public static string StatusText(this AppointmentStatus status) => status switch
        {
            AppointmentStatus.Scheduled => "Запланирован",
            AppointmentStatus.Completed => "Выполнен",
            _ => "Отменён"
        };

        public static string StatusCss(this AppointmentStatus status) => status switch
        {
            AppointmentStatus.Scheduled => "status-scheduled",
            AppointmentStatus.Completed => "status-completed",
            _ => "status-cancelled"
        };
    }
}
