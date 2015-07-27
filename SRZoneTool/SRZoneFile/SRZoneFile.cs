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
    /// Reads and writes Saints Row Zone Files.
    /// </summary>
    class SRZoneFile
    {
        // CONSTANTS

        public const string XmlTagName = "czn_pc";      // Used in XML documents

        // FIELD VALUES
        
        private List<SRZoneSection> fileSections = new List<SRZoneSection>();

        /// <summary>
        /// Reads the zone file into memory.
        /// </summary>
        /// <param name="cznFile">File system path to the ".czn_pc" zone file.</param>
        public void ReadFile(string cznFile)
        {
            SRTrace.WriteLine("");
            SRTrace.WriteLine("-------------------------------------------------------------------------------");
            SRTrace.WriteLine("ZONE DATA FILE:  " + Path.GetFileName(cznFile));

            FileStream stream = null;
            try
            {
                stream = new FileStream(cznFile, FileMode.Open, FileAccess.Read);
                SRBinaryReader binaryReader = new SRBinaryReader(stream);
                int index = 0;
                while (binaryReader.BaseStream.Position <= binaryReader.BaseStream.Length - 4)
                    fileSections.Add(new SRZoneSection(binaryReader, index++));
            }
            catch (Exception e)
            {
                // Add context information for the error message
                e.Data["Action"] = "Reading Zone file";
                throw;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        /// <summary>
        /// Writes the zone to a file.
        /// </summary>
        /// <param name="cznFile">File system path to the ".czn_pc" zone file.</param>
        public void WriteFile(string cznFile)
        {
            FileStream stream = null;
            SRBinaryWriter binaryWriter = null;
            try
            {
                stream = File.Open(cznFile, FileMode.Create);
                binaryWriter = new SRBinaryWriter(stream);
                int index = 0;
                foreach (SRZoneSection section in fileSections)
                    section.Write(binaryWriter, index++);
            }
            catch (Exception e)
            {
                // Add context information for the error message
                e.Data["Action"] = "Writing Zone file";
                throw;
            }
            finally
            {
                if (binaryWriter != null)
                    binaryWriter.Close();
            }
        }

        /// <summary>
        /// Reads the zone file into memory.
        /// </summary>
        /// <param name="cznFile">File system path to the ".czn_pc" zone file.</param>
        public void ReadXmlFile(string xmlFile)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlFile);
                XmlNodeList selectNodeList = xmlDocument.SelectNodes("/root/" + XmlTagName + "/" + SRZoneSection.XmlTagName);
                int index = 0;
                foreach (XmlNode selectNode in selectNodeList)
                    fileSections.Add(new SRZoneSection(selectNode, index++));
            }
            catch (Exception e)
            {
                // Add context information for the error message
                e.Data["Action"] = "Reading XML file";
                throw;
            }
        }

        /// <summary>
        /// Writes the zone file to an XML file.
        /// </summary>
        /// <param name="xmlFile">File system path to the XML file to be created.</param>
        public void WriteXmlFile(string xmlFile)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlElement xmlRoot = xmlDocument.CreateElement("root");
                xmlDocument.AppendChild(xmlRoot);
                XmlElement cznFileNode = xmlDocument.CreateElement(XmlTagName);
                xmlRoot.AppendChild(cznFileNode);
                int index = 0;
                foreach (SRZoneSection section in fileSections)
                    section.WriteXml(cznFileNode, index++);
                xmlDocument.Save(xmlFile);
            }
            catch (Exception e)
            {
                // Add context information for the error message
                e.Data["Action"] = "Writing XML file";
                throw;
            }
        }
    }
}
