using System.Resources;

namespace MedicalCenterWeb.Services
{
    public static class Strings
    {
        private static readonly ResourceManager Manager =
            new("MedicalCenterWeb.Resources.Strings", typeof(Strings).Assembly);

        public static string AppName => Manager.GetString(nameof(AppName)) ?? "Искра";
        public static string AppSubtitle => Manager.GetString(nameof(AppSubtitle)) ?? "Информационная система медицинского центра";
        public static string LoginTitle => Manager.GetString(nameof(LoginTitle)) ?? "Вход в систему";
        public static string LoginUsername => Manager.GetString(nameof(LoginUsername)) ?? "Логин";
        public static string LoginPassword => Manager.GetString(nameof(LoginPassword)) ?? "Пароль";
        public static string LoginButton => Manager.GetString(nameof(LoginButton)) ?? "Войти";
        public static string NavHome => Manager.GetString(nameof(NavHome)) ?? "Главная";
        public static string NavDoctors => Manager.GetString(nameof(NavDoctors)) ?? "Врачи";
        public static string NavProcedures => Manager.GetString(nameof(NavProcedures)) ?? "Процедуры";
        public static string NavPatients => Manager.GetString(nameof(NavPatients)) ?? "Пациенты";
        public static string NavAppointments => Manager.GetString(nameof(NavAppointments)) ?? "Приёмы";
        public static string NavSchedule => Manager.GetString(nameof(NavSchedule)) ?? "Расписание";
        public static string NavReports => Manager.GetString(nameof(NavReports)) ?? "Отчёты";
        public static string NavAuditLog => Manager.GetString(nameof(NavAuditLog)) ?? "Журнал";
        public static string NavUsers => Manager.GetString(nameof(NavUsers)) ?? "Пользователи";
        public static string NavMyAppointments => Manager.GetString(nameof(NavMyAppointments)) ?? "Мои приёмы";
        public static string Logout => Manager.GetString(nameof(Logout)) ?? "Выйти";
        public static string ChangePassword => Manager.GetString(nameof(ChangePassword)) ?? "Сменить пароль";
        public static string ThemeToggle => Manager.GetString(nameof(ThemeToggle)) ?? "Сменить тему";
        public static string FooterPhone => Manager.GetString(nameof(FooterPhone)) ?? "+375 17 388-12-12";
    }
}
