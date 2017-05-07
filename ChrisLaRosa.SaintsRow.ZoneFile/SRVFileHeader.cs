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
    /// V File Header.
    /// This data block appears within the Zone Header file.
    /// </summary>
    public class SRVFileHeader : SRDataBlockSingleBase
    {

        public const string XmlTagName = "v_file_header";     // Used in XML documents

        public const int Alignment = 1;

        // CONSTANTS

        public readonly string[] WorldZoneTypeNames =
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

        // OPTIONS

        public bool OptionNameReferenceIdentifier = true;

        // FIELD VALUES

        private UInt16 signature;
        private UInt16 version;
        private UInt32 refDataStart;
        private UInt32 unknown;
        private List<string> referenceData;

        // PROPERTIES

        public List<string> ReferenceStringList { get { return referenceData; } }

        // LOCAL VARIABLES

        private Dictionary<long, string> referenceNamesByReadOffset;
        private Dictionary<string, long> referenceWriteOffsetsByName;

        // CONSTRUCTORS

        public SRVFileHeader()
        {
        }

        public SRVFileHeader(SRBinaryReader binaryReader)
        {
            Read(binaryReader);
        }

        public SRVFileHeader(XmlNode parentNode)
        {
            ReadXml(parentNode);
        }

        // HELPERS

        public string GetReferenceNameByReadOffset(long offset)
        {
            if (!referenceNamesByReadOffset.ContainsKey(offset))
                throw new SRZoneFileException("Reference does not exist in V-File Header at offset \"" + offset.ToString() + "\".");
            return referenceNamesByReadOffset[offset];
        }

        public long GetReferenceWriteOffsetByName(string name)
        {
            if (referenceWriteOffsetsByName == null)
                BuildReferenceWriteOffsetsByName();
            if (!referenceWriteOffsetsByName.ContainsKey(name))
                throw new SRZoneFileException("Reference \"" + name + "\" does not exist in V-File Header.");
            return referenceWriteOffsetsByName[name];
        }

        private void BuildReferenceWriteOffsetsByName()
        {
            referenceWriteOffsetsByName = new Dictionary<string, long>();

            UInt32 offset = 0;
            int count = referenceData.Count;
            for (int i = 0; i < count; i++)
            {
                string id = (i + 1).ToString();
                string name = referenceData[i];
                referenceWriteOffsetsByName.Add(id, offset);
                if (!referenceWriteOffsetsByName.ContainsKey(name))
                    referenceWriteOffsetsByName.Add(name, offset);
                offset += (UInt32)name.Length + 1;
            }
        }

        // READERS / WRITERS

        /// <summary>
        /// Reads a data block from a file binary stream.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the block from.  Must point to the beginning of the block.</param>
        /// <param name="size">Maximum number of bytes to read.</param>
        public void Read(SRBinaryReader binaryReader)
        {
            SRTrace.WriteLine("");
            SRTrace.WriteLine("V-FILE HEADER:");
            signature = binaryReader.ReadUInt16();
            SRTrace.WriteLine("  V-File Signature:      0x{0:X4}", signature);
            if (signature != 0x3854)
                throw new Exception("Incorrect V-file signature.  Not a valid zone header file.");
            version = binaryReader.ReadUInt16();
            SRTrace.WriteLine("  V-File Version:        {0}", version);
            if (version != 4)
                throw new Exception("Incorrect V-file version.");
            int refDataSize = binaryReader.ReadInt32();
            SRTrace.WriteLine("  Reference Data Size:   {0}", refDataSize);
            refDataStart = binaryReader.ReadUInt32();
            SRTrace.WriteLine("  Reference Data Start:  0x{0:X8}", refDataStart);
//            if (refDataStart != 0)
//                throw new SRZoneFileException("Expected reference data start to be zero.");
            int refCount = binaryReader.ReadInt32();
            SRTrace.WriteLine("  Reference Count:       {0}", refCount);
            unknown = binaryReader.ReadUInt32();
            SRTrace.WriteLine("  Unknown:               0x{0:X8}", unknown);
            binaryReader.BaseStream.Seek(12, SeekOrigin.Current);
            long refDataOffset = binaryReader.BaseStream.Position;
            SRTrace.WriteLine("");
            SRTrace.WriteLine("  REFERENCE DATA:");
            referenceData = new List<string>(refCount);
            referenceNamesByReadOffset = new Dictionary<long, string>(refCount);
            var positionDataStart = binaryReader.BaseStream.Position;
            for (int i = 1; i <= refCount; i++)
            {
                long offset = binaryReader.BaseStream.Position - positionDataStart;
                string name = binaryReader.ReadString();
                SRTrace.WriteLine("   {0,3}. {1}", i, name);
                referenceData.Add(name);
                referenceNamesByReadOffset.Add(offset, OptionNameReferenceIdentifier ? name : i.ToString());
            }
            var finalNull = binaryReader.ReadByte();
            if (finalNull != 0)
                throw new Exception("Expected trailing null byte.");
        }

        /// <summary>
        /// Writes the data block to a file binary stream.
        /// </summary>
        /// <param name="binaryWriter">Binary writer to write the block to.</param>
        override public void Write(SRBinaryWriter binaryWriter)
        {
            binaryWriter.Write(signature);
            binaryWriter.Write(version);
            var positionSize = binaryWriter.BaseStream.Position;
            binaryWriter.Write((UInt32)0);      // refDataSize (will rewrite later)
            binaryWriter.Write(refDataStart);
            binaryWriter.Write((UInt32)referenceData.Count);
            binaryWriter.Write((UInt32)unknown);
            for (int i = 0; i < 12; i++)
                binaryWriter.Write((Byte)0);
            referenceWriteOffsetsByName = new Dictionary<string, long>(referenceData.Count);
            var positionDataStart = binaryWriter.BaseStream.Position;
            for (int i = 0; i < referenceData.Count; i++)
            {
                string id = (i + 1).ToString();
                string name = referenceData[i];
                long offset = binaryWriter.BaseStream.Position - positionDataStart;
                referenceWriteOffsetsByName.Add(id, offset);
                if (!referenceWriteOffsetsByName.ContainsKey(name))
                    referenceWriteOffsetsByName.Add(name, offset);
                binaryWriter.Write(name);
            }
            var actualDataSize = binaryWriter.BaseStream.Position - positionDataStart;
            binaryWriter.Write((Byte)0);

            // Update the value size with the actual number of bytes written
            binaryWriter.Seek((int)positionSize, SeekOrigin.Begin);
            binaryWriter.Write((UInt32)actualDataSize);
            binaryWriter.Seek(0, SeekOrigin.End);
        }

        /// <summary>
        /// Reads the contents of a data block from an XML document.
        /// This may read one or more nodes from the given parentNode.
        /// This can return false if it doesn't find any data nodes in the parentNode.
        /// </summary>
        /// <param name="parentNode">XML node to read from.</param>
        /// <returns>true</returns>
        override public void ReadXml(XmlNode parentNode)
        {
            SRXmlNodeReader reader = new SRXmlNodeReader(parentNode, XmlTagName);
            signature = reader.ReadUInt16("signature");
            version = reader.ReadUInt16("version");
            refDataStart = reader.ReadUInt32("ref_data_start");
            unknown = reader.ReadUInt32("unknown");
            XmlNodeList referenceNodes = reader.Node.SelectNodes("./references/reference");
            var numReferences = referenceNodes.Count;
            referenceData = new List<string>(numReferences);
            for (int i = 0; i < numReferences; i++)
                referenceData.Add(referenceNodes[i].InnerText);
        }

        /// <summary>
        /// Writes the data block to an XML document.
        /// This may add one or more nodes to the given parentNode.
        /// </summary>
        /// <param name="parentNode">XML node to add this data block to.</param>
        /// <param name="index">Optional index attribute.</param>
        override public void WriteXml(XmlNode parentNode)
        {
            SRXmlNodeWriter writer = new SRXmlNodeWriter(parentNode, XmlTagName);
            writer.WriteHex("signature", signature);
            writer.Write("version", version);
            writer.Write("ref_data_start", refDataStart);
            writer.Write("unknown", unknown);
            SRXmlNodeWriter referenceDataWriter = new SRXmlNodeWriter(writer, "references");
            for (int i = 0; i < referenceData.Count; i++)
                referenceDataWriter.Write("reference", referenceData[i], i + 1);
        }
    }
}
