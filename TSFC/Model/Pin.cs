using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace TSFC.Model
{
    [Serializable]
    public class Pin
    {
        public enum TypePin
        {
            NULL, VCC, GND, INPUT, OUTPUT, SPEC
        }
        Regex[] regices = new Regex[] { new Regex("VCC"),       new Regex("GND"),
                                        new Regex("A\\d{1,2}"), new Regex("D\\d{1,2}") };
        private string name;
        List<string> states = new List<string>();
        private string workState;
        private string noWorkState;


        public static int AmountStates { get; set; }
        public int Number { get; set; }
        public string Name
        {
            get => name;
            set { name = value; SetName(value); }
        }
        public string WorkState 
        { 
            get => workState;
            set { SetState(value, true); }
        }
        public string NoWorkState 
        { 
            get => noWorkState; 
            set { SetState(value, false); }
        }
        public TypePin Type { get; set; }
        public List<string> States 
        { 
            get => states; 
            set => states = value; 
        }

        public Pin()
        {
            Number = 0;
            Name = "";
            Type = TypePin.NULL;
            WorkState = "";
            NoWorkState = "";
        }
        public Pin(int number)
        {
            Number = number;
        }

        public void SetAmountStates(int amount)
        {
            States = new List<string>();
            for (int i = 0; i < amount; i++)
            {
                States.Add("");
            }
            AmountStates = amount;
        }

        public void SetName(string name)
        {
            if (name == "VCC")
            {
                Type = TypePin.VCC;
                workState = "VCC";
                noWorkState = "VCC";
            }
            else if (name == "GND")
            {
                Type = TypePin.GND;
                workState = "GND";
                noWorkState = "GND";
            }
            else if (Regex.IsMatch(name, "A\\d{1,2}"))
            {
                Type = TypePin.INPUT;
            }
            else if (Regex.IsMatch(name, "D\\d{1,2}"))
            {
                Type = TypePin.OUTPUT;
            }
            else if (string.IsNullOrWhiteSpace(name))
            {
                Type = TypePin.NULL;
                workState = "";
                noWorkState = "";
            }
            else
            {
                Type = TypePin.SPEC;
            }

        }

        public void SetState(string state, bool isWork)
        {
            string[] states = new string[] { "0", "1", "X" };
            if (!Array.Exists(states, s => s == state)) return;

            if (isWork)
            {
                workState = state;
            }
            else
            {
                noWorkState = state;
            }
        }

        public int ReturnNumberInputOutputPin()
        {
            if (Type != TypePin.OUTPUT && Type != TypePin.INPUT)
                throw new Exception();

            int result = Convert.ToInt32(Regex.Replace(Name, "\\D", ""));
            return result;
        }

    }
}
