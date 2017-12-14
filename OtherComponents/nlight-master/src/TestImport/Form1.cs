using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLight;
using NLight.IO.Text;
using System.IO;

namespace TestImport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = @"c:\a CSV con PuntoeVirgola Header.txt";
            using (var csv = new DelimitedRecordReader(new StreamReader(path)))
            {
                csv.DelimiterCharacter = ';';

                if (csv.ReadColumnHeaders() == ReadResult.Success)
                { 
                    while (csv.Read() == ReadResult.Success)
                    {
                        string text = string.Empty;
                        bool bOK = csv.TryGetValue(0, string.Empty, out text);

                        MessageBox.Show(text);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = @"c:\provascrittura.csv";

            using (var csv = new DelimitedRecordWriter(new StreamWriter(path)))
            {
                csv.DelimiterCharacter = ';';
                csv.useQuoteString = false;

                DelimitedRecordColumn aCol1 = new DelimitedRecordColumn("A1");
                DelimitedRecordColumn aCol2 = new DelimitedRecordColumn("A2");
                DelimitedRecordColumn aCol3 = new DelimitedRecordColumn("A3");

                csv.Columns.Add(aCol1);
                csv.Columns.Add(aCol2);
                csv.Columns.Add(aCol3);

                csv.WriteRecordStart();

                csv.WriteField("Val1");
                csv.WriteField("Val2");
                csv.WriteField("Val3");

                csv.WriteRecordEnd();

                csv.WriteRecordStart();

                csv.WriteField("Val4");
                csv.WriteField("Val5");
                csv.WriteField("Val6");

                csv.WriteRecordEnd();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string path = @"c:\a - Fixed Column.txt";
            using (var fix = new FixedWidthRecordReader(new StreamReader(path)))
            {
                fix.Columns.Add(new FixedWidthRecordColumn("Item", 0, 20));
                fix.Columns.Add(new FixedWidthRecordColumn("Quantity", 20, 20));
                fix.Columns.Add(new FixedWidthRecordColumn("UnitValue", 40, 20));

                while (fix.Read() == ReadResult.Success)
                {
                    string outval = string.Empty;
                    bool ok = fix.TryGetValue<string>("Item", "", out outval);
                    if (ok == false)
                        return;
                }
            }
        }
    }
}
