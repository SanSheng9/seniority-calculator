using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Seniority_calculator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AppWindow.Resize(new SizeInt32(560, 400)); if (AppWindow.Presenter is OverlappedPresenter presenter) { presenter.IsResizable = false; }
        }

        private void StartPeriod_KeyDown(
        object sender,
        KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;

            string text = StartPeriod.Text;

            if (DateTime.TryParseExact(
                text,
                "ddMMyyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date))
            {
                // Дата корректная
                StartPeriod.Text = date.ToString("dd.MM.yyyy");

                // Переходим к следующему полю
                EndPeriod.Focus(FocusState.Programmatic);
            }
            else
            {
                // Дата некорректная
                StartPeriod.SelectAll();
            }

            // Не передаём Enter дальше
            e.Handled = true;
        }

        private void EndPeriod_KeyDown(
        object sender,
        KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;

            string text = EndPeriod.Text;

            if (DateTime.TryParseExact(
                text,
                "ddMMyyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date))
            {
                // Дата корректная
                EndPeriod.Text = date.ToString("dd.MM.yyyy");
                AddedSeniorityYears.Focus(FocusState.Programmatic);

            }
            else
            {
                // Дата некорректная
                EndPeriod.SelectAll();
            }

            // Не передаём Enter дальше
            e.Handled = true;
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (!DateTime.TryParseExact(StartPeriod.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime start_period) ||
                !DateTime.TryParseExact(EndPeriod.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime end_period))
            {
                ResultTextBlock.Text = "Пожалуйста, введите корректные даты начала и окончания периода в формате ДД.ММ.ГГГГ.";
                return;
            }

            Period period = new Period(start_period, end_period);
            
            if (AddedSeniorityDays.Text == "" || AddedSeniorityMonths.Text == "" || AddedSeniorityYears.Text == "")
            {
                ResultTextBlock.Text = "Пожалуйста, введите стаж в днях, месяцах и годах.";
                return;
            }

            int added_days;
            int added_months;
            int added_years;

            if (!(int.TryParse(AddedSeniorityDays.Text, out added_days) && int.TryParse(AddedSeniorityMonths.Text, out added_months) && int.TryParse(AddedSeniorityYears.Text, out added_years)))
            {
                ResultTextBlock.Text = "Пожалуйста, введите корректные числовые значения для стажа.";
                return;
            }

            Seniority added_seniority = new Seniority(added_years, added_months, added_days);

            List<Period> periods = new List<Period> { period };

            Calculation calculation = new Calculation("Иванов Иван Иванович", periods, added_seniority);

            Seniority final_seniority = calculation.FinalSeniority();

            ResultTextBlock.Text = $"Итоговый стаж: {final_seniority.Years} лет, {final_seniority.Months} месяцев, {final_seniority.Days} дней.";
        }
    }

    class Period()
    {
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }

        public Period(DateTime start_period, DateTime end_period) : this() { StartPeriod = start_period; EndPeriod = end_period; }

        public TimeSpan GetPeriodDuration()
        {
            return EndPeriod- StartPeriod;
        }
    }
 
    struct Seniority()
    {
        public int Years { get; set; }
        public int Months { get; set; }
        public int Days { get; set; }
        public Seniority(int years, int months, int days) : this() { Years = years; Months = months; Days = days; }
    }

    class Calculation()
    {
        public string FullName { get; set; } = string.Empty;
        public List<Period> Periods { get; set; } = new List<Period>();
        public Seniority AddedSeniority { get; set; }

        public Calculation(string full_name, List<Period> periods, Seniority added_seniority) : this() { FullName = full_name; Periods = periods; AddedSeniority = added_seniority; }

        public Seniority FinalSeniority()
        {
            int total_days = Periods.Sum(p => (int)p.GetPeriodDuration().TotalDays);
            total_days += AddedSeniority.Years * 365 + AddedSeniority.Months * 30 + AddedSeniority.Days;
            int years = total_days / 365;
            int months = (total_days % 365) / 30;
            int days = (total_days % 365) % 30;
            return new Seniority(years, months, days);
        }
    }

}
