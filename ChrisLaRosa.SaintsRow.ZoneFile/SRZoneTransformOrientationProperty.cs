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
    class SRZoneTransformOrientationProperty : SRZoneTransformProperty
    {
        // FIELD VALUES

        protected SRQuaternionOrientation orientation;

        // PROPERTIES

        public override UInt16 Type { get { return TransformOrientationType; } }
        public SRQuaternionOrientation Orientation { get { return orientation; } }        // Callers can set individual member variables

        // CONSTRUCTORS

        public SRZoneTransformOrientationProperty(Int32 nameCrc, SRPosition position = null, SRQuaternionOrientation orientation = null)
            : base(nameCrc, position)
        {
            this.orientation = orientation != null ? orientation : new SRQuaternionOrientation();
        }

        // PROTECTED READERS / WRITERS

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadData(SRBinaryReader binaryReader, int size)
        {
            base.ReadData(binaryReader, 12);    // Read the position part
            orientation = new SRQuaternionOrientation(binaryReader);
            SRTrace.WriteLine("                   Orientation (x,y,z,w):  " + orientation.ToString());
            if (size != 28)
                throw new SRZoneFileException("Transform/Orientation length is wrong.");
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteData(SRBinaryWriter binaryWriter)
        {
            base.WriteData(binaryWriter);       // Write the position part
            orientation.Write(binaryWriter);
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadXmlData(XmlNode parentNode)
        {
            base.ReadXmlData(parentNode);          // Read the position part
            orientation = new SRQuaternionOrientation(parentNode);
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteXmlData(XmlNode parentNode)
        {
            base.WriteXmlData(parentNode);         // Write the position part
            orientation.WriteXml(parentNode);
        }

        // SYSTEM OBJECT OVERRIDES

        public override string ToString()
        {
            return position.ToString() + ", " + orientation.ToString();
        }
    }
}
