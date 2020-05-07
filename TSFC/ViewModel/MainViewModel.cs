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

        public List<Pin> SpecPins
        {
            get
            {
                List<Pin> list = new List<Pin>();
                if (model.Pins != null && model.Pins.Count > 0)
                    list = model.Pins.Where(p => p.Type == Pin.TypePin.SPEC).ToList();
                return list;
            }
        }

        public string PathBinaryFile
        {
            get => model.PathBinaryFile;
            set { model.PathBinaryFile = value; RaisePropertyChanged(); }
        }

        public string TestSequence => model.TestSequence;

        public bool IsGenerateAll { get; set; } = true;
        public int AmountAddresses { get; set; }



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
            RaisePropertyChanged(nameof(LogicTable));
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
                RaisePropertyChanged(nameof(SpecPins));
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


        #region ICommand
        public ICommand AmountPinsOk_Button { get; private set; }
        public ICommand CheckPins_Button { get; private set; }
        public ICommand ChooseBinaryFile_Button { get; private set; }
        public ICommand GenerateCode_Button { get; private set; }
        public ICommand OpenChip_Button { get; private set; }
        public ICommand SaveChip_Button { get; private set; }
        public ICommand SaveTestSequenceInFile_Button { get; private set; }
        public ICommand OpenLogicTableWindow_Button { get; private set; }

        #endregion

    }
}