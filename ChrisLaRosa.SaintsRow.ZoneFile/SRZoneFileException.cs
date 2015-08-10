//
// Copyright (C) 2015 Christopher LaRosa
//
// Based on Saints Row: The Third zone file format info and discussion here:
// https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/
//

using System;

namespace ChrisLaRosa.SaintsRow.ZoneFile
{
    /// <summary>
    /// Exception that is thrown when a problem is found in a zone file.
    /// </summary>
    class SRZoneFileException : Exception
    {
        public SRZoneFileException()
        {
        }

        public SRZoneFileException(string message)
            : base(message)
        {
        }

        public SRZoneFileException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public SRZoneFileException(string message, long filePosition)
            : base(message)
        {
            Data["Position"] = filePosition;
        }
    }
}
