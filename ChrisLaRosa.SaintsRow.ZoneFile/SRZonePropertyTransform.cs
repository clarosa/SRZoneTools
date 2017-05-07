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
    /// <summary>
    /// Object property value which represents a transform.
    /// </summary>
    class SRZonePropertyTransform : SRZoneProperty
    {
        // FIELD VALUES

        protected SRPosition position;

        // PROPERTIES

        public override UInt16 Type { get { return TransformType; } }
        public SRPosition Position { get { return position; } }      // Callers can set individual member variables

        // CONSTRUCTORS

        public SRZonePropertyTransform(Int32 nameCrc, SRPosition position = null)
        {
            this.nameCrc = nameCrc;
            this.position = position != null ? position : new SRPosition();
        }

        // PROTECTED READERS / WRITERS

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadData(SRBinaryReader binaryReader, int size)
        {
            position = new SRPosition(binaryReader);
            SRTrace.WriteLine("        Value:     Position (x,y,z):  " + position.ToString());
            if (size != 12)
                throw new SRZoneFileException("Transform length is wrong.");
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteData(SRBinaryWriter binaryWriter)
        {
            position.Write(binaryWriter);
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadXmlData(XmlNode parentNode)
        {
            position = new SRPosition(parentNode);
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteXmlData(XmlNode parentNode)
        {
            position.WriteXml(parentNode);
        }

        // SYSTEM OBJECT OVERRIDES

        public override string ToString()
        {
            return position.ToString();
        }
    }
}
