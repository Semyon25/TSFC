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

        public string GenerateTestSequence()
        {
            const char devider = ' ';
            const string startString = "FC 1 TS1 ";
            List<string> codes = new List<string>();
            var isNoWorkingStateAvailable = Pins.Where(p => (p.Type == Pin.TypePin.SPEC && p.NoWorkState != p.WorkState)).Count() > 0;

            for (int i = 0; i < Table.Lines.Count; i++)
            {
                string code = startString;
                foreach (var pin in Pins)
                {
                    if (pin.Type == Pin.TypePin.VCC || pin.Type == Pin.TypePin.GND || pin.Type == Pin.TypePin.SPEC)
                    {
                        code += pin.WorkState;
                    }
                    else if (pin.Type == Pin.TypePin.INPUT)
                    {
                        int num = pin.ReturnNumberInputOutputPin();
                        code += Table.Lines[i].Inputs[num];
                    }
                    else if (pin.Type == Pin.TypePin.OUTPUT)
                    {
                        int num = pin.ReturnNumberInputOutputPin();
                        code += Table.Lines[i].Outputs[num];
                    }

                    code += devider;
                }
                codes.Add(code);

                if (!isNoWorkingStateAvailable) { continue; }

                code = startString;
                foreach (var pin in Pins)
                {
                    if (pin.Type == Pin.TypePin.VCC || pin.Type == Pin.TypePin.GND || pin.Type == Pin.TypePin.SPEC)
                    {
                        code += pin.NoWorkState;
                    }
                    else if (pin.Type == Pin.TypePin.INPUT)
                    {
                        int num = pin.ReturnNumberInputOutputPin();
                        code += Table.Lines[i].Inputs[num];
                    }
                    else if (pin.Type == Pin.TypePin.OUTPUT)
                    {
                        int num = pin.ReturnNumberInputOutputPin();
                        code += Table.Lines[i].Outputs[num];
                    }

                    code += devider;
                }
                codes.Add(code);
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


    }
}
