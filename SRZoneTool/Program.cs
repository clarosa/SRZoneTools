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
            Console.WriteLine("Supports Saints Row: The Third and Saints Row IV.");
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
            string svnRevision = "$Revision: 1089 $";
            Regex regex = new Regex(@"\D");
            string revision = regex.Replace(svnRevision, "");
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            Version ver = assemName.Version;
            Console.WriteLine("{0} version {1}.{2}.{3}.{4} by Quantum at saintsrowmods.com",
                              programName, ver.Major, ver.Minor, ver.Build, revision);

            string outputFile = null;
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

                SRZoneCombinedFile srZoneFile = new SRZoneCombinedFile();
                foreach (string fileName in extra)
                    srZoneFile.ReadFile(fileName);
                if (outputFile != null)
                    srZoneFile.WriteFile(outputFile);
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
                    string[] reserved = { "Action", "Section", "Object", "Property", "Position" };
                    string[] ordered = { "Section", "Object", "Property" };
                    string context = "";
                    foreach (string name in ordered)
                        if (e.Data.Contains(name))
                            context += ", " + name + " #" + e.Data[name];
                    foreach (string name in e.Data.Keys)
                        if (Array.IndexOf(reserved, name) < 0)
                            context += ", " + name + " #" + e.Data[name];
                    string action = e.Data.Contains("Action") ? (string)e.Data["Action"] : "";
                    context = context.Length > 2 ? " at " + context.Substring(2) : "";
                    errorWriter.WriteLine(action + context + ".");
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
