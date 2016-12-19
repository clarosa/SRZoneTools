//
// Copyright (C) 2015-2016 Christopher LaRosa
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
using ChrisLaRosa.SaintsRow.ZoneFile;

namespace SRReadZone
{
    class SRBinaryReader : BinaryReader
    {
        public static bool showPosition = false;
        public static bool showType = false;

        private long lastPosition = -1;

        public SRBinaryReader(Stream s) : base(s) {}

        public string ReadStringZ()
        {
            string s = "";
            char c;
            while ((c = this.ReadChar()) != '\0')
                s += c;
            return s;
        }

        public void SetDisplayPosition()
        {
            lastPosition = this.BaseStream.Position;
        }

        private void WriteString(string str, string type = "")
        {
            if (lastPosition >= 0)
            {
                if (showPosition)
                    Console.Write("{0:X8}  ", lastPosition);
                if (showType)
                    Console.Write("{0,-9}  ", type);
            }
            Console.WriteLine(str);
            lastPosition = this.BaseStream.Position;
        }

        public void WriteLine(string format, params object[] list)
        {
            WriteString(String.Format(format, list));
        }

        public void WriteLine1(string format, params object[] list)
        {
            WriteString(String.Format(format, list), list[0].GetType().Name);
        }

        public void WriteLineT(string type, string format, params object[] list)
        {
            WriteString(String.Format(format, list), type);
        }

        public void WriteLineX(string format, params object[] list)
        {
            if (lastPosition >= 0)
            {
                if (showPosition)
                    Console.Write(">>>>>>>>  ");
                if (showType)
                    Console.Write("           ");
            }
            Console.WriteLine(format, list);
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

        public static Dictionary<UInt32, string> ReferenceData = null;

        public SRZoneHeader(string czhFile, bool normalize)
        {
            ReferenceData = new Dictionary<UInt32, string>();
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
                headerFileStream = new FileStream(czhFile, FileMode.Open, FileAccess.Read);
                SRBinaryReader binaryReader = new SRBinaryReader(headerFileStream);
                binaryReader.WriteLine("");
                binaryReader.WriteLine("V-FILE HEADER:");
                Int16 signature = binaryReader.ReadInt16();
                binaryReader.WriteLine1("  V-File Signature:      0x{0:X4}", signature);
                if (signature != 0x3854)
                    throw new Exception("Incorrect signature.  Not a valid zone header file.");
                Int16 version = binaryReader.ReadInt16();
                binaryReader.WriteLine1("  V-File Version:        {0}", version);
                if (version != 4)
                    throw new Exception("Incorrect version.");
                Int32 refDataSize = binaryReader.ReadInt32();
                binaryReader.WriteLine1("  Reference Data Size:   {0}", refDataSize);
                Int32 refDataStart = binaryReader.ReadInt32();
                binaryReader.WriteLine1("  Reference Data Start:  0x{0:X8}", refDataStart);
                Int32 refCount = binaryReader.ReadInt32();
                binaryReader.WriteLine1("  Reference Count:       {0}", refCount);
                binaryReader.BaseStream.Seek(16, SeekOrigin.Current);
                long refDataOffset = binaryReader.BaseStream.Position;
                binaryReader.WriteLine("");
                binaryReader.WriteLine("  REFERENCE DATA:");
                for (int i = 1; i <= refCount; i++)
                {
                    uint offset = (uint)(binaryReader.BaseStream.Position - refDataOffset);
                    string name = binaryReader.ReadStringZ();
                    ReferenceData[offset] = name;
                    binaryReader.WriteLineT("StringZ", "   {0,3}. {1}", i, name);
                }

                binaryReader.BaseStream.Seek((binaryReader.BaseStream.Position | 0x000F) + 1, SeekOrigin.Begin);
                binaryReader.WriteLine("");
                binaryReader.WriteLine("WORLD ZONE HEADER:");
                string signature2 = new string(binaryReader.ReadChars(4));
                binaryReader.WriteLineT("Byte[4]", "  World Zone Signature:   " + signature2);
                if (signature2 != "SR3Z")
                    throw new Exception("Incorrect signature.");
                Int32 version2 = binaryReader.ReadInt32();
                binaryReader.WriteLine1("  World Zone Version:     {0}", version2);
                if (version2 != 29 && version2 != 32)  // version 29 = SR3, 32 = SR4
                    throw new Exception("Incorrect version.");
                Int32 v_file_header_ptr = binaryReader.ReadInt32();
                binaryReader.WriteLine1("  V-File Header Pointer:  0x{0:X8}", v_file_header_ptr);
                float x = binaryReader.ReadSingle();
                float y = binaryReader.ReadSingle();
                float z = binaryReader.ReadSingle();
                binaryReader.WriteLineT("float[3]", "  File Reference Offset:  {0}, {1}, {2}", x, y, z);
                Int32 wz_file_reference = binaryReader.ReadInt32();
                binaryReader.WriteLine1("  WZ File Reference Ptr:  0x{0:X8}", wz_file_reference);
                Int16 num_file_references = binaryReader.ReadInt16();
                binaryReader.WriteLine1("  Number of File Refs:    {0}", num_file_references);
                Byte zone_type = binaryReader.ReadByte();
                string typeName = (zone_type < worldZoneTypeNames.Length) ? worldZoneTypeNames[zone_type] : "unknown";
                binaryReader.WriteLine1("  Zone Type:              {0} ({1})", zone_type, typeName);
                Byte unused = binaryReader.ReadByte();
                binaryReader.WriteLine1("  Unused:                 {0}", unused);
                Int32 interior_trigger_ptr = binaryReader.ReadInt32();
                binaryReader.WriteLine1("  Interior Trigger Ptr:   0x{0:X8}  (run-time)", interior_trigger_ptr);
                Int16 number_of_triggers  = binaryReader.ReadInt16();
                binaryReader.WriteLine1("  Number of Triggers:     {0,-10}  (run-time)", number_of_triggers);
                Int16 extra_objects = binaryReader.ReadInt16();
                binaryReader.WriteLine1("  Extra Objects:          {0}", extra_objects);
                binaryReader.BaseStream.Seek(24, SeekOrigin.Current);

                binaryReader.WriteLine("");
                binaryReader.WriteLine("  MESH FILE REFERENCES" + (normalizeUnits ? " (STANDARD COORDINATE UNITS):" : " (RAW COMPRESSED VALUES):"));
                binaryReader.WriteLine("             X       Y       Z   Pitch    Bank  Heading  File Name");
                binaryReader.WriteLine("        ------  ------  ------  ------  ------  -------  ---------");
                // binaryReader.BaseStream.Seek((binaryReader.BaseStream.Position | 0x000F) + 1, SeekOrigin.Begin);
                for (int i = 1; i <= num_file_references; i++)
                {
                    Int16 m_pos_x = binaryReader.ReadInt16();
                    Int16 m_pos_y = binaryReader.ReadInt16();
                    Int16 m_pos_z = binaryReader.ReadInt16();
                    Int16 pitch = binaryReader.ReadInt16();
                    Int16 bank = binaryReader.ReadInt16();
                    Int16 heading = binaryReader.ReadInt16();
                    uint m_str_offset = binaryReader.ReadUInt16();
                    string name = ReferenceData.ContainsKey(m_str_offset) ? ReferenceData[m_str_offset] : "<invalid>";
                    
                    if (normalizeUnits)
                    {
                        float f_pos_x = MeshPosToFloat(m_pos_x);
                        float f_pos_y = MeshPosToFloat(m_pos_y);
                        float f_pos_z = MeshPosToFloat(m_pos_z);
                        float f_pitch = MeshOrientToFloat(pitch);
                        float f_bank = MeshOrientToFloat(bank);
                        float f_heading = MeshOrientToFloat(heading);
                        binaryReader.WriteLineT("Int16[6]", " {0,4}. {1,7:0.00} {2,7:0.00} {3,7:0.00} {4,7:0.00} {5,7:0.00} {6,8:0.00}  {8}", i, f_pos_x, f_pos_y, f_pos_z, f_pitch, f_bank, f_heading, m_str_offset, name);
                    }
                    else
                    {
                        binaryReader.WriteLineT("Int16[6]", " {0,4}.{1,8}{2,8}{3,8}{4,8}{5,8} {6,8}  {8}", i, m_pos_x, m_pos_y, m_pos_z, pitch, bank, heading, m_str_offset, name);
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

        public void ReadGeometrySection(SRBinaryReader binaryReader, UInt32 cpu_size, long sectionDataStart)
        {
            Dictionary<UInt32, string> meshNamesList = new Dictionary<UInt32, string>();

            binaryReader.WriteLine("");
            binaryReader.WriteLine("  CRUNCHED REFERENCE GEOMETRY:");
            Int32 num_meshes = binaryReader.ReadInt32();
            binaryReader.WriteLine1("    Number of Meshes:     {0}", num_meshes);
            Int32 mesh_names_size = binaryReader.ReadInt32();
            binaryReader.WriteLine1("    Mesh Names Size:      {0}", mesh_names_size);

            // binaryReader.BaseStream.Seek(16, SeekOrigin.Current);
            long refDataOffset = binaryReader.BaseStream.Position;
            long refDataEnd = refDataOffset + mesh_names_size;
            binaryReader.WriteLine("");
            binaryReader.WriteLine("    MESH NAMES LIST:");
            for (int i = 1; binaryReader.BaseStream.Position < refDataEnd; i++)
            {
                long position = binaryReader.BaseStream.Position;
                string name = binaryReader.ReadStringZ();
                binaryReader.ReadByte();
                meshNamesList[(UInt32)(position - refDataOffset)] = name;
                binaryReader.WriteLineT("StringZ", "    {0,4}.  {1}", i, name);
                // Align on a word boundry
                while ((binaryReader.BaseStream.Position & 0x00000001) != 0)
                    binaryReader.ReadByte();
            }
            if (binaryReader.BaseStream.Position != refDataEnd)
                throw new Exception("Mesh Names List not expected size.");
            // Align on a 16-byte boundry
            while ((binaryReader.BaseStream.Position & 0x0000000F) != 0)
                binaryReader.ReadByte();
            binaryReader.WriteLine("");
            binaryReader.WriteLine("    MESHES LIST:");
            binaryReader.WriteLine("           *r_mesh  *material_map  File Name (from *r_mesh)");
            binaryReader.WriteLine("           -------  -------------  ------------------------");
            for (int i = 1; i <= num_meshes; i++)
            {
                uint r_mesh_offset = binaryReader.ReadUInt32();
                uint material_map_offset = binaryReader.ReadUInt32();
                string name = (SRZoneHeader.ReferenceData != null && SRZoneHeader.ReferenceData.ContainsKey(r_mesh_offset)) ?
                               SRZoneHeader.ReferenceData[r_mesh_offset] : "<invalid>";
                binaryReader.WriteLineT("UInt32[2]", "    {0,4}. {1,6}       {2,6}      {3}", i, r_mesh_offset, material_map_offset, name);
            }
            // Align on a 16 byte boundry
            while ((binaryReader.BaseStream.Position & 0x0000000F) != 0)
                binaryReader.ReadByte();
            binaryReader.WriteLine("");
            binaryReader.WriteLine("    FAST OBJECTS LIST:");
            for (int i = 1; i <= num_meshes; i++)
            {
                long start = binaryReader.BaseStream.Position;
                binaryReader.WriteLine("");
                binaryReader.WriteLine("      FAST OBJECT #{0}:", i);
                binaryReader.WriteLine1("        Object Handle:          0x{0:X16}", binaryReader.ReadUInt64());
                binaryReader.WriteLine1("        m_render_update_next:   0x{0:X8}", binaryReader.ReadUInt32());
                UInt32 nameOffset = binaryReader.ReadUInt32();
                binaryReader.WriteLine1("        name_offset:            0x{0:X8}", nameOffset);
                string name = meshNamesList.ContainsKey(nameOffset) ? meshNamesList[nameOffset] : "<invalid>";
                binaryReader.WriteLineX("        Name (from offset):     {0}", name);
                binaryReader.WriteLineT("float[3]", "        Position (x,y,z):       {0}, {1}, {2}", binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                binaryReader.WriteLineT("float[4]", "        Orientation (x,y,z,w):  {0}, {1}, {2}, {3}", binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                binaryReader.WriteLine("        etc.");
                binaryReader.BaseStream.Seek(start + 112, SeekOrigin.Begin);
            }
            binaryReader.WriteLine("");
            binaryReader.WriteLine("    MESH VARIANT DATA:");
            binaryReader.WriteLine("      ???");
        }

        public void ReadObjectSection(SRBinaryReader binaryReader, UInt32 cpu_size, long sectionDataStart)
        {
            binaryReader.WriteLine("");
            binaryReader.WriteLine("  OBJECT SECTION HEADER:");
            Int32 signature = binaryReader.ReadInt32();
            binaryReader.WriteLine1("    Header Signature:     0x{0:X8}", signature);
            Int32 version = binaryReader.ReadInt32();
            binaryReader.WriteLine1("    Version:              {0}", version);
            Int32 num_objects = binaryReader.ReadInt32();
            binaryReader.WriteLine1("    Number of Objects:    {0}", num_objects);
            Int32 num_handles = binaryReader.ReadInt32();
            binaryReader.WriteLine1("    Number of Handles:    {0}", num_handles);
            Int32 flags = binaryReader.ReadInt32();
            binaryReader.WriteLine1("    Flags:                0x{0:X8}", flags);
            Int32 handle_list_ptr = binaryReader.ReadInt32();
            binaryReader.WriteLine1("    Handle List Pointer:  0x{0:X8}  (run-time)", handle_list_ptr);
            Int32 object_data_ptr = binaryReader.ReadInt32();
            binaryReader.WriteLine1("    Object Data Pointer:  0x{0:X8}  (run-time)", object_data_ptr);
            Int32 object_data_size = binaryReader.ReadInt32();
            binaryReader.WriteLine1("    Object Data Size:     {0,-10}  (run-time)", object_data_size);
            binaryReader.WriteLine("");
            binaryReader.WriteLine("    HANDLE LIST:");
            for (int i = 1; i <= num_handles; i++)
            {
                UInt64 handle = binaryReader.ReadUInt64();
                // if (i > 10 && i < num_handles - 10) continue;
                binaryReader.WriteLineT("UInt64", "     {0,3}. 0x{1:X16}", i, handle);
            }

            int block = 1;
            while (block <= num_handles && binaryReader.BaseStream.Position < sectionDataStart + cpu_size)
            {
                // Align on a dword boundry
                while ((binaryReader.BaseStream.Position & 0x00000003) != 0)
                    binaryReader.ReadByte();
                if (binaryReader.BaseStream.Position >= sectionDataStart + cpu_size)
                    break;
                binaryReader.WriteLine("");
                binaryReader.WriteLine("    OBJECT #{0}:", block++);
                UInt64 handle_offset = binaryReader.ReadUInt64();
                binaryReader.WriteLine1("      Handle Offset:         0x{0:X16}", handle_offset);
                UInt64 parent_handle_offset = binaryReader.ReadUInt64();
                binaryReader.WriteLine1("      Parent Handle Offset:  0x{0:X16}", parent_handle_offset);
                UInt32 object_type_hash = binaryReader.ReadUInt32();
                if (SRZoneObjectTypes.hasName(object_type_hash))
                    binaryReader.WriteLine1("      Object Type:           0x{0:X8} ({1})", object_type_hash, SRZoneObjectTypes.Name(object_type_hash));
                else
                    binaryReader.WriteLine1("      Object Type (Hash):    0x{0:X8}", object_type_hash);
                Int16 number_of_properties = binaryReader.ReadInt16();
                binaryReader.WriteLine1("      Number of Properties:  {0}", number_of_properties);
                Int16 buffer_size = binaryReader.ReadInt16();
                binaryReader.WriteLine1("      Buffer Size:           {0}", buffer_size);
                UInt16 name_offset = binaryReader.ReadUInt16();
                binaryReader.WriteLine1("      Name Offset:           {0}", name_offset);
                Int16 padding = binaryReader.ReadInt16();
                binaryReader.WriteLine1("      Padding:               {0}", padding);
                long savePosition = binaryReader.BaseStream.Position;
                binaryReader.BaseStream.Seek(name_offset, SeekOrigin.Current);
                string objectName = binaryReader.ReadStringZ();
                binaryReader.WriteLineX("      Object Name:           " + objectName);
                binaryReader.BaseStream.Seek(savePosition, SeekOrigin.Begin);
                for (int i = 1; i <= number_of_properties; i++)
                {
                    // Align on a dword boundry
                    while ((binaryReader.BaseStream.Position & 0x00000003) != 0)
                        binaryReader.ReadByte();
                    binaryReader.WriteLine("");
                    binaryReader.WriteLine("      PROPERTY #{0}:", i);
                    uint type = binaryReader.ReadUInt16();
                    string typeName = (type < propertyTypeNames.Length) ? propertyTypeNames[type] : "unknown";
                    binaryReader.WriteLineT("UInt16", "        Type:      {0} ({1})", type, typeName);
                    Int16 size = binaryReader.ReadInt16();
                    binaryReader.WriteLine1("        Size:      {0} bytes", size);
                    Int32 name_crc = binaryReader.ReadInt32();
                    binaryReader.WriteLine1("        Name CRC:  0x{0:X8} ({0})", name_crc, name_crc);
                    long dataBegin = binaryReader.BaseStream.Position;
                    switch (type)
                    {
                        case 0:
                            binaryReader.WriteLineT("StringZ", "        Value:     \"" + binaryReader.ReadStringZ() + "\"");
                            break;
                        case 2:
                            binaryReader.WriteLineT("float[3]", "        Value:     Position (x,y,z):  {0}, {1}, {2}", binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                            break;
                        case 3:
                            binaryReader.WriteLineT("float[3]", "        Value:     Position (x,y,z):       {0}, {1}, {2}", binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                            binaryReader.WriteLineT("float[4]", "                   Orientation (x,y,z,w):  {0}, {1}, {2}, {3}", binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                            break;
                        default:
                            if (size > 16)
                            {
                                binaryReader.WriteLine("        Value:     ???");
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
                                binaryReader.WriteLine("        Value:     {1}", i, hexBuffer);
                            }
                            break;
                    }
                    binaryReader.BaseStream.Seek(dataBegin + size, SeekOrigin.Begin);
                }
            }
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
                    binaryReader.WriteLine("");
                    binaryReader.WriteLine("SECTION #{0}:", section++);
                    UInt32 section_id = binaryReader.ReadUInt32();
                    binaryReader.WriteLine1("  Section ID:   0x{0:X8}", section_id);
                    UInt32 id = (section_id & 0x7FFFFFFF);
                    if (id < 0x2233 || id >= 0x2300)
                        throw new Exception("Invalid section ID.  Not a valid zone file.");
                    if (ChrisLaRosa.SaintsRow.ZoneFile.SRZoneSectionIdentifiers.SectionTypes.ContainsKey(id))
                    {
                        binaryReader.WriteLineX("  Name:         " + ChrisLaRosa.SaintsRow.ZoneFile.SRZoneSectionIdentifiers.SectionTypes[id].name);
                        binaryReader.WriteLineX("  Description:  " + ChrisLaRosa.SaintsRow.ZoneFile.SRZoneSectionIdentifiers.SectionTypes[id].description);
                    }
                    UInt32 cpu_size = binaryReader.ReadUInt32();
                    binaryReader.WriteLine1("  CPU Size:     {0} bytes", cpu_size);
                    UInt32 gpu_size = 0;
                    if ((section_id & 0x80000000) != 0)
                    {
                        gpu_size = binaryReader.ReadUInt32();
                        binaryReader.WriteLine1("  GPU Size:     {0} bytes", gpu_size);
                    }
                    long sectionDataStart = binaryReader.BaseStream.Position;
                    if (cpu_size == 0 && gpu_size == 0)
                    {
                        binaryReader.WriteLine("");
                        binaryReader.WriteLine("  EMPTY SECTION = EOF");
                        break;
                    }

                    switch (id)
                    {
                        case 0x2233: ReadGeometrySection(binaryReader, cpu_size, sectionDataStart); break;
                        case 0x2234: ReadObjectSection(binaryReader, cpu_size, sectionDataStart); break;
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
            Console.WriteLine("Supports Saints Row: The Third and Saints Row IV.");
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
            string svnRevision = "$Revision: 1257 $";
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
                { "p|position", "show the file position (hex) column",  v => SRBinaryReader.showPosition = v != null },
                { "t|type", "show the data type column",  v => SRBinaryReader.showType = v != null },
                { "h|help", "show this message and exit", v => showHelp = v != null }
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
