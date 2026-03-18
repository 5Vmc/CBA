using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Babu.Config
{
    public class ByteService : MonoBehaviour
    {
        public static DataTable LoadTables(BinaryReader br)
        {
            DataTable table = new DataTable();
            table._binaryReader = br;

            return table;
        }
    }
}
