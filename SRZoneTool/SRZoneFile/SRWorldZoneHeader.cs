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
    /// World Zone Header.
    /// This data block appears within the Zone Header file.
    /// </summary>
    class SRWorldZoneHeader : SRDataBlockSingleBase
    {
        // CONSTANTS

        public const string XmlTagName = "world_zone_header";     // Used in XML documents

        public const int Alignment = 16;

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

        string signature;
        UInt32 version;
        UInt32 fileReferencesPtr;
        SRTransform fileReferenceOffset;
        Byte zoneType;
        List<SRZoneMeshFileReference> references;

        // LOCAL VARIABLES

        private SRVFileHeader vFileHeader;

        // CONSTRUCTORS

        public SRWorldZoneHeader(SRVFileHeader vFileHeader)
        {
            this.vFileHeader = vFileHeader;
        }

        public SRWorldZoneHeader(SRBinaryReader binaryReader, SRVFileHeader vFileHeader)
        {
            this.vFileHeader = vFileHeader;
            Read(binaryReader);
        }

        public SRWorldZoneHeader(XmlNode parentNode, SRVFileHeader vFileHeader)
        {
            this.vFileHeader = vFileHeader;
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
            binaryReader.Align(Alignment);
            Console.WriteLine("");
            Console.WriteLine("WORLD ZONE HEADER:  [file offset 0x{0:X8}]", binaryReader.BaseStream.Position);
            signature = new string(binaryReader.ReadChars(4));
            Console.WriteLine("  World Zone Signature:   " + signature);
            if (signature != "SR3Z")
                throw new SRZoneFileException("Incorrect world zone signature.", binaryReader.BaseStream.Position - 4);
            version = binaryReader.ReadUInt32();
            Console.WriteLine("  World Zone Version:     {0}", version);
            if (version != 29 && version != 32)  // version 29 = SR3, 32 = SR4
                throw new SRZoneFileException("Incorrect world zone version.");
            int v_file_header_ptr = binaryReader.ReadInt32();
            Console.WriteLine("  V-File Header Pointer:  0x{0:X8}", v_file_header_ptr);
            float x = binaryReader.ReadSingle();
            float y = binaryReader.ReadSingle();
            float z = binaryReader.ReadSingle();
            Console.WriteLine("  File Reference Offset:  {0}, {1}, {2}", x, y, z);
            fileReferenceOffset = new SRTransform(x, y, z);
            fileReferencesPtr = binaryReader.ReadUInt32();
            Console.WriteLine("  WZ File Reference Ptr:  0x{0:X8}", fileReferencesPtr);
            int num_file_references = binaryReader.ReadInt16();
            Console.WriteLine("  Number of File Refs:    {0}", num_file_references);
            zoneType = binaryReader.ReadByte();
            string typeName = (zoneType < WorldZoneTypeNames.Length) ? WorldZoneTypeNames[zoneType] : "unknown";
            Console.WriteLine("  Zone Type:              {0} ({1})", zoneType, typeName);
            int unused = binaryReader.ReadByte();
            Console.WriteLine("  Unused:                 {0}", unused);
            if (unused != 0)
                throw new SRZoneFileException("Expected unused field to be zero.");
            int interiorTriggerPtr = binaryReader.ReadInt32();
            Console.WriteLine("  Interior Trigger Ptr:   0x{0:X8}  (run-time)", interiorTriggerPtr);
            if (interiorTriggerPtr != 0)
                throw new SRZoneFileException("Expected interior trigger pointer to be zero.");
            int numberOfTriggers = binaryReader.ReadInt16();
            Console.WriteLine("  Number of Triggers:     {0,-10}  (run-time)", numberOfTriggers);
            if (numberOfTriggers != 0)
                throw new SRZoneFileException("Expected number of triggers to be zero.");
            int extraObjects = binaryReader.ReadInt16();
            Console.WriteLine("  Extra Objects:          {0}", extraObjects);
            if (extraObjects != 0)
                throw new SRZoneFileException("Expected extra objects to be zero.");
            binaryReader.BaseStream.Seek(24, SeekOrigin.Current);
            Console.WriteLine("");
            Console.WriteLine("  MESH FILE REFERENCES:  [file offset 0x{0:X8}]", binaryReader.BaseStream.Position);
            references = new List<SRZoneMeshFileReference>(num_file_references);
            for (int i = 0; i < num_file_references; i++)
                references.Add(new SRZoneMeshFileReference(binaryReader, i, vFileHeader));
        }

        /// <summary>
        /// Writes the data block to a file binary stream.
        /// </summary>
        /// <param name="binaryWriter">Binary writer to write the block to.</param>
        override public void Write(SRBinaryWriter binaryWriter)
        {
            binaryWriter.Align(Alignment);
            for (int i = 0; i < 4; i++)
                binaryWriter.Write((Byte)signature[i]);
            binaryWriter.Write(version);
            binaryWriter.Write((UInt32)0);
            binaryWriter.Write((Single)fileReferenceOffset.x);
            binaryWriter.Write((Single)fileReferenceOffset.y);
            binaryWriter.Write((Single)fileReferenceOffset.z);
            binaryWriter.Write(fileReferencesPtr);
            binaryWriter.Write((UInt16)references.Count);
            binaryWriter.Write((Byte)zoneType);
            binaryWriter.Write((Byte)0);                        // Unused
            binaryWriter.Write((UInt32)0);                      // Interior Trigger Ptr (runtime)
            binaryWriter.Write((UInt16)0);                      // Number of Triggers (runtime)
            binaryWriter.Write((UInt16)0);                      // Extra objects (runtime)
            for (int i = 0; i < 24; i++)
                binaryWriter.Write((Byte)0);
            for (int i = 0; i < references.Count; i++)
                references[i].Write(binaryWriter, i);
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
            signature = reader.ReadString("signature");
            version = reader.ReadUInt32("version");
            SRXmlNodeReader offsetReader = new SRXmlNodeReader(reader.Node, "file_reference_offset");
            fileReferenceOffset = new SRTransform();
            fileReferenceOffset.x = offsetReader.ReadSingle("x");
            fileReferenceOffset.y = offsetReader.ReadSingle("y");
            fileReferenceOffset.z = offsetReader.ReadSingle("z");
            fileReferencesPtr = reader.ReadUInt32("file_references_ptr");
            zoneType = (Byte)reader.ReadUInt16("zone_type");
            XmlNodeList referenceNodes = reader.Node.SelectNodes("./mesh_file_references/" + SRZoneMeshFileReference.XmlTagName);
            references = new List<SRZoneMeshFileReference>(referenceNodes.Count);
            for (int i = 0; i < referenceNodes.Count; i++)
                references.Add(new SRZoneMeshFileReference(referenceNodes[i], i, vFileHeader));
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
            writer.Write("signature", signature);
            writer.Write("version", version);
            SRXmlNodeWriter offsetWriter = new SRXmlNodeWriter(writer, "file_reference_offset");
            offsetWriter.Write("x", fileReferenceOffset.x);
            offsetWriter.Write("y", fileReferenceOffset.y);
            offsetWriter.Write("z", fileReferenceOffset.z);
            writer.WriteHex("file_references_ptr", fileReferencesPtr);
            writer.Write("zone_type", zoneType);
            string zoneTypeName = (zoneType < WorldZoneTypeNames.Length) ? WorldZoneTypeNames[zoneType] : "unknown";
            writer.Write("zone_type_description", zoneTypeName);    // informational only
            SRXmlNodeWriter referenceDataWriter = new SRXmlNodeWriter(writer, "mesh_file_references");
            for (int i = 0; i < references.Count; i++)
                references[i].WriteXml(referenceDataWriter.Node, i);
        }
    }
}
