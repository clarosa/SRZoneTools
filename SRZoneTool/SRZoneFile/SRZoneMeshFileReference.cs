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
    class SRZoneMeshFileReference : SRDataBlockMultiBase
    {
        // CONSTANTS

        public const string XmlTagName = "mesh_file_reference";      // Used in XML documents
        public const string BlockName = "Mesh File Reference";       // Used in Exception reporting

        public const int Alignment = 1;

        // FIELD VALUES

        private SRRawDataBlock data;

        // CONSTRUCTORS

        public SRZoneMeshFileReference()
        {
        }

        public SRZoneMeshFileReference(SRBinaryReader binaryReader, int index)
        {
            Read(binaryReader, index);
        }

        public SRZoneMeshFileReference(XmlNode parentNode, int index)
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
                data = new SRRawDataBlock(binaryReader, 14);
                SRTrace.WriteLine("    REFERENCE #{0}:  {1}", index + 1, data.ToString());
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
                data.Write(binaryWriter);
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
                data = new SRRawDataBlock(thisNode);
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
                data.WriteXml(writer.Node);
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
