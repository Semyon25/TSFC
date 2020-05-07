using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace TSFC.Model
{
    class MainModel : ObservableObject
    {
        private static MainModel model;
        public static MainModel Model => model ?? (model = new MainModel());


        string pathBinaryFile;
        LogicTable table;
        int amountPins;

        public int AmountPins { get => amountPins; private set => amountPins = value; }
        public void SetAmountPins(int amount)
        {
            amountPins = amount;

            Pin[] pinsArray = new Pin[amountPins];
            //Pins = Array.AsReadOnly(pinsArray);
            for (int i = 0; i < amountPins; i++)
            {
                pinsArray[i] = new Pin(i + 1);
            }
            Pins = pinsArray.ToList();
        }

        public string TestSequence { get; private set; }

        public List<Pin> Pins { get; private set; }
        public string PathBinaryFile { get => pathBinaryFile; set => pathBinaryFile = value; }
        public LogicTable Table { get => table; set => table = value; }

        public MainModel()
        {

        }

        public void CheckChip()
        {
            var list = Pins.Where(p => p.Type == Pin.TypePin.NULL).Select(p => p.Number).ToList();
            if (list.Count > 0)
            {
                throw new Exception("Проверьте пины: " + string.Join(" ", list));
            }
            var n = Pins.Where(p => p.Type == Pin.TypePin.INPUT).Count();
            if (n == 0)
            {
                throw new Exception("Нет ни одного входного пина");
            }
            n = Pins.Where(p => p.Type == Pin.TypePin.OUTPUT).Count();
            if (n == 0)
            {
                throw new Exception("Нет ни одного выходного пина");
            }
        }

        public void LoadBinaryFile(string path)
        {
            this.PathBinaryFile = path;
            byte[] bytes = ReadFile(path);
            int NumInputPins = Pins.Where(p => p.Type == Pin.TypePin.INPUT).Count();
            int NumOutputPins = Pins.Where(p => p.Type == Pin.TypePin.OUTPUT).Count();
            Table = new LogicTable(NumInputPins, NumOutputPins);
            for (int i = 0; i < bytes.Length; i++)
            {
                Table.Add(i, bytes[i]);
            }
        }

        byte[] ReadFile(string path)
        {
            byte[] bytes;
            using (var read = File.Open(path, FileMode.Open))
            {
                bytes = new byte[read.Length];
                read.Read(bytes, 0, Convert.ToInt32(read.Length));

            }
            return bytes;
        }

        public string GenerateTestSequence(int amount)
        {
            const char devider = ' ';
            const string startString = "FC 1 TS1 ";
            int amountAddresses = amount == 0 ? Table.Lines.Count : amount;
            List<string> codes = new List<string>();
            var isNoWorkingStateAvailable = Pins.Where(p => (p.Type == Pin.TypePin.SPEC && p.NoWorkState != p.WorkState)).Count() > 0;

            for (int i = 0; i < amountAddresses; i++)
            {
                for (int state = 0; state < Pin.AmountStates; ++state)
                {
                    string code = startString;
                    foreach (var pin in Pins)
                    {
                        if (pin.Type == Pin.TypePin.VCC || pin.Type == Pin.TypePin.GND)
                        {
                            code += pin.WorkState;
                        }
                        else if (pin.Type == Pin.TypePin.SPEC)
                        {
                            code += pin.States[state];
                        }
                        else if (pin.Type == Pin.TypePin.INPUT)
                        {
                            if (pin.States[state] == "1" || pin.States[state] == "0" || pin.States[state] == "Z")
                            {
                                code += pin.States[state];
                            }
                            else 
                            {
                                int num = pin.ReturnNumberInputOutputPin();
                                code += Table.Lines[i].Inputs[num];
                            }
                        }
                        else if (pin.Type == Pin.TypePin.OUTPUT)
                        {
                            if (pin.States[state] == "H" || pin.States[state] == "L" || pin.States[state] == "X")
                            {
                                code += pin.States[state];
                            }
                            else
                            {
                                int num = pin.ReturnNumberInputOutputPin();
                                code += Table.Lines[i].Outputs[num];
                            }
                        }

                        code += devider;
                    }
                    codes.Add(code);

                }
            }

            string result = string.Join("\n", codes);
            TestSequence = result;
            return result;
        }

        internal void DeserializeChip(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Pin>));
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                Pins = (List<Pin>)serializer.Deserialize(fs);
                AmountPins = Pins.Count;
                Pin.AmountStates = Pins.Where(p => p.Type == Pin.TypePin.INPUT).First().States.Count;
            }
        }

        public void SerializeChip(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Pin>));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {

                serializer.Serialize(fs, Pins);
            }
        }

        public void SetAmountVectorStates(int amount)
        {
            foreach (var pin in Pins)
            {
                pin.SetAmountStates(amount);
            }
        }


    }
}
