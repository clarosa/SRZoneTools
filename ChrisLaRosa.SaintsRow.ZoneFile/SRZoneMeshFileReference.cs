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
    /// Mesh File Reference.
    /// Zero or more of these data blocks are contained within the SRWorldZoneHeader.
    /// </summary>
    public class SRZoneMeshFileReference : SRDataBlockMultiBase
    {
        // CONSTANTS

        public const string XmlTagName = "mesh_file_reference";      // Used in XML documents
        public const string BlockName = "Mesh File Reference";       // Used in Exception reporting

        public const int Alignment = 1;

        // FIELD VALUES

        Int16 m_pos_x;
        Int16 m_pos_y;
        Int16 m_pos_z;
        Int16 pitch;
        Int16 bank;
        Int16 heading;
        string name;

        // LOCAL VARIABLES

        private SRVFileHeader vFileHeader;

        // CONSTRUCTORS

        public SRZoneMeshFileReference(SRVFileHeader vFileHeader)
        {
            this.vFileHeader = vFileHeader;
        }

        public SRZoneMeshFileReference(SRBinaryReader binaryReader, int index, SRVFileHeader vFileHeader)
        {
            this.vFileHeader = vFileHeader;
            Read(binaryReader, index);
        }

        public SRZoneMeshFileReference(XmlNode parentNode, int index, SRVFileHeader vFileHeader)
        {
            this.vFileHeader = vFileHeader;
            ReadXml(parentNode, index);
        }

        // CONVERSION METHODS

        // Uncompress a 16-bit mesh file reference position offset
        // https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/#post-78567
        public float MeshPosToFloat(Int16 pos)
        {
            return (float)pos / (float)(1 << 6);
        }

        public Int16 MeshPosToInt16(float pos)
        {
            return (Int16)Math.Round(pos * (1 << 6));
        }

        // Uncompress a 16-bit mesh file reference orientation
        // https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/#post-78567
        public float MeshOrientToFloat(Int16 pos)
        {
            return (float)pos / (float)(1 << 12);
        }

        public Int16 MeshOrientToInt16(float pos)
        {
            return (Int16)Math.Round(pos * (1 << 12));
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
                m_pos_x = binaryReader.ReadInt16();
                m_pos_y = binaryReader.ReadInt16();
                m_pos_z = binaryReader.ReadInt16();
                pitch = binaryReader.ReadInt16();
                bank = binaryReader.ReadInt16();
                heading = binaryReader.ReadInt16();
                int m_str_offset = binaryReader.ReadInt16();
                SRTrace.WriteLine("    REFERENCE #{0}:  {1},{2},{3},{4},{5},{6},{7}", index + 1, m_pos_x, m_pos_y, m_pos_z, pitch, bank, heading, m_str_offset);
                name = vFileHeader.GetReferenceNameByReadOffset(m_str_offset);
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
                binaryWriter.Write(m_pos_x);
                binaryWriter.Write(m_pos_y);
                binaryWriter.Write(m_pos_z);
                binaryWriter.Write(pitch);
                binaryWriter.Write(bank);
                binaryWriter.Write(heading);
                binaryWriter.Write((UInt16)vFileHeader.GetReferenceWriteOffsetByName(name));
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
                name = reader.ReadString("file");
                m_pos_x = MeshPosToInt16(reader.ReadSingle("pos_x"));
                m_pos_y = MeshPosToInt16(reader.ReadSingle("pos_y"));
                m_pos_z = MeshPosToInt16(reader.ReadSingle("pos_z"));
                pitch = MeshOrientToInt16(reader.ReadSingle("pitch"));
                bank = MeshOrientToInt16(reader.ReadSingle("bank"));
                heading = MeshOrientToInt16(reader.ReadSingle("heading"));
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
                writer.Write("file", name);
                writer.Write("pos_x", MeshPosToFloat(m_pos_x));
                writer.Write("pos_y", MeshPosToFloat(m_pos_y));
                writer.Write("pos_z", MeshPosToFloat(m_pos_z));
                writer.Write("pitch", MeshOrientToFloat(pitch));
                writer.Write("bank", MeshOrientToFloat(bank));
                writer.Write("heading", MeshOrientToFloat(heading));
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
