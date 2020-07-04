using System;

namespace MogboardDataExporter
{
    public static class Util
    {
        public static string GetIconFolder(int iconId) => (Math.Floor(iconId / 1000d) * 1000).ToString("000000");
    }
}
