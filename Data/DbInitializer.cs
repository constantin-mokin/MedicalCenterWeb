using MedicalCenterWeb.Models;
using MedicalCenterWeb.Services;

namespace MedicalCenterWeb.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            if (context.Users.Any()) return;

            var admin = new User
            {
                Username = "admin", Password = "admin",
                FirstName = "Константин", LastName = "Мокин",
                Phone = "+375173881212", Email = "admin@iskra.by",
                Role = UserRole.Admin, IsActive = true
            };
            context.Users.Add(admin);
            await context.SaveChangesAsync();

            var uSavitskaya = new User
            {
                Username = "doc1", Password = "doc1",
                FirstName = "Марина", LastName = "Савицкая",
                Phone = "+375297112004", Email = "m.savitskaya@iskra.by",
                Role = UserRole.Doctor, IsActive = true
            };
            var uKovalenko = new User
            {
                Username = "doc2", Password = "doc2",
                FirstName = "Дмитрий", LastName = "Коваленко",
                Phone = "+375337215004", Email = "d.kovalenko@iskra.by",
                Role = UserRole.Doctor, IsActive = true
            };
            var uZhukovskaya = new User
            {
                Username = "doc3", Password = "doc3",
                FirstName = "Ольга", LastName = "Жуковская",
                Phone = "+375257139004", Email = "o.zhukovskaya@iskra.by",
                Role = UserRole.Doctor, IsActive = true
            };
            var uDubovik = new User
            {
                Username = "doc4", Password = "doc4",
                FirstName = "Алексей", LastName = "Дубовик",
                Phone = "+375291583004", Email = "a.dubovik@iskra.by",
                Role = UserRole.Doctor, IsActive = true
            };
            var uBaranovskaya = new User
            {
                Username = "doc5", Password = "doc5",
                FirstName = "Екатерина", LastName = "Барановская",
                Phone = "+375445708004", Email = "e.baranovskaya@iskra.by",
                Role = UserRole.Doctor, IsActive = true
            };
            var uGrinevich = new User
            {
                Username = "doc6", Password = "doc6",
                FirstName = "Сергей", LastName = "Гриневич",
                Phone = "+375293056004", Email = "s.grinevich@iskra.by",
                Role = UserRole.Doctor, IsActive = true
            };
            context.Users.AddRange(uSavitskaya, uKovalenko, uZhukovskaya, uDubovik, uBaranovskaya, uGrinevich);
            await context.SaveChangesAsync();

            var dSavitskaya = new Doctor
            {
                UserId = uSavitskaya.Id,
                Specialization = "Кардиолог",
                YearsOfExperience = 12,
                Bio = "Врач-кардиолог высшей категории. Окончила БГМУ, клиническая ординатура по кардиологии на базе РНПЦ «Кардиология». Член Белорусского общественного объединения кардиологов."
            };
            var dKovalenko = new Doctor
            {
                UserId = uKovalenko.Id,
                Specialization = "Невролог",
                YearsOfExperience = 9,
                Bio = "Врач-невролог первой категории. Окончил БГМУ, специализация по лечению головной боли и вегетативных расстройств."
            };
            var dZhukovskaya = new Doctor
            {
                UserId = uZhukovskaya.Id,
                Specialization = "Терапевт",
                YearsOfExperience = 15,
                Bio = "Врач-терапевт высшей категории. Окончила Минский государственный медицинский институт. Наблюдение и лечение хронических заболеваний."
            };
            var dDubovik = new Doctor
            {
                UserId = uDubovik.Id,
                Specialization = "Офтальмолог",
                YearsOfExperience = 8,
                Bio = "Врач-офтальмолог. Окончил БГМУ. Диагностика и коррекция нарушений рефракции, подбор средств коррекции зрения."
            };
            var dBaranovskaya = new Doctor
            {
                UserId = uBaranovskaya.Id,
                Specialization = "Эндокринолог",
                YearsOfExperience = 11,
                Bio = "Врач-эндокринолог первой категории. Окончила БГМУ. Заболевания щитовидной железы, нарушения углеводного обмена."
            };
            var dGrinevich = new Doctor
            {
                UserId = uGrinevich.Id,
                Specialization = "Оториноларинголог",
                YearsOfExperience = 7,
                Bio = "Врач-оториноларинголог. Окончил ГрГМУ. Лечение заболеваний уха, горла и носа, включая маленьких пациентов."
            };
            context.Doctors.AddRange(dSavitskaya, dKovalenko, dZhukovskaya, dDubovik, dBaranovskaya, dGrinevich);
            await context.SaveChangesAsync();

            var uKovaleva = new User
            {
                Username = "staff", Password = "staff",
                FirstName = "Ольга", LastName = "Ковалёва",
                Phone = "+375294170564", Email = "o.kovaleva@iskra.by",
                Role = UserRole.Staff, IsActive = true
            };
            context.Users.Add(uKovaleva);
            await context.SaveChangesAsync();
            context.Staff.Add(new Staff
            {
                UserId = uKovaleva.Id,
                Position = "Старший регистратор",
                Description = "Запись на приём, ведение базы пациентов, справочная информация."
            });

            var pTherapistPrimary = new Procedure { Name = "Первичная консультация терапевта", Description = "Приём, осмотр, измерение артериального давления, оформление документации", Price = 32.00m, DurationMinutes = 30 };
            var pTherapistRepeat = new Procedure { Name = "Повторная консультация терапевта", Description = "Осмотр и оценка результатов лечения", Price = 26.00m, DurationMinutes = 20 };
            var pCardioConsult = new Procedure { Name = "Консультация кардиолога", Description = "Приём кардиолога, интерпретация ЭКГ, подбор терапии", Price = 45.00m, DurationMinutes = 30 };
            var pEkg = new Procedure { Name = "ЭКГ с расшифровкой", Description = "Электрокардиография в 12 отведениях с заключением врача", Price = 28.00m, DurationMinutes = 20 };
            var pEcho = new Procedure { Name = "ЭхоКГ (УЗИ сердца)", Description = "Ультразвуковое исследование сердца с допплерографией", Price = 62.00m, DurationMinutes = 40 };
            var pHolter = new Procedure { Name = "Суточное мониторирование ЭКГ (Холтер)", Description = "Установка регистратора, суточная запись и расшифровка", Price = 85.00m, DurationMinutes = 30 };
            var pNeuroConsult = new Procedure { Name = "Консультация невролога", Description = "Приём невролога, оценка рефлексов и координации", Price = 42.00m, DurationMinutes = 30 };
            var pEndoConsult = new Procedure { Name = "Консультация эндокринолога", Description = "Приём, осмотр, интерпретация гормонального профиля", Price = 45.00m, DurationMinutes = 30 };
            var pThyroidUsi = new Procedure { Name = "УЗИ щитовидной железы", Description = "Ультразвуковое исследование щитовидной железы", Price = 38.00m, DurationMinutes = 25 };
            var pEyeConsult = new Procedure { Name = "Консультация офтальмолога", Description = "Проверка остроты зрения, осмотр глазного дна", Price = 38.00m, DurationMinutes = 30 };
            var pIop = new Procedure { Name = "Измерение внутриглазного давления", Description = "Тонометрия по Маклакову", Price = 15.00m, DurationMinutes = 15 };
            var pLorConsult = new Procedure { Name = "Консультация оториноларинголога", Description = "Приём ЛОР-врача, осмотр ЛОР-органов", Price = 40.00m, DurationMinutes = 30 };
            var pEarWash = new Procedure { Name = "Промывание слуховых проходов", Description = "Удаление серных пробок, восстановление слуха", Price = 25.00m, DurationMinutes = 20 };
            context.Procedures.AddRange(
                pTherapistPrimary, pTherapistRepeat, pCardioConsult, pEkg, pEcho, pHolter,
                pNeuroConsult, pEndoConsult, pThyroidUsi, pEyeConsult, pIop, pLorConsult, pEarWash);

            var patMoroz = new Patient { FirstName = "Александр", LastName = "Мороз", Phone = "+375297642108", Email = "a.moroz@tut.by", DateOfBirth = new DateTime(1982, 4, 12), Gender = Gender.Male, MedicalHistory = "Гипертоническая болезнь с 2019 года. Аллергических реакций нет. Группа крови A(II), Rh+." };
            var patLisitskaya = new Patient { FirstName = "Вероника", LastName = "Лисицкая", Phone = "+375334521749", Email = "veronika.lisitskaya@gmail.com", DateOfBirth = new DateTime(1994, 9, 30), Gender = Gender.Female, MedicalHistory = "Хронический тонзиллит в детстве. Поллиноз (апрель–май), аллергия на пыльцу берёзы." };
            var patDanilovich = new Patient { FirstName = "Дарья", LastName = "Данилович", Phone = "+375257139046", Email = "d.danilovich@yandex.by", DateOfBirth = new DateTime(1988, 1, 25), Gender = Gender.Female, MedicalHistory = "Профилактические осмотры. Хронических заболеваний не выявлено. Группа крови O(I), Rh−." };
            var patProkhorov = new Patient { FirstName = "Максим", LastName = "Прохоров", Phone = "+375291583372", Email = "m.prokhorov@mail.ru", DateOfBirth = new DateTime(1976, 11, 17), Gender = Gender.Male, MedicalHistory = "Мигрень с 2005 года. Операция по поводу аппендицита (2011). Аллергии нет." };
            var patKovalchuk = new Patient { FirstName = "Анна", LastName = "Ковальчук", Phone = "+375445708419", Email = "anna.kovalchuk@tut.by", DateOfBirth = new DateTime(2001, 6, 8), Gender = Gender.Female, MedicalHistory = "Студентка БГУ. Нарушений не выявлено, повышенная зрительная нагрузка." };
            var patSnitko = new Patient { FirstName = "Владимир", LastName = "Снитко", Phone = "+375293056651", Email = "v.snitko@mail.ru", DateOfBirth = new DateTime(1965, 3, 3), Gender = Gender.Male, MedicalHistory = "Хронический гастрит с 2010 года. ИБС не подтверждена. Группа крови B(III), Rh+." };
            var patRusakovich = new Patient { FirstName = "Юлия", LastName = "Русакович", Phone = "+375336412877", Email = "yuliya.rusakovich@gmail.com", DateOfBirth = new DateTime(1997, 12, 14), Gender = Gender.Female, MedicalHistory = "Обильные менструации, склонность к анемии. Аллергий нет." };
            var patVoloshin = new Patient { FirstName = "Егор", LastName = "Волошин", Phone = "+375259841236", Email = "", DateOfBirth = new DateTime(2010, 7, 22), Gender = Gender.Male, MedicalHistory = "Школьник, наблюдается у невролога по поводу головных болей. Прививки по календарю." };
            var patGrin = new Patient { FirstName = "Наталья", LastName = "Гринь", Phone = "+375294170530", Email = "n.grin@tut.by", DateOfBirth = new DateTime(1971, 5, 19), Gender = Gender.Female, MedicalHistory = "Пролапс митрального клапана наблюдается с 2015 года. Группа крови A(II), Rh−." };
            var patPashkevich = new Patient { FirstName = "Олег", LastName = "Пашкевич", Phone = "+375448029315", Email = "o.pashkevich@yandex.by", DateOfBirth = new DateTime(1983, 10, 2), Gender = Gender.Male, MedicalHistory = "Хронический тонзиллит с детства. Перенёс COVID-19 в лёгкой форме (2021)." };
            var patBorovik = new Patient { FirstName = "Светлана", LastName = "Боровик", Phone = "+375332054781", Email = "s.borovik@gmail.com", DateOfBirth = new DateTime(1990, 8, 28), Gender = Gender.Female, MedicalHistory = "Аутоиммунный тиреоидит под наблюдением с 2022 года. Аллергических реакций нет." };
            var patLevdansky = new Patient { FirstName = "Артём", LastName = "Левданский", Phone = "+375296387029", Email = "artyom.levdansky@mail.ru", DateOfBirth = new DateTime(1999, 2, 11), Gender = Gender.Male, MedicalHistory = "Миопия с 2015 года. Занимается киберспортом, высокая зрительная нагрузка." };
            context.Patients.AddRange(
                patMoroz, patLisitskaya, patDanilovich, patProkhorov, patKovalchuk, patSnitko,
                patRusakovich, patVoloshin, patGrin, patPashkevich, patBorovik, patLevdansky);
            await context.SaveChangesAsync();

            AddSchedule(context, dZhukovskaya, DayOfWeek.Monday, 8, 16);
            AddSchedule(context, dZhukovskaya, DayOfWeek.Tuesday, 8, 16);
            AddSchedule(context, dZhukovskaya, DayOfWeek.Wednesday, 8, 16);
            AddSchedule(context, dZhukovskaya, DayOfWeek.Thursday, 8, 16);
            AddSchedule(context, dZhukovskaya, DayOfWeek.Friday, 8, 16);

            AddSchedule(context, dSavitskaya, DayOfWeek.Monday, 9, 15);
            AddSchedule(context, dSavitskaya, DayOfWeek.Wednesday, 9, 15);
            AddSchedule(context, dSavitskaya, DayOfWeek.Friday, 9, 13);

            AddSchedule(context, dKovalenko, DayOfWeek.Tuesday, 8, 14);
            AddSchedule(context, dKovalenko, DayOfWeek.Thursday, 8, 14);
            AddSchedule(context, dKovalenko, DayOfWeek.Saturday, 9, 13);

            AddSchedule(context, dDubovik, DayOfWeek.Tuesday, 10, 17);
            AddSchedule(context, dDubovik, DayOfWeek.Thursday, 10, 17);
            AddSchedule(context, dDubovik, DayOfWeek.Saturday, 10, 14);

            AddSchedule(context, dBaranovskaya, DayOfWeek.Monday, 9, 14);
            AddSchedule(context, dBaranovskaya, DayOfWeek.Wednesday, 12, 18);
            AddSchedule(context, dBaranovskaya, DayOfWeek.Friday, 9, 14);

            AddSchedule(context, dGrinevich, DayOfWeek.Tuesday, 9, 15);
            AddSchedule(context, dGrinevich, DayOfWeek.Thursday, 9, 15);
            AddSchedule(context, dGrinevich, DayOfWeek.Saturday, 10, 13);

            await context.SaveChangesAsync();

            await MinskSeedData.SeedVisitsAsync(context,
                dZhukovskaya, dSavitskaya, dKovalenko, dDubovik, dBaranovskaya, dGrinevich,
                pTherapistPrimary, pTherapistRepeat, pCardioConsult, pEkg, pEcho, pHolter,
                pNeuroConsult, pEndoConsult, pThyroidUsi, pEyeConsult, pIop, pLorConsult, pEarWash,
                patMoroz, patLisitskaya, patDanilovich, patProkhorov, patKovalchuk, patSnitko,
                patRusakovich, patVoloshin, patGrin, patPashkevich, patBorovik, patLevdansky);
        }

        private static void AddSchedule(ApplicationDbContext context, Doctor doctor, DayOfWeek day, int startHour, int endHour)
        {
            context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = doctor.Id,
                DayOfWeek = day,
                StartTime = new TimeSpan(startHour, 0, 0),
                EndTime = new TimeSpan(endHour, 0, 0)
            });
        }
    }
}
