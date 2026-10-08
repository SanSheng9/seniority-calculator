using System;
using System.Collections.Generic;
using System.Linq;

namespace Seniority_calculator.Source
{
    internal class Calculation
    {
        public string FullName { get; set; } = string.Empty;

        public List<WorkPeriod> Periods { get; set; } = new();

        public Seniority AddedSeniority { get; set; }

        public Calculation(
            string fullName,
            List<WorkPeriod> periods,
            Seniority addedSeniority)
        {
            FullName = fullName;
            Periods = periods;
            AddedSeniority = addedSeniority;
        }

        public Seniority FinalSeniority()
        {
            // Сначала объединяем пересекающиеся периоды.
            List<WorkPeriod> mergedPeriods = MergePeriods();

            Seniority result = new Seniority(0, 0, 0);

            // Рассчитываем каждый период отдельно.
            foreach (var period in mergedPeriods)
            {
                Seniority periodSeniority =
                    CalculatePeriod(period);

                result = AddSeniority(
                    result,
                    periodSeniority);
            }

            // Добавляем стаж, введённый пользователем вручную.
            result = AddSeniority(
                result,
                AddedSeniority);

            return result;
        }

        /// <summary>
        /// Рассчитывает один рабочий период
        /// через календарные годы, месяцы и дни.
        /// </summary>
        private Seniority CalculatePeriod(
            WorkPeriod period)
        {
            DateOnly start = period.StartPeriod;
            DateOnly end = period.EndPeriod;

            int years = 0;
            int months = 0;

            DateOnly current = start;

            // Сначала определяем количество полных лет.
            while (true)
            {
                DateOnly next = current.AddYears(1);

                if (next > end)
                    break;

                current = next;
                years++;
            }

            // Затем определяем количество полных месяцев.
            while (true)
            {
                DateOnly next = current.AddMonths(1);

                if (next > end)
                    break;

                current = next;
                months++;
            }

            // Всё, что осталось — дни.
            int days =
                end.DayNumber - current.DayNumber + 1;

            return new Seniority(
                years,
                months,
                days);
        }

        /// <summary>
        /// Объединяет пересекающиеся и следующие
        /// непосредственно друг за другом периоды.
        /// </summary>
        private List<WorkPeriod> MergePeriods()
        {
            if (Periods.Count == 0)
                return new List<WorkPeriod>();

            var sortedPeriods = Periods
                .OrderBy(p => p.StartPeriod)
                .ThenBy(p => p.EndPeriod)
                .ToList();

            var mergedPeriods = new List<WorkPeriod>();

            DateOnly currentStart =
                sortedPeriods[0].StartPeriod;

            DateOnly currentEnd =
                sortedPeriods[0].EndPeriod;

            for (int i = 1; i < sortedPeriods.Count; i++)
            {
                WorkPeriod nextPeriod =
                    sortedPeriods[i];

                // Периоды пересекаются или идут подряд.
                if (nextPeriod.StartPeriod.DayNumber
                    <= currentEnd.DayNumber + 1)
                {
                    if (nextPeriod.EndPeriod > currentEnd)
                    {
                        currentEnd = nextPeriod.EndPeriod;
                    }
                }
                else
                {
                    mergedPeriods.Add(
                        new WorkPeriod(
                            currentStart,
                            currentEnd));

                    currentStart =
                        nextPeriod.StartPeriod;

                    currentEnd =
                        nextPeriod.EndPeriod;
                }
            }

            // Добавляем последний период.
            mergedPeriods.Add(
                new WorkPeriod(
                    currentStart,
                    currentEnd));

            return mergedPeriods;
        }

        /// <summary>
        /// Складывает два значения стажа.
        /// </summary>
        private Seniority AddSeniority(
            Seniority first,
            Seniority second)
        {
            int years =
                first.Years + second.Years;

            int months =
                first.Months + second.Months;

            int days =
                first.Days + second.Days;

            // 30 дней = 1 месяц.
            months += days / 30;
            days %= 30;

            // 12 месяцев = 1 год.
            years += months / 12;
            months %= 12;

            return new Seniority(
                years,
                months,
                days);
        }
    }

    internal struct Seniority
    {
        public int Years { get; set; }

        public int Months { get; set; }

        public int Days { get; set; }

        public Seniority(
            int years,
            int months,
            int days)
        {
            Years = years;
            Months = months;
            Days = days;
        }
    }
}