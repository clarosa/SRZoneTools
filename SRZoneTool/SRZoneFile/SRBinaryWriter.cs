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
    /// Extension of the System.IO.BinaryWriter class with some added functions.
    /// </summary>
    class SRBinaryWriter : BinaryWriter
    {
        public SRBinaryWriter(Stream s) : base(s) { }

        /// <summary>
        /// Writes a null-terminated string to this stream in the current encoding of the BinaryWriter,
        /// and advances the current position of the stream in accordance with the encoding used and
        /// the specific characters being written to the stream.  Overrides the base class method.
        /// </summary>
        /// <param name="s">The value to write.</param>
        public override void Write(string s)
        {
            Write(s.ToCharArray());
            Write((byte)0);
        }

        /// <summary>
        /// Write zero padding bytes until the stream position is a multiple of the given coefficient.
        /// If the stream position is already a multiple of the coefficient, nothing will be written.
        /// </summary>
        /// <param name="n">Coefficient to align to in bytes (typically 2 or 4).</param>
        public void Align(int n)
        {
            while (BaseStream.Position % n != 0)
                Write((Byte)0);
        }
    }
}
