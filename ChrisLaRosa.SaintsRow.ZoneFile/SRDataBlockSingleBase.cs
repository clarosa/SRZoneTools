//
// Copyright (C) 2015 Christopher LaRosa
//
// Based on Saints Row: The Third zone file format info and discussion here:
// https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/
//

using System;
using System.Xml;

namespace ChrisLaRosa.SaintsRow.ZoneFile
{
    /// <summary>
    /// Abstract base class for data blocks that can have only a single instance within their parent data block.
    /// </summary>
    abstract class SRDataBlockSingleBase : SRDataBlockBase
    {
        /// <summary>
        /// Reads a data block from a file binary stream.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the block from.  Must point to the beginning of the block.</param>
        /// <param name="size">Maximum number of bytes to read.</param>
        // abstract public void Read(SRBinaryReader binaryReader, int size);

        /// <summary>
        /// Writes the data block to a file binary stream.
        /// </summary>
        /// <param name="binaryWriter">Binary writer to write the block to.</param>
        abstract public void Write(SRBinaryWriter binaryWriter);

        /// <summary>
        /// Reads the contents of a data block from an XML document.
        /// This reads a single XML element from the given parent element with a name that reflects the data block type.
        /// The XML element will contain additional child elements that contain the block's data.
        /// If the XML element or any of the required child elements are not found, an SRZoneException will occur.
        /// </summary>
        /// <param name="parentNode">XML element which contains an instance of this data block.</param>
        abstract public void ReadXml(XmlNode parentNode);

        /// <summary>
        /// Writes the data block to an XML document.
        /// This adds a single XML element to the given parent element which represents an instance of this data block.
        /// The XML element will contain additional child elements that contain the block's data.
        /// </summary>
        /// <param name="parentNode">XML element to add this data block to.</param>
        abstract public void WriteXml(XmlNode parentNode);
    }
}
