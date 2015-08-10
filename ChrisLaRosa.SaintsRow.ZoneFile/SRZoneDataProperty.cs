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
    /// Object property value which represents generic data.
    /// </summary>
    class SRZoneDataProperty : SRZoneProperty
    {
        // FIELD VALUES

        private UInt16 type;
        private SRRawDataBlock data;

        // PROPERTIES

        public override UInt16 Type { get { return type; } }
        public SRRawDataBlock Data { get { return data; } set { data = value; } }

        // CONSTRUCTORS

        public SRZoneDataProperty(UInt16 type, Int32 nameCrc, SRRawDataBlock data = null)
        {
            this.type = type;
            this.nameCrc = nameCrc;
            this.data = data;
        }

        // PROTECTED READERS / WRITERS

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadData(SRBinaryReader binaryReader, int size)
        {
            data = new SRRawDataBlock(binaryReader, size);
            SRTrace.WriteLine("        Value:     " + data.ToString());
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteData(SRBinaryWriter binaryWriter)
        {
            data.Write(binaryWriter);
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadXmlData(XmlNode thisNode)
        {
            data = new SRRawDataBlock(thisNode);
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteXmlData(XmlNode thisNode)
        {
            data.WriteXml(thisNode);
        }

        // SYSTEM OBJECT OVERRIDES

        public override string ToString()
        {
            return data.ToString();
        }
    }
}
