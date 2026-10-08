using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seniority_calculator.Source
{
    internal class WorkPeriod
    {
        public DateOnly StartPeriod { get; set; }
        public DateOnly EndPeriod { get; set; }

        public WorkPeriod(
            DateOnly startPeriod,
            DateOnly endPeriod)
        {
            StartPeriod = startPeriod;
            EndPeriod = endPeriod;
        }

    }
}