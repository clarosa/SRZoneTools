//
// Copyright (C) 2015 Christopher LaRosa
//
// Based on Saints Row: The Third zone file format info and discussion here:
// https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Options;
using ChrisLaRosa.SaintsRow.ZoneFile;

namespace ChrisLaRosa.SaintsRow.SRZoneTool
{
    class Program
    {
        static void ShowHelp(string programName, OptionSet options)
        {
            Console.WriteLine();
            Console.WriteLine("Converts a zone file or zone header file to and from XML format.");
            Console.WriteLine("Supports Saints Row: The Third, Saints Row 4, and Saints Row: Gat Out Of Hell.");
            Console.WriteLine();
            Console.WriteLine("Usage: " + programName + " filename [OPTIONS]+");
            Console.WriteLine();
            Console.WriteLine("  filename     input file (\".czn_pc\", \".czh_pc\", or \".xml\")");
            Console.WriteLine();
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }
        
        static string FormatFromFileName(string fileName)
        {
            if (Regex.IsMatch(fileName, @"(?i)\.czn\w*$"))
                return "czn";
            else if (Regex.IsMatch(fileName, @"(?i)\.czh\w*$"))
                return "czh";
            else if (Regex.IsMatch(fileName, @"(?i)\.x\w*$"))
                return "xml";

            return null;
        }

        static int Main(string[] args)
        {
            string programName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            string svnRevision = "$Revision: 1075 $";
            Regex regex = new Regex(@"\D");
            string revision = regex.Replace(svnRevision, "");
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            Version ver = assemName.Version;
            Console.WriteLine("{0} version {1}.{2}.{3}.{4} by Quantum at saintsrowmods.com",
                              programName, ver.Major, ver.Minor, ver.Build, revision);

            string inputFile = null;
            string outputFile = null;
            string inputFormat = null;
            string outputFormat = null;
            Boolean showHelp = false;

            // See http://tirania.org/blog/archive/2008/Oct-14.html
            OptionSet options = new OptionSet() {
                { "o|output=", "output file (\".czn_pc\", \".czh_pc\", or \".xml\")", v => outputFile = v },
                // { "i|input-format=", "input format: czh, czn, or xml (default=auto)", v => inputFormat = v },
                // { "f|format=", "output format: czh, czn, or xml (default=auto)", v => outputFormat = v },
                { "v|verbose", "display detailed information when reading files", v => { if (v != null) SRTrace.Enable = true; } },
                { "h|help",  "show this message and exit", v => showHelp = v != null },
                { "no-keep-padding", "don't preserve property padding on zone file read", v => { if (v != null) SRZoneProperty.OptionPreservePadding = false; } },
                { "no-parse-object", "don't parse objects on zone file read", v => { if (v != null) SRZoneSection.OptionParseObjects = false; } },
                { "no-parse-property", "don't parse property values on zone file read", v => { if (v != null) SRZoneProperty.OptionParseValues = false; } },
                { "rebuild-handle-list", "rebuild object handle list before writing", v => { if (v != null) SRZoneSectionDataObjects.OptionRebuildHandleList = true; } },
            };

            try
            {
                List<string> extra;
                extra = options.Parse(args);
                if (showHelp || extra.Count <= 0)
                {
                    ShowHelp(programName, options);
                    return 1;
                }
                inputFile = extra[0];
                if (extra.Count > 1)
                    throw new OptionException("Illegal command line argument \"" + extra[0] + "\".", null);
                if (inputFormat == null && (inputFormat = FormatFromFileName(inputFile)) == null)
                    throw new OptionException("Can't determine input file format from file name.", null);
                if (outputFormat == null && outputFile != null && (outputFormat = FormatFromFileName(outputFile)) == null)
                    throw new OptionException("Can't determine output file format from file name.", null);
                if (inputFormat != "czn" && inputFormat != "czh" && inputFormat != "xml")
                    throw new OptionException("Unsupported input format (" + inputFormat + ")", null);
                if (outputFile != null && outputFormat != "czn" && outputFormat != "czh" && outputFormat != "xml")
                    throw new OptionException("Unsupported output format (" + outputFormat + ")", null);

                if (inputFile == outputFile)
                    throw new OptionException("Input and output file are the same.", null);
                if (((inputFormat == "czh" || inputFormat == "czn") && outputFormat != null && outputFormat != "xml" && outputFormat != inputFormat) ||
                    (inputFormat == "xml" && outputFormat != null && outputFormat != "czh" && outputFormat != "czn"))
                    throw new OptionException("Unsupported input/output format combination.", null);

                SRZoneFile sr3ZoneFile = new SRZoneFile();
                SRZoneHeaderFile sr3ZoneHeaderFile = new SRZoneHeaderFile();
                if (inputFormat == "xml" && outputFormat == "czh")
                    sr3ZoneHeaderFile.ReadXmlFile(inputFile);
                else if (inputFormat == "xml")
                    sr3ZoneFile.ReadXmlFile(inputFile);
                else if (inputFormat == "czh")
                    sr3ZoneHeaderFile.ReadFile(inputFile);
                else
                    sr3ZoneFile.ReadFile(inputFile);
                if (outputFile != null)
                {
                    if (outputFormat == "xml" && inputFormat == "czh")
                        sr3ZoneHeaderFile.WriteXmlFile(outputFile);
                    else if (outputFormat == "xml")
                        sr3ZoneFile.WriteXmlFile(outputFile);
//                        sr3ZoneHeaderFile.WriteXmlFile(outputFile);
                    else if (outputFormat == "czh")
                        sr3ZoneHeaderFile.WriteFile(outputFile);
                    else
                        sr3ZoneFile.WriteFile(outputFile);
                }
            }
            catch (OptionException e)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine();
                errorWriter.WriteLine("ERROR: " + e.Message);
                errorWriter.WriteLine("       Type `" + programName + " --help' for more information.");
                return 1;
            }
            catch (Exception e)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine();
                errorWriter.WriteLine("ERROR:");
                errorWriter.WriteLine(e.Message);
                if (e.Data.Count > 0) {
                    string[] names = { "Section", "Object", "Property" };
                    string context = "";
                    foreach (string name in names)
                        if (e.Data.Contains(name))
                            context += ", " + name + " #" + e.Data[name];
                    string action = e.Data.Contains("Action") ? (string)e.Data["Action"] : "at";
                    if (context.Length > 2)
                        context = context.Substring(2);
                    errorWriter.WriteLine(action + " " + context);
                    if (e.Data.Contains("Position"))
                        errorWriter.WriteLine("File position: " + String.Format("0x{0:X8}", e.Data["Position"]));
                }
                #if DEBUG
                errorWriter.WriteLine();
                errorWriter.WriteLine(e.ToString());
                #endif
                return 1;
            }
            return 0;
        }
    }
}
