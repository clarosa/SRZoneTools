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
    /// Abstract base class for all data blocks.
    /// </summary>
    public abstract class SRDataBlockBase
    {
        /// <summary>
        /// Calculates the smallest number that must be added to the given offset
        /// such that the resulting offset is an even multiple of the given boundry.
        /// </summary>
        public static int AlignPaddingSize(long offset, int boundry)
        {
            return (offset % boundry == 0) ? 0 : (boundry - (int)(offset % boundry));
        }

        /// <summary>
        /// Calculates the next higher offset that is a multiple of the given boundry.
        /// Returns the same number if it's already a multiple of the given boundry.
        /// </summary>
        public static long AlignUp(long offset, int boundry)
        {
            return offset + AlignPaddingSize(offset, boundry);
        }
    }
}
