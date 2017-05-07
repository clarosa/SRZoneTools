Zone File Tools for Saints Row by Quantum at saintsrowmods.com
Supports Saints Row: The Third and Saints Row IV.

This package contains Windows command-line tools that can be used to work with
zone files (*.czh_pc and *.czn_pc).  This assumes you have some familiarity
with the Windows command line.

INSTALLATION

  Nothing needs to be installed.  Just put the ".exe" files somewhere in your
  PATH and run them.

TOOLS

  SRReadZone.exe      Parses and displays the contents of a zone header file
                      and the corresponding zone data file.  Type the command
                      with no parameters to display a usage message.
  
  SRZoneTool.exe      Converts zone files to and from XML format.  Type the
                      command with no parameters to display a usage message.
                      For more information about this tool, see the
                      "SRZoneTool.html" document included in this package.

  SRZoneFinder.exe    Finds zone files closest to a set of coordinates.
                      Type the command with no parameters to display a usage
                      message.

  SRPatchFile.exe     Writes one or more bytes to specific locations in an
                      existing file.  Great for scripting!

PACKAGE VERSION HISTORY

  0.1    [2015-07-08]  Initial release of SRReadZone tool.
  0.2    [2015-07-09]  Added "-n" normalize option to SRReadZone tool.
  0.3    [2015-07-26]  Initial release of SRZoneTool tool.
  0.4    [2015-07-27]  SRZoneTool now parses all known data structures.
                       Can combine header and zone data in a single XML file.
                       Full SR4 compatibility for all tools.
  1.0    [2015-07-29]  First official release.  Updated XML format.
                       Added SRReadZone.html document.
  1.1    [2015-08-12]  Added SRZoneFinder tool.
  1.2    [2015-08-15]  Added new command line options to SRZoneFinder tool.
                       Reduced default verbosity in SRZoneTool.
  1.3    [2015-09-16]  Updated SRReadZone tool to parse zone file section type
                       0x2233 (crunched reference geometry) up to and including
                       object coordinates.
  2.0    [2015-09-27]  Changes to SRZoneTool XML format (see details below).
                       Additional XML file validation.
  2.1    [2016-10-17]  Added "--position" and "--type" options to SRReadZone.
  2.2    [2016-10-26]  Added SRPatchFile tool.
  2.3    [2016-12-18]  Added object type names in SRReadZone and ZRZoneTool.
                       Changed all <description> elements to XML comments.
  2.4    [2017-04-25]  Added "audio_emitter" object type name in SRReadZone
                       and ZRZoneTool.
  2.5    [2017-05-01]  Added the complete (?) list of object type names to
                       SRReadZone and ZRZoneTool, contributed by Minimaul at
                       saintsrowmods.com.
  3.0    [2017-05-07]  Updated SRZoneTool to parse and convert zone file
                       section type 0x2233 (crunched reference geometry) up to
                       and including object coordinates.  Can still read XML
                       files created by version 2.x.

XML CHANGES MADE IN VERSION 2.0

  1.  Enclosed the value part of each <property> in a <value> element.
  2.  Renamed the following elements:
      a.  <transform>             to  <position>
      b.  <handle_offset>         to  <handle>
      c.  <parent_handle_offset>  to  <parent_handle>

-------------------------------------------------------------------------------
DISCLAIMER:

This program is free software.  Use it at your own risk.  I've tested it, and
it works well for me, but I cannot be responsible if anything goes wrong.
You can redistribute it as long as you give credit to the original author.
