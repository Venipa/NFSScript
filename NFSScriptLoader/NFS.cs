using System;
using System.Diagnostics;
using System.IO;
using NFSScript;
using NFSScript.Core;

namespace NFSScriptLoader
{
    internal struct NFS
    {
        public static NFSGame DetectGameExecutableInDirectory
        {
            get
            {
                if (File.Exists("speed.exe"))
                {
                    using (StreamReader streamReader = new StreamReader("speed.exe"))
                    {
                        string text;
                        while ((text = streamReader.ReadLine()) != null)
                        {
                            if (text.Contains("Most Wanted"))
                            {
                                return NFSGame.MW;
                            }
                            if (text.Contains("underground"))
                            {
                                return NFSGame.Underground;
                            }
                        }
                    }
                    return (NFSGame)255;
                }
                if (File.Exists("speed2.exe"))
                {
                    return NFSGame.Underground2;
                }
                if (File.Exists("NFSC.exe"))
                {
                    return NFSGame.Carbon;
                }
                if (File.Exists("nfs.exe"))
                {
                    using (StreamReader streamReader2 = new StreamReader("nfs.exe"))
                    {
                        string text2;
                        while ((text2 = streamReader2.ReadLine()) != null)
                        {
                            if (text2.Contains("Undercover"))
                            {
                                return NFSGame.Undercover;
                            }
                        }
                    }
                    return NFSGame.ProStreet;
                }
                if (File.Exists("nfsw.exe"))
                {
                    return NFSGame.Undetermined;
                }
                return NFSGame.None;
            }
        }

        public static bool IsGameMinimized()
        {
            return GameMemory.isMinimized();
        }

        public static bool IsGameFocused()
        {
            return GameMemory.isFocused();
        }

        public static bool IsGameRunning()
        {
            return Process.GetProcessesByName(Program.gameMemory.GetMainProcess().ProcessName).Length != 0;
        }

        public const string SPEED_EXE = "speed.exe";

        public const string SPEED2_EXE = "speed2.exe";

        public const string NFSC_EXE = "NFSC.exe";

        public const string NFS_EXE = "nfs.exe";

        public const string NFSW_EXE = "nfsw.exe";
    }
}
