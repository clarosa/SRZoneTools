//
// Copyright (C) 2015 Christopher LaRosa
//
// Based on Saints Row: The Third zone file format info and discussion here:
// https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/
//

using System;
using System.IO;
using System.Text;

namespace ChrisLaRosa.SaintsRow.ZoneFile
{
    /// <summary>
    /// Extension of the System.IO.BinaryReader class with some added functions.
    /// </summary>
    public class SRBinaryReader : BinaryReader
    {
        public SRBinaryReader(Stream s) : base(s) { }

        /// <summary>
        /// Reads a null-terminated string from the current stream.
        /// Overrides the base class method.
        /// </summary>
        /// <param name="s">The value to write.</param>
        public override string ReadString()
        {
            StringBuilder sb = new StringBuilder();
            char c;
            while ((c = ReadChar()) != '\0')
                sb.Append(c);
            return sb.ToString();
        }

        /// <summary>
        /// Read and discard bytes until the stream position is a multiple of the given coefficient.
        /// If the stream position is already a multiple of the coefficient, nothing will be read.
        /// </summary>
        /// <param name="n">Coefficient to align to in bytes (typically 2 or 4).</param>
        public void Align(int n)
        {
            while (BaseStream.Position % n != 0)
                ReadByte();
        }
    }
}
