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
    /// XML Node Writer for Saints Row XML Zone Files.
    /// This class has helper methods that make it easier to write Saints Row XML Zone Files.
    /// </summary>
    class SRXmlNodeWriter
    {
        private XmlNode parentNode;

        public XmlNode Node
        {
            get
            {
                return parentNode;
            }
        }

        public SRXmlNodeWriter(XmlNode node)
        {
            parentNode = node;
        }

        public SRXmlNodeWriter(XmlNode node, string name, int index = 0)
        {
            parentNode = CreateNode(node, name, index);
        }

        public SRXmlNodeWriter(SRXmlNodeWriter parentWriter, string name, int index = 0)
        {
            parentNode = parentWriter.CreateNode(name, index);
        }

        public static XmlNode CreateNode(XmlNode parentNode, string name, int index = 0)
        {
            XmlElement newNode = parentNode.OwnerDocument.CreateElement(name);
            if (index > 0)
                newNode.SetAttribute("index", index.ToString());
            parentNode.AppendChild(newNode);
            return newNode;
        }

        public XmlNode CreateNode(string name, int index = 0)
        {
            return CreateNode(parentNode, name, index);
        }

        public static XmlNode CreateComment(XmlNode parentNode, string text)
        {
            XmlComment newComment = parentNode.OwnerDocument.CreateComment(" " + text + " ");
            parentNode.AppendChild(newComment);
            return newComment;
        }

        public XmlNode CreateComment(string text)
        {
            return CreateComment(parentNode, text);
        }

        public void Write(string name, string value, int index = 0)
        {
            XmlNode node = CreateNode(name, index);
            // The following is a work-around for a quirk of the XmlWriter.
            // Normally, if the innerText is not empty, XML entities are written as "<tag>value</tag>".
            // But if innerText is an empty string, the XML entity is written as "<tag>\n    </tag>".
            // This causes problems during read because the newline is added to the string (very BAD).
            // The solution is not to add ANY innerText when the string is an empty string, which properly
            // causes an empty string to be written (as "<tag />").
            if (value != "")
                node.InnerText = value;
        }

        public void Write(string name, Int16 value, int index = 0)
        {
            Write(name, String.Format("{0}", value), index);
        }

        public void Write(string name, Int32 value, int index = 0)
        {
            Write(name, String.Format("{0}", value), index);
        }

        public void Write(string name, Int64 value, int index = 0)
        {
            Write(name, String.Format("{0}", value), index);
        }

        public void Write(string name, UInt16 value, int index = 0)
        {
            Write(name, String.Format("{0}", value), index);
        }

        public void Write(string name, UInt32 value, int index = 0)
        {
            Write(name, String.Format("{0}", value), index);
        }

        public void Write(string name, UInt64 value, int index = 0)
        {
            Write(name, String.Format("{0}", value), index);
        }

        public void Write(string name, Single value, int index = 0)
        {
            string s = ((double)value).ToString();
            if (value == 0)
            {
                Byte[] bytes = new Byte[4];
                Single[] values = new Single[1] { value };
                Buffer.BlockCopy(values, 0, bytes, 0, 4); // byte-wise copy of ff into ii
                if (bytes[3] == 0x80)
                    s = "-0";
            }
            Write(name, s, index);
        }

        public void WriteHex(string name, Int32 value, int index = 0)
        {
            Write(name, String.Format("0x{0:X8}", (UInt32)value), index);
        }

        public void WriteHex(string name, UInt16 value, int index = 0)
        {
            Write(name, String.Format("0x{0:X4}", value), index);
        }

        public void WriteHex(string name, UInt32 value, int index = 0)
        {
            Write(name, String.Format("0x{0:X8}", value), index);
        }

        public void WriteHex(string name, UInt64 value, int index = 0)
        {
            Write(name, String.Format("0x{0:X16}", value), index);
        }

        public void WriteComment(string text)
        {
            CreateComment(text);
        }
    }
}
