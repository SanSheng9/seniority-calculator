using Microsoft.UI.Xaml.Controls;

namespace Seniority_calculator.Source
{
    internal class WorkPeriodRow
    {
        public TextBox StartTextBox { get; }
        public TextBox EndTextBox { get; }

        public WorkPeriodRow(
            TextBox startTextBox,
            TextBox endTextBox)
        {
            StartTextBox = startTextBox;
            EndTextBox = endTextBox;
        }
    }
}