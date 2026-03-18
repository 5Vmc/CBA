using System.Collections.Generic;
using System.IO;

namespace Babu.Config
{
    public class DataTable
    {
        private List<string> _fileds = new List<string>();
        private List<DataRow> _data_rows = new List<DataRow>();
        public BinaryReader _binaryReader { get; set; }

        public void CloseReader()
        {
            _binaryReader?.Close();
        }

        public void setFileds(List<string> fields) { _fileds = fields; }

        public int filedIndex(string field)
        {
            for (int i = 0; i < _fileds.Count; ++i)
            {
                if (_fileds[i] == field)
                {
                    return i;
                }
            }
            return -1;
        }

        public void insertRow(DataRow data_row)
        {
            data_row.SetFields(_fileds);
            _data_rows.Add(data_row);
        }

        public DataRow getDataRow(int index)
        {
            return _data_rows[index];
        }

        public int getRowCount()
        {
            return _data_rows.Count;
        }
    }
}