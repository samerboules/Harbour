using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class ContainerColorer
{
    public static long GetHexColorFromPrefix(string prefix, bool reefer = false)
    {
        if (reefer)
            return 0xFFFAFAFA;

        long result;
        if (prefixList.TryGetValue(prefix, out result))
            return result;
        else
            return GetRandomColor();
    }

    public static long GetRandomColor()
    {
        Random rnd = new Random();
        return prefixList.ElementAt(rnd.Next(prefixList.Count - 1)).Value;
    }

    private static Dictionary<String, long> prefixList = new Dictionary<String, long>
    {
        { "AMFU", 0xFFA2554D },
        { "APHU", 0xFFA2554D },
        { "APRU", 0xFF8C9AA6 },
        { "APZU", 0xFF204190 },
        { "BEAU", 0xFF204190 },
        { "BMOU", 0xFF204190 },
        { "BSIU", 0xFF204190 },
        { "CAIU", 0xFF204190 },
        { "CARU", 0xFF023D75 },
        { "CAXU", 0xFF023D75 },
        { "CBHU", 0xFF023D75 },
        { "CCLU", 0xFF023D75 },
        { "CGMU", 0xFF023D75 },
        { "CLHU", 0xFF023D75 },
        { "CMAU", 0xFF023D75 },
        { "CPSU", 0xFF023D75 },
        { "CRLU", 0xFF023D75 },
        { "CRSU", 0xFF023D75 },
        { "CRXU", 0xFF023D75 },
        { "CSLU", 0xFF023D75 },
        { "CSNU", 0xFF023D75 },
        { "CXDU", 0xFF023D75 },
        { "CXRU", 0xFF023D75 },
        { "DFSU", 0xFF023D75 },
        { "DRYU", 0xFF023D75 },
        { "DVRU", 0xFF023D75 },
        { "ECMU", 0xFF023D75 },
        { "EGHU", 0xFF023D75 },
        { "EGSU", 0xFF023D75 },
        { "EISU", 0xFF023D75 },
        { "EITU", 0xFF33757B },
        { "EMCU", 0xFF33757B },
        { "FCIU", 0xFF138F51 },
        { "FSCU", 0xFF138F51 },
        { "GATU", 0xFF138F51 },
        { "GESU", 0xFFFF6600 },
        { "GLDU", 0xFFFF6600 },
        { "HASU", 0xFFFF6600 },
        { "HDMU", 0xFFFF6600 },
        { "HLBU", 0xFFFF6600 },
        { "HLXU", 0xFFFF6600 },
        { "HMCU", 0xFFFF6600 },
        { "IMTU", 0xFFFF6600 },
        { "INKU", 0xFFFF6600 },
        { "IPXU", 0xFFFF6600 },
        { "KKFU", 0xFF138F51 },
        { "KKTU", 0xFF138F51 },
        { "MAEU", 0xFFFE5429 },
        { "MAGU", 0xFFFE5429 },
        { "MEDU", 0xFFFE5429 },
        { "MNBU", 0xFFFE5429 },
        { "MOAU", 0xFFFE5429 },
        { "MOFU", 0xFFFE5429 },
        { "MORU", 0xFFFE5429 },
        { "MOTU", 0xFFCE4231 },
        { "MRKU", 0xFFCE4231 },
        { "MSCU", 0xFF347CD0 },
        { "MSKU", 0xFF347CD0 },
        { "MWCU", 0xFFD32A87 },
        { "NYKU", 0xFFD32A87 },
        { "OOCU", 0xFFD32A87 },
        { "OOLU", 0xFFD32A87 },
        { "PCIU", 0xFFD32A87 },
        { "PONU", 0xFFD32A87 },
        { "SEGU", 0xFFCFCFCF },
        { "SUDU", 0xFFCFCFCF },
        { "SZLU", 0xFFD32A87 },
        { "TCKU", 0xFFD32A87 },
        { "TCLU", 0xFFD32A87 },
        { "TCNU", 0xFFEAB75E },
        { "TEMU", 0xFFEAB75E },
        { "TGHU", 0xFFEAB75E },
        { "TRHU", 0xFFCCCCB0 },
        { "TRIU", 0xFF1D4092 },
        { "TRLU", 0xFF167B42 },
        { "TTNU", 0xFFCCCCB0 },
        { "UACU", 0xFFCCCCB0 },
        { "UESU", 0xFFCCCCB0 },
        { "UETU", 0xFFCCCCB0 },
        { "WHLU", 0xFFCCCCB0 },
        { "XINU", 0xFFCCCCB0 },
        { "YMLU", 0xFFCCCCB0 },
        { "YMMU", 0xFFA2554D },
        { "ZCSU", 0xFFA2554D },
        { "ZIMU", 0xFFA2554D }
    };
}
