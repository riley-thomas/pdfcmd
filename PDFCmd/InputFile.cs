using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace PDFCmd
{
    class InputFile
    {

        public Dictionary<string, string> fields;

        public InputFile(string path = null)
        {
            fields = new Dictionary<string, string>();
            if (path != null)
            {
                foreach (string line in File.ReadLines(path))
                {
                    if (line == null || line.Trim() == "") continue;
                    if (!Regex.IsMatch(line.Trim(), @"^([A-Z\]\[\._0-9]){1,50}=.{1,50}$", RegexOptions.IgnoreCase)) continue;
                    string[] f = line.Trim().Split('=');
                    if (fields.ContainsKey(f[0])) continue;
                    fields.Add(f[0], f[1]);
                }
            }
        }
    }
}
