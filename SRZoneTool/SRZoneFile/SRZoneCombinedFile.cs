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
using System.Text.RegularExpressions;
using System.Xml;

namespace ChrisLaRosa.SaintsRow.ZoneFile
{
    /// <summary>
    /// Reads and writes Saints Row Zone Files.
    /// </summary>
    class SRZoneCombinedFile
    {
        // CONSTANTS

        public const string XmlZoneDataTagName = "czn_pc";          // Used in XML documents
        public const string XmlZoneHeaderTagName = "czh_pc";        // Used in XML documents

        // FIELD VALUES
        
        private SRVFileHeader vFileHeader = null;
        private SRWorldZoneHeader worldZoneHeader = null;
        private List<SRZoneSection> fileSections = null;

        /// <summary>
        /// Reads the zone header file into memory.
        /// </summary>
        /// <param name="czhFile">File system path to the ".czn_pc" zone file.</param>
        public void ReadHeaderFile(string czhFile)
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
                worldZoneHeader = new SRWorldZoneHeader(binaryReader, vFileHeader);
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
        public void WriteHeaderFile(string czhFile)
        {
            FileStream stream = null;
            SRBinaryWriter binaryWriter = null;
            try
            {
                if (vFileHeader == null || worldZoneHeader == null)
                    throw new SRZoneFileException("No zone header to write.");
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
        /// <param name="cznFile">File system path to the ".czn_pc" zone file.</param>
        public void ReadDataFile(string cznFile)
        {
            SRTrace.WriteLine("");
            SRTrace.WriteLine("-------------------------------------------------------------------------------");
            SRTrace.WriteLine("ZONE DATA FILE:  " + Path.GetFileName(cznFile));

            FileStream stream = null;
            try
            {
                fileSections = new List<SRZoneSection>();
                stream = new FileStream(cznFile, FileMode.Open, FileAccess.Read);
                SRBinaryReader binaryReader = new SRBinaryReader(stream);
                int index = 0;
                while (binaryReader.BaseStream.Position <= binaryReader.BaseStream.Length - 4)
                    fileSections.Add(new SRZoneSection(binaryReader, index++));
            }
            catch (Exception e)
            {
                // Add context information for the error message
                e.Data["Action"] = "Reading Zone Data file";
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
        public void WriteDataFile(string cznFile)
        {
            FileStream stream = null;
            SRBinaryWriter binaryWriter = null;
            try
            {
                if (fileSections == null)
                    throw new SRZoneFileException("No zone data to write.");
                stream = File.Open(cznFile, FileMode.Create);
                binaryWriter = new SRBinaryWriter(stream);
                int index = 0;
                foreach (SRZoneSection section in fileSections)
                    section.Write(binaryWriter, index++);
            }
            catch (Exception e)
            {
                // Add context information for the error message
                e.Data["Action"] = "Writing Zone Data file";
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
        /// <param name="xmlFile">File system path to the ".xml" zone file.</param>
        public void ReadXmlFile(string xmlFile)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                // Must preserve whitespace during read so that values that begin and/or end
                // with whitespace are not trimmed (e.g. "<tag> </tag>").
                xmlDocument.PreserveWhitespace = true;
                xmlDocument.Load(xmlFile);
                if (xmlDocument.SelectSingleNode("/root/" + XmlZoneDataTagName) != null)
                {
                    fileSections = new List<SRZoneSection>();
                    XmlNodeList selectNodeList = xmlDocument.SelectNodes("/root/" + XmlZoneDataTagName + "/" + SRZoneSection.XmlTagName);
                    int index = 0;
                    foreach (XmlNode selectNode in selectNodeList)
                        fileSections.Add(new SRZoneSection(selectNode, index++));
                }
                XmlNode zoneHeaderNode = xmlDocument.SelectSingleNode("/root/" + XmlZoneHeaderTagName);
                if (zoneHeaderNode != null)
                {
                    vFileHeader = new SRVFileHeader(zoneHeaderNode);
                    worldZoneHeader = new SRWorldZoneHeader(zoneHeaderNode, vFileHeader);
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
                if (vFileHeader != null && worldZoneHeader != null)
                {
                    XmlElement czhFileNode = xmlDocument.CreateElement(XmlZoneHeaderTagName);
                    xmlRoot.AppendChild(czhFileNode);
                    vFileHeader.WriteXml(czhFileNode);
                    worldZoneHeader.WriteXml(czhFileNode);
                }
                if (fileSections != null)
                {
                    XmlElement cznFileNode = xmlDocument.CreateElement(XmlZoneDataTagName);
                    xmlRoot.AppendChild(cznFileNode);
                    int index = 0;
                    foreach (SRZoneSection section in fileSections)
                        section.WriteXml(cznFileNode, index++);
                }
                xmlDocument.Save(xmlFile);
            }
            catch (Exception e)
            {
                // Add context information for the error message
                e.Data["Action"] = "Writing XML file";
                throw;
            }
        }

        public void ReadFile(string fileName)
        {
            if (Regex.IsMatch(fileName, @"(?i)\.czn\w*$"))
                ReadDataFile(fileName);
            else if (Regex.IsMatch(fileName, @"(?i)\.czh\w*$"))
                ReadHeaderFile(fileName);
            else if (Regex.IsMatch(fileName, @"(?i)\.x\w*$"))
                ReadXmlFile(fileName);
            else
                throw new SRZoneFileException("Can't determine file format from file name.", null);
        }

        public void WriteFile(string fileName)
        {
            if (Regex.IsMatch(fileName, @"(?i)\.czn\w*$"))
                WriteDataFile(fileName);
            else if (Regex.IsMatch(fileName, @"(?i)\.czh\w*$"))
                WriteHeaderFile(fileName);
            else if (Regex.IsMatch(fileName, @"(?i)\.x\w*$"))
                WriteXmlFile(fileName);
            else
                throw new SRZoneFileException("Can't determine file format from file name.", null);
        }
    }
}
