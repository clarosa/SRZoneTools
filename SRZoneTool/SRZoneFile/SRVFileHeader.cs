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
    /// Object Data Section.
    /// This data block appears within the CPU Data of a Section block of type 0x2234.
    /// It contains a header followed by a list of Object data items.
    /// </summary>
    class SRVFileHeader : SRDataBlockSingleBase
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


        // FIELD VALUES

        private UInt16 signature;
        private UInt16 version;
        private List<string> referenceData;

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

        // READERS / WRITERS

        /// <summary>
        /// Reads a data block from a file binary stream.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the block from.  Must point to the beginning of the block.</param>
        /// <param name="size">Maximum number of bytes to read.</param>
        public void Read(SRBinaryReader binaryReader)
        {
            Console.WriteLine("");
            Console.WriteLine("V-FILE HEADER:");
            signature = binaryReader.ReadUInt16();
            Console.WriteLine("  V-File Signature:      0x{0:X4}", signature);
            if (signature != 0x3854)
                throw new Exception("Incorrect signature.  Not a valid zone header file.");
            version = binaryReader.ReadUInt16();
            Console.WriteLine("  V-File Version:        {0}", version);
            if (version != 4)
                throw new Exception("Incorrect version.");
            int refDataSize = binaryReader.ReadInt32();
            Console.WriteLine("  Reference Data Size:   {0}", refDataSize);
            int refDataStart = binaryReader.ReadInt32();
            Console.WriteLine("  Reference Data Start:  0x{0:X8}", refDataStart);
            if (refDataStart != 0)
                throw new SRZoneFileException("Expected reference data start to be zero.");
            int refCount = binaryReader.ReadInt32();
            Console.WriteLine("  Reference Count:       {0}", refCount);
            binaryReader.BaseStream.Seek(16, SeekOrigin.Current);
            long refDataOffset = binaryReader.BaseStream.Position;
            Console.WriteLine("");
            Console.WriteLine("  REFERENCE DATA:");
            referenceData = new List<string>(refCount);
            for (int i = 1; i <= refCount; i++)
            {
                string name = binaryReader.ReadString();
                Console.WriteLine("   {0,3}. {1}", i, name);
                referenceData.Add(name);
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
            binaryWriter.Write((UInt32)0);      // refDataStart
            binaryWriter.Write((UInt32)referenceData.Count);
            for (int i = 0; i < 16; i++)
                binaryWriter.Write((Byte)0);
            var positionDataStart = binaryWriter.BaseStream.Position;
            for (int i = 0; i < referenceData.Count; i++)
                binaryWriter.Write(referenceData[i]);
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
            XmlNodeList referenceNodes = reader.Node.SelectNodes("./reference_data/reference");
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
            SRXmlNodeWriter referenceDataWriter = new SRXmlNodeWriter(writer, "reference_data");
            for (int i = 0; i < referenceData.Count; i++)
                referenceDataWriter.Write("reference", referenceData[i], i + 1);
        }
    }
}
