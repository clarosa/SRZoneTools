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
    /// Object property value which represents a string.
    /// </summary>
    public class SRZoneStringProperty : SRZoneProperty
    {
        // FIELD VALUES

        private String value;

        // PROPERTIES

        public override UInt16 Type { get { return StringType; } }
        public string Value { get { return value; } set { this.value = value; } }

        // CONSTRUCTORS

        public SRZoneStringProperty(Int32 nameCrc, string value = null)
        {
            this.nameCrc = nameCrc;
            this.value = value;
        }

        // PROTECTED READERS / WRITERS

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadData(SRBinaryReader binaryReader, int size)
        {
            value = binaryReader.ReadString();
            SRTrace.WriteLine("        Value:     \"" + value + "\"");
            if (value.Length + 1 != size)
                throw new SRZoneFileException("String length is wrong.");
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteData(SRBinaryWriter binaryWriter)
        {
            binaryWriter.Write(value);
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadXmlData(XmlNode thisNode)
        {
            SRXmlNodeReader reader = new SRXmlNodeReader(thisNode);
            value = reader.ReadString("string");
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteXmlData(XmlNode thisNode)
        {
            SRXmlNodeWriter writer = new SRXmlNodeWriter(thisNode);
            writer.Write("string", value);
        }

        // SYSTEM OBJECT OVERRIDES

        // IMPORTANT:  This must always return the string value only because other functions rely on that.
        public override string ToString()
        {
            return value;
        }
    }
}
