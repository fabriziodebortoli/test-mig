using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.HashAlgorithms;
using System.IO;

namespace Microarea.TaskBuilderNet.CRCCalculator
{
    public partial class Form_CRCCalc : Form
    {
        CRC crcCalculator;
        public Form_CRCCalc()
        {
            InitializeComponent();
            crcCalculator = new CRC(new Parameters(32, 0x04C11DB7, 0xFFFFFFFF, true, true, 0xFFFFFFFF, 0xCBF43926));         
        }

        private void Button_browse_Click(object sender, EventArgs e)
        {         
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_filePath.Text = openFileDialog1.FileName;
            }

        }

        private void Button_calc_Click(object sender, EventArgs e)
        {
            try
            {
                Stream myStream = null;
                crcCalculator.Initialize();
                if(!String.IsNullOrEmpty(textBox_filePath.Text) && Path.IsPathRooted (openFileDialog1.FileName))
                { 
                    myStream = openFileDialog1.OpenFile();
                    label_source.Text = "from file";
                }
                else
                {
                    if (!String.IsNullOrEmpty(textBox_string.Text))
                    { 
                        MemoryStream stream = new MemoryStream();
                        StreamWriter writer = new StreamWriter(stream);
                        writer.Write(textBox_string.Text);
                        writer.Flush();
                        stream.Position = 0;
                        myStream = stream;
                        label_source.Text = "from string";
                    }
                }

                if (myStream != null)
                {
                    using (myStream)
                    {
                        byte[] crc = crcCalculator.ComputeHash(myStream);
                        myStream.Close();
                        int size = crcCalculator.HashSize;
                        String zeroToBeReplaced = String.Empty;
                        int hexByteNum = 16 - (size % 4 > 0 ? size / 4 + 1 : size / 4);
                        if(hexByteNum > 0)
                        { 
                            StringBuilder sbZeroToBeReplaced = new StringBuilder(hexByteNum);
                            while (hexByteNum-- > 0)
                                sbZeroToBeReplaced.Append("0");
                            zeroToBeReplaced = sbZeroToBeReplaced.ToString();
                        }

                        String output = BitConverter.ToString(crc);
                        output = output.Replace("-","");
                        if(!String.IsNullOrEmpty(zeroToBeReplaced))
                            output = output.Replace(zeroToBeReplaced, "0x");
                        textBox_crc.Text = output;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }

        private void TextBox_filePath_TextChanged(object sender, EventArgs e)
        {
            label_source.Text = String.Empty;
            textBox_crc.Text = String.Empty;
        }

        private void textBox_string_TextChanged(object sender, EventArgs e)
        {
            label_source.Text = String.Empty;
            textBox_crc.Text = String.Empty;
        }
    }
}
