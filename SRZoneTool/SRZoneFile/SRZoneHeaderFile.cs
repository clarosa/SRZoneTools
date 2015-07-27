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
    /// Reads and writes Saints Row Zone Header Files.
    /// </summary>
    class SRZoneHeaderFile
    {
        // CONSTANTS

        public const string XmlTagName = "czh_pc";      // Used in XML documents

        // FIELD VALUES

        private SRVFileHeader vFileHeader;
        private SRWorldZoneHeader worldZoneHeader;

        /// <summary>
        /// Reads the zone header file into memory.
        /// </summary>
        /// <param name="czhFile">File system path to the ".czn_pc" zone file.</param>
        public void ReadFile(string czhFile)
        {
            SRTrace.WriteLine("");
            SRTrace.WriteLine("-------------------------------------------------------------------------------");
            SRTrace.WriteLine("ZONE HEADER FILE:  " + Path.GetFileName(czhFile));

            FileStream stream = null;
            try
            {
                stream = new FileStream(czhFile, FileMode.Open, FileAccess.Read);
                SRBinaryReader binaryReader = new SRBinaryReader(stream);
                vFileHeader = new SRVFileHeader(binaryReader);
                worldZoneHeader = new SRWorldZoneHeader(binaryReader);
            }
            catch (Exception e)
            {
                // Add context information for the error message
                e.Data["Action"] = "Reading Zone Header file";
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
        /// <param name="czhFile">File system path to the ".czh_pc" zone file.</param>
        public void WriteFile(string czhFile)
        {
            FileStream stream = null;
            SRBinaryWriter binaryWriter = null;
            try
            {
                stream = File.Open(czhFile, FileMode.Create);
                binaryWriter = new SRBinaryWriter(stream);
                vFileHeader.Write(binaryWriter);
                worldZoneHeader.Write(binaryWriter);
            }
            catch (Exception e)
            {
                // Add context information for the error message
                e.Data["Action"] = "Writing Zone Header file";
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
        /// <param name="czhFile">File system path to the ".czh_pc" zone file.</param>
        public void ReadXmlFile(string xmlFile)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlFile);
                XmlNode fileNode = xmlDocument.SelectSingleNode("/root/" + XmlTagName);
                if (fileNode != null)
                {
                    vFileHeader = new SRVFileHeader(fileNode);
                    worldZoneHeader = new SRWorldZoneHeader(fileNode);
                }
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
                XmlElement czhFileNode = xmlDocument.CreateElement(XmlTagName);
                xmlRoot.AppendChild(czhFileNode);
                vFileHeader.WriteXml(czhFileNode);
                worldZoneHeader.WriteXml(czhFileNode);
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
