using QSim.ConsoleApp.Middleware.StackingSystem;
using System.Collections.Generic;
using System.Linq;


namespace QSim.ConsoleApp.Utilities
{
    static class ContainerGenerator
    {
        private const string nums = "0123456789";
        private static List<string> _containerPrefixes = new List<string>()
        {
            "AMFU","APHU","APRU","APZU","BEAU","BMOU","BSIU","CAIU","CARU","CAXU","CBHU","CCLU",
            "CGMU","CLHU","CMAU","CPSU","CRLU","CRSU","CRXU","CSLU","CSNU","CXDU","CXRU","DFSU",
            "DRYU","DVRU","ECMU","EGHU","EGSU","EISU","EITU","EMCU","FCIU","FSCU","GATU","GESU",
            "GLDU","HASU","HDMU","HLBU","HLXU","HMCU","IMTU","INKU","IPXU","KKFU","KKTU","MAEU",
            "MAGU","MEDU","MNBU","MOAU","MOFU","MORU","MOTU","MRKU","MSCU","MSKU","MWCU","NYKU",
            "OOCU","OOLU","PCIU","PONU","SEGU","SUDU","SZLU","TCKU","TCLU","TCNU","TEMU","TGHU",
            "TRHU","TRIU","TRLU","TTNU","UACU","UESU","UETU","WHLU","XINU","YMLU","YMMU","ZCSU",
            "ZIMU"
        };

        public static string GetRandomContainerNumber()
        {
            string containerNumber = "";
            while (containerNumber == "" || Stacking.Instance.HasContainer(containerNumber))
            {
                containerNumber = GenerateRandomPrefix() + GenerateRandomNumber();
            }

            return containerNumber;
        }

        private static string GenerateRandomNumber()
        {
            return new string(Enumerable.Repeat(nums, 7).Select(s => s[RandomNumberGenerator.NextNumber(s.Length)]).ToArray());
        }

        private static string GenerateRandomPrefix()
        {
            return _containerPrefixes[RandomNumberGenerator.NextNumber(_containerPrefixes.Count)];
        }
    }
}
