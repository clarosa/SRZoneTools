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
    /// Abstract base class for data blocks that can appear multiple times in a sequence.
    /// Functions within this class are passed an index which is used for informational purposes only.
    /// </summary>
    abstract class SRDataBlockMultiBase : SRDataBlockBase
    {
        /// <summary>
        /// Reads a data block from a file binary stream.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the block from.  Must point to the beginning of the block.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        // abstract public void Read(SRBinaryReader binaryReader, int index, int size);

        /// <summary>
        /// Writes the data block to a file binary stream.
        /// </summary>
        /// <param name="binaryWriter">Binary writer to write the block to.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        abstract public void Write(SRBinaryWriter binaryWriter, int index);

        /// <summary>
        /// Reads the contents of a data block from an XML document.
        /// This assumes that the given XML element represents an instance of this data block.
        /// The XML element will contain additional child elements that contain the block's data.
        /// If any of the required child elements are not found, an SRZoneException will occur.
        /// </summary>
        /// <param name="thisNode">XML element which represents an instance of this data block.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        // abstract public void ReadXml(XmlNode thisNode, int index);

        /// <summary>
        /// Writes the data block to an XML document.
        /// This adds a single XML element to the given parent element which represents an instance of this data block.
        /// The XML element will contain additional child elements that contain the block's data.
        /// </summary>
        /// <param name="parentNode">XML element to add this data block to.</param>
        /// <param name="index">Index within a sequence (starts at 0).</param>
        abstract public void WriteXml(XmlNode parentNode, int index);
    }
}
