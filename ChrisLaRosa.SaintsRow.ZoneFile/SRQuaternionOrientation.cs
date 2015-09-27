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
    public class SRQuaternionOrientation
    {
        public float x, y, z, w;

        public SRQuaternionOrientation(float x = 0, float y = 0, float z = 0, float w = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public SRQuaternionOrientation(SRBinaryReader binaryReader)
        {
            Read(binaryReader);
        }

        public SRQuaternionOrientation(XmlNode parentNode)
        {
            ReadXml(parentNode);
        }

        public void Read(SRBinaryReader binaryReader)
        {
            x = binaryReader.ReadSingle();
            y = binaryReader.ReadSingle();
            z = binaryReader.ReadSingle();
            w = binaryReader.ReadSingle();
        }

        public void Write(SRBinaryWriter binaryWriter)
        {
            binaryWriter.Write(x);
            binaryWriter.Write(y);
            binaryWriter.Write(z);
            binaryWriter.Write(w);
        }

        public void ReadXml(XmlNode parentNode)
        {
            SRXmlNodeReader reader = new SRXmlNodeReader(parentNode, "orientation");
            x = reader.ReadSingle("x");
            y = reader.ReadSingle("y");
            z = reader.ReadSingle("z");
            w = reader.ReadSingle("w");
        }

        public void WriteXml(XmlNode parentNode)
        {
            SRXmlNodeWriter writer = new SRXmlNodeWriter(parentNode, "orientation");
            writer.Write("x", x);
            writer.Write("y", y);
            writer.Write("z", z);
            writer.Write("w", w);
        }
        
        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3})", x, y, z, w);
        }
    }
}
