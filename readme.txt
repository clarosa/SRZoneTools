Zone File Tools for Saints Row by Quantum at saintsrowmods.com
Supports Saints Row: The Third, Saints Row 4, and Saints Row: Gat Out Of Hell.

This package contains Windows command-line tools that can be used to work with
zone files (*.czh_pc and *.czn_pc).  This assumes you have some familiarity
with the Windows command line.

This is still a work in progress.  Anything can change with future revisions,
including XML file formats.

INSTALLATION

  Nothing needs to be installed.  Just put the ".exe" files somewhere in your
  PATH and run them.

TOOLS

  SRReadZone.exe    Parses and displays the contents of a zone header file and
                    the corresponding zone data file.  Type the command with no
                    parameters to display a usage message.
  
  SRZoneTool.exe    Converts zone files to and from XML format.  Type the
                    command with no parameters to display a usage message.

XML FILE FORMAT DESCRIPTION

  The XML format created by SRZoneTool is unique to this tool and is not a
  standard schema.  The format is very similar to Saints Row ".xtbl" files.
  Since this is a pre-release version, the format of this file can and
  probably will change in future versions.

  There are, however, some conventions that are used in this file:

  <rawdata>         Contains a block of raw unparsed data.  This may occur
                    anywhere in the file where the parser does not know how to
                    interpret the data within the block.  In future versions,
                    this may be replaced by parsed data.

SRZONETOOL VERIFICATION

  If you would like to verify the accurate operation of SRZoneTool for a
  particular zone file or zone header file, type the following commands in
  order at the command line, replacing "filename" with the name of the file you
  are testing:
  
    SRZoneTool filename.czn_pc -o filename.xml
    SRZoneTool filename.xml -o filename2.czn_pc
    fc /b filename.czn_pc filename2.czn_pc
  
  These commands convert the file to XML format and then back to the original
  format, and then compare the new file to the original file.
  You should see the message "FC: no differences encountered" if the tool has
  converted the file in both directions successfully.

  For example:
    SRZoneTool sr3_city~fm06`.czn_pc -o sr3_city~fm06`.xml
    SRZoneTool sr3_city~fm06`.xml -o sr3_city~fm06`2.czn_pc
    fc /b sr3_city~fm06`.czn_pc sr3_city~fm06`2.czn_pc

PACKAGE VERSION HISTORY

  0.1    [2015-07-08]  Initial release of SRReadZone tool.
  0.2    [2015-07-09]  Added "-n" normalize option to SRReadZone tool.
  0.3    [2015-07-26]  Initial release of ZRZoneTool tool.

-------------------------------------------------------------------------------
DISCLAIMER:

This program is free software.  Use it at your own risk.  I've tested it, and
it works well for me, but I cannot be responsible if anything goes wrong.
You can redistribute it as long as you give credit to the original author.
