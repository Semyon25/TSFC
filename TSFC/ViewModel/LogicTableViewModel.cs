using GalaSoft.MvvmLight;
using System.Data;
using TSFC.Model;

namespace TSFC.ViewModel
{
    public class LogicTableViewModel : ViewModelBase
    {
        MainModel model;

        public DataTable LogicTable
        {
            get
            {
                if (model.Table == null) return null;

                DataTable data = new DataTable();

                int numRows = model.Table.Lines.Count;
                int numCol = model.Table.NumColumns;

                data.Columns.Add("№");
                for (int i = 0; i < model.Table.NumInputColumns; i++)
                {
                    data.Columns.Add($"A{i}");
                }
                for (int i = 0; i < model.Table.NumOutputColumns; i++)
                {
                    data.Columns.Add($"D{i}");
                }

                for (int i = 0; i < numRows; i++)
                {
                    object[] elems = new object[numCol + 1];
                    int j = 0;
                    elems[j] = i + 1;
                    ++j;
                    foreach (var item in model.Table.Lines[i].Inputs)
                    {
                        elems[j] = item;
                        j++;
                    }
                    foreach (var item in model.Table.Lines[i].Outputs)
                    {
                        elems[j] = item;
                        j++;
                    }
                    data.Rows.Add(elems);
                }

                return data;
            }
        }

        public LogicTableViewModel()
        {
            model = MainModel.Model;
        }
    }
}
