using MedicalCenterWeb.Models;
using MedicalCenterWeb.Services;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterWeb.Data
{
    internal static class MinskSeedData
    {
        private sealed record RecTemplate(string Diagnosis, string Chronic, string Recommendations, string Notes);

        private static Appointment Mk(Patient p, Doctor d, Procedure pr, DateTime dt, AppointmentStatus st) => new()
        {
            PatientId = p.Id,
            DoctorId = d.Id,
            ProcedureId = pr.Id,
            AppointmentDateTime = dt,
            Status = st,
            RecordedPrice = pr.Price
        };

        public static async Task SeedVisitsAsync(
            ApplicationDbContext context,
            Doctor dZhukovskaya, Doctor dSavitskaya, Doctor dKovalenko, Doctor dDubovik, Doctor dBaranovskaya, Doctor dGrinevich,
            Procedure pTherapistPrimary, Procedure pTherapistRepeat, Procedure pCardioConsult, Procedure pEkg, Procedure pEcho, Procedure pHolter,
            Procedure pNeuroConsult, Procedure pEndoConsult, Procedure pThyroidUsi, Procedure pEyeConsult, Procedure pIop, Procedure pLorConsult, Procedure pEarWash,
            Patient patMoroz, Patient patLisitskaya, Patient patDanilovich, Patient patProkhorov, Patient patKovalchuk, Patient patSnitko,
            Patient patRusakovich, Patient patVoloshin, Patient patGrin, Patient patPashkevich, Patient patBorovik, Patient patLevdansky)
        {
            var today = AppClock.Today;
            var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7; // Пн=0 … Вс=6
            var lastWeekMonday = today.AddDays(-daysSinceMonday - 7);
            var lastMon = lastWeekMonday;
            var lastTue = lastWeekMonday.AddDays(1);
            var lastWed = lastWeekMonday.AddDays(2);
            var lastThu = lastWeekMonday.AddDays(3);
            var lastFri = lastWeekMonday.AddDays(4);
            var lastSat = lastWeekMonday.AddDays(5);

            var a1 = Mk(patMoroz, dZhukovskaya, pTherapistPrimary, lastMon.AddHours(9), AppointmentStatus.Completed);
            var a2 = Mk(patLisitskaya, dZhukovskaya, pTherapistRepeat, lastMon.AddHours(10), AppointmentStatus.Completed);
            var a3 = Mk(patDanilovich, dSavitskaya, pCardioConsult, lastMon.AddHours(10), AppointmentStatus.Completed);
            var a4 = Mk(patProkhorov, dKovalenko, pNeuroConsult, lastTue.AddHours(9), AppointmentStatus.Completed);
            var a5 = Mk(patKovalchuk, dDubovik, pEyeConsult, lastTue.AddHours(11), AppointmentStatus.Completed);
            var a6 = Mk(patSnitko, dZhukovskaya, pTherapistPrimary, lastWed.AddHours(9), AppointmentStatus.Completed);
            var a7 = Mk(patGrin, dSavitskaya, pEcho, lastWed.AddHours(11), AppointmentStatus.Completed);
            var a8 = Mk(patBorovik, dBaranovskaya, pEndoConsult, lastWed.AddHours(13), AppointmentStatus.Completed);
            var a9 = Mk(patKovalchuk, dZhukovskaya, pTherapistPrimary, lastWed.AddHours(15), AppointmentStatus.Cancelled);
            var a10 = Mk(patVoloshin, dKovalenko, pNeuroConsult, lastThu.AddHours(10), AppointmentStatus.Completed);
            var a11 = Mk(patPashkevich, dGrinevich, pLorConsult, lastThu.AddHours(11), AppointmentStatus.Completed);
            var a12 = Mk(patPashkevich, dGrinevich, pEarWash, lastThu.AddHours(11).AddMinutes(40), AppointmentStatus.Completed);
            var a13 = Mk(patMoroz, dSavitskaya, pHolter, lastFri.AddHours(10), AppointmentStatus.Completed);
            var a14 = Mk(patRusakovich, dZhukovskaya, pTherapistPrimary, lastFri.AddHours(14), AppointmentStatus.Completed);
            var a15 = Mk(patLevdansky, dDubovik, pEyeConsult, lastSat.AddHours(11), AppointmentStatus.Completed);
            var a16 = Mk(patVoloshin, dGrinevich, pLorConsult, lastSat.AddHours(10).AddMinutes(30), AppointmentStatus.Completed);

            context.Appointments.AddRange(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16);
            await context.SaveChangesAsync();

            MedicalRecord Rec(Appointment a, string diagnosis, string chronic, string recommendations, string notes) => new()
            {
                AppointmentId = a.Id,
                Diagnosis = diagnosis,
                ChronicDiseases = chronic,
                Recommendations = recommendations,
                Notes = notes,
                CreatedAt = a.AppointmentDateTime
            };

            context.MedicalRecords.AddRange(
                Rec(a1, "Артериальная гипертензия I степени, риск 2. Ожирение I степени.", "Гипертоническая болезнь у матери.",
                    "Лозартан 50 мг утром. Поваренная соль до 5 г/сут. Дневник АД с двукратным измерением. Повторный визит через 2 недели.",
                    "АД 148/92 мм рт. ст., ЧСС 76. ИМТ 31,2."),
                Rec(a2, "ОРВИ, период выздоровления.", "Поллиноз, апрель–май.",
                    "Витамин D 2000 МЕ/сут, 4 недели. Избегать переохлаждения.",
                    "Жалоб нет, лёгкие аускультативно чистые."),
                Rec(a3, "Нейроциркуляторная дистония по гипертоническому типу.", "—",
                    "Режим сна 8 часов. Кофеин — не более 1 чашки в день. Магний B6 по 1 таб. 2 р/сут, 3 недели.",
                    "АД 135/85 мм рт. ст., ЧСС 78 в минуту."),
                Rec(a4, "Мигрень без ауры, лёгкое течение.", "Мигрень у отца.",
                    "Ведение дневника головной боли. Режим сна. Суматриптан 50 мг при приступе, не более 2 таб./сут.",
                    "Приступы 1–2 раза в месяц. Очаговой симптоматики нет."),
                Rec(a5, "Спазм аккомодации. Астенопический синдром.", "—",
                    "Зрительная гимнастика каждые 40 минут работы. Увлажняющие капли по 1 кап. 3 р/сут, 2 недели. Ограничение экранного времени.",
                    "Острота зрения OD 0,8, OS 0,7; с коррекцией 1,0."),
                Rec(a6, "Хронический гастрит, стадия ремиссии.", "Хронический гастрит с 2010 года.",
                    "Диета (стол № 1). Омепразол 20 мг утром, 2 недели. Контроль самочувствия через месяц.",
                    "Живот мягкий, безболезненный при пальпации."),
                Rec(a7, "Пролапс митрального клапана 1 степени с регургитацией 1 степени.", "Пролапс МК наблюдается с 2015 года.",
                    "Наблюдение кардиолога 1 раз в год, ЭхоКГ ежегодно.",
                    "ФВ ЛЖ 62 %. Камеры сердца не расширены."),
                Rec(a8, "Субклинический гипотиреоз. Аутоиммунный тиреоидит (вероятно).", "—",
                    "Контроль ТТГ, св. Т4, АТ-ТПО через 2 месяца. Йодид калия 100 мкг/сут.",
                    "ТТГ 6,8 мЕд/л, св. Т4 в норме. Щитовидная железа не увеличена."),
                Rec(a10, "Головная боль напряжения, эпизодическая.", "—",
                    "Режим дня, прогулки не менее 1 ч/сут. Ограничение гаджетов до 1 ч/день. Массаж шейно-воротниковой зоны, 10 сеансов.",
                    "Неврологический статус без очаговой симптоматики."),
                Rec(a11, "Хронический тонзиллит вне обострения.", "Хронический тонзиллит с детства.",
                    "Промывание миндалин, курс 5 процедур. Тонзилотрен по 1 таб. 3 р/сут, 2 недели.",
                    "Миндалины рыхлые, гнойных пробок нет."),
                Rec(a12, "Серная пробка с двух сторон.", "—",
                    "Промывание слуховых проходов выполнено. Контроль при необходимости.",
                    "Слуховые проходы свободны, слух восстановлен."),
                Rec(a13, "Суточная запись ЭКГ: синусовый ритм. Единичные наджелудочковые экстрасистолы.", "Артериальная гипертензия I степени.",
                    "Контрольная ЭКГ через 3 месяца. Продолжение антигипертензивной терапии.",
                    "За сутки 120 наджелудочковых экстрасистол, желудочковых нет. Циркадный индекс 1,24."),
                Rec(a14, "Железодефицитная анемия лёгкой степени.", "Обильные менструации.",
                    "Железа сульфат 100 мг 2 р/сут во время еды, 1 месяц. Общий анализ крови через месяц.",
                    "Hb 112 г/л, ферритин 9 нг/мл."),
                Rec(a15, "Миопия слабой степени обоих глаз.", "Миопия с 2015 года.",
                    "Очки для дали. Ограничение зрительной нагрузки. Осмотр офтальмолога 1 раз в год.",
                    "Vis OD 0,6, OS 0,5. Рефракция −1,25 / −1,0 дптр."),
                Rec(a16, "Вазомоторный ринит.", "Аллергический ринит у матери.",
                    "Избегать резких перепадов температуры. Изотонический назальный спрей 3 р/сут.",
                    "Слизистая носа отёчная, бледная. Носовое дыхание умеренно затруднено."));
            await context.SaveChangesAsync();

            var plans = new (Doctor Doc, Procedure[] Pool, Dictionary<DayOfWeek, (int From, int To)> Hours)[]
            {
                (dZhukovskaya, new[] { pTherapistPrimary, pTherapistRepeat, pEkg }, new Dictionary<DayOfWeek, (int, int)>
                {
                    [DayOfWeek.Monday] = (8, 16), [DayOfWeek.Tuesday] = (8, 16), [DayOfWeek.Wednesday] = (8, 16),
                    [DayOfWeek.Thursday] = (8, 16), [DayOfWeek.Friday] = (8, 16)
                }),
                (dSavitskaya, new[] { pCardioConsult, pEkg, pEcho, pHolter }, new Dictionary<DayOfWeek, (int, int)>
                {
                    [DayOfWeek.Monday] = (9, 15), [DayOfWeek.Wednesday] = (9, 15), [DayOfWeek.Friday] = (9, 13)
                }),
                (dKovalenko, new[] { pNeuroConsult }, new Dictionary<DayOfWeek, (int, int)>
                {
                    [DayOfWeek.Tuesday] = (8, 14), [DayOfWeek.Thursday] = (8, 14), [DayOfWeek.Saturday] = (9, 13)
                }),
                (dDubovik, new[] { pEyeConsult, pIop }, new Dictionary<DayOfWeek, (int, int)>
                {
                    [DayOfWeek.Tuesday] = (10, 17), [DayOfWeek.Thursday] = (10, 17), [DayOfWeek.Saturday] = (10, 14)
                }),
                (dBaranovskaya, new[] { pEndoConsult, pThyroidUsi }, new Dictionary<DayOfWeek, (int, int)>
                {
                    [DayOfWeek.Monday] = (9, 14), [DayOfWeek.Wednesday] = (12, 18), [DayOfWeek.Friday] = (9, 14)
                }),
                (dGrinevich, new[] { pLorConsult, pEarWash }, new Dictionary<DayOfWeek, (int, int)>
                {
                    [DayOfWeek.Tuesday] = (9, 15), [DayOfWeek.Thursday] = (9, 15), [DayOfWeek.Saturday] = (10, 13)
                })
            };

            var patients = new[] { patMoroz, patLisitskaya, patDanilovich, patProkhorov, patKovalchuk, patSnitko,
                patRusakovich, patVoloshin, patGrin, patPashkevich, patBorovik, patLevdansky };

            var templates = new Dictionary<Procedure, RecTemplate[]>
            {
                [pTherapistPrimary] = new[]
                {
                    new RecTemplate("Артериальная гипертензия I степени, риск 2.", "Гипертоническая болезнь у матери.",
                        "Лозартан 50 мг утром. Соль до 5 г/сут, дневник АД. Контроль через 2 недели.", "АД 145/90 мм рт. ст., ЧСС 74."),
                    new RecTemplate("ОРВИ, среднетяжёлое течение.", "—",
                        "Обильное питьё, парацетамол при температуре выше 38,0. Постельный режим 2–3 дня.", "Температура 37,6, гиперемия зева."),
                    new RecTemplate("Профилактический осмотр. Патология не выявлена.", "—",
                        "Режим труда и отдыха, ежегодный профилактический осмотр.", "Жалоб нет. АД 120/78 мм рт. ст.")
                },
                [pTherapistRepeat] = new[]
                {
                    new RecTemplate("Артериальная гипертензия, на фоне терапии достигнуто целевое АД.", "Гипертоническая болезнь с 2019 года.",
                        "Терапию продолжать. Контроль АД дважды в неделю.", "АД 128/80 мм рт. ст., ЧСС 70."),
                    new RecTemplate("Хронический гастрит, стадия ремиссии. Жалоб нет.", "Хронический гастрит с 2010 года.",
                        "Диета, стол № 1. Контроль самочувствия через месяц.", "Живот мягкий, безболезненный.")
                },
                [pEkg] = new[]
                {
                    new RecTemplate("Синусовый ритм, ЧСС 66 в мин. ЭОС не отклонена.", "—",
                        "Повторная ЭКГ при появлении жалоб.", "Патологических изменений не выявлено."),
                    new RecTemplate("Синусовая тахикардия 92 в мин. Неспецифические изменения миокарда.", "—",
                        "Консультация кардиолога, контроль ЭКГ через месяц.", "Интервал PQ в норме.")
                },
                [pCardioConsult] = new[]
                {
                    new RecTemplate("Гипертоническая болезнь II стадии, риск 3.", "Гипертоническая болезнь, ИБС у отца.",
                        "Периндоприл 5 мг, амлодипин 5 мг. Липидограмма. Осмотр через месяц.", "АД 155/95 мм рт. ст., ЧСС 78."),
                    new RecTemplate("Ишемическая болезнь сердца, стенокардия напряжения I ФК.", "ИБС с 2021 года.",
                        "Нитроглицерин при приступе. Бисопролол 2,5 мг утром. Нагрузки дозировать.", "Приступы 2–3 раза в месяц."),
                    new RecTemplate("Нейроциркуляторная дистония по кардиальному типу.", "—",
                        "Режим сна 8 часов. Магний B6 по 1 таб. 2 р/сут. Кофеин ограничить.", "АД 130/85 мм рт. ст., тоны сердца ясные.")
                },
                [pEcho] = new[]
                {
                    new RecTemplate("Уплотнение створок аортального клапана. Аортальная регургитация 1 степени.", "—",
                        "Наблюдение кардиолога 1 раз в год.", "ФВ ЛЖ 63 %. Камеры сердца не расширены."),
                    new RecTemplate("Пролапс митрального клапана 1 степени с регургитацией 1 степени.", "Наблюдается с 2015 года.",
                        "ЭхоКГ ежегодно. Физических ограничений нет.", "ФВ ЛЖ 65 %. Пролапс до 5 мм.")
                },
                [pHolter] = new[]
                {
                    new RecTemplate("Суточная запись ЭКГ: пароксизмов не зарегистрировано.", "Пароксизмы в анамнезе.",
                        "Продолжение терапии. Контроль через 6 месяцев.", "Циркадный индекс 1,20."),
                    new RecTemplate("Редкие наджелудочковые экстрасистолы (до 60 в сутки).", "—",
                        "Контроль ЭКГ через 3 месяца.", "Пауз и желудочковых экстрасистол не зарегистрировано.")
                },
                [pNeuroConsult] = new[]
                {
                    new RecTemplate("Головная боль напряжения, эпизодическая.", "—",
                        "Режим сна, прогулки не менее 1 ч/сут. Массаж шейно-воротниковой зоны.", "Неврологический статус без очаговой симптоматики."),
                    new RecTemplate("Мигрень без ауры, лёгкое течение.", "Мигрень у отца.",
                        "Дневник головной боли. Суматриптан 50 мг при приступе.", "Приступы 1–2 раза в месяц."),
                    new RecTemplate("Остеохондроз шейного отдела, вертебробазилярная недостаточность.", "Шейный остеохондроз.",
                        "ЛФК для шейного отдела, курс массажа 10 сеансов.", "Лёгкая пошатываемость в пробе Ромберга.")
                },
                [pEndoConsult] = new[]
                {
                    new RecTemplate("Субклинический гипотиреоз.", "—",
                        "Контроль ТТГ через 2 месяца. Йодид калия 100 мкг/сут.", "ТТГ 6,2 мЕд/л, св. Т4 в норме."),
                    new RecTemplate("Аутоиммунный тиреоидит, эутиреоз.", "АИТ с 2022 года.",
                        "Контроль ТТГ 1 раз в 6 месяцев.", "Щитовидная железа не увеличена."),
                    new RecTemplate("Нарушение толерантности к глюкозе.", "Сахарный диабет 2 типа у матери.",
                        "Диета с ограничением углеводов, снижение массы тела на 5 %. HbA1c через 3 месяца.", "Глюкоза натощак 6,3 ммоль/л.")
                },
                [pThyroidUsi] = new[]
                {
                    new RecTemplate("УЗИ: диффузные изменения паренхимы щитовидной железы.", "—",
                        "Контроль УЗИ через 6 месяцев.", "Объём железы в норме."),
                    new RecTemplate("УЗИ: узловое образование правой доли 6 мм, TI-RADS 2.", "—",
                        "Пункция не показана. УЗИ через год.", "Кровоток в узле не изменён.")
                },
                [pEyeConsult] = new[]
                {
                    new RecTemplate("Спазм аккомодации. Астенопический синдром.", "—",
                        "Зрительная гимнастика, увлажняющие капли. Ограничить экранное время.", "Острота зрения 0,8/0,7, с коррекцией 1,0."),
                    new RecTemplate("Миопия слабой степени обоих глаз.", "Миопия с 2015 года.",
                        "Очки для дали. Осмотр офтальмолога 1 раз в год.", "Рефракция −1,25 / −1,0 дптр."),
                    new RecTemplate("Конъюнктивит неинфекционный (сухой глаз).", "—",
                        "Увлажняющие капли 3 р/сут, 2 недели.", "Проба Ширмера 6 мм.")
                },
                [pIop] = new[]
                {
                    new RecTemplate("Внутриглазное давление в норме.", "Глаукома у бабушки.",
                        "Контроль ВГД 1 раз в год.", "OD 18, OS 19 мм рт. ст.")
                },
                [pLorConsult] = new[]
                {
                    new RecTemplate("Хронический тонзиллит вне обострения.", "Хронический тонзиллит с детства.",
                        "Промывание миндалин курсом 5 процедур.", "Миндалины рыхлые, пробок нет."),
                    new RecTemplate("Острый катаральный ринит.", "—",
                        "Изотонический назальный спрей, обильное питьё.", "Слизистая носа отёчная."),
                    new RecTemplate("Вазомоторный ринит.", "Аллергический ринит у матери.",
                        "Избегать перепадов температур, спрей 3 р/сут.", "Носовое дыхание умеренно затруднено.")
                },
                [pEarWash] = new[]
                {
                    new RecTemplate("Серная пробка с одной стороны.", "—",
                        "Промывание выполнено. Контроль при необходимости.", "Слуховой проход свободен, слух восстановлен.")
                }
            };

            var history = new List<Appointment>();
            var historyRecords = new List<MedicalRecord>();
            GenerateRange(history, historyRecords,
                lastMon.AddDays(-98), lastMon.AddDays(-1),
                AppointmentStatus.Completed, plans, patients, templates);
            context.Appointments.AddRange(history);
            context.MedicalRecords.AddRange(historyRecords);
            await context.SaveChangesAsync();

            var horizon = new DateTime(2026, 11, 13);
            if (horizon < today.AddDays(1))
            {
                horizon = today.AddDays(21);
            }
            var future = new List<Appointment>();
            GenerateRange(future, null,
                today, horizon,
                AppointmentStatus.Scheduled, plans, patients, templates,
                notBefore: AppClock.Now);

            var horizonSlots = new (Doctor Doc, Procedure Proc, Patient Pat, int Hour)[]
            {
                (dZhukovskaya, pTherapistPrimary, patKovalchuk, 10),
                (dSavitskaya, pCardioConsult, patDanilovich, 11),
                (dBaranovskaya, pEndoConsult, patBorovik, 12)
            };
            foreach (var (doc, proc, pat, hour) in horizonSlots)
            {
                var dt = horizon.AddHours(hour);
                if (future.Any(a => a.DoctorId == doc.Id && a.AppointmentDateTime == dt)) continue;
                future.Add(Mk(pat, doc, proc, dt, AppointmentStatus.Scheduled));
            }

            context.Appointments.AddRange(future);
            await context.SaveChangesAsync();

            var now = AppClock.Now;
            var audit = new List<AuditLog>
            {
                new() { Username = "kovaleva", Action = "Добавление пациента", Details = "Олег Пашкевич", Timestamp = today.AddDays(-72).AddHours(9).AddMinutes(15) },
                new() { Username = "kovaleva", Action = "Добавление пациента", Details = "Юлия Русакович", Timestamp = today.AddDays(-68).AddHours(10).AddMinutes(5) },
                new() { Username = "kovaleva", Action = "Добавление пациента", Details = "Артём Левданский", Timestamp = today.AddDays(-55).AddHours(11).AddMinutes(40) },
                new() { Username = "kovaleva", Action = "Добавление пациента", Details = "Егор Волошин", Timestamp = today.AddDays(-40).AddHours(9).AddMinutes(50) },

                new() { Username = "kovaleva", Action = "Запись на приём", Details = "Пациент: Мороз Александр, Врач: Жуковская Ольга", Timestamp = lastMon.AddHours(8).AddMinutes(55) },
                new() { Username = "kovaleva", Action = "Вход в систему", Details = "Пользователь: kovaleva", Timestamp = lastMon.AddHours(8).AddMinutes(30) },
                new() { Username = "savitskaya", Action = "Вход в систему", Details = "Пользователь: savitskaya", Timestamp = lastMon.AddHours(9).AddMinutes(58) },
                new() { Username = "savitskaya", Action = "Отметка выполнения приёма", Details = "Приём #3, пациент: Данилович Дарья", Timestamp = lastMon.AddHours(10).AddMinutes(35) },
                new() { Username = "admin", Action = "Вход в систему", Details = "Пользователь: admin", Timestamp = lastTue.AddHours(8).AddMinutes(30) },
                new() { Username = "kovaleva", Action = "Отмена приёма", Details = "ID: 9 (пациент не подтвердил визит)", Timestamp = lastWed.AddHours(14).AddMinutes(20) },
                new() { Username = "savitskaya", Action = "Сохранение медкарты", Details = "Приём #7, пациент: Гринь Наталья", Timestamp = lastWed.AddHours(11).AddMinutes(25) },
                new() { Username = "admin", Action = "Обновление процедуры", Details = "ЭхоКГ (УЗИ сердца)", Timestamp = lastThu.AddHours(12) },
                new() { Username = "kovaleva", Action = "Запись на приём", Details = "Пациент: Левданский Артём, Врач: Дубовик Алексей", Timestamp = lastFri.AddHours(16).AddMinutes(40) },
                new() { Username = "admin", Action = "Сброс пароля", Details = "Пользователь: grinevich (Сергей Гриневич)", Timestamp = lastFri.AddHours(17).AddMinutes(5) }
            };

            foreach (var c in history.Where(a => a.Status == AppointmentStatus.Cancelled).Take(4))
            {
                audit.Add(new AuditLog
                {
                    Username = "kovaleva",
                    Action = "Отмена приёма",
                    Details = $"ID: {c.Id} (пациент не явился)",
                    Timestamp = c.AppointmentDateTime.Date.AddHours(10)
                });
            }

            var futureSamples = await context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Patient)
                .Where(a => a.AppointmentDateTime >= today.AddDays(3))
                .OrderBy(a => a.Id)
                .Take(5)
                .ToListAsync();
            var k = 1;
            foreach (var f in futureSamples)
            {
                var doctorName = f.Doctor.User.LastName + " " + f.Doctor.User.FirstName[0] + ".";
                var patientName = f.Patient.LastName + " " + f.Patient.FirstName[0] + ".";
                audit.Add(new AuditLog
                {
                    Username = "kovaleva",
                    Action = "Запись на приём",
                    Details = $"Пациент: {patientName}, Врач: {doctorName}, {f.AppointmentDateTime:dd.MM в HH:mm}",
                    Timestamp = now.AddMinutes(-7 * k)
                });
                k++;
            }

            context.AuditLogs.AddRange(audit);
            await context.SaveChangesAsync();
        }

        private static void GenerateRange(
            List<Appointment> appointments,
            List<MedicalRecord>? records,
            DateTime from,
            DateTime to,
            AppointmentStatus status,
            (Doctor Doc, Procedure[] Pool, Dictionary<DayOfWeek, (int From, int To)> Hours)[] plans,
            Patient[] patients,
            Dictionary<Procedure, RecTemplate[]> templates,
            DateTime? notBefore = null)
        {
            var dayIndex = 0;
            for (var date = from.Date; date <= to.Date; date = date.AddDays(1), dayIndex++)
            {
                if (date.DayOfWeek == DayOfWeek.Sunday) continue;

                for (var pi = 0; pi < plans.Length; pi++)
                {
                    var plan = plans[pi];
                    if (!plan.Hours.TryGetValue(date.DayOfWeek, out var hours)) continue;

                    var code = (dayIndex * 31 + pi * 17) % 50;
                    if (code >= 30) continue;
                    var slots = code % 3 == 0 ? 2 : 1;

                    DateTime? prevSlot = null;
                    for (var s = 0; s < slots; s++)
                    {
                        var span = Math.Max(2, hours.To - hours.From - 1);
                        var hour = hours.From + ((code + s * 2 + pi) % span);
                        var minute = (code % 2 == 0) ? 0 : 30;
                        var dt = date.AddHours(hour).AddMinutes(minute);
                        if (notBefore.HasValue && dt <= notBefore.Value) continue;
                        if (prevSlot != null && dt == prevSlot.Value) dt = dt.AddMinutes(30);
                        prevSlot = dt;

                        var patient = patients[(dayIndex * 5 + pi * 2 + s) % patients.Length];
                        var procedure = plan.Pool[(dayIndex + s + pi) % plan.Pool.Length];

                        var isCancelled = status == AppointmentStatus.Completed
                            && ((dayIndex * 7 + pi * 3 + s) % 23) == 4;
                        var appt = Mk(patient, plan.Doc, procedure, dt,
                            isCancelled ? AppointmentStatus.Cancelled : status);
                        appointments.Add(appt);

                        if (records != null && appt.Status == AppointmentStatus.Completed)
                        {
                            var tpl = templates[procedure];
                            var t = tpl[(dayIndex + s) % tpl.Length];
                            records.Add(new MedicalRecord
                            {
                                Appointment = appt,
                                Diagnosis = t.Diagnosis,
                                ChronicDiseases = t.Chronic,
                                Recommendations = t.Recommendations,
                                Notes = t.Notes,
                                CreatedAt = dt
                            });
                        }
                    }
                }
            }
        }
    }
}
