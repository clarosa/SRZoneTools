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

-------------------------------------------------------------------------------
DISCLAIMER:

This program is free software.  Use it at your own risk.  I've tested it, and
it works well for me, but I cannot be responsible if anything goes wrong.
You can redistribute it as long as you give credit to the original author.
