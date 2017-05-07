//
// Copyright (C) 2015-2017 Christopher LaRosa
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
    /// Crunched Reference Geometry Data Section.
    /// This data block appears within the CPU Data of a Section block of type 0x2233.
    /// </summary>
    public class SRZoneSectionDataCrunchedGeometry : SRDataBlockSingleBase
    {
        public const string XmlTagName = "crunched_geometry_data";       // Used in XML documents
        public const string SectionTitle = "Crunched Geometry Section";  // Used in Exception reporting
        public const int Alignment = 1;

        // OPTIONS

        // FIELD VALUES

        private UInt32 meshCount;       // number of meshes
        private UInt32 meshNamesSize;
        private List<SRZoneFastObject> fastObjectList;
        private SRDataBlockSingleBase rawData;

        // PROPERTIES

        // LOCAL VARIABLES

        private SRVFileHeader vFileHeader = null;
        private Dictionary<UInt32, string> meshNamesByReadOffset;
        private Dictionary<string, UInt32> meshWriteOffsetsByName;

        // CONSTRUCTORS

        public SRZoneSectionDataCrunchedGeometry(SRVFileHeader vFileHeader)
        {
            ValidateAndSetHeader(vFileHeader);
        }

        public SRZoneSectionDataCrunchedGeometry(SRVFileHeader vFileHeader, SRBinaryReader binaryReader, int size)
        {
            ValidateAndSetHeader(vFileHeader);
            Read(binaryReader, size);
        }

        public SRZoneSectionDataCrunchedGeometry(SRVFileHeader vFileHeader, XmlNode parentNode)
        {
            ValidateAndSetHeader(vFileHeader);
            ReadXml(parentNode);
        }

        // HELPERS

        private void ValidateAndSetHeader(SRVFileHeader vFileHeader)
        {
            if (vFileHeader == null)
                throw(new SRZoneFileException("Zone header is required to parse " + SectionTitle + "."));
            this.vFileHeader = vFileHeader;
        }

        public string GetMeshNameByReadOffset(UInt32 offset)
        {
            if (!meshNamesByReadOffset.ContainsKey(offset))
                // return "<invalid>";
                throw new SRZoneFileException("Mesh does not exist in " + SectionTitle + " at offset \"" + offset.ToString() + "\".");
            return meshNamesByReadOffset[offset];
        }

        public UInt32 GetMeshWriteOffsetsByName(string name)
        {
            if (!meshWriteOffsetsByName.ContainsKey(name))
                throw new SRZoneFileException("Mesh name \"" + name + "\" does not exist in " + SectionTitle + ".");
            return meshWriteOffsetsByName[name];
        }

        // READERS / WRITERS

        /// <summary>
        /// Reads a data block from a file binary stream.
        /// </summary>
        /// <param name="binaryReader">Binary reader to read the block from.  Must point to the beginning of the block.</param>
        /// <param name="size">Maximum number of bytes to read.</param>
        public void Read(SRBinaryReader binaryReader, int size)
        {
            long cpuDataStartOffset = binaryReader.BaseStream.Position;

            SRTrace.WriteLine("");
            SRTrace.WriteLine("  CRUNCHED REFERENCE GEOMETRY:");
            meshCount = binaryReader.ReadUInt32();
            SRTrace.WriteLine("    Number of Meshes:     {0}", meshCount);
            meshNamesSize = binaryReader.ReadUInt32();
            SRTrace.WriteLine("    Mesh Names Size:      {0}", meshNamesSize);

            fastObjectList = new List<SRZoneFastObject>((int)meshCount);
            for (int i = 0; i < meshCount; i++)
                fastObjectList.Add(new SRZoneFastObject(this));

            long refDataOffset = binaryReader.BaseStream.Position;
            long refDataEnd = refDataOffset + meshNamesSize;
            SRTrace.WriteLine("");
            SRTrace.WriteLine("    MESH NAMES LIST:  [file offset 0x{0:X8}]", binaryReader.BaseStream.Position);
            meshNamesByReadOffset = new Dictionary<UInt32, string>();
            for (int i = 0; binaryReader.BaseStream.Position < refDataEnd; i++)
            {
                long position = binaryReader.BaseStream.Position;
                string name = binaryReader.ReadString();
                binaryReader.ReadByte();
                meshNamesByReadOffset[(UInt32)(position - refDataOffset)] = name;
                SRTrace.WriteLine("    {0,4}.  {1}", i + 1, name);
                binaryReader.Align(2);  // Align on a 2-byte boundry
            }
            if (binaryReader.BaseStream.Position != refDataEnd)
                throw new SRZoneFileException("Mesh Names List not expected size.");

            binaryReader.Align(16);  // Align on a 16-byte boundry
            SRTrace.WriteLine("");
            SRTrace.WriteLine("    MESHES LIST:  [file offset 0x{0:X8}]", binaryReader.BaseStream.Position);
            SRTrace.WriteLine("           *r_mesh  *material_map  File Name (from *r_mesh)");
            SRTrace.WriteLine("           -------  -------------  ------------------------");
            for (int i = 0; i < meshCount; i++)
            {
                UInt32 rMeshOffset = binaryReader.ReadUInt32();
                UInt32 materialMapOffset = binaryReader.ReadUInt32();
                string name = vFileHeader.GetReferenceNameByReadOffset(rMeshOffset);
                fastObjectList[i].fileName = name;
                fastObjectList[i].materialMapOffset = materialMapOffset;
                SRTrace.WriteLine("    {0,4}. {1,6}       {2,6}      {3}", i + 1, rMeshOffset, materialMapOffset, name);
            }

            binaryReader.Align(16);  // Align on a 16-byte boundry
            SRTrace.WriteLine("");
            SRTrace.WriteLine("    FAST OBJECTS LIST:");
            for (int i = 0; i < meshCount; i++)
                fastObjectList[i].Read(binaryReader, i);

            SRTrace.WriteLine("");
            SRTrace.WriteLine("    MESH VARIANT DATA:  [file offset 0x{0:X8}]", binaryReader.BaseStream.Position);
            SRTrace.WriteLine("      ???");

            int rawDataSize = size - (int)(binaryReader.BaseStream.Position - cpuDataStartOffset);
            if (rawDataSize < 0)
                throw new SRZoneFileException("Actual section size exceeds section size specified in header.");
            rawData = new SRRawDataBlock(binaryReader, rawDataSize);
        }
    
        /// <summary>
        /// Writes the data block to a file binary stream.
        /// </summary>
        /// <param name="binaryWriter">Binary writer to write the block to.</param>
        override public void Write(SRBinaryWriter binaryWriter)
        {
            UInt32 meshCount = (UInt32)fastObjectList.Count;
            binaryWriter.Write(meshCount);
            var meshNamesSizeStart = binaryWriter.BaseStream.Position;
            binaryWriter.Write((UInt32)0);      // meshNamesSize (will rewrite later)
            meshWriteOffsetsByName = new Dictionary<string, UInt32>();
            long startPosition = binaryWriter.BaseStream.Position;
            foreach (SRZoneFastObject fastObject in fastObjectList)
            {
                if (!meshWriteOffsetsByName.ContainsKey(fastObject.name))
                {
                    meshWriteOffsetsByName[fastObject.name] = (UInt32)(binaryWriter.BaseStream.Position - startPosition);
                    binaryWriter.Write(fastObject.name);
                    binaryWriter.Write((Byte)0);
                    binaryWriter.Align(2);
                }
            }
            var meshNamesSize = binaryWriter.BaseStream.Position - startPosition;
            binaryWriter.Seek((int)meshNamesSizeStart, SeekOrigin.Begin);
            binaryWriter.Write((UInt32)meshNamesSize);
            binaryWriter.Seek(0, SeekOrigin.End);
            binaryWriter.Align(16);  // Align on a 16-byte boundry
            foreach (SRZoneFastObject fastObject in fastObjectList)
            {
                UInt32 rMeshOffset = (UInt32)vFileHeader.GetReferenceWriteOffsetByName(fastObject.fileName);
                binaryWriter.Write(rMeshOffset);
                binaryWriter.Write(fastObject.materialMapOffset);
            }
            binaryWriter.Align(16);  // Align on a 16-byte boundry
            for (int i = 0; i < meshCount; i++)
                fastObjectList[i].Write(binaryWriter, i);
            rawData.Write(binaryWriter);
        }

        /// <summary>
        /// Reads the contents of a data block from an XML document.
        /// This may read one or more nodes from the given parentNode.
        /// This can return false if it doesn't find any data nodes in the parentNode.
        /// </summary>
        /// <param name="parentNode">XML node to read from.</param>
        /// <returns>true</returns>
        override public void ReadXml(XmlNode parentNode)
        {
            SRXmlNodeReader reader = new SRXmlNodeReader(parentNode, XmlTagName);

            XmlNodeList fastObjectNodes = reader.Node.SelectNodes("./fast_objects/" + SRZoneFastObject.XmlTagName);
            var numFastObjects = fastObjectNodes.Count;
            fastObjectList = new List<SRZoneFastObject>(numFastObjects);
            for (int i = 0; i < numFastObjects; i++)
                fastObjectList.Add(new SRZoneFastObject(this, fastObjectNodes[i], i));
            XmlNode meshVariantDataNode = reader.GetNode("mesh_variant_data");
            rawData = new SRRawDataBlock(meshVariantDataNode);
        }
        
        /// <summary>
        /// Writes the data block to an XML document.
        /// This may add one or more nodes to the given parentNode.
        /// </summary>
        /// <param name="parentNode">XML node to add this data block to.</param>
        /// <param name="index">Optional index attribute.</param>
        override public void WriteXml(XmlNode parentNode)
        {
            int index;

            SRXmlNodeWriter writer = new SRXmlNodeWriter(parentNode, XmlTagName);

            XmlNode fastObjectsNode = writer.CreateNode("fast_objects");
            index = 0;
            foreach (SRZoneFastObject srFastObject in fastObjectList)
                srFastObject.WriteXml(fastObjectsNode, index++);
            XmlNode meshVariantDataNode = writer.CreateNode("mesh_variant_data");
            rawData.WriteXml(meshVariantDataNode);
        }
    }
}
