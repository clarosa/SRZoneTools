//
// Copyright (C) 2015 Christopher LaRosa
//
// Based on Saints Row: The Third zone file format info and discussion here:
// https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace ChrisLaRosa.SaintsRow.ZoneFile
{
    /// <summary>
    /// Zone file section block.  The zone file consists of a series of these blocks.
    /// </summary>
    class SRZoneSection : SRDataBlockMultiBase
    {
        // CONSTANTS

        public const string XmlTagName = "section";     // Used in XML documents
        public const string BlockName = "Section";      // Used in Exception reporting

        public const int Alignment = 4;                 // Align on a dword boundry

        public static readonly Dictionary<UInt32, string> SectionTypes = new Dictionary<UInt32, string>()
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

        // OPTIONS

        public static bool OptionParseObjects = true;      // Parse objects when reading zone files

        // FIELD VALUES

        private UInt32 sectionID = 0;
        private UInt32 gpuSize = 0;
        private SRDataBlockSingleBase cpuData = null;

        // CONSTRUCTORS

        public SRZoneSection()
        {
            sectionID = 0;
            gpuSize = 0;
        }

        public SRZoneSection(SRBinaryReader binaryReader, int index)
        {
            Read(binaryReader, index);
        }

        public SRZoneSection(XmlNode parentNode, int index)
        {
            ReadXml(parentNode, index);
        }

        // ACCESSORS
        
        public UInt32 SectionType()
        {
            return sectionID & 0x7FFFFFFF;
        }

        public bool HasDescription()
        {
            return SectionTypes.ContainsKey(SectionType());
        }

        public string Description()
        {
            return HasDescription() ? SectionTypes[SectionType()] : "";
        }

        public bool HasGPUData()
        {
            return (sectionID & 0x80000000) != 0;
        }

        // READERS / WRITERS

        /// <summary>
        /// Reads a section block from a .czn_pc file binary stream.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the block from.  Must point to the beginning of the block.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        public void Read(SRBinaryReader binaryReader, int index)
        {
            try
            {
                binaryReader.Align(Alignment);
                SRTrace.WriteLine("");
                SRTrace.WriteLine("SECTION #{0}:  [file offset 0x{1:X8}]", index + 1, binaryReader.BaseStream.Position);
                sectionID = binaryReader.ReadUInt32();
                SRTrace.WriteLine("  Section ID:   0x{0:X8}", sectionID);
                if (SectionType() < 0x2233 || SectionType() >= 0x2300)
                    throw new SRZoneFileException("Invalid section ID.  Not a valid zone file.");
                if (HasDescription())
                    SRTrace.WriteLine("  Description:  " + Description());
                var cpuSize = binaryReader.ReadUInt32();
                SRTrace.WriteLine("  CPU Size:     {0} bytes", cpuSize);
                gpuSize = 0;
                if (HasGPUData())
                {
                    gpuSize = binaryReader.ReadUInt32();
                    SRTrace.WriteLine("  GPU Size:     {0} bytes", gpuSize);
                }
                if (cpuSize == 0)
                    cpuData = null;
                else if (OptionParseObjects && SectionType() == 0x2234)
                    cpuData = new SRZoneSectionDataObjects(binaryReader, (int)cpuSize);
                else
                    cpuData = new SRRawDataBlock(binaryReader, (int)cpuSize);
            }
            catch (Exception e)
            {
                // Add context information for the error message
                if (index >= 0)
                    e.Data[BlockName] = index + 1;
                throw;
            }
        }

        /// <summary>
        /// Writes the section block to a .czn_pc file binary stream.
        /// Recalculates the CPU data size so the original size is not used.
        /// </summary>
        /// <param name="binaryWriter">Binary writer to write the block to.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        override public void Write(SRBinaryWriter binaryWriter, int index)
        {
            try
            {
                binaryWriter.Align(Alignment);
                binaryWriter.Write(sectionID);
                var positionCpuSize = binaryWriter.BaseStream.Position;
                binaryWriter.Write((UInt32)0);      // Placeholder for CPU size, which is rewritten below
                if (HasGPUData())
                    binaryWriter.Write(gpuSize);
                var positionCpuDataStart = binaryWriter.BaseStream.Position;
                if (cpuData != null)
                    cpuData.Write(binaryWriter);
                var actualCpuDataSize = binaryWriter.BaseStream.Position - positionCpuDataStart;

                // Update the CPU data size with the actual number of bytes written
                binaryWriter.Seek((int)positionCpuSize, SeekOrigin.Begin);
                binaryWriter.Write((UInt32)actualCpuDataSize);
                binaryWriter.Seek(0, SeekOrigin.End);
            }
            catch (Exception e)
            {
                // Add context information for the error message
                if (index >= 0)
                    e.Data[BlockName] = index + 1;
                throw;
            }
        }

        /// <summary>
        /// Reads the contents of a section block from an XML document.
        /// </summary>
        /// <param name="node">XML node which represents an instance of this data block.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        public void ReadXml(XmlNode thisNode, int index)
        {
            try
            {
                SRXmlNodeReader reader = new SRXmlNodeReader(thisNode);
                sectionID = reader.ReadUInt32("id");
                gpuSize = reader.ReadUInt32("gpu_size");
                XmlNode cpuDataNode = reader.GetNodeOptional("cpu_data");
                if (cpuDataNode == null)
                    cpuData = null;
                else if (SRRawDataBlock.HasRawXmlData(cpuDataNode))
                    cpuData = new SRRawDataBlock(cpuDataNode);
                else
                    cpuData = new SRZoneSectionDataObjects(cpuDataNode);
            }
            catch (Exception e)
            {
                // Add context information for the error message
                if (index >= 0)
                    e.Data[BlockName] = index + 1;
                throw;
            }
        }

        /// <summary>
        /// Writes the section block to an XML document.
        /// </summary>
        /// <param name="parentNode">XML node to add this section to.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        override public void WriteXml(XmlNode parentNode, int index)
        {
            try
            {
                SRXmlNodeWriter writer = new SRXmlNodeWriter(parentNode, XmlTagName, index + 1);

                writer.WriteHex("id", sectionID);
                if (HasDescription())
                    writer.Write("description", Description());
                if (HasGPUData())
                    writer.Write("gpu_size", gpuSize);
                if (cpuData != null)
                    cpuData.WriteXml(writer.CreateNode("cpu_data"));
            }
            catch (Exception e)
            {
                // Add context information for the error message
                if (index >= 0)
                    e.Data[BlockName] = index + 1;
                throw;
            }
        }
    }
}
