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
    /// Object Data Item.
    /// One or more of these data blocks are contained within an SRZoneSectionDataObjects block.
    /// It contains a header followed by a SRPropertyData block.
    /// </summary>
    class SRZoneObject : SRDataBlockMultiBase
    {
        // CONSTANTS

        public const string XmlTagName = "object";      // Used in XML documents
        public const string BlockName = "Object";       // Used in Exception reporting

        public const int Alignment = 4;                 // Align on a dword boundry

        // FIELD VALUES

        private UInt64 handleOffset = 0;
        private UInt64 parentHandleOffset = 0;
        private Int32 objectTypeHash = 0;
        private UInt16 padding = 0;
        private UInt16 nameIndex = 0;                   // Calculated property index of nameOffset
        private List<SRZoneProperty> propertyList;

        // PROPERTIES

        public UInt64 HandleOffset { get { return handleOffset; } }

        // CONSTRUCTORS

        public SRZoneObject()
        {
        }

        public SRZoneObject(SRBinaryReader binaryReader, int index)
        {
            Read(binaryReader, index);
        }

        public SRZoneObject(XmlNode parentNode, int index)
        {
            ReadXml(parentNode, index);
        }

        // READERS / WRITERS

        /// <summary>
        /// Reads a data block from a file binary stream.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the block from.  Must point to the beginning of the block.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        public void Read(SRBinaryReader binaryReader, int index)
        {
            try
            {
                binaryReader.Align(Alignment);
                SRTrace.WriteLine("");
                SRTrace.WriteLine("    OBJECT #{0}:  [file offset 0x{1:X8}]", index + 1, binaryReader.BaseStream.Position);
                handleOffset = binaryReader.ReadUInt64();
                SRTrace.WriteLine("      Handle Offset:         0x{0:X16}", handleOffset);
                parentHandleOffset = binaryReader.ReadUInt64();
                SRTrace.WriteLine("      Parent Handle Offset:  0x{0:X16}", parentHandleOffset);
                objectTypeHash = binaryReader.ReadInt32();
                SRTrace.WriteLine("      Object Type Hash:      0x{0:X8}", objectTypeHash);
                var propertyCount = binaryReader.ReadUInt16();
                SRTrace.WriteLine("      Number of Properties:  {0}", propertyCount);
                var bufferSize = binaryReader.ReadUInt16();
                SRTrace.WriteLine("      Buffer Size:           {0}", bufferSize);
                var nameOffset = binaryReader.ReadUInt16();
                SRTrace.WriteLine("      Name Offset:           {0}", nameOffset);
                padding = binaryReader.ReadUInt16();
                SRTrace.WriteLine("      Padding:               {0}", padding);
                if (propertyCount == 0)
                    throw new SRZoneFileException("Object has no properties.");
                propertyList = new List<SRZoneProperty>(propertyCount);
                var namePosition = binaryReader.BaseStream.Position + nameOffset - SRZoneProperty.DataOffset;
                nameIndex = 0;
                for (int i = 0; i < propertyCount; i++)
                {
                    if (AlignUp(binaryReader.BaseStream.Position, SRZoneProperty.Alignment) == namePosition)
                        nameIndex = (UInt16)(i + 1);
                    propertyList.Add(SRZoneProperty.Create(binaryReader, i));
                }
                if (nameOffset != 0 && nameIndex == 0)
                    throw new SRZoneFileException("Name Offset does not point to a valid property.");
                // Note: "is SRZoneStringProperty" is not used here in case OptionParseValues is false:
                if (nameIndex != 0 && !(propertyList[nameIndex - 1].Type == SRZoneProperty.StringType))
                    throw new SRZoneFileException("Name Offset does not point to a string property.");
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
        /// Writes the data block to a file binary stream.
        /// </summary>
        /// <param name="binaryWriter">Binary writer to write the block to.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        override public void Write(SRBinaryWriter binaryWriter, int index)
        {
            try
            {
                binaryWriter.Align(Alignment);
                binaryWriter.Write(handleOffset);
                binaryWriter.Write(parentHandleOffset);
                binaryWriter.Write(objectTypeHash);
                binaryWriter.Write((UInt16)propertyList.Count);
                var positionBufferSize = binaryWriter.BaseStream.Position;
                binaryWriter.Write((UInt16)0);                      // Buffer size will be rewritten below
                binaryWriter.Write((UInt16)0);                      // Name offset will be rewritten below
                binaryWriter.Write(padding);
                UInt16 newNameOffset = 0;
                var startPosition = binaryWriter.BaseStream.Position;
                for (int i = 0; i < propertyList.Count; i++)
                {
                    binaryWriter.Align(SRZoneProperty.Alignment);
                    if (i == nameIndex - 1)
                        newNameOffset = (UInt16)(binaryWriter.BaseStream.Position - startPosition + SRZoneProperty.DataOffset);
                    propertyList[i].Write(binaryWriter, i);
                }
                binaryWriter.Align(SRZoneProperty.Alignment);       // Align for buffer size calculation
                UInt16 newBufferSize = (UInt16)(binaryWriter.BaseStream.Position - startPosition);

                // Update the buffer size and name offset with the calculated values
                binaryWriter.Seek((int)positionBufferSize, SeekOrigin.Begin);
                binaryWriter.Write(newBufferSize);
                binaryWriter.Write(newNameOffset);
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
        /// Reads the contents of a data block from an XML document.
        /// </summary>
        /// <param name="thisNode">XML node which represents an instance of this data block.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        public void ReadXml(XmlNode thisNode, int index)
        {
            try
            {
                SRXmlNodeReader reader = new SRXmlNodeReader(thisNode);
                handleOffset = reader.ReadUInt64("handle_offset");
                parentHandleOffset = reader.ReadUInt64("parent_handle_offset");
                objectTypeHash = reader.ReadInt32("object_type_hash");
                nameIndex = reader.ReadUInt16("name_index");
                padding = reader.ReadUInt16("padding");
                XmlNodeList propertyNodes = reader.Node.SelectNodes("./properties/" + SRZoneProperty.XmlTagName);
                propertyList = new List<SRZoneProperty>(propertyNodes.Count);
                for (int i = 0; i < propertyNodes.Count; i++)
                    propertyList.Add(SRZoneProperty.Create(propertyNodes[i], i));
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
        /// Writes the data block to an XML document.
        /// </summary>
        /// <param name="parentNode">XML node to add this data block to.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        override public void WriteXml(XmlNode parentNode, int index)
        {
            try
            {
                SRXmlNodeWriter writer = new SRXmlNodeWriter(parentNode, XmlTagName, index + 1);

                writer.WriteHex("handle_offset", handleOffset);
                writer.WriteHex("parent_handle_offset", parentHandleOffset);
                writer.WriteHex("object_type_hash", objectTypeHash);
                writer.Write("name_index", nameIndex);
                writer.Write("padding", padding);

                XmlNode propertiesNode = writer.CreateNode("properties");
                int i = 0;
                foreach (SRZoneProperty property in propertyList)
                    property.WriteXml(propertiesNode, i++);
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
