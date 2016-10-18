﻿//
// Copyright (C) 2016 Christopher LaRosa
//
// Based on Saints Row: The Third zone file format info and discussion here:
// https://www.saintsrowmods.com/forum/threads/sr3-zone-file-format.2855/
//

using System;
using System.Collections.Generic;

namespace ChrisLaRosa.SaintsRow.ZoneFile
{
    public class NameDescription
    {
        public string name;
        public string description;

        public NameDescription(string n, string d) {
            name = n;
            description = d;
        }
    };

    /// <summary>
    /// Zone file section identifiers from Kinzie's Toy Box:
    /// https://github.com/volition-inc/Kinzies-Toy-Box/blob/master/file_formats.md
    /// </summary>
    public static class SRZoneSectionIdentifiers
    {
        public static readonly Dictionary<UInt32, NameDescription> SectionTypes = new Dictionary<UInt32, NameDescription>()
        {
            { 0x2233, new NameDescription("Crunched reference geometry", "References to level mesh files (not the lmeshes themselves)") },
            { 0x2234, new NameDescription("Objects",                "Game objects, as placed in the World Editor") },
            { 0x2235, new NameDescription("Navmesh",                "The navmesh data for the zone") },
            { 0x2236, new NameDescription("Traffic",                "Traffic data for the zone") },
            { 0x2237, new NameDescription("World Editor geometry",  "Editor created geometry (patches, roads, path deform meshes)") },
            { 0x2238, new NameDescription("Sidewalks",              "Sidewalk data for the zone") },
            { 0x2239, new NameDescription("Trailer",                "???") },
            { 0x2240, new NameDescription("Light clip meshes",      "Light clip mesh data") },
            { 0x2241, new NameDescription("Traffic signals",        "Traffic signal data") },
            { 0x2242, new NameDescription("Mover constraints",      "Constraints for movers in the zone") },
            { 0x2243, new NameDescription("Interior triggers",      "Interior trigger volumes") },
            { 0x2244, new NameDescription("Heightmap",              "Heightmap data for the zone (stitched into the larger world)") },
            { 0x2245, new NameDescription("c_object RBB-tree",      "RBB-tree for collecting c_objects at runtime") },
            { 0x2246, new NameDescription("Undergrowth",            "Undergrowth data") },
            { 0x2247, new NameDescription("Water volumes",          "Water volume data") },
            { 0x2248, new NameDescription("Wave killers",           "Wave killer data") },
            { 0x2249, new NameDescription("Water surfaces",         "Water surface data") },
            { 0x2250, new NameDescription("Parking",                "Parking data") },
            { 0x2251, new NameDescription("Rain killer meshes",     "Rain killer mesh data") },
            { 0x2252, new NameDescription("Supplemental LOD",       "Autogenerated 4th LOD data") },
            { 0x2253, new NameDescription("Fading grid data",       "City fading grid data. Also used for mip streamer calculations") },
            { 0x2254, new NameDescription("Audio Emitter RBB-tree", "RBB-tree for collecting audio emitters at runtime") },
            { 0x2255, new NameDescription("Havok Pathfinding data", "Havok pathfinding data for the zone" ) }
        };
    }
}
