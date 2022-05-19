using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Jlaive
{
    public class Patcher
    {
        public static byte[] Fix(byte[] input)
        {
            ModuleDef module = ModuleDefMD.Load(input);
            foreach (TypeDef type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    IList<Instruction> instr = method.Body.Instructions;
                    for (var i = 0; i < instr.Count; i++)
                    {
                        Console.WriteLine(instr[i]);
                        if (instr[i].OpCode == OpCodes.Callvirt && instr[i].ToString().Contains("System.Diagnostics.ProcessModule::get_FileName()"))
                        {
                            instr.Insert(i + 1, OpCodes.Ldnull.ToInstruction());
                            instr.Insert(i + 2, OpCodes.Call.ToInstruction(method.Module.Import(GetSystemMethod(typeof(Path), "ChangeExtension"))));
                        }
                        else if (instr[i].OpCode == OpCodes.Callvirt && instr[i].ToString().Contains("System.Reflection.Assembly::get_Location()"))
                        {
                            instr.Insert(i + 1, OpCodes.Ldnull.ToInstruction());
                            instr.Insert(i + 2, OpCodes.Call.ToInstruction(method.Module.Import(GetSystemMethod(typeof(Path), "ChangeExtension"))));
                        }
                        else if (instr[i].OpCode == OpCodes.Call && instr[i].ToString().Contains("System.Reflection.Assembly::GetEntryAssembly()"))
                        {
                            instr[i] = OpCodes.Call.ToInstruction(method.Module.Import(GetSystemMethod(typeof(Assembly), "GetExecutingAssembly")));
                        }
                    }
                    method.Body.SimplifyBranches();
                }
            }
            MemoryStream ms = new MemoryStream();
            module.Write(ms);
            byte[] output = ms.ToArray();
            ms.Dispose();
            return output;
        }

        private static MethodDef GetSystemMethod(Type type, string name)
        {
            string filename = type.Module.FullyQualifiedName;
            ModuleDefMD module = ModuleDefMD.Load(filename);
            TypeDef[] types = module.GetTypes().ToArray();
            foreach (TypeDef t in types)
            {
                if (t.Name == type.Name)
                {
                    foreach (var m in t.Methods)
                    {
                        if (m.Name == name)
                        {
                            return m;
                        }
                    }
                }
            }
            return null;
        }
    }
}
