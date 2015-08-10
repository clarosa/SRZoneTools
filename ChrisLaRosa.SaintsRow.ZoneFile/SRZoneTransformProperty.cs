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
    class SRTransform
    {
        public float x, y, z;

        public SRTransform(float x = 0, float y = 0, float z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", x, y, z);
        }
    }

    /// <summary>
    /// Object property value which represents a transform.
    /// </summary>
    class SRZoneTransformProperty : SRZoneProperty
    {
        // FIELD VALUES

        protected SRTransform transform;

        // PROPERTIES

        public override UInt16 Type { get { return TransformType; } }
        public SRTransform Transform { get { return transform; } }      // Callers can set individual member variables

        // CONSTRUCTORS

        public SRZoneTransformProperty(Int32 nameCrc, SRTransform transform = null)
        {
            this.nameCrc = nameCrc;
            this.transform = transform != null ? transform : new SRTransform();
        }

        // PROTECTED READERS / WRITERS

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadData(SRBinaryReader binaryReader, int size)
        {
            transform = new SRTransform();
            transform.x = binaryReader.ReadSingle();
            transform.y = binaryReader.ReadSingle();
            transform.z = binaryReader.ReadSingle();
            SRTrace.WriteLine("        Value:     Position (x,y,z):  " + transform.ToString());
            if (size != 12)
                throw new SRZoneFileException("Transform length is wrong.");
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteData(SRBinaryWriter binaryWriter)
        {
            binaryWriter.Write(transform.x);
            binaryWriter.Write(transform.y);
            binaryWriter.Write(transform.z);
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void ReadXmlData(XmlNode parentNode)
        {
            SRXmlNodeReader reader = new SRXmlNodeReader(parentNode, "transform");
            transform = new SRTransform();
            transform.x = reader.ReadSingle("x");
            transform.y = reader.ReadSingle("y");
            transform.z = reader.ReadSingle("z");
        }

        // See description of this method in the abstract base class SRZoneProperty.
        protected override void WriteXmlData(XmlNode parentNode)
        {
            SRXmlNodeWriter writer = new SRXmlNodeWriter(parentNode, "transform");
            writer.Write("x", transform.x);
            writer.Write("y", transform.y);
            writer.Write("z", transform.z);
        }

        // SYSTEM OBJECT OVERRIDES

        public override string ToString()
        {
            return transform.ToString();
        }
    }
}
