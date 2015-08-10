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
    public class SRZoneObjectSectionCpuData : SRDataBlockSingleBase
    {

        public const string XmlTagName = "object_data";     // Used in XML documents

        public const int Alignment = 1;

        // OPTIONS

        public static bool OptionRebuildHandleList = false;     // Rebuild the handle list before writing

        // FIELD VALUES

        UInt32 signature;
        UInt32 version;
        UInt32 flags;
        UInt32 handleListPointer;
        UInt32 objectDataPointer;
        UInt32 objectDataSize;
        List<UInt64> handleList;
        List<SRZoneObject> objectList;

        // PROPERTIES

        public List<UInt64> HandleList { get { return handleList; } }
        public List<SRZoneObject> ObjectList { get { return objectList; } }

        // CONSTRUCTORS

        public SRZoneObjectSectionCpuData()
        {
        }

        public SRZoneObjectSectionCpuData(SRBinaryReader binaryReader, int size)
        {
            Read(binaryReader, size);
        }

        public SRZoneObjectSectionCpuData(XmlNode parentNode)
        {
            ReadXml(parentNode);
        }

        // READERS / WRITERS

        /// <summary>
        /// Reads a data block from a file binary stream.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the block from.  Must point to the beginning of the block.</param>
        /// <param name="size">Maximum number of bytes to read.</param>
        public void Read(SRBinaryReader binaryReader, int size)
        {
            SRTrace.WriteLine("");
            SRTrace.WriteLine("  OBJECT SECTION HEADER:");
            signature = binaryReader.ReadUInt32();
            SRTrace.WriteLine("    Header Signature:     0x{0:X8}", signature);
            if (signature != 0x574F4246)
                throw new SRZoneFileException("Invalid section ID");
            version = binaryReader.ReadUInt32();
            SRTrace.WriteLine("    Version:              {0}", version);
            if (version != 5)
                throw new SRZoneFileException("Invalid version number");
            var numObjects = binaryReader.ReadUInt32();
            SRTrace.WriteLine("    Number of Objects:    {0}", numObjects);
            var numHandles = binaryReader.ReadUInt32();
            SRTrace.WriteLine("    Number of Handles:    {0}", numHandles);
            flags = binaryReader.ReadUInt32();
            SRTrace.WriteLine("    Flags:                0x{0:X8}", flags);
            handleListPointer = binaryReader.ReadUInt32();
            SRTrace.WriteLine("    Handle List Pointer:  0x{0:X8}  (run-time)", handleListPointer);
            objectDataPointer = binaryReader.ReadUInt32();
            SRTrace.WriteLine("    Object Data Pointer:  0x{0:X8}  (run-time)", objectDataPointer);
            objectDataSize = binaryReader.ReadUInt32();
            SRTrace.WriteLine("    Object Data Size:     {0,-10}  (run-time)", objectDataSize);
            SRTrace.WriteLine("");
            SRTrace.WriteLine("    HANDLE LIST:");
            handleList = new List<UInt64>((int)numHandles);
            for (int i = 0; i < numHandles; i++)
            {
                handleList.Add(binaryReader.ReadUInt64());
                SRTrace.WriteLine("     {0,3}. 0x{1:X16}", i + 1, handleList[i]);
            }
            objectList = new List<SRZoneObject>((int)numObjects);
            for (int i = 0; i < numObjects; i++)
                objectList.Add(new SRZoneObject(binaryReader, i));
        }

        /// <summary>
        /// Writes the data block to a file binary stream.
        /// </summary>
        /// <param name="binaryWriter">Binary writer to write the block to.</param>
        override public void Write(SRBinaryWriter binaryWriter)
        {
            binaryWriter.Write(signature);
            binaryWriter.Write(version);
            binaryWriter.Write((UInt32)objectList.Count);
            binaryWriter.Write((UInt32)handleList.Count);
            binaryWriter.Write(flags);
            binaryWriter.Write(handleListPointer);
            binaryWriter.Write(objectDataPointer);
            binaryWriter.Write(objectDataSize);

            if (OptionRebuildHandleList)
            {
                // BUILD HANDLE LIST
                UInt64[] newHandleList = new UInt64[objectList.Count];
                for (int i = 0; i < objectList.Count; i++)
                    newHandleList[i] = objectList[i].HandleOffset;
                Array.Sort(newHandleList);
                for (int i = 0; i < newHandleList.Length; i++)
                    binaryWriter.Write(newHandleList[i]);
            }
            else
            {
                for (int i = 0; i < handleList.Count; i++)
                    binaryWriter.Write(handleList[i]);
            }

            for (int i = 0; i < objectList.Count; i++)
                objectList[i].Write(binaryWriter, i);
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
            signature = reader.ReadUInt32("signature");
            version = reader.ReadUInt32("version");
            flags = reader.ReadUInt32("flags");
            handleListPointer = reader.ReadUInt32("handle_list_pointer");
            objectDataPointer = reader.ReadUInt32("object_data_pointer");
            objectDataSize = reader.ReadUInt32("object_data_size");
            XmlNodeList handleNodes = reader.Node.SelectNodes("./handles/handle");
            var numHandles = handleNodes.Count;
            handleList = new List<UInt64>((int)numHandles);
            for (int i = 0; i < numHandles; i++)
                handleList.Add(SRXmlNodeReader.ReadUInt64(handleNodes[i]));

            XmlNodeList objectNodes = reader.Node.SelectNodes("./objects/" + SRZoneObject.XmlTagName);
            var numObjects = objectNodes.Count;
            objectList = new List<SRZoneObject>(numObjects);
            for (int i = 0; i < numObjects; i++)
                objectList.Add(new SRZoneObject(objectNodes[i], i));
        }

        /// <summary>
        /// Writes the data block to an XML document.
        /// This may add one or more nodes to the given parentNode.
        /// </summary>
        /// <param name="parentNode">XML node to add this data block to.</param>
        /// <param name="index">Optional index attribute.</param>
        override public void WriteXml(XmlNode parentNode)
        {
            int index;

            SRXmlNodeWriter writer = new SRXmlNodeWriter(parentNode, XmlTagName);

            writer.WriteHex("signature", signature);
            writer.Write("version", version);
            writer.WriteHex("flags", flags);
            writer.WriteHex("handle_list_pointer", handleListPointer);
            writer.WriteHex("object_data_pointer", objectDataPointer);
            writer.Write("object_data_size", objectDataSize);

            SRXmlNodeWriter handleWriter = new SRXmlNodeWriter(writer, "handles");
            index = 0;
            foreach (UInt64 handle in handleList)
                handleWriter.WriteHex("handle", handle, ++index);

            XmlNode objectsNode = writer.CreateNode("objects");
            index = 0;
            foreach (SRZoneObject srObject in objectList)
                srObject.WriteXml(objectsNode, index++);
        }
    }
}
