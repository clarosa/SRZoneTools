//
// Copyright (C) 2015 Christopher LaRosa
//
// Based on Saints Row: The Third zone file format info and discussion here:
// https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace ChrisLaRosa.SaintsRow.ZoneFile
{
    public class SRPosition
    {
        public float x, y, z;

        public SRPosition(float x = 0, float y = 0, float z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SRPosition(SRBinaryReader binaryReader)
        {
            Read(binaryReader);
        }

        public SRPosition(XmlNode parentNode, string name = "position")
        {
            ReadXml(parentNode, name);
        }

        public void Read(SRBinaryReader binaryReader)
        {
            x = binaryReader.ReadSingle();
            y = binaryReader.ReadSingle();
            z = binaryReader.ReadSingle();
        }

        public void Write(SRBinaryWriter binaryWriter)
        {
            binaryWriter.Write(x);
            binaryWriter.Write(y);
            binaryWriter.Write(z);
        }

        public void ReadXml(XmlNode parentNode, string name = "position")
        {
            SRXmlNodeReader reader = new SRXmlNodeReader(parentNode, name);
            x = reader.ReadSingle("x");
            y = reader.ReadSingle("y");
            z = reader.ReadSingle("z");
        }

        public void WriteXml(XmlNode parentNode, string name = "position")
        {
            SRXmlNodeWriter writer = new SRXmlNodeWriter(parentNode, name);
            writer.Write("x", x);
            writer.Write("y", y);
            writer.Write("z", z);
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", x, y, z);
        }
    }
}
