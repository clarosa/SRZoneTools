//
// Copyright (C) 2015 Christopher LaRosa
//
// Based on Saints Row: The Third zone file format info and discussion here:
// https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ChrisLaRosa.SaintsRow.ZoneFile
{
    /// <summary>
    /// Raw data block.  This contains any arbitrary block of data.
    /// </summary>
    class SRRawDataBlock : SRDataBlockSingleBase
    {
        // CONSTANTS

        public const string XmlTagName = "rawdata";         // Used in XML documents

        // FIELD VALUES

        private Byte[] data;

        // CONSTRUCTORS

        public SRRawDataBlock()
        {
            data = new Byte[0];
        }

        public SRRawDataBlock(SRBinaryReader binaryReader, int size)
        {
            Read(binaryReader, size);
        }

        public SRRawDataBlock(XmlNode parentNode)
        {
            ReadXml(parentNode);
        }

        // ACCESSORS

        public static bool HasRawXmlData(XmlNode parentNode)
        {
            return parentNode.SelectSingleNode("./" + XmlTagName) != null;
        }

        // PRIVATE METHODS

        private string ToHexString()
        {
            StringBuilder builder = new StringBuilder(data.Length * 3);
            string format = "{0:X2}";
            foreach (Byte b in data)
            {
                builder.AppendFormat(format, b);
                format = " {0:X2}";
            }
            return builder.ToString();
        }

        // READERS / WRITERS

        /// <summary>
        /// Reads a data block from a file binary stream.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the block from.  Must point to the beginning of the block.</param>
        /// <param name="size">Maximum number of bytes to read.</param>
        public void Read(SRBinaryReader binaryReader, int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException("size", "Datablock size must not be zero.");
            data = new Byte[size];
            if (binaryReader.Read(data, 0, size) != size)
                throw new SRZoneFileException("EOF reached prematurely");
        }

        /// <summary>
        /// Writes the data block to a file binary stream.
        /// </summary>
        /// <param name="binaryWriter">Binary writer to write the block to.</param>
        override public void Write(SRBinaryWriter binaryWriter)
        {
            binaryWriter.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Reads the contents of a data block from an XML document.
        /// </summary>
        /// <param name="parentNode">XML node to read from.</param>
        override public void ReadXml(XmlNode parentNode)
        {
            List<byte> buffer = new List<byte>();
            XmlNode dataNode = parentNode.SelectSingleNode("./" + XmlTagName);
            if (dataNode == null)
                throw new SRZoneFileException("Missing child element \"" + XmlTagName + "\" in element \"" + parentNode.Name + "\""); 
            string[] values = dataNode.InnerText.Split(null);
            // Console.WriteLine(">>>> {0}", values.Length);
            foreach (string s in values)
                if (s != "")
                    buffer.Add(Convert.ToByte(s, 16));
            if (buffer.Count == 0)
                throw new SRZoneFileException("Element \"" + XmlTagName + "\" must not be empty.");
            data = buffer.ToArray();
        }

        /// <summary>
        /// Writes the data block to an XML document.
        /// </summary>
        /// <param name="parentNode">XML node to add this data block to.</param>
        override public void WriteXml(XmlNode parentNode)
        {
            XmlElement dataElement = parentNode.OwnerDocument.CreateElement(XmlTagName);
            dataElement.SetAttribute("format", "hex");
            dataElement.InnerText = ToHexString();
            parentNode.AppendChild(dataElement);
        }

        /// <summary>
        /// Returns the entire contents of the data block as a byte array.
        /// </summary>
        /// <returns>Data block contents.</returns>
        public Byte[] Data()
        {
            return data;
        }

        /// <summary>
        /// Returns the size of the data block in bytes.
        /// </summary>
        /// <returns>Size of the data block in bytes.</returns>
        public int Size()
        {
            return data.Length;
        }

        // SYSTEM OBJECT OVERRIDES

        public override string ToString()
        {
            return ToHexString();
        }
    }
}
