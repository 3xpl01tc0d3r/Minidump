using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;

namespace Minidump
{
    class Program
    {
        // This is for reference.
        public static class MINIDUMP_TYPE
        {
            public const int MiniDumpNormal = 0x00000000;
            public const int MiniDumpWithDataSegs = 0x00000001;
            public const int MiniDumpWithFullMemory = 0x00000002;
            public const int MiniDumpWithHandleData = 0x00000004;
            public const int MiniDumpFilterMemory = 0x00000008;
            public const int MiniDumpScanMemory = 0x00000010;
            public const int MiniDumpWithUnloadedModules = 0x00000020;
            public const int MiniDumpWithIndirectlyReferencedMemory = 0x00000040;
            public const int MiniDumpFilterModulePaths = 0x00000080;
            public const int MiniDumpWithProcessThreadData = 0x00000100;
            public const int MiniDumpWithPrivateReadWriteMemory = 0x00000200;
            public const int MiniDumpWithoutOptionalData = 0x00000400;
            public const int MiniDumpWithFullMemoryInfo = 0x00000800;
            public const int MiniDumpWithThreadInfo = 0x00001000;
            public const int MiniDumpWithCodeSegs = 0x00002000;
        }


        [DllImport("dbghelp.dll", SetLastError = true)]
        static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);

        public static void dump(IntPtr processhandle, uint processId, string processname)
        {
            try
            {
                bool status;
                string filename = processname + "_" + processId + ".dmp";

                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Write))
                {
                    status = MiniDumpWriteDump(processhandle, processId, fs.SafeFileHandle, (uint)2, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                }
                if (status)
                {
                    Console.WriteLine($"[+] {processname} process dumped successfully and saved at {Directory.GetCurrentDirectory()}\\{filename}");
                }
                else
                {
                    Console.WriteLine("Cannot Dump the process");
                    Console.WriteLine("[+] " + Marshal.GetExceptionCode());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void logo()
        {
            Console.WriteLine();
            Console.WriteLine("###################################################");
            Console.WriteLine("#  __  __ ___ _   _ ___ ____  _   _ __  __ ____   #");
            Console.WriteLine("# |  \\/  |_ _| \\ | |_ _|  _ \\| | | |  \\/  |  _ \\  #");
            Console.WriteLine("# | |\\/| || ||  \\| || || | | | | | | |\\/| | |_) | #");
            Console.WriteLine("# | |  | || || |\\  || || |_| | |_| | |  | |  __/  #");
            Console.WriteLine("# |_|  |_|___|_| \\_|___|____/ \\___/|_|  |_|_|     #");
            Console.WriteLine("#                                                 #");
            Console.WriteLine("###################################################");
            Console.WriteLine();
        }

        public static void help()
        {


            string help = @"
*****************Help*****************
[+] The program is designed to dump full memory of the process by specifing process name or process id.

[+] Dump process memory using process name
[+] Minidump.exe /pname:notepad

[+] Dump process memory using process id
[+] Minidump.exe /pid:123

";
            Console.WriteLine(help);
        }

        private static void Main(string[] args)
        {
            try
            {
                logo();
                // https://github.com/GhostPack/Rubeus/blob/master/Rubeus/Domain/ArgumentParser.cs#L10

                var arguments = new Dictionary<string, string>();
                foreach (var argument in args)
                {
                    var idx = argument.IndexOf(':');
                    if (idx > 0)
                        arguments[argument.Substring(0, idx)] = argument.Substring(idx + 1);
                    else
                        arguments[argument] = string.Empty;
                }

                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Console.WriteLine($"[+] Process running with {principal.Identity.Name} privileges with HIGH integrity.");
                }
                else
                {
                    Console.WriteLine($"[+] Process running with {principal.Identity.Name} privileges with MEDIUM / LOW integrity.");
                }

                if (arguments.Count == 0)
                {
                    Console.WriteLine("[+] No arguments specified. Please refer the help section for more details.");
                    help();
                }
                else if (arguments.ContainsKey("/pname"))
                {
                    Process[] process = Process.GetProcessesByName(arguments["/pname"]);
                    if (process.Length > 0)
                    {
                        for (int i = 0; i < process.Length; i++)
                        {
                            Console.WriteLine($"[+] Dumping {process[i].ProcessName} process");
                            Console.WriteLine($"[+] {process[i].ProcessName} process handler {process[i].Handle}");
                            Console.WriteLine($"[+] {process[i].ProcessName} process id {process[i].Id}");
                            dump(process[i].Handle, (uint)process[i].Id, process[i].ProcessName);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[+] {arguments["/pname"]} process is not running.");
                    }
                }
                else if (arguments.ContainsKey("/pid"))
                {
                    int procid = Convert.ToInt32(arguments["/pid"]);
                    Process process = Process.GetProcessById(procid);
                    Console.WriteLine($"[+] Dumping {process.ProcessName} process");
                    Console.WriteLine($"[+] {process.ProcessName} process handler {process.Handle}");
                    Console.WriteLine($"[+] {process.ProcessName} process id {process.Id}");
                    dump(process.Handle, (uint)process.Id, process.ProcessName);
                }
                else
                {
                    Console.WriteLine("[+] Invalid argument. Please refer the help section for more details.");
                    help();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}

