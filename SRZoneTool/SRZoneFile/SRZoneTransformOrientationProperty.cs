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
    class SROrientation
    {
        public float x, y, z, w;
        public SROrientation(float x = 0, float y = 0, float z = 0, float w = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3})", x, y, z, w);
        }
    }

    /// <summary>
    /// Object property value which represents a transform.
    /// </summary>
    class SRZoneTransformOrientationProperty : SRZoneTransformProperty
    {
        // FIELD VALUES

        protected SROrientation orientation;

        // PROPERTIES

        public override UInt16 Type { get { return TransformOrientationType; } }
        public SROrientation Orientation { get { return orientation; } }        // Callers can set individual member variables

        // CONSTRUCTORS

        public SRZoneTransformOrientationProperty(Int32 nameCrc, SRTransform transform = null, SROrientation orientation = null)
            : base(nameCrc, transform)
        {
            this.orientation = orientation != null ? orientation : new SROrientation();
        }

        // PROTECTED READERS / WRITERS

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadData(SRBinaryReader binaryReader, int size)
        {
            base.ReadData(binaryReader, 12);    // Read the transform part
            orientation = new SROrientation();
            orientation.x = binaryReader.ReadSingle();
            orientation.y = binaryReader.ReadSingle();
            orientation.z = binaryReader.ReadSingle();
            orientation.w = binaryReader.ReadSingle();
            SRTrace.WriteLine("                   Orientation (x,y,z,w):  " + orientation.ToString());
            if (size != 28)
                throw new SRZoneFileException("Transform/Orientation length is wrong.");
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteData(SRBinaryWriter binaryWriter)
        {
            base.WriteData(binaryWriter);       // Write the transform part
            binaryWriter.Write(orientation.x);
            binaryWriter.Write(orientation.y);
            binaryWriter.Write(orientation.z);
            binaryWriter.Write(orientation.w);
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadXmlData(XmlNode parentNode)
        {
            base.ReadXmlData(parentNode);          // Read the transform part
            SRXmlNodeReader reader = new SRXmlNodeReader(parentNode, "orientation");
            orientation = new SROrientation();
            orientation.x = reader.ReadSingle("x");
            orientation.y = reader.ReadSingle("y");
            orientation.z = reader.ReadSingle("z");
            orientation.w = reader.ReadSingle("w");
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteXmlData(XmlNode parentNode)
        {
            base.WriteXmlData(parentNode);         // Write the transform part
            SRXmlNodeWriter writer = new SRXmlNodeWriter(parentNode, "orientation");
            writer.Write("x", orientation.x);
            writer.Write("y", orientation.y);
            writer.Write("z", orientation.z);
            writer.Write("w", orientation.w);
        }

        // SYSTEM OBJECT OVERRIDES

        public override string ToString()
        {
            return transform.ToString() + ", " + orientation.ToString();
        }
    }
}
