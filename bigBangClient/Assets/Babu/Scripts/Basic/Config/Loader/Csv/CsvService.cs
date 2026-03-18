using System;
using System.Collections.Generic;
using UnityEngine;

namespace Babu.Config
{
    public class CSVService
    {
        enum PaseLineState
        {
            STATE__NORMAL, // 普通字符串;
            STATE__QUOTA, // 进入双引号;
        };

        private static List<string> ParseLine(string line)
        {
            List<string> ret = new List<string>();
            if (line.Length == 0) return ret;

            char comma = ',';
            char quota = '"';
            PaseLineState state = PaseLineState.STATE__NORMAL;
            int pos = 0;
            string str_value = "";

            do
            {
                char chr = line[pos];

                switch (state)
                {
                    case PaseLineState.STATE__NORMAL:
                    {
                        if (chr == quota)
                        {
                            state = PaseLineState.STATE__QUOTA;
                        }
                        else if (chr == comma)
                        {
                            ret.Add(str_value.Trim(' '));
                            str_value = "";
                        }
                        else
                        {
                            if (chr == '\t') continue;
                            str_value += chr;
                        }
                    }
                        break;

                    case PaseLineState.STATE__QUOTA:
                    {
                        if (chr == quota)
                        {
                            state = PaseLineState.STATE__NORMAL;
                        }
                        else
                        {
                            str_value += chr;
                        }
                    }
                        break;
                    default:
                        break;
                }
            } while (++pos < line.Length);

            if (state == PaseLineState.STATE__QUOTA)
            {
                Debug.Log("csv line format error");
            }

            ret.Add(str_value.Trim(' '));

            return ret;
        }
        
        public static DataTable LoadTables(string textData)
        {
            string[] lines = textData.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            DataTable table = new DataTable();
            if (lines.Length < 1) return table;
            string field_str = lines[0].Trim(new char[] {'\r', ' ', '\t'});
            table.setFileds(ParseLine(field_str));

            for (int i = 1; i < lines.Length; ++i)
            {
                string value_str = lines[i].Trim(new char[] {'\r', ' ', '\t'});
                DataRow row = new DataRow();
                row.SetValues(ParseLine(value_str));
                table.insertRow(row);
            }

            return table;
        }
        
    }
}