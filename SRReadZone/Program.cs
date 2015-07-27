//
// Copyright (C) 2015 Christopher LaRosa
//
// Based on Zone File Format info and discussion here:
// https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/
//

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Mono.Options;

namespace SRReadZone
{
    class SRBinaryReader : BinaryReader
    {
        public SRBinaryReader(Stream s) : base(s) {}

        public string ReadStringZ()
        {
            string s = "";
            char c;
            while ((c = this.ReadChar()) != '\0')
                s += c;
            return s;

        }
    }

    class SRZoneHeader
    {
        private bool normalizeUnits = false;  // Normalize mesh file reference coordinates

        private readonly string[] worldZoneTypeNames =
        {
            "Unknown",
            "Global Always Loaded",
            "Streaming",
            "Streaming Always Loaded",
            "Test Level",
            "Mission",
            "Activity",
            "Interior",
            "Interior Always Loaded",
            "Test Level Always Loaded",
            "Mission Always Loaded",
            "High LOD",
            "Num World Zone Types"
        };

        public SRZoneHeader(string czhFile, bool normalize)
        {
            normalizeUnits = normalize;
            ReadFile(czhFile);
        }

        // Uncompress a 16-bit mesh file reference position offset
        // https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/#post-78567
        public float MeshPosToFloat(Int16 pos)
        {
            return (float)pos / (float)(1 << 6);
        }

        // Uncompress a 16-bit mesh file reference orientation
        // https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/#post-78567
        public float MeshOrientToFloat(Int16 pos)
        {
            return (float)pos / (float)(1 << 12);
        }

        public void ReadFile(string czhFile)
        {
            FileStream headerFileStream = null;
            try
            {
                Console.WriteLine("");
                headerFileStream = new FileStream(czhFile, FileMode.Open, FileAccess.Read);
                SRBinaryReader binaryReader = new SRBinaryReader(headerFileStream);
                Console.WriteLine("V-FILE HEADER:");
                int signature = binaryReader.ReadInt16();
                Console.WriteLine("  V-File Signature:      0x{0:X4}", signature);
                if (signature != 0x3854)
                    throw new Exception("Incorrect signature.  Not a valid zone header file.");
                int version = binaryReader.ReadInt16();
                Console.WriteLine("  V-File Version:        {0}", version);
                if (version != 4)
                    throw new Exception("Incorrect version.");
                int refDataSize = binaryReader.ReadInt32();
                Console.WriteLine("  Reference Data Size:   {0}", refDataSize);
                int refDataStart = binaryReader.ReadInt32();
                Console.WriteLine("  Reference Data Start:  0x{0:X8}", refDataStart);
                int refCount = binaryReader.ReadInt32();
                Console.WriteLine("  Reference Count:       {0}", refCount);
                binaryReader.BaseStream.Seek(16, SeekOrigin.Current);
                long refDataOffset = binaryReader.BaseStream.Position;
                Console.WriteLine("");
                Console.WriteLine("  REFERENCE DATA:");
                for (int i = 1; i <= refCount; i++)
                {
                    string name = binaryReader.ReadStringZ();
                    Console.WriteLine("   {0,3}. {1}", i, name);
                }
                Console.WriteLine("");
                binaryReader.BaseStream.Seek((binaryReader.BaseStream.Position | 0x000F) + 1, SeekOrigin.Begin);

                Console.WriteLine("WORLD ZONE HEADER:");
                string signature2 = new string(binaryReader.ReadChars(4));
                Console.WriteLine("  World Zone Signature:   " + signature2);
                if (signature2 != "SR3Z")
                    throw new Exception("Incorrect signature.");
                int version2 = binaryReader.ReadInt32();
                Console.WriteLine("  World Zone Version:     {0}", version2);
                if (version2 != 29)
                    throw new Exception("Incorrect version.");
                int v_file_header_ptr = binaryReader.ReadInt32();
                Console.WriteLine("  V-File Header Pointer:  0x{0:X8}", v_file_header_ptr);
                float x = binaryReader.ReadSingle();
                float y = binaryReader.ReadSingle();
                float z = binaryReader.ReadSingle();
                Console.WriteLine("  File Reference Offset:  {0}, {1}, {2}", x, y, z);
                int wz_file_reference = binaryReader.ReadInt32();
                Console.WriteLine("  WZ File Reference Ptr:  0x{0:X8}", wz_file_reference);
                int num_file_references = binaryReader.ReadInt16();
                Console.WriteLine("  Number of File Refs:    {0}", num_file_references);
                int zone_type = binaryReader.ReadByte();
                string typeName = (zone_type < worldZoneTypeNames.Length) ? worldZoneTypeNames[zone_type] : "unknown";
                Console.WriteLine("  Zone Type:              {0} ({1})", zone_type, typeName);
                int unused = binaryReader.ReadByte();
                Console.WriteLine("  Unused:                 {0}", unused);
                int interior_trigger_ptr = binaryReader.ReadInt32();
                Console.WriteLine("  Interior Trigger Ptr:   0x{0:X8}  (run-time)", interior_trigger_ptr);
                int number_of_triggers  = binaryReader.ReadInt16();
                Console.WriteLine("  Number of Triggers:     {0,-10}  (run-time)", number_of_triggers);
                int extra_objects = binaryReader.ReadInt16();
                Console.WriteLine("  Extra Objects:          {0}", extra_objects);
                binaryReader.BaseStream.Seek(24, SeekOrigin.Current);

                Console.WriteLine("");
                Console.WriteLine("  MESH FILE REFERENCES" + (normalizeUnits ? " (STANDARD COORDINATE UNITS):" : " (RAW COMPRESSED VALUES):"));
                Console.WriteLine("             X       Y       Z   Pitch    Bank  Heading  File Name");
                Console.WriteLine("        ------  ------  ------  ------  ------  -------  ---------");
                // binaryReader.BaseStream.Seek((binaryReader.BaseStream.Position | 0x000F) + 1, SeekOrigin.Begin);
                for (int i = 1; i <= num_file_references; i++)
                {
                    Int16 m_pos_x = binaryReader.ReadInt16();
                    Int16 m_pos_y = binaryReader.ReadInt16();
                    Int16 m_pos_z = binaryReader.ReadInt16();
                    Int16 pitch = binaryReader.ReadInt16();
                    Int16 bank = binaryReader.ReadInt16();
                    Int16 heading = binaryReader.ReadInt16();
                    int m_str_offset = binaryReader.ReadInt16();

                    long savePosition = binaryReader.BaseStream.Position;
                    binaryReader.BaseStream.Seek(refDataOffset + m_str_offset, SeekOrigin.Begin);
                    string name = binaryReader.ReadStringZ();
                    binaryReader.BaseStream.Seek(savePosition, SeekOrigin.Begin);                 
                    
                    if (normalizeUnits)
                    {
                        float f_pos_x = MeshPosToFloat(m_pos_x);
                        float f_pos_y = MeshPosToFloat(m_pos_y);
                        float f_pos_z = MeshPosToFloat(m_pos_z);
                        float f_pitch = MeshOrientToFloat(pitch);
                        float f_bank = MeshOrientToFloat(bank);
                        float f_heading = MeshOrientToFloat(heading);
                        Console.WriteLine(" {0,4}. {1,7:0.00} {2,7:0.00} {3,7:0.00} {4,7:0.00} {5,7:0.00} {6,8:0.00}  {8}", i, f_pos_x, f_pos_y, f_pos_z, f_pitch, f_bank, f_heading, m_str_offset, name);
                    }
                    else
                    {
                        Console.WriteLine(" {0,4}.{1,8}{2,8}{3,8}{4,8}{5,8} {6,8}  {8}", i, m_pos_x, m_pos_y, m_pos_z, pitch, bank, heading, m_str_offset, name);
                    }
                }
            }
            finally
            {
                if (headerFileStream != null)
                    headerFileStream.Close();
            }
        }
    }

    class SRZoneFile
    {
        private readonly Dictionary<UInt32, string> sectionTypes = new Dictionary<UInt32, string>()
        {
            { 0x2233, "crunched reference geometry - transforms and things for level meshes" },
            { 0x2234, "objects - nav points, environmental effects, and many, many more things" },
            { 0x2235, "navmesh" },
            { 0x2236, "traffic data" },
            { 0x2237, "world editor generated geometry - things directly made from the editor like terrain" },
            { 0x2238, "sidewalk data" },
            { 0x2239, "section trailer (??)" },
            { 0x2240, "light clip meshes" },
            { 0x2241, "traffic signal data" },
            { 0x2242, "mover constraint data" },
            { 0x2243, "zone triggers(interiors, missions)" },
            { 0x2244, "heightmap" },
            { 0x2245, "cobject rbb tree - cobjects are things that are not a full on object like tables and chairs" },
            { 0x2246, "undergrowth - foliage" },
            { 0x2247, "water volumes" },
            { 0x2248, "wave killers" },
            { 0x2249, "water surfaces" },
            { 0x2250, "parking data" },
            { 0x2251, "rain killers" },
            { 0x2252, "level mesh supplemental lod data" },
            { 0x2253, "cobject grid data - object fading" },
            { 0x2254, "ae rbb (??)" },
            { 0x2255, "havok pathfinding data(SR4 only?)" }
        };
        private readonly string[] propertyTypeNames =
        {
            "string",
            "data",
            "compressed transform",
            "compressed transform with quaternion orientation"
        };

        public SRZoneFile(string cznFile)
        {
            this.ReadFile(cznFile);
        }

        public void ReadFile(string cznFile)
        {
            FileStream headerFileStream = null;
            try
            {
                headerFileStream = new FileStream(cznFile, FileMode.Open, FileAccess.Read);
                SRBinaryReader binaryReader = new SRBinaryReader(headerFileStream);
                int section = 1;
                while (binaryReader.BaseStream.Position <= binaryReader.BaseStream.Length - 4)
                {
                    Console.WriteLine("");
                    Console.WriteLine("SECTION #{0}:  [file offset 0x{1:X8}]", section++, binaryReader.BaseStream.Position);
                    UInt32 section_id = binaryReader.ReadUInt32();
                    Console.WriteLine("  Section ID:   0x{0:X8}", section_id);
                    UInt32 id = (section_id & 0x7FFFFFFF);
                    if (id < 0x2233 || id >= 0x2300)
                        throw new Exception("Invalid section ID.  Not a valid zone file.");
                    if (sectionTypes.ContainsKey(id))
                        Console.WriteLine("  Description:  " + sectionTypes[id]);
                    UInt32 cpu_size = binaryReader.ReadUInt32();
                    Console.WriteLine("  CPU Size:     {0} bytes", cpu_size);
                    UInt32 gpu_size = 0;
                    if ((section_id & 0x80000000) != 0)
                    {
                        gpu_size = binaryReader.ReadUInt32();
                        Console.WriteLine("  GPU Size:     {0} bytes", gpu_size);
                    }
                    long sectionDataStart = binaryReader.BaseStream.Position;
                    if (cpu_size == 0 && gpu_size == 0)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("  EMPTY SECTION = EOF");
                        break;
                    }

                    if (binaryReader.ReadInt32() == 0x574F4246)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("  OBJECT SECTION HEADER:");
                        Console.WriteLine("    Header Signature:     0x574F4246");
                        int version = binaryReader.ReadInt32();
                        Console.WriteLine("    Version:              {0}", version);
                        int num_objects = binaryReader.ReadInt32();
                        Console.WriteLine("    Number of Objects:    {0}", num_objects);
                        int num_handles = binaryReader.ReadInt32();
                        Console.WriteLine("    Number of Handles:    {0}", num_handles);
                        int flags = binaryReader.ReadInt32();
                        Console.WriteLine("    Flags:                0x{0:X8}", flags);
                        int handle_list_ptr = binaryReader.ReadInt32();
                        Console.WriteLine("    Handle List Pointer:  0x{0:X8}  (run-time)", handle_list_ptr);
                        int object_data_ptr = binaryReader.ReadInt32();
                        Console.WriteLine("    Object Data Pointer:  0x{0:X8}  (run-time)", object_data_ptr);
                        int object_data_size = binaryReader.ReadInt32();
                        Console.WriteLine("    Object Data Size:     {0,-10}  (run-time)", object_data_size);
                        Console.WriteLine("");
                        Console.WriteLine("    HANDLE LIST:");
                        for (int i = 1; i <= num_handles; i++)
                        {
                            UInt64 handle = binaryReader.ReadUInt64();
                            // if (i > 10 && i < num_handles - 10) continue;
                            Console.WriteLine("     {0,3}. 0x{1:X16}", i, handle);
                        }

                        int block = 1;
                        while (block <= num_handles && binaryReader.BaseStream.Position < sectionDataStart + cpu_size)
                        {
                            // Align on a dword boundry
                            while ((binaryReader.BaseStream.Position & 0x00000003) != 0)
                                binaryReader.ReadByte();
                            if (binaryReader.BaseStream.Position >= sectionDataStart + cpu_size)
                                break;
                            Console.WriteLine("");
                            Console.WriteLine("    OBJECT #{0}:  [file offset 0x{1:X8}]", block++, binaryReader.BaseStream.Position);
                            UInt64 handle_offset = binaryReader.ReadUInt64();
                            Console.WriteLine("      Handle Offset:         0x{0:X16}", handle_offset);
                            UInt64 parent_handle_offset = binaryReader.ReadUInt64();
                            Console.WriteLine("      Parent Handle Offset:  0x{0:X16}", parent_handle_offset);
                            int object_type_hash = binaryReader.ReadInt32();
                            Console.WriteLine("      Object Type Hash:      0x{0:X8}", object_type_hash);
                            int number_of_properties = binaryReader.ReadInt16();
                            Console.WriteLine("      Number of Properties:  {0}", number_of_properties);
                            int buffer_size = binaryReader.ReadInt16();
                            Console.WriteLine("      Buffer Size:           {0}", buffer_size);
                            UInt16 name_offset = binaryReader.ReadUInt16();
                            Console.WriteLine("      Name Offset:           {0}", name_offset);
                            int padding = binaryReader.ReadInt16();
                            Console.WriteLine("      Padding:               {0}", padding);
                            long savePosition = binaryReader.BaseStream.Position;
                            binaryReader.BaseStream.Seek(name_offset, SeekOrigin.Current);
                            string objectName = binaryReader.ReadStringZ();
                            Console.WriteLine("      Object Name:           " + objectName);
                            binaryReader.BaseStream.Seek(savePosition, SeekOrigin.Begin);
                            for (int i = 1; i <= number_of_properties; i++)
                            {
                                // Align on a dword boundry
                                while ((binaryReader.BaseStream.Position & 0x00000003) != 0)
                                    binaryReader.ReadByte();
                                Console.WriteLine("");
                                Console.WriteLine("      PROPERTY #{0}:  [file offset 0x{1:X8}]", i, binaryReader.BaseStream.Position);
                                uint type = binaryReader.ReadUInt16();
                                string typeName = (type < propertyTypeNames.Length) ? propertyTypeNames[type] : "unknown";
                                Console.WriteLine("        Type:      {0} ({1})", type, typeName);
                                int size = binaryReader.ReadInt16();
                                Console.WriteLine("        Size:      {0} bytes", size);
                                int name_crc = binaryReader.ReadInt32();
                                Console.WriteLine("        Name CRC:  0x{0:X8} ({0})", name_crc, name_crc);
                                long dataBegin = binaryReader.BaseStream.Position;
                                switch (type)
                                {
                                    case 0:
                                        Console.WriteLine("        Value:     \"" + binaryReader.ReadStringZ() + "\"");
                                        break;
                                    case 2:
                                        Console.WriteLine("        Value:     Position (x,y,z):  {0}, {1}, {2}", binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                                        break;
                                    case 3:
                                        Console.WriteLine("        Value:     Position (x,y,z):       {0}, {1}, {2}", binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                                        Console.WriteLine("                   Orientation (x,y,z,w):  {0}, {1}, {2}, {3}", binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                                        break;
                                    default:
                                        if (size > 16)
                                        {
                                            Console.WriteLine("        Value:     ??? [file offset 0x{0:X8}]", binaryReader.BaseStream.Position);
                                        }
                                        else
                                        {
                                            byte[] buffer = new byte[size];
                                            binaryReader.Read(buffer, 0, size);
                                            string hexBuffer = "";
                                            for (int j = 0; j < size; j++)
                                            {
                                                hexBuffer += buffer[j].ToString("X2") + ' ';
                                            }
                                            Console.WriteLine("        Value:     {1}", i, hexBuffer);
                                        }
                                        break;
                                }
                                binaryReader.BaseStream.Seek(dataBegin + size, SeekOrigin.Begin);
                            }
                        }
                    }

                    binaryReader.BaseStream.Seek(sectionDataStart + cpu_size, SeekOrigin.Begin);
                    // Align on a dword boundry
                    while ((binaryReader.BaseStream.Position & 0x00000003) != 0)
                        binaryReader.ReadByte();
                }
            }
            finally
            {
                if (headerFileStream != null)
                    headerFileStream.Close();
            }
        }
    }

    class Program
    {
        static void ShowHelp(string programName, OptionSet options)
        {
            Console.WriteLine("Parses and displays the contents of a Saints Row zone file.");
            Console.WriteLine("Supports Saints Row: The Third, Saints Row 4, and Saints Row: Gat Out Of Hell.");
            Console.WriteLine("This actually parses 2 files:");
            Console.WriteLine("  1.  CPU Zone Header file (\"filename.czh_pc\")");
            Console.WriteLine("  2.  CPU Zone [Data] file (\"filename.czn_pc\")");
            Console.WriteLine("");
            Console.WriteLine("Usage: " + programName + " [OPTIONS]+ filename");
            Console.WriteLine("");
            Console.WriteLine("  filename    Name of the \".czh_pc\" or \".czn_pc\" file to display.");
            Console.WriteLine("              The file extension will be ignored and replaced by");
            Console.WriteLine("              \".czh_pc\" and \".czn_pc\".");
            Console.WriteLine();
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }

        static int Main(string[] args)
        {
            string programName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            string svnRevision = "$Revision: 1075 $";
            Regex regex = new Regex(@"\D");
            string revision = regex.Replace(svnRevision, "");
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            Version ver = assemName.Version;
            Console.WriteLine("{0} version {1}.{2}.{3}.{4} by Quantum at saintsrowmods.com",
                              programName, ver.Major, ver.Minor, ver.Build, revision);
            Console.WriteLine();

            string inputFile = null; 
            Boolean normalizeUnits = false;
            Boolean showHelp = false;

            // See http://tirania.org/blog/archive/2008/Oct-14.html
            OptionSet options = new OptionSet() {
                { "n|normalize", "convert raw compressed mesh coordinate values to standard floating-point game coordinate units",  v => normalizeUnits = v != null },
                { "h|help",  "show this message and exit", v => showHelp = v != null }
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
                inputFile = extra[0];
            }
            catch (OptionException e)
            {
                Console.WriteLine("ERROR:  " + e.Message);
                Console.WriteLine("        Type `" + programName + " --help' for more information.");
                return 1;
            }

            string czhFile = Path.ChangeExtension(inputFile, ".czh_pc");
            string cznFile = Path.ChangeExtension(inputFile, ".czn_pc");

            try
            {
                Console.WriteLine("-------------------------------------------------------------------------------");
                Console.WriteLine("ZONE HEADER FILE:  " + czhFile);
                if (File.Exists(czhFile))
                {
                    SRZoneHeader sr3ZoneHeader = new SRZoneHeader(czhFile, normalizeUnits);
                }
                else {
                    Console.WriteLine("Can't open zone header file \"" + czhFile + "\" -- skipped.");
                }

                Console.WriteLine("");
                Console.WriteLine("-------------------------------------------------------------------------------");
                Console.WriteLine("ZONE DATA FILE:  " + cznFile);
                if (File.Exists(cznFile))
                {
                    SRZoneFile sr3ZoneFile = new SRZoneFile(cznFile);
                }
                else
                {
                    Console.WriteLine("Can't open zone data file \"" + cznFile + "\" -- skipped.");
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
