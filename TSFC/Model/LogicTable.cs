using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSFC.Model
{
    public class LogicTable
    {
        public int NumColumns { get; private set; }
        public int NumInputColumns { get; private set; }
        public int NumOutputColumns { get; private set; }

        List<LogicLine> lines = new List<LogicLine>();

        public LogicTable(int numInput, int numOutput)
        {
            LogicLine.SetInputOutputPins(numInput, numOutput);
            NumInputColumns = numInput;
            NumOutputColumns = numOutput;
            NumColumns = numInput + numOutput;
        }

        public List<LogicLine> Lines { get => lines; set => lines = value; }

        public void Add(int Address, int Value)
        {
            var line = new LogicLine(Address, Value);
            Lines.Add(line);
        }

    }
}
