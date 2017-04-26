//
// Copyright (C) 2016 Christopher LaRosa
//
// Based on Saints Row: The Third zone file format info and discussion here:
// https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/
//

using System;
using System.Collections.Generic;

namespace ChrisLaRosa.SaintsRow.ZoneFile
{
    /// <summary>
    /// Zone file object types.
    /// Object type hashes calculated by "ThomasJepp.SaintsRow.Hashes.CrcVolition()".
    /// </summary>
    public static class SRZoneObjectTypes
    {
        private static readonly Dictionary<UInt32, String> ObjectTypes = new Dictionary<UInt32, String>()
        {
            { 0x252BD154, "script_mover" },
            { 0x445C1F3D, "navpoint" },
            { 0x454D826F, "audio_emitter" },
            // { 0x71DDAD68, "???" },
            { 0x7AD3AD35, "script_group" },
            { 0x97335A0B, "script_vehicle" },
            { 0xA518E725, "script_npc" },
            // { 0xB3FB6098, "???" },
            // { 0xB426E6A6, "???" },
            // { 0xBE2F0901, "???" },
            { 0xD14F1482, "scripted_path" },
            // { 0xDA1A82F0, "???" },
            { 0xDE264D4E, "action_node_group" },
            { 0xF300989F, "trigger_volume" }
        };

        public static bool hasName(UInt32 crc)
        {
            return ObjectTypes.ContainsKey(crc);
        }

        public static string Name(UInt32 crc)
        {
            return hasName(crc) ? ObjectTypes[crc] : "";
        }
    }
}
