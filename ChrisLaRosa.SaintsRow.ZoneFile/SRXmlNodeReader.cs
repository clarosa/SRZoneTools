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
    /// XML Node Reader for Saints Row XML Zone Files.
    /// This class has helper methods that make it easier to read Saints Row XML Zone Files.
    /// </summary>
    class SRXmlNodeReader
    {
        private XmlNode parentNode;

        public XmlNode Node
        {
            get
            {
                return parentNode;
            }
        }

        public SRXmlNodeReader(XmlNode node)
        {
            parentNode = node;
        }

        public SRXmlNodeReader(XmlNode node, string name)
        {
            parentNode = GetNode(node, name);
        }

        public static XmlNode GetNode(XmlNode parentNode, string name)
        {
            XmlNode thisNode = parentNode.SelectSingleNode("./" + name);
            if (thisNode == null)
                throw new SRZoneFileException("Missing child element \"" + name + "\" in element \"" + parentNode.Name + "\"");
            return thisNode;
        }

        public XmlNode GetNode(string name)
        {
            return GetNode(parentNode, name);
        }

        public static XmlNode GetNodeOptional(XmlNode parentNode, string name)
        {
            return parentNode.SelectSingleNode("./" + name);
        }

        public XmlNode GetNodeOptional(string name)
        {
            return GetNodeOptional(parentNode, name);
        }

        public static string ReadString(XmlNode thisNode)
        {
            return thisNode.InnerText;
        }

        public string ReadString(string name)
        {
            return ReadString(GetNode(name));
        }

        public Int16 ReadInt16(string name)
        {
            string s = ReadString(name);
            if (s.Length > 2 && s.ToLower().Substring(0, 2) == "0x")
                return Convert.ToInt16(s.Substring(2), 16);
            return Convert.ToInt16(s);
        }

        public Int32 ReadInt32(string name)
        {
            string s = ReadString(name);
            if (s.Length > 2 && s.ToLower().Substring(0, 2) == "0x")
                return Convert.ToInt32(s.Substring(2), 16);
            return Convert.ToInt32(s);
        }

        public Int64 ReadInt64(string name)
        {
            string s = ReadString(name);
            if (s.Length > 2 && s.ToLower().Substring(0, 2) == "0x")
                return Convert.ToInt64(s.Substring(2), 16);
            return Convert.ToInt64(s);
        }

        public UInt16 ReadUInt16(string name)
        {
            string s = ReadString(name);
            if (s.Length > 2 && s.ToLower().Substring(0, 2) == "0x")
                return Convert.ToUInt16(s.Substring(2), 16);
            return Convert.ToUInt16(s);
        }

        public UInt32 ReadUInt32(string name)
        {
            string s = ReadString(name);
            if (s.Length > 2 && s.ToLower().Substring(0, 2) == "0x")
                return Convert.ToUInt32(s.Substring(2), 16);
            return Convert.ToUInt32(s);
        }

        public static UInt64 ReadUInt64(XmlNode thisNode)
        {
            string s = ReadString(thisNode);
            if (s.Length > 2 && s.ToLower().Substring(0, 2) == "0x")
                return Convert.ToUInt64(s.Substring(2), 16);
            return Convert.ToUInt64(s);
        }

        public UInt64 ReadUInt64(string name)
        {
            return ReadUInt64(GetNode(parentNode, name));
        }

        public Single ReadSingle(string name)
        {
            string s = ReadString(name);
            if (s == "-0")
            {
                Byte[] negativeZero = new Byte[4] { 0x00, 0x00, 0x00, 0x80 };
                Single[] value = new Single[1];
                Buffer.BlockCopy(negativeZero, 0, value, 0, 4); // byte-wise copy of ff into ii
                return value[0];
            }
            return Convert.ToSingle(s);
        }
    }
}
