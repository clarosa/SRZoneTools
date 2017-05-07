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
    /// Object Data Section.
    /// This data block appears within the CPU Data of a Section block of type 0x2234.
    /// It contains a header followed by a list of Object data items.
    /// </summary>
    public class SRZoneFastObject : SRDataBlockMultiBase
    {

        public const string XmlTagName = "fast_object";     // Used in XML documents
        public const string BlockName = "Fast Object";      // Used in Exception reporting

        public const int Alignment = 1;

        // OPTIONS

        // FIELD VALUES

        public string name;
        public string fileName;
        public UInt32 materialMapOffset;
        private UInt64 handle;
        private UInt32 mRenderUpdateNext;
        private SRPosition position;
        private SRQuaternionOrientation orientation;
        private SRRawDataBlock unparsedData;

        // PROPERTIES

        public UInt64 Handle { get { return handle; } }

        // LOCAL VARIABLES

        private SRZoneSectionDataCrunchedGeometry geometrySection = null;

        // CONSTRUCTORS

        public SRZoneFastObject(SRZoneSectionDataCrunchedGeometry geometrySection)
        {
            this.geometrySection = geometrySection;
        }

        public SRZoneFastObject(SRZoneSectionDataCrunchedGeometry geometrySection, SRBinaryReader binaryReader, int index)
        {
            this.geometrySection = geometrySection;
            Read(binaryReader, index);
        }

        public SRZoneFastObject(SRZoneSectionDataCrunchedGeometry geometrySection, XmlNode parentNode, int index)
        {
            this.geometrySection = geometrySection;
            ReadXml(parentNode, index);
        }

        // READERS / WRITERS

        /// <summary>
        /// Reads a data block from a file binary stream.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the block from.  Must point to the beginning of the block.</param>
        /// <param name="size">Maximum number of bytes to read.</param>
        public void Read(SRBinaryReader binaryReader, int index)
        {
            try
            {
                long start = binaryReader.BaseStream.Position;
                SRTrace.WriteLine("");
                SRTrace.WriteLine("      FAST OBJECT #{0}:  [file offset 0x{1:X8}]", index + 1, binaryReader.BaseStream.Position);
                SRTrace.WriteLine("        Object Handle:          0x{0:X16}", handle = binaryReader.ReadUInt64());
                SRTrace.WriteLine("        m_render_update_next:   0x{0:X8}", mRenderUpdateNext = binaryReader.ReadUInt32());
                UInt32 nameOffset = binaryReader.ReadUInt32();
                SRTrace.WriteLine("        name_offset:            0x{0:X8}", nameOffset);
                name = geometrySection.GetMeshNameByReadOffset(nameOffset);
                SRTrace.WriteLine("        Name (from offset):     {0}", name);
                position = new SRPosition(binaryReader);
                SRTrace.WriteLine("        Position (x,y,z):       {0}", position.ToString());
                orientation = new SRQuaternionOrientation(binaryReader);
                SRTrace.WriteLine("        Orientation (x,y,z,w):  {0}", orientation.ToString());
                SRTrace.WriteLine("        etc.");
                int unparsedSize = 112 - 44;
                unparsedData = new SRRawDataBlock(binaryReader, unparsedSize);
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
        override public void Write(SRBinaryWriter binaryWriter, int index)
        {
            try
            {
                binaryWriter.Write(handle);
                binaryWriter.Write(mRenderUpdateNext);
                UInt32 nameOffset = geometrySection.GetMeshWriteOffsetsByName(name);
                binaryWriter.Write(nameOffset);
                position.Write(binaryWriter);
                orientation.Write(binaryWriter);
                unparsedData.Write(binaryWriter);
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
        /// This may read one or more nodes from the given parentNode.
        /// This can return false if it doesn't find any data nodes in the parentNode.
        /// </summary>
        /// <param name="parentNode">XML node to read from.</param>
        /// <returns>true</returns>
        public void ReadXml(XmlNode thisNode, int index)
        {
            try
            {
                SRXmlNodeReader reader = new SRXmlNodeReader(thisNode);

                name = reader.ReadString("name");
                fileName = reader.ReadString("file");
                materialMapOffset = reader.ReadUInt32("material_map_offset");
                handle = reader.ReadUInt64("handle");
                mRenderUpdateNext = reader.ReadUInt32("m_render_update_next");
                position = new SRPosition(thisNode);
                orientation = new SRQuaternionOrientation(thisNode);
                unparsedData = new SRRawDataBlock(thisNode);
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
        /// This may add one or more nodes to the given parentNode.
        /// </summary>
        /// <param name="parentNode">XML node to add this data block to.</param>
        /// <param name="index">Optional index attribute.</param>
        override public void WriteXml(XmlNode parentNode, int index)
        {
            try
            {
                SRXmlNodeWriter writer = new SRXmlNodeWriter(parentNode, XmlTagName, index + 1);

                writer.Write("name", name);
                writer.Write("file", fileName);
                writer.Write("material_map_offset", materialMapOffset);
                writer.WriteHex("handle", handle);
                writer.WriteHex("m_render_update_next", mRenderUpdateNext);
                position.WriteXml(writer.Node);
                orientation.WriteXml(writer.Node);
                unparsedData.WriteXml(writer.Node);
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
