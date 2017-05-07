using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SRZoneToolTests
{
    class Program
    {
        private const string srZoneTool = @"..\SRZoneTool\bin\Debug\SRZoneTool.exe";

        static int Main(string[] args)
        {
            if (args.Count() < 1)
            {
                Console.WriteLine("Tests SRZoneTool using all zone files in the given root directory and all subdirectories, recursively.");
                Console.WriteLine();
                Console.WriteLine("Usage: SRZoneToolTests directory");
                return 1;
            }
            string path = args[0];

            if (!File.Exists(srZoneTool))
            {
                Console.Error.WriteLine("Error: SRZoneTool does not exist at \"{0}\".", srZoneTool);
                return 1;
            }

            TraverseTree(path);

            return 0;
        }

        public static void TraverseTree(string root)
        {
            Stack<string> dirs = new Stack<string>(20);

            if (!Directory.Exists(root))
            {
                Console.Error.WriteLine("Error: Directory \"{0}\" does not exist.", root);
                Exit(1);
            }

            dirs.Push(root);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();

                string[] subDirs = Directory.GetDirectories(currentDir);
                string[] files = Directory.GetFiles(currentDir);

                foreach (string file in files)
                {
                    if (Regex.IsMatch(file, @"\.czn_pc$"))
                    {
                        Console.WriteLine("{0}", file);
                        if (TestFile(file, true))
                            return;
                    }
                }

                foreach (string str in subDirs)
                    dirs.Push(str);
            }
        }

        public static bool TestFile(string file, bool compareXml = false)
        {
            string czhFile = Path.ChangeExtension(file, ".czh_pc");
            string cznFile = Path.ChangeExtension(file, ".czn_pc");

            File.Copy(czhFile, "test1.czh_pc", true);
            File.Copy(cznFile, "test1.czn_pc", true);
            System(srZoneTool, "test1.czh_pc test1.czn_pc -o test2.xml");
            System(srZoneTool, "test2.xml -o test3.czh_pc");
            System(srZoneTool, "test2.xml -o test3.czn_pc");
            if (compareXml)
            {
                System(srZoneTool, "test3.czh_pc test3.czn_pc -o test4.xml");
                // System("fc.exe", "test2.xml test4.xml");
            }
            System("fc.exe", "/b test1.czh_pc test3.czh_pc");
            System("fc.exe", "/b test1.czn_pc test3.czn_pc");
            return false;
        }

        public static void System(string command, string arguments)
        {
            // Console.WriteLine(command + " " + arguments);
            Process process = new Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
                Exit(process.ExitCode);
        }

        public static void Exit(int code)
        {
            Environment.Exit(code);
        }
    }
}
