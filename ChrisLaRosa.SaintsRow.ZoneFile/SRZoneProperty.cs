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
    /// Object Property Abstract Base Class.
    /// One or more of these data blocks are contained within an SRZoneObject block.
    /// </summary>
    public abstract class SRZoneProperty : SRDataBlockMultiBase
    {
        // CONSTANTS

        public const string XmlTagName = "property";        // Used in XML documents
        public const string BlockName = "Property";         // Used in Exception reporting

        public const int Alignment = 4;                     // Align on a dword boundry
        public const int DataOffset = 8;                    // Offset from start of block where data begins

        public const UInt16 StringType = 0;                 // Type number representing a string
        public const UInt16 DataType = 1;                   // Type number representing arbitrary data
        public const UInt16 TransformType = 2;              // Type number representing a transformation
        public const UInt16 TransformOrientationType = 3;   // Type number representing a transformation and orientation

        public static readonly string[] PropertyTypeNames =
        {
            "string",
            "data",
            "compressed transform",
            "compressed transform with quaternion orientation"
        };

        // OPTIONS

        public static bool OptionParseValues = true;        // Parse values when reading zone files
        public static bool OptionPreservePadding = true;    // Preserve padding bytes after data

        // FIELD VALUES

        protected Int32 nameCrc = 0;
        private SRRawDataBlock paddingData = null;

        // PROPERTIES

        public abstract UInt16 Type { get; }                 // Cannot be set externally, since it's tied to the class
        public Int32 NameCrc { get { return nameCrc; } set { nameCrc = value; } }

        // FACTORY METHODS (STATIC)
        
        /// <summary>
        /// Creates a new Property item of the specified derived type.
        /// The item will initially have default data.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <returns></returns>
        public static SRZoneProperty Create(UInt16 type, Int32 nameCrc)
        {
            SRZoneProperty property;
            if (OptionParseValues)
            {
                switch (type)
                {
                    case StringType:
                        property = new SRZoneStringProperty(nameCrc); break;
                    case DataType:
                        property = new SRZoneDataProperty(type, nameCrc); break;
                    case TransformType:
                        property = new SRZoneTransformProperty(nameCrc); break;
                    case TransformOrientationType:
                        property = new SRZoneTransformOrientationProperty(nameCrc); break;
                    default:
                        throw new SRZoneFileException("Unknown property type (" + type.ToString() + ").");
                }
            }
            else
                property = new SRZoneDataProperty(type, nameCrc);
            return property;
        }

        /// <summary>
        /// Reads a property block from a file binary stream and creates a new property object containing the data.
        /// The returned property will be an instance of one of the concrete derived property classes.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the block from.  Must point to the beginning of the block.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        /// <returns>The new zone property which was read from the input stream.</returns>
        public static SRZoneProperty Create(SRBinaryReader binaryReader, int index)
        {
            SRZoneProperty property;
            try
            {
                // Read the common header
                binaryReader.Align(Alignment);
                SRTrace.WriteLine("");
                SRTrace.WriteLine("      PROPERTY #{0}:  [file offset 0x{1:X8}]", index + 1, binaryReader.BaseStream.Position);
                UInt16 type = binaryReader.ReadUInt16();
                string typeName = (type < PropertyTypeNames.Length) ? PropertyTypeNames[type] : "unknown";
                SRTrace.WriteLine("        Type:      {0} ({1})", type, typeName);
                UInt16 size = binaryReader.ReadUInt16();
                SRTrace.WriteLine("        Size:      {0} bytes", size);
                Int32 nameCrc = binaryReader.ReadInt32();
                SRTrace.WriteLine("        Name CRC:  0x{0:X8} ({0})", nameCrc, nameCrc);

                // Create the appropriate derived class based on the header information
                property = Create(type, nameCrc);

                // Read the class-specific data into the derived class
                property.ReadData(binaryReader, size);

                // WARNING:  There's a bunch of cruft after the "size" length which is part of the padding to
                // a dword boundry, but if we don't save it then the output file won't compare to the input file.
                if (OptionPreservePadding)
                {
                    var paddingSize = AlignPaddingSize(binaryReader.BaseStream.Position, Alignment);
                    if (paddingSize > 0)
                        property.paddingData = new SRRawDataBlock(binaryReader, paddingSize);
                    else
                        property.paddingData = null;
                }
            }
            catch (Exception e)
            {
                // Add context information for the error message
                if (index >= 0)
                    e.Data[BlockName] = index + 1;
                throw;
            }
            return property;
        }

        /// <summary>
        /// Reads a property from an XML document and creates a new property object containing the data.
        /// The returned property will be an instance of one of the concrete derived property classes.
        /// </summary>
        /// <param name="thisNode">XML node which represents an instance of this data block.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        /// <returns>The new zone property which was read from the input stream.</returns>
        public static SRZoneProperty Create(XmlNode parentNode, int index)
        {
            SRZoneProperty property;
            try
            {
                // Read the common header
                SRXmlNodeReader reader = new SRXmlNodeReader(parentNode);
                UInt16 type = reader.ReadUInt16("type");
                Int32 nameCrc = reader.ReadInt32("name_crc");

                // Create the appropriate derived class based on the header information
                if (SRRawDataBlock.HasRawXmlData(parentNode))
                    property = new SRZoneDataProperty(type, nameCrc);
                else
                    property = Create(type, nameCrc);

                // Read the class-specific data into the derived class
                property.ReadXmlData(SRXmlNodeReader.GetNode(parentNode, "value"));

                // Read the optional padding data
                XmlNode paddingNode = reader.GetNodeOptional("padding");
                if (paddingNode != null && SRRawDataBlock.HasRawXmlData(paddingNode))
                    property.paddingData = new SRRawDataBlock(paddingNode);
            }
            catch (Exception e)
            {
                // Add context information for the error message
                if (index >= 0)
                    e.Data[BlockName] = index + 1;
                throw;
            }
            return property;
        }

        // WRITERS

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
                binaryWriter.Write(Type);
                var positionSize = binaryWriter.BaseStream.Position;
                binaryWriter.Write((UInt16)0);                          // Size Will be rewritten below
                binaryWriter.Write(nameCrc);
                var positionDataStart = binaryWriter.BaseStream.Position;
                WriteData(binaryWriter);
                var actualDataSize = binaryWriter.BaseStream.Position - positionDataStart;
                if (paddingData != null && paddingData.Size() == AlignPaddingSize(binaryWriter.BaseStream.Position, Alignment))
                    paddingData.Write(binaryWriter);

                // Update the value size with the actual number of bytes written
                binaryWriter.Seek((int)positionSize, SeekOrigin.Begin);
                binaryWriter.Write((UInt16)actualDataSize);
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
        /// Writes the data block to an XML document.
        /// </summary>
        /// <param name="parentNode">XML node to add this data block to.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        override public void WriteXml(XmlNode parentNode, int index)
        {
            try
            {
                SRXmlNodeWriter writer = new SRXmlNodeWriter(parentNode, XmlTagName, index + 1);
                if (Type < PropertyTypeNames.Length)
                    writer.WriteComment(PropertyTypeNames[Type]);
                writer.Write("type", Type);
                // string typeName = (Type < PropertyTypeNames.Length) ? PropertyTypeNames[Type] : "unknown";
                // writer.Write("type_description", typeName);
                writer.WriteHex("name_crc", nameCrc);
                WriteXmlData(writer.CreateNode("value"));
                if (paddingData != null)
                    paddingData.WriteXml(writer.CreateNode("padding"));
            }
            catch (Exception e)
            {
                // Add context information for the error message
                if (index >= 0)
                    e.Data[BlockName] = index + 1;
                throw;
            }
        }

        // PROTECTED ABSTRACT METHODS

        /// <summary>
        /// Reads the property data for a specific property type from a file binary stream.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the property data from.
        ///   Must point to the beginning of the property data immediately following the header.</param>
        /// <param name="index">Number of bytes to read.</param>
        protected abstract void ReadData(SRBinaryReader binaryReader, int size);

        /// <summary>
        /// Reads the property data for a specific property type from an XML document.
        /// This will read one or more nodes from the given propertyNode.
        /// </summary>
        /// <param name="propertyNode">XML node which represents an instance of this property.</param>
        protected abstract void ReadXmlData(XmlNode propertyNode);

        /// <summary>
        /// Writes the property data for a specific property type to a file binary stream.
        /// </summary>
        /// <param name="binaryWriter">Binary writer to write the property data to.</param>
        protected abstract void WriteData(SRBinaryWriter binaryWriter);

        /// <summary>
        /// Writes the property data for a specific property type to an XML document.
        /// This will add one or more nodes to the given propertyNode.
        /// </summary>
        /// <param name="propertyNode">XML node which represents an instance of this property.</param>
        protected abstract void WriteXmlData(XmlNode propertyNode);
    }
}
