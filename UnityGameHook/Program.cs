using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace UnityGameHook
{
    class Program
    {
        private const int WaitToTerminate = 10000;

        static void Main(string[] args)
        {
            // 
            Console.WriteLine("Loading settings...");

            // Hooking config
            Settings settings = new Settings("ArenaConcederSettings.xml");
            string pluginPath = "Assembly-ScriptHook.dll";
            string gameDirectory = settings.GetString("GameDirectory");
            string assemblyName = settings.GetString("AssemblyName");
            string assemblyClass = settings.GetString("AssemblyClass");
            string assemblyClassMember = settings.GetString("AssemblyClassMember");

            // 
            Console.WriteLine("Settings loaded:\n\tGameDir: " + gameDirectory + "\n\tHook Into Assembly:" + assemblyName + "\n\tHook: " + assemblyClass + "." + assemblyClassMember);

            //
            string dataPath = null;
            string exeName = null;
            string rootPath = null;
            foreach (string path in Directory.GetDirectories(gameDirectory))
            {
                if (path.EndsWith("_Data"))
                {
                    dataPath = path;
                    exeName = dataPath.Replace("_Data", ".exe");
                    rootPath = Directory.GetParent(exeName).FullName;
                    Console.WriteLine("Found data path: " + dataPath);
                    break;
                }
            }

            // Load assemblies
            string managedPath = dataPath + "/Managed/";
            string assemblyPath = managedPath + assemblyName;

            // 
            Console.WriteLine("Backing up " + assemblyName);

            // Save clean backup if it doesn't exist
            string cleanAssemblyPath = assemblyPath + ".clean";
            if (!File.Exists(cleanAssemblyPath))
                File.Copy(assemblyPath, cleanAssemblyPath);
            else
                File.Copy(cleanAssemblyPath, assemblyPath, true);


            // 
            Console.WriteLine("Loading assemblies...");

            // Load assemblies with resolver
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(managedPath);
            byte[] assemblyFile = File.ReadAllBytes(assemblyPath);
            var hookAssembly = Assembly.Load(File.ReadAllBytes("./" + pluginPath));
            var gameAssembly = AssemblyDefinition.ReadAssembly(new MemoryStream(assemblyFile), new ReaderParameters { AssemblyResolver = resolver });

            //
            Console.WriteLine("Assemblies loaded.\nInjecting hook....");

            // Get hook class
            var hookClass = hookAssembly.GetType("GameHook");

            // Add reference to script hook assembly
            string hookAssemblyName = hookAssembly.GetName().Name;
            Version hookAssemblyVersion = hookAssembly.GetName().Version;
            if (gameAssembly.MainModule.AssemblyReferences.Where(a => a.Name == hookAssemblyName).Count() == 0)
                gameAssembly.MainModule.AssemblyReferences.Add(new AssemblyNameReference(hookAssemblyName, hookAssemblyVersion));

            // Inject hook call
            TypeDefinition hookScript = gameAssembly.MainModule.GetType(assemblyClass);
            MethodDefinition hookFunc = hookScript.Methods.Where(m => m.Name == assemblyClassMember).Single();

            if (hookFunc.Body.Instructions[0].OpCode != OpCodes.Call || ((MethodReference)hookFunc.Body.Instructions[0].Operand).Name != "Hook")
            {
                ILProcessor Processor = hookFunc.Body.GetILProcessor();
                Processor.InsertBefore(hookFunc.Body.Instructions[0],
                    Processor.Create(OpCodes.Call, gameAssembly.MainModule.ImportReference(hookClass.GetMethod("Hook")))
                );
            }

            // 
            Console.WriteLine("Saving modded " + assemblyName);

            // Save assembly
            gameAssembly.Write(assemblyPath);

            // Copy hook into MTGA managed folder
            File.Copy("./" + pluginPath, Path.Combine(managedPath, pluginPath), true);

            // Copy settings for injected plugin
            File.Copy("./ArenaConcederSettings.xml", Path.Combine(managedPath, "ArenaConcederSettings.xml"), true);

            //
            Console.WriteLine("Saved. Launching MTGA...");
            


            // Launch game
            var process = Process.Start(exeName);
            //process.WaitForExit();

            // Wait an arbitrary amount of time before reverting the assembly back to original
            Console.WriteLine("Waiting " + (WaitToTerminate/1000f) + " seconds to revert and exit.");
            System.Threading.Thread.Sleep(10000);

            // 
            Console.WriteLine("Reverting " + assemblyName + " and exiting.");

            // Restore original assembly
            File.WriteAllBytes(assemblyPath, assemblyFile);

        }
    }
}
