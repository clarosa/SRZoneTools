using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRZoneFinder
{
    class Program
    {
        static bool compareDistance = false;
        static float givenX;
        static float givenZ;

        class SRBinaryReader : BinaryReader
        {
            public SRBinaryReader(Stream s) : base(s) { }

            public string ReadStringZ()
            {
                string s = "";
                char c;
                while ((c = this.ReadChar()) != '\0')
                    s += c;
                return s;

            }
        }

        class FilePosition : IComparable<FilePosition>
        {
            public string Name;
            public float X, Y, Z;
            public double DistanceSquared;

            public FilePosition(string name, float x, float y, float z)
            {
                Name = name;
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
            bool success = true;
            FileStream headerFileStream = null;
            try {
                headerFileStream = new FileStream(czhFile, FileMode.Open, FileAccess.Read);
                SRBinaryReader binaryReader = new SRBinaryReader(headerFileStream);
                int signature = binaryReader.ReadInt16();
                if (signature != 0x3854)
                    throw new Exception("Incorrect v-file header signature.  Not a valid zone header file.");
                int version = binaryReader.ReadInt16();
                if (version != 4)
                    throw new Exception("Incorrect v-file header version.");
                int refDataSize = binaryReader.ReadInt32();
//              binaryReader.BaseStream.Seek(24 + refDataSize, SeekOrigin.Current);
                int refDataStart = binaryReader.ReadInt32();
                int refCount = binaryReader.ReadInt32();
                binaryReader.BaseStream.Seek(16, SeekOrigin.Current);
                long refDataOffset = binaryReader.BaseStream.Position;
                for (int i = 1; i <= refCount; i++)
                    binaryReader.ReadStringZ();
                binaryReader.BaseStream.Seek((binaryReader.BaseStream.Position | 0x000F) + 1, SeekOrigin.Begin);
                // World Zone Header
                string signature2 = new string(binaryReader.ReadChars(4));
                if (signature2 != "SR3Z")
                    throw new Exception("Incorrect world zone header signature.");
                int version2 = binaryReader.ReadInt32();
                if (version2 != 29 && version2 != 32)  // version 29 = SR3, 32 = SR4
                    throw new Exception("Incorrect world zone header version.");
                int v_file_header_ptr = binaryReader.ReadInt32();
                x = binaryReader.ReadSingle();
                y = binaryReader.ReadSingle();
                z = binaryReader.ReadSingle();
            }
            catch (Exception e)
            {
                Console.WriteLine(czhFile);
                Console.WriteLine("    " + e.Message);
                x = y = z = 0;
                success = false;
            }
            finally
            {
                if (headerFileStream != null)
                    headerFileStream.Close();
            }
            return success;
        }

        static void ShowHelp(string programName)
        {
            Console.WriteLine();
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
        }

        static void Main(string[] args)
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

            List<FilePosition> files;

            if (args.Length <= 0)
            {
                ShowHelp(programName);
                return;
            }

            string fileName = args[0];
            if (args.Length >= 3)
            {
                compareDistance = true;
                givenX = Convert.ToSingle(args[1]);
                givenZ = Convert.ToSingle(args[2]);
            }

            string[] paths = Directory.GetFiles(args[0], "*.czh_pc", SearchOption.AllDirectories);
            files = new List<FilePosition>(paths.Length);
            foreach (string path in paths)
            {
                float x, y, z;
                if (GetCoordinates(path, out x, out y, out z))
                {
                    string name = Path.GetFileName(path);
                    if (x != 0 || y != 0 || z != 0)
                        files.Add(new FilePosition(name, x, y, z));
                }
            }
            files.Sort();
            foreach (FilePosition file in files)
            {
                Console.WriteLine("{0,8:0.00} {1,8:0.00} {2,8:0.00}  {3}", file.X, file.Y, file.Z, file.Name);
            }
        }
    }
}
