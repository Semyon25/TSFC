using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSFC.Model
{
    public class LogicLine
    {
        static int numberInputPins;
        static int numberOutputPins;
        public static void SetInputOutputPins(int numIn, int numOut)
        {
            numberInputPins = numIn;
            numberOutputPins = numOut;
        }
        readonly List<char> inputs;
        readonly List<char> outputs;
        public int Address { get; private set; }
        int value;

        public List<char> Outputs => outputs;

        public List<char> Inputs => inputs;

        public LogicLine(int address, int value)
        {
            this.Address = address;
            this.value = value;
            string binaryInput = Convert.ToString(address, 2);
            inputs = ConvertToInputList(binaryInput);
            string binaryOutput = Convert.ToString(value, 2);
            outputs = ConvertToOutputList(binaryOutput);
        }

        List<char> ConvertToOutputList(string binary)
        {
            List<char> list = new List<char>();
            int counter = 0;
            for (counter = 0; counter < binary.Length; counter++)
            {

                list.Insert(0, binary[counter] == '0' ? 'L' : 'H');
            }
            for (; counter < numberOutputPins; counter++)
            {
                list.Add('L');
            }
            return list;
        }

        List<char> ConvertToInputList(string binary)
        {
            List<char> list = new List<char>();
            int counter = 0;
            for (counter = 0; counter < binary.Length; counter++)
            {
                list.Insert(0,binary[counter]);
            }
            for (; counter < numberInputPins; counter++)
            {
                list.Add('0');
            }
            return list;
        }
    }
}
