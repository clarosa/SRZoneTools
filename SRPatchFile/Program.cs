using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mono.Options;

namespace SRPatchFile
{
    class Program
    {
        static void ShowHelp(string programName, OptionSet options)
        {
            Console.WriteLine("Writes one or more bytes to specific locations in an existing file.");
            Console.WriteLine();
            Console.WriteLine("Usage: " + programName + " filename [pos:][xx[xx[...]]] [pos:][xx[xx[...]]] ...");
            Console.WriteLine();
            Console.WriteLine("  filename  Name of the file to patch.  This file will be directly modified.");
            Console.WriteLine("  pos       Position in the file where the bytes will be written (hexadecimal).");
            Console.WriteLine("  xx        2-digit byte value to be written (hexadecimal).");
            Console.WriteLine("            Multiple byte values will be written to consecutive locations.");
        }

        static int Main(string[] args)
        {
            string programName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            string svnRevision = "$Revision: 1246 $";
            Regex regex = new Regex(@"\D");
            string revision = regex.Replace(svnRevision, "");
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            Version ver = assemName.Version;
            Console.WriteLine("{0} version {1}.{2}.{3}.{4} by Quantum at saintsrowmods.com",
                              programName, ver.Major, ver.Minor, ver.Build, revision);
            Console.WriteLine();

            string fileName = null;
            Boolean showHelp = false;

            // See http://tirania.org/blog/archive/2008/Oct-14.html
            OptionSet options = new OptionSet() {
                { "h|help", "show this message and exit", v => showHelp = v != null }
            };

            List<string> extra;

            try
            {
                extra = options.Parse(args);
                if (showHelp || extra.Count <= 0)
                {
                    ShowHelp(programName, options);
                    return 1;
                }
                fileName = extra[0];
            }
            catch (OptionException e)
            {
                Console.WriteLine("ERROR:  " + e.Message);
                Console.WriteLine("        Type `" + programName + " --help' for more information.");
                return 1;
            }

            try
            {
                BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Open, FileAccess.Write));
                UInt64 position = 0;

                for (int i = 1; i < extra.Count; i++)
                {
                    String pattern = @"^(([0-9A-Fa-f]+):)?([0-9A-Fa-f]*)$";
                    Match m = Regex.Match(extra[i], pattern);
                    if (!m.Success)
                        throw new System.ArgumentException("Syntax error: " + extra[i]);
                    if (m.Groups[2].Value.Length > 0)
                        position = Convert.ToUInt64(m.Groups[2].Value, 16);
                    string value = m.Groups[3].Value;
                    for (int j = 0; j < value.Length; j += 2)
                    {
                        Byte b = (Byte)Convert.ToUInt32(value.Substring(j, 2), 16);
                        Console.WriteLine("{0:X8}: {1:X2}", position, b);
                        writer.Seek((int)position, SeekOrigin.Begin);
                        writer.Write(b);
                        position++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR:  " + e.Message);
                return 1;
            }

            return 0;
        }
    }
}
