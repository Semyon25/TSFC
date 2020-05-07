using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TSFC.Model;

namespace TSFC.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        MainModel model;
        private int amountPins = 24;
        public int AmountPins
        {
            get { return amountPins; }
            set { amountPins = value; RaisePropertyChanged(); }
        }

        public List<Pin> LeftSideChip
        {
            get
            {
                List<Pin> list = new List<Pin>();
                if (model.Pins != null && model.Pins.Count > 0)
                    list = model.Pins.Where(p => p.Number <= AmountPins / 2).ToList();
                return list;
            }
        }
        public List<Pin> RightSideChip
        {
            get
            {
                List<Pin> list = new List<Pin>();
                if (model.Pins != null && model.Pins.Count > 0)
                    for (int i = model.Pins.Count - 1; i >= AmountPins / 2; i--)
                    {
                        list.Add(model.Pins[i]);
                    }
                return list;
            }
        }

        public DataTable SpecPins { get; set; }

        public string PathBinaryFile
        {
            get => model.PathBinaryFile;
            set { model.PathBinaryFile = value; RaisePropertyChanged(); }
        }

        public string TestSequence => model.TestSequence;

        public bool IsGenerateAll { get; set; } = true;
        public int AmountAddresses { get; set; }

        public int AmountVectorStates { get; set; } = 1;


        public MainViewModel()
        {
            model = MainModel.Model;
            AmountPinsOk_Button = new RelayCommand(AmountPinsOk);
            CheckPins_Button = new RelayCommand(CheckPins);
            ChooseBinaryFile_Button = new RelayCommand(ChooseBinaryFile);
            GenerateCode_Button = new RelayCommand(GenerateCode);
            OpenChip_Button = new RelayCommand(OpenChip);
            SaveChip_Button = new RelayCommand(SaveChip);
            SaveTestSequenceInFile_Button = new RelayCommand(SaveTestSequenceInFile);
            OpenLogicTableWindow_Button = new RelayCommand(OpenLogicTableWindow);
            Ready_Button = new RelayCommand(Ready);
            RaisePropertyChanged(nameof(LogicTable));
        }

        private void Ready()
        {
            List<string> inputPin = new List<string>();
            List<string> outputPin = new List<string>();
            List<List<string>> specPins = new List<List<string>>();

            for (int i = 3; i < SpecPins.Columns.Count; i++)
            {
                specPins.Add(new List<string>());
            }

            for (int i = 0; i<SpecPins.Rows.Count; ++i)
            {
                var items = SpecPins.Rows[i].ItemArray;
                inputPin.Add((string)items[1]);
                outputPin.Add((string)items[2]);
                for (int j = 3; j < items.Count(); j++)
                {
                    specPins[j - 3].Add((string)items[j]);
                }
            }
            
            foreach (var pin in model.Pins.Where(p=>p.Type == Pin.TypePin.INPUT).ToList())
            {
                pin.States = inputPin;
            }
            foreach (var pin in model.Pins.Where(p => p.Type == Pin.TypePin.OUTPUT).ToList())
            {
                pin.States = outputPin;
            }
            for (int i = 3; i < SpecPins.Columns.Count; i++)
            {
                string name = SpecPins.Columns[i].ColumnName;
                model.Pins.Where(p => p.Name == name).First().States = specPins[i-3];
            }
        }

        private void OpenLogicTableWindow()
        {
            LogicTableWindow window = new LogicTableWindow();
            window.Show();
        }

        private void SaveTestSequenceInFile()
        {
            string path;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Test sequence (*.txt)|*.txt";
            if (dialog.ShowDialog() == true)
            {
                path = dialog.FileName;
                using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(TestSequence);
                }
            }
        }

        private void OpenChip()
        {
            string path;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "chip (*.xml)|*.xml";
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (dialog.ShowDialog() == true)
            {
                path = dialog.FileName;
                model.DeserializeChip(path);
                AmountPins = model.AmountPins;
                RaisePropertyChanged(nameof(LeftSideChip));
                RaisePropertyChanged(nameof(RightSideChip));
                SpecPins = ConvertPinsToDataTable(model.Pins);
                RaisePropertyChanged(nameof(SpecPins));
                AmountVectorStates = Pin.AmountStates;
                RaisePropertyChanged(nameof(AmountVectorStates));
            }
        }

        private void SaveChip()
        {
            string path;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "chip (*.xml)|*.xml";
            if (dialog.ShowDialog() == true)
            {
                path = dialog.FileName;
                model.SerializeChip(path);
            }
            
        }

        private void GenerateCode()
        {
            try
            {
                model.LoadBinaryFile(PathBinaryFile);

                model.GenerateTestSequence(IsGenerateAll? 0 : AmountAddresses);

                RaisePropertyChanged(nameof(TestSequence));
                RaisePropertyChanged(nameof(LogicTable));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ChooseBinaryFile()
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Binary files(*.bin)|*.bin";
                dialog.InitialDirectory = Directory.GetCurrentDirectory();
                if (dialog.ShowDialog() == true)
                {
                    PathBinaryFile = dialog.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void CheckPins()
        {
            try
            {
                model.CheckChip();
                model.SetAmountVectorStates(AmountVectorStates);
                SpecPins = ConvertPinsToDataTable(model.Pins);
                RaisePropertyChanged(nameof(SpecPins));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AmountPinsOk()
        {
            try
            {
                model.SetAmountPins(AmountPins);
                RaisePropertyChanged(nameof(LeftSideChip));
                RaisePropertyChanged(nameof(RightSideChip));
                RaisePropertyChanged(nameof(SpecPins));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        DataTable ConvertPinsToDataTable(List<Pin> pins)
        {
            DataTable data = new DataTable();

            var col = data.Columns.Add("¹");
            col.ReadOnly = true;
            data.Columns.Add("A");
            data.Columns.Add("D");
            var specPinsName = pins.Where(p => p.Type == Pin.TypePin.SPEC).Select(p => p.Name).ToList();
            foreach (var p in specPinsName)
            {
                data.Columns.Add(p);
            }

            var inputPin = pins.Where(p => p.Type == Pin.TypePin.INPUT).Select(p=>p.States).FirstOrDefault();
            var outputPin = pins.Where(p => p.Type == Pin.TypePin.OUTPUT).Select(p => p.States).FirstOrDefault();
            var specPins = pins.Where(p => p.Type == Pin.TypePin.SPEC).Select(p => p.States).ToList();

            for (int i = 0; i < Pin.AmountStates; i++)
            {
                DataRow row = data.NewRow();
                row[0] = (i + 1).ToString();
                row[1] = inputPin[i];
                row[2] = outputPin[i];
                for (int j = 0; j < specPins.Count; j++)
                {
                    row[3 + j] = specPins[j][i];
                }
                data.Rows.Add(row);
            }

            return data;
        }


        #region ICommand
        public ICommand AmountPinsOk_Button { get; private set; }
        public ICommand CheckPins_Button { get; private set; }
        public ICommand ChooseBinaryFile_Button { get; private set; }
        public ICommand GenerateCode_Button { get; private set; }
        public ICommand OpenChip_Button { get; private set; }
        public ICommand SaveChip_Button { get; private set; }
        public ICommand SaveTestSequenceInFile_Button { get; private set; }
        public ICommand OpenLogicTableWindow_Button { get; private set; }
        public ICommand Ready_Button { get; private set; }

        #endregion

    }
}