using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogParser.Engine.Classes
{
    public class Config
    {
        public static string Current_Version { get; } = "1.0.6";
        public static bool CheckAutoUpdates { get; set; } = true;
        public static bool CreateAutoBackups { get; set; } = true;
    }
}
