//
// Copyright (C) 2015-2017 Christopher LaRosa
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
    public class SRZoneSection : SRDataBlockMultiBase
    {
        // CONSTANTS

        public const string XmlTagName = "section";     // Used in XML documents
        public const string BlockName = "Section";      // Used in Exception reporting

        public const int Alignment = 4;                 // Align on a dword boundry

        // OPTIONS

        public static bool OptionParseObjects = true;      // Parse objects when reading zone files
        public static bool OptionParseFastObjects = true;  // Parse fast objects when reading zone files

        // FIELD VALUES

        private UInt32 sectionID = 0;
        private UInt32 gpuSize = 0;
        private SRDataBlockSingleBase cpuData = null;

        // PROPERTIES

        public UInt32 SectionId { get { return sectionID; } }
        public UInt32 GpuSize { get { return gpuSize; } }
        public SRDataBlockSingleBase CpuData { get { return cpuData; } }

        // LOCAL VARIABLES

        private SRVFileHeader vFileHeader = null;

        // CONSTRUCTORS

        public SRZoneSection(SRVFileHeader vFileHeader)
        {
            this.vFileHeader = vFileHeader;
            sectionID = 0;
            gpuSize = 0;
        }

        public SRZoneSection(SRVFileHeader vFileHeader, SRBinaryReader binaryReader, int index)
        {
            this.vFileHeader = vFileHeader;
            Read(binaryReader, index);
        }

        public SRZoneSection(SRVFileHeader vFileHeader, XmlNode parentNode, int index)
        {
            this.vFileHeader = vFileHeader;
            ReadXml(parentNode, index);
        }

        // ACCESSORS
        
        public UInt32 SectionType()
        {
            return sectionID & 0x7FFFFFFF;
        }

        public bool HasDescription()
        {
            return SRZoneSectionIdentifiers.SectionTypes.ContainsKey(SectionType());
        }

        public string Description()
        {
            return HasDescription() ? SRZoneSectionIdentifiers.SectionTypes[SectionType()] : "";
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
                else if (OptionParseFastObjects && SectionType() == 0x2233)
                    cpuData = new SRZoneSectionDataCrunchedGeometry(vFileHeader, binaryReader, (int)cpuSize);
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
                else if (SectionType() == 0x2233)
                    cpuData = new SRZoneSectionDataCrunchedGeometry(vFileHeader, cpuDataNode);
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

                if (HasDescription())
                    writer.WriteComment(Description());
                writer.WriteHex("id", sectionID);
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
