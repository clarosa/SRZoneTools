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
        /*
         * These are Volition CRC hashes of object type names
         * and seem to correspond to the object_type_hash field
         */
        private enum SRObjectType : uint
        {
            action_node = 0x9819E2A7,
            action_node_group = 0xDE264D4E,
            activity_start = 0x50C01AFB,
            audio_emitter = 0x454D826F,
            audio_emitter_multi = 0xD3C09F8C,
            base_object = 0xAB4A8269,
            battlefront = 0x9F3E6097,
            brass = 0xA6CFE79E,
            city_takeover_region = 0xDBEDBF51,
            climbable_spline = 0x8FC2C162,
            cover_node = 0x5229C9DA,
            crib = 0x69653449,
            ctg_object = 0x81431719,
            cto_start = 0x948906B7,
            cutscene_slate_obj = 0x423D4B45,
            damage_region = 0x13ECAFE7,
            detour_hull = 0x105A7D93,
            district = 0x54E38BEE,
            door = 0xABA18B31,
            effect_object = 0xDA1A82F0,
            flashpoint = 0x324FE51A,
            garage_info = 0xC378FD50,
            general_mover = 0x71DDAD68,
            hood = 0x15A381D8,
            hotspot = 0xD5DF5C6D,
            hotspot_target = 0xA932538B,
            human = 0x634022E8,
            interior_volume = 0x82606C1C,
            item = 0x01A3592D,
            level_light = 0x6A44E812,
            level_respawn = 0x4652DBF2,
            light_list = 0x161048E8,
            light_vis_volume = 0x2F481200,
            mission_info = 0x991E5B37,
            navpoint = 0x445C1F3D,
            navpoint_path = 0x302B6A22,
            npc = 0xB9CDAF3E,
            object_debris = 0x89E8818E,
            object_rig = 0xEB1C95CA,
            object_tree = 0x41ACD323,
            occluder = 0xD489BF77,
            parking_spot = 0xBA73CC6F,
            photo_op = 0x573CE01E,
            physical_object = 0xC68ACA17,
            player = 0x29DBDBC6,
            projectile = 0xDF712A1B,
            resource_object = 0xB78242F1,
            roadblock = 0x2C033366,
            script_group = 0x7AD3AD35,
            script_group_object = 0x59FC5EA5,
            script_interior = 0x2BB7B8C5,
            script_item = 0xB426E6A6,
            script_light_group = 0xB3FB6098,
            script_mover = 0x252BD154,
            script_npc = 0xA518E725,
            script_peered = 0x8D42DB8B,
            script_vehicle = 0x97335A0B,
            scripted_mission_start = 0x5E20A824,
            scripted_path = 0xD14F1482,
            shop_object = 0x46883C82,
            silent_mission_start = 0x4D3AEC73,
            smooth_scripted_path = 0xBE2F0901,
            spawn_region = 0xA94DBBA7,
            survival = 0x82470BA4,
            trigger_volume = 0xF300989F,
            vehicle = 0x86EC3BF8,
            vfx_volume = 0x765E72F7,
            weapon = 0xD8F10645,

            UNKNOWN = 0xFFFFFFFF
        }

        public static bool hasName(UInt32 crc)
        {
            return Enum.IsDefined(typeof(SRObjectType), crc);
        }

        public static string Name(UInt32 crc)
        {
            return Enum.GetName(typeof(SRObjectType), crc) ?? "";
        }
    }
}
