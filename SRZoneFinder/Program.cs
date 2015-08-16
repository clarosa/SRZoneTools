using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;
using ChrisLaRosa.SaintsRow.ZoneFile;

namespace SRZoneFinder
{
    class Program
    {
        static bool compareDistance = false;
        static float givenX;
        static float givenZ;

        class FilePosition : IComparable<FilePosition>
        {
            public string File;
            public float X, Y, Z;
            public double DistanceSquared;

            public FilePosition(string file, float x, float y, float z)
            {
                File = file;
                X = x;
                Y = y;
                Z = z;
                if (compareDistance)
                    DistanceSquared = DistanceTo(x, y, z);
            }

            public static double DistanceTo(float x, float y, float z)
            {
                return Math.Pow(x - givenX, 2) + Math.Pow(z - givenZ, 2);
            }

            public int CompareTo(FilePosition other)
            {
                int d;
                if (compareDistance)
                    return DistanceSquared.CompareTo(DistanceTo(other.X, other.Y, other.Z));
                else
                    return (d = X.CompareTo(other.X)) != 0 ? d : (d = Z.CompareTo(other.Z)) != 0 ? d : Y.CompareTo(other.Y);
            }
        }

        static bool GetCoordinates(string czhFile, out float x, out float y, out float z)
        {
            Boolean success = true;
            try
            {
                SRZoneCombinedFile file = new SRZoneCombinedFile(czhFile);
                x = file.WorldZoneHeader.FileReferenceOffset.x;
                y = file.WorldZoneHeader.FileReferenceOffset.y;
                z = file.WorldZoneHeader.FileReferenceOffset.z;
            }
            catch (Exception e)
            {
                Console.WriteLine(czhFile);
                Console.WriteLine("    " + e.Message);
                x = y = z = 0;
                success = false;
            }
            return success;
        }

        static void ShowHelp(string programName, OptionSet options)
        {
            Console.WriteLine("Scans a directory and all it's subdirectories for Saints Row Zone Header files");
            Console.WriteLine("(\".czh_pc\") and builds a list of their world coordinates and file names.");
            Console.WriteLine("Then displays the list sorted either by X and Y coordinate (if no reference");
            Console.WriteLine("point is specified) or by distance from the given reference point");
            Console.WriteLine("(closest first).");
            Console.WriteLine();
            Console.WriteLine("Usage: {0} directory [X Z]", programName);
            Console.WriteLine();
            Console.WriteLine("  directory   Directory to scan for zone header files.");
            Console.WriteLine("  X           X coordinate of reference point (East/West).");
            Console.WriteLine("  Z           Z coordinate of reference point (North/South).");
            Console.WriteLine();
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }

        static int Main(string[] args)
        {
            string programName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            string svnRevision = "$Revision: 1101 $";
            Regex regex = new Regex(@"\D");
            string revision = regex.Replace(svnRevision, "");
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            Version ver = assemName.Version;
            Console.WriteLine("{0} version {1}.{2}.{3}.{4} by Quantum at saintsrowmods.com",
                              programName, ver.Major, ver.Minor, ver.Build, revision);
            Console.WriteLine();

            Boolean showHelp = false;
            Boolean showFullPath = false;
            Boolean showHeader = true;
            int maxNumberLines = 9999;

            // See http://tirania.org/blog/archive/2008/Oct-14.html
            OptionSet options = new OptionSet() {
                { "b|bare",     "Don't display column headings.", v => showHeader = v == null },
                { "f|fullpath", "Show the full path with the zone file name.", v => showFullPath = v != null },
                { "h|?|help",   "Show this message and exit.", v => showHelp = v != null },
                { "n|number=",  "Print only the first \"n\" files (closest).", v => maxNumberLines = Int32.Parse(v) }
            };

            try
            {
                List<string> extra;
                extra = options.Parse(args);
                if (showHelp || extra.Count <= 0)
                {
                    ShowHelp(programName, options);
                    return 1;
                }
                string fileName = extra[0];

                if (extra.Count >= 3)
                {
                    compareDistance = true;
                    givenX = Convert.ToSingle(extra[1]);
                    givenZ = Convert.ToSingle(extra[2]);
                }

                List<FilePosition> files;
                string[] paths = Directory.GetFiles(fileName, "*.czh_pc", SearchOption.AllDirectories);
                files = new List<FilePosition>(paths.Length);
                foreach (string path in paths)
                {
                    float x, y, z;
                    if (GetCoordinates(path, out x, out y, out z))
                    {
                        string file = showFullPath ? path : Path.GetFileName(path);
                        if (x != 0 || y != 0 || z != 0)
                            files.Add(new FilePosition(file, x, y, z));
                    }
                }
                files.Sort();
                int line = 0;
                if (showHeader)
                {
                    Console.WriteLine("    X        Y        Z     Zone Header File {0}", showFullPath ? "Path" : "Name");
                    Console.WriteLine(" -------  -------  -------  ---------------------");
                }
                foreach (FilePosition file in files)
                {
                    if (line++ >= maxNumberLines)
                        break;
                    Console.WriteLine("{0,8:0.00} {1,8:0.00} {2,8:0.00}  {3}", file.X, file.Y, file.Z, file.File);
                }
            }
            catch (OptionException e)
            {
                Console.WriteLine("ERROR:  " + e.Message);
                Console.WriteLine("        Type `" + programName + " --help' for more information.");
                return 1;
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
