//
// Copyright (C) 2015 Christopher LaRosa
//
// Based on Saints Row: The Third zone file format info and discussion here:
// https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/
//

using System;
using System.IO;

namespace ChrisLaRosa.SaintsRow.ZoneFile
{
    /// <summary>
    /// Provides more robust WriteLine functionality than the built-in Trace class.
    /// </summary>
    public class SRTrace
    {
        public static bool Enable = false;

        /// <summary>
        /// Writes to the console only when SRTrace is enabled.
        /// Takes the same parameters as Console.WriteLine().
        /// </summary>
        public static void WriteLine(string format, params Object[] list)
        {
            if (Enable)
                Console.WriteLine(format, list);
        }
    }
}
