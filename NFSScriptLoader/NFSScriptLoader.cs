using System;
using NFSScript.Core;

namespace NFSScriptLoader
{
    public struct NFSScriptLoader
    {
        public static VAMemory GetRunningGameMemory()
        {
            return Program.gameMemory;
        }

        public const string LOADER_TAG = "NFSScriptLoader";

        public const string ERROR_TAG = "NFSScriptLoader ERROR";

        public const string INFO_TAG = "NFSScriptLoader INFO";

        public const string DEBUG_TAG = "NFSScriptLoader DEBUG";

        public const string SCRIPTS_FOLDER = "scripts\\";
    }
}
