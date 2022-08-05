using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Crybat
{
    public enum EncryptionMode
    {
        AES,
        XOR
    }

    public class Utils
    {
        public static byte[] GetEmbeddedResource(string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            MemoryStream ms = new MemoryStream();
            Stream stream = asm.GetManifestResourceStream(name);
            stream.CopyTo(ms);
            stream.Dispose();
            byte[] ret = ms.ToArray();
            ms.Dispose();
            return ret;
        }

        public static string GetEmbeddedString(string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            StreamReader stream = new StreamReader(asm.GetManifestResourceStream(name));
            string ret = stream.ReadToEnd();
            stream.Close();
            stream.Dispose();
            return ret;
        }

        public static byte[] Encrypt(EncryptionMode type, byte[] input, byte[] key, byte[] iv)
        {
            switch (type)
            {
                case EncryptionMode.AES:
                    {
                        AesManaged aes = new AesManaged();
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        ICryptoTransform encryptor = aes.CreateEncryptor(key, iv);
                        byte[] encrypted = encryptor.TransformFinalBlock(input, 0, input.Length);
                        encryptor.Dispose();
                        aes.Dispose();
                        return encrypted;
                    }
                case EncryptionMode.XOR:
                    {
                        for (int i = 0; i < input.Length; i++)
                        {
                            input[i] = (byte)(input[i] ^ key[i % key.Length]);
                        }
                        return input;
                    }
            }
            return null;
        }

        public static byte[] Compress(byte[] bytes)
        {
            MemoryStream msi = new MemoryStream(bytes);
            MemoryStream mso = new MemoryStream();
            GZipStream gs = new GZipStream(mso, CompressionMode.Compress);
            msi.CopyTo(gs);
            gs.Dispose();
            mso.Dispose();
            msi.Dispose();
            return mso.ToArray();
        }

        public static string RandomString(int length, Random rng)
        {
            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[rng.Next(s.Length)]).ToArray());
        }

        public static bool IsAssembly(string path)
        {
            try
            {
                AssemblyName.GetAssemblyName(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
