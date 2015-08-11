using System;
using ChrisLaRosa.SaintsRow.ZoneFile;

namespace SRZoneExample
{
    /// <summary>
    /// This very simple example program reads all sections in a ".czn_pc" file,
    /// parses the objects section data, and prints the value of all string properties.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            SRZoneCombinedFile file = new SRZoneCombinedFile(args[0]);
            foreach (SRZoneSection section in file.SectionList)
                if (section.CpuData is SRZoneObjectSectionCpuData)
                    foreach (SRZoneObject zoneObject in ((SRZoneObjectSectionCpuData)section.CpuData).ObjectList)
                        foreach (SRZoneProperty zoneProperty in zoneObject.PropertyList)
                            if (zoneProperty is SRZoneStringProperty)
                                Console.WriteLine(zoneProperty.ToString());
        }
    }
}
