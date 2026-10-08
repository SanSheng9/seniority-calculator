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
using Seniority_calculator.Source;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Seniority_calculator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainWindow : Window
    {

        private const int MaxPeriodRows = 5;

        private readonly List<WorkPeriodRow> _periodRows = new();


        public MainWindow()
        {
            InitializeComponent();
            //if (AppWindow.Presenter is OverlappedPresenter presenter)
            //{
            //    presenter.IsResizable = false;
            //}

            // 1. Получаем дескриптор окна (HWND)
            IntPtr hWnd = WindowNative.GetWindowHandle(this);

            // 2. Получаем WindowId по HWND
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

            // 3. Получаем экземпляр AppWindow
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            // 4. Задаем новые размеры (например, ширина 1000, высота 700 пикселей)
            appWindow.Resize(new SizeInt32 { Width = 400, Height = 600 });

            // Запрещаем изменение размера окна
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
            }

            ExtendsContentIntoTitleBar = true;
            // Replace system title bar with the WinUI TitleBar.
            SetTitleBar(AppTitleBar);

            SetupFinalSeniority(0, 0, 0);
            AddPeriodRow();

        }

        private void SetupFinalSeniority(
            int years,
            int months,
            int days)
        {
            FinalSeniority.Text =
                $"{years} лет, {months} месяцев, {days} дней.";
        }

        private void AddPeriodRow()
        {

            if (_periodRows.Count >= MaxPeriodRows)
                return;

            bool isFirstRow = _periodRows.Count == 0;

            if (_periodRows.Count == 5) return;

            var startTextBox = new TextBox
            {
                Header = isFirstRow ? "Дата приема" : null,
                PlaceholderText = "ДДММГГГГ",
                MaxLength = 10,
                Height = isFirstRow ? 60 : 30
            };

            var endTextBox = new TextBox
            {
                Header = isFirstRow ? "Дата увольнения" : null,
                PlaceholderText = "ДДММГГГГ",
                MaxLength = 10,
                Height = isFirstRow ? 60 : 30
            };

            var grid = new Grid
                {
                    ColumnSpacing = 8
                };

            grid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Grid.SetColumn(startTextBox, 0);
            Grid.SetColumn(endTextBox, 1);

            grid.Children.Add(startTextBox);
            grid.Children.Add(endTextBox);

            PeriodsPanel.Children.Add(grid);

            var row = new WorkPeriodRow(
                startTextBox,
                endTextBox);

            _periodRows.Add(row);

            startTextBox.KeyDown += StartTextBox_KeyDown;
            endTextBox.KeyDown += EndTextBox_KeyDown;

            startTextBox.Focus(FocusState.Programmatic);
        }

        private void StartTextBox_KeyDown(
           object sender,
           KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter && e.Key != Windows.System.VirtualKey.Tab)
                return;

            var textBox = (TextBox)sender;

            if (!TryParseDate(textBox.Text, out DateOnly date))
            {
                textBox.SelectAll();
                e.Handled = true;
                return;
            }

            textBox.Text = date.ToString("dd.MM.yyyy");

            var row = _periodRows.First(
                x => x.StartTextBox == textBox);

            row.EndTextBox.Focus(FocusState.Programmatic);

            e.Handled = true;
        }

        private void EndTextBox_KeyDown(
         object sender,
         KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter && e.Key != Windows.System.VirtualKey.Tab)
                return;

            var textBox = (TextBox)sender;

            if (!TryParseDate(textBox.Text, out DateOnly endDate))
            {
                textBox.SelectAll();
                e.Handled = true;
                return;
            }

            textBox.Text = endDate.ToString("dd.MM.yyyy");

            var row = _periodRows.First(
                x => x.EndTextBox == textBox);

            if (!TryParseDate(
                    row.StartTextBox.Text,
                    out DateOnly startDate))
            {
                return;
            }

            if (endDate < startDate)
            {
                textBox.SelectAll();
                e.Handled = true;
                return;
            }

            CalculateSeniority();

            if (_periodRows.Count < MaxPeriodRows)
            {
                AddPeriodRow();
            }

            e.Handled = true;

        }

        private bool TryParseDate(
        string text,
        out DateOnly date)
        {
            if (DateOnly.TryParseExact(
                text,
                "ddMMyyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
            {
                return true;
            }

            return DateOnly.TryParseExact(
                text,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        private void CalculateSeniority()
        {

            List<WorkPeriod> periods = new List<WorkPeriod>();

            foreach (var row in _periodRows)
            {
                if (!TryParseDate(
                        row.StartTextBox.Text,
                        out DateOnly startDate))
                {
                    break;
                }

                if (!TryParseDate(
                        row.EndTextBox.Text,
                        out DateOnly endDate))
                {
                    break;
                }

                if (endDate < startDate)
                {
                    break;
                }

                periods.Add(
                    new WorkPeriod(startDate, endDate));
            }

            int addedDays = GetNumberBoxValue(
                AddedSeniorityDays);

            int addedMonths = GetNumberBoxValue(
                AddedSeniorityMonths);

            int addedYears = GetNumberBoxValue(
                AddedSeniorityYears);

            var addedSeniority = new Seniority(
                addedYears,
                addedMonths,
                addedDays);

            var calculation = new Calculation(
                "Иванов Иван Иванович",
                periods,
                addedSeniority);

            Seniority finalSeniority =
                calculation.FinalSeniority();

            SetupFinalSeniority(
                finalSeniority.Years,
                finalSeniority.Months,
                finalSeniority.Days);

        }
        private int GetNumberBoxValue(NumberBox numberBox)
        {
            if (double.IsNaN(numberBox.Value))
                return 0;

            return (int)numberBox.Value;
        }

        //Обработчики полей добавленного стажа
        private void AddedSeniorityYearsChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            CalculateSeniority();
        }
        private void AddedSeniorityMonthsChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            CalculateSeniority();
        }
        private void AddedSeniorityDaysChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            CalculateSeniority();
        }
    }

}
