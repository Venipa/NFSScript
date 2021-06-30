using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Microsoft.CSharp;
using NFSScript;
using NFSScript.Core;

namespace NFSScriptLoader
{
    public class Program
    {
        private static int EntryPoint(string pwzArgument)
        {
            Log.Print("NFSScriptLoader INFO", "NFSScript by Dennis Stanistan");
            Program.INIInit();
            Program.GetNFSGame();
            Program.Start();
            return 0;
        }

        private static void Main(string[] args)
        {
            Program.EntryPoint("NFSScript");
        }

        private static void Terminate()
        {
            Program.CallScriptsOnExit();
            Program.timer.Dispose();
            Program.scripts = null;
            Program.inGameplay = false;
            Program.inRace = false;
            Program.gameMemory = null;
            Program._hookID = IntPtr.Zero;
            Environment.Exit(0);
        }

        private static void INIInit()
        {
            INIFile inifile = new INIFile("NFSScriptSettings.ini");
            if (File.Exists(inifile.Path))
            {
                if (inifile.KeyExists("ShowConsole", "NFSScript"))
                {
                    Program.settingShowConsole = Program.FlexiableParse(inifile.Read("ShowConsole", "NFSScript"));
                }
                if (inifile.KeyExists("Debug", "NFSScript"))
                {
                    Program.settingDebug = Program.FlexiableParse(inifile.Read("Debug", "NFSScript"));
                }
            }
            if (Program.settingShowConsole == 0)
            {
                NativeMethods.ShowWindow(NativeMethods.GetConsoleWindow(), 0);
            }
        }

        private static void GetNFSGame()
        {
            Log.Print("NFSScriptLoader INFO", "Getting executeable in directory.");
            Program.currentNFSGame = NFS.DetectGameExecutableInDirectory;
            int num = 0;
            switch (Program.currentNFSGame)
            {
                case NFSGame.None:
                    Log.Print("NFSScriptLoader ERROR", "A valid NFS executeable was not found.");
                    Environment.Exit(0);
                    return;
                case NFSGame.Underground:
                    Log.Print("NFSScriptLoader INFO", "Need for Speed: Underground detected.");
                    Program.SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "speed.exe")));
                    break;
                case NFSGame.Underground2:
                    Log.Print("NFSScriptLoader INFO", "Need for Speed: Underground 2 detected.");
                    Program.SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "speed2.exe")));
                    break;
                case NFSGame.MW:
                    Log.Print("NFSScriptLoader INFO", "Need for Speed: Most Wanted detected.");
                    Program.SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "speed.exe")));
                    break;
                case NFSGame.Carbon:
                    Log.Print("NFSScriptLoader INFO", "Need for Speed: Carbon detected.");
                    Program.SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NFSC.exe")));
                    break;
                case NFSGame.ProStreet:
                    Log.Print("NFSScriptLoader INFO", "Need for Speed: ProStreet detected.");
                    Program.SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nfs.exe")));
                    break;
                case NFSGame.Undercover:
                    Log.Print("NFSScriptLoader INFO", "Need for Speed: Undercover detected.");
                    Program.SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nfs.exe")));
                    break;
                case NFSGame.Undetermined:
                    Log.Print("NFSScriptLoader INFO", "Waiting for game's launch.");
                    while (Process.GetProcessesByName("nfsw").Length == 0 && num < 120)
                    {
                        Thread.Sleep(1000);
                        num++;
                    }
                    if (Process.GetProcessesByName("nfsw").Length == 0)
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        Log.Print("NFSScriptLoader INFO", "Need for Speed: World detected.");
                        Program.SetProcessVariables(Process.GetProcessesByName("nfsw")[0]);
                    }
                    break;
                default:
                    Log.Print("NFSScriptLoader ERROR", "A valid NFS executeable was not found.");
                    Environment.Exit(0);
                    return;
            }
            NFSScript.NFSScript.CurrentLoadedNFSGame = Program.currentNFSGame;
        }

        private static void GetGameMemory(string processName)
        {
            Program.gameMemory = new VAMemory(Program.gameProcessName);
            Program.gameMemory.ReadInt32((IntPtr)0);
            GameMemory.memory = Program.gameMemory;
            if (Program.settingDebug == 1)
            {
                VAMemory.debugMode = true;
            }
        }

        private static void Start()
        {
            Program.LoadScripts();
            Program.GetGameMemory(Program.gameProcessName);
            Program.CallPreScriptMethod();
            Log.Print("NFSScriptLoader INFO", string.Format("{0} {1} {2}", "Delaying the loader's thread for", 5, "seconds before initializing."));
            Thread.Sleep(5000);
            Log.Print("NFSScriptLoader INFO", "Delay is over, initializing.");
            if (NFS.IsGameRunning())
            {
                Program.CallInitScriptMethod();
                Program.WaitForGameLoad();
                Program.ApplyAndLoadIntPtrs();
                Log.Print("NFSScriptLoader INFO", "Game is fully loaded.");
                Program.CallMainScriptMethod();
                Program.timer = new System.Timers.Timer();
                Program.timer.Interval = 1.0;
                Program.timer.Elapsed += Program.Update_Elapsed;
                Program.timer.Start();
                Program.SetKeyboardHook();
                return;
            }
            Environment.Exit(0);
        }

        private static void WaitForGameLoad()
        {
            Log.Print("NFSScriptLoader INFO", "Waiting for the game to fully load. (Disabled in Most Wanted)");
            switch (Program.currentNFSGame)
            {
                case NFSGame.None:
                case NFSGame.MW:
                    break;
                case NFSGame.Underground:
                    while (Program.gameMemory.ReadByte((IntPtr)7343744) != 1)
                    {
                        if (!NFS.IsGameRunning())
                        {
                            return;
                        }
                        Thread.Sleep(100);
                    }
                    break;
                case NFSGame.Underground2:
                    while (Program.gameMemory.ReadByte((IntPtr)8590636) != 1)
                    {
                        if (!NFS.IsGameRunning())
                        {
                            return;
                        }
                        Thread.Sleep(100);
                    }
                    break;
                case NFSGame.Carbon:
                    while (Program.gameMemory.ReadByte((IntPtr)12297221) != 1)
                    {
                        if (!NFS.IsGameRunning())
                        {
                            return;
                        }
                        Thread.Sleep(100);
                    }
                    break;
                case NFSGame.ProStreet:
                    while (Program.gameMemory.ReadByte((IntPtr)10811144) != 1)
                    {
                        if (!NFS.IsGameRunning())
                        {
                            return;
                        }
                        Thread.Sleep(100);
                    }
                    break;
                case NFSGame.Undercover:
                    while (Program.gameMemory.ReadByte((IntPtr)14193640) != 1)
                    {
                        if (!NFS.IsGameRunning())
                        {
                            return;
                        }
                        Thread.Sleep(100);
                    }
                    break;
                case NFSGame.Undetermined:
                    while (Program.gameMemory.ReadByte((IntPtr)Program.gameMemory.getBaseAddress + 8953180) != 1 && NFS.IsGameRunning())
                    {
                        Thread.Sleep(100);
                    }
                    break;
                default:
                    return;
            }
        }

        private static void ApplyAndLoadIntPtrs()
        {
            switch (Program.currentNFSGame)
            {
                case NFSGame.None:
                    break;
                case NFSGame.Underground:
                    Program.GAMEPLAY_ACTIVE = (IntPtr)7554356;
                    Program.IS_ACTIVITY_MODE = (IntPtr)7554356;
                    return;
                case NFSGame.Underground2:
                    Program.GAMEPLAY_ACTIVE = (IntPtr)8567412;
                    Program.IS_ACTIVITY_MODE = IntPtr.Zero;
                    return;
                case NFSGame.MW:
                    Program.GAMEPLAY_ACTIVE = (IntPtr)9388228;
                    Program.IS_ACTIVITY_MODE = (IntPtr)9516842;
                    return;
                case NFSGame.Carbon:
                    Program.GAMEPLAY_ACTIVE = (IntPtr)10988420;
                    Program.IS_ACTIVITY_MODE = (IntPtr)12025401;
                    return;
                case NFSGame.ProStreet:
                    Program.GAMEPLAY_ACTIVE = (IntPtr)10833596;
                    Program.IS_ACTIVITY_MODE = (IntPtr)12583870;
                    return;
                case NFSGame.Undercover:
                    Program.GAMEPLAY_ACTIVE = (IntPtr)14193138;
                    Program.IS_ACTIVITY_MODE = IntPtr.Zero;
                    return;
                case NFSGame.Undetermined:
                    Program.GAMEPLAY_ACTIVE = (IntPtr)Program.gameMemory.getBaseAddress + 8972352;
                    Program.IS_ACTIVITY_MODE = (IntPtr)Program.gameMemory.getBaseAddress + 9510673;
                    break;
                default:
                    return;
            }
        }

        private static void LoadScripts()
        {
            Program.scripts = new List<ModScript>();
            string[] files = Directory.GetFiles("scripts\\", "*.dll");
            string[] files2 = Directory.GetFiles("scripts\\", "*.cs");
            string[] files3 = Directory.GetFiles("scripts\\", "*.vb");
            int num = 0;
            for (int i = 0; i < files.Length; i++)
            {
                if (Program.IsValidAssembly(files[i]))
                {
                    ModScript modScript = new ModScript(files[i]);
                    Program.scripts.Add(modScript);
                    num++;
                    Log.Print("NFSScriptLoader INFO", string.Format("{0} {1}", "Loaded", modScript.File));
                }
            }
            for (int j = 0; j < files2.Length; j++)
            {
                Program.CompileScript(files2[j]);
                num++;
            }
            for (int k = 0; k < files3.Length; k++)
            {
                Program.CompileScript(files3[k]);
                num++;
            }
            Log.Print("NFSScriptLoader INFO", string.Format("{0} scripts are loaded.", num));
        }

        private static void CallPreScriptMethod()
        {
            for (int i = 0; i < Program.scripts.Count; i++)
            {
                if (Program.scripts[i].HasPre)
                {
                    Program.scripts[i].CallModFunction(ModScript.ModMethod.Pre, new object[0]);
                }
            }
        }

        private static void CallInitScriptMethod()
        {
            for (int i = 0; i < Program.scripts.Count; i++)
            {
                if (Program.scripts[i].HasInitialize)
                {
                    Program.scripts[i].CallModFunction(ModScript.ModMethod.Initialize, new object[0]);
                }
            }
        }

        private static void CallMainScriptMethod()
        {
            for (int i = 0; i < Program.scripts.Count; i++)
            {
                if (Program.scripts[i].HasMain)
                {
                    Program.scripts[i].CallModFunction(ModScript.ModMethod.Main, new object[0]);
                }
            }
        }

        private static void CallScriptsEvents()
        {
            bool flag = false;
            if (Program.currentNFSGame != NFSGame.Undercover && Program.currentNFSGame != NFSGame.Undetermined)
            {
                flag = (Program.gameMemory.ReadByte(Program.GAMEPLAY_ACTIVE) == 1);
            }
            else if (Program.currentNFSGame == NFSGame.Undercover)
            {
                flag = (Program.gameMemory.ReadInt32((IntPtr)14309304) == 6);
            }
            else if (Program.currentNFSGame == NFSGame.Undetermined)
            {
                flag = (Program.gameMemory.ReadInt32((IntPtr)Program.gameMemory.getBaseAddress + 9422640) == 6);
            }
            if (flag && !Program.inGameplay)
            {
                for (int i = 0; i < Program.scripts.Count; i++)
                {
                    if (Program.scripts[i].HasOnGameplayStart && !Program.scripts[i].IsInGameplay)
                    {
                        Program.scripts[i].CallModFunction(ModScript.ModMethod.OnGameplayStart, new object[0]);
                        Program.scripts[i].IsInGameplay = true;
                    }
                }
                Program.inGameplay = true;
            }
            if (!flag && Program.inGameplay)
            {
                for (int j = 0; j < Program.scripts.Count; j++)
                {
                    if (Program.scripts[j].HasOnGameplayExit && Program.scripts[j].IsInGameplay)
                    {
                        Program.scripts[j].CallModFunction(ModScript.ModMethod.OnGameplayExit, new object[0]);
                        Program.scripts[j].IsInGameplay = false;
                    }
                }
                Program.inGameplay = false;
            }
            if (Program.gameMemory.ReadByte(Program.IS_ACTIVITY_MODE) == 1 && !Program.inRace)
            {
                for (int k = 0; k < Program.scripts.Count; k++)
                {
                    if (Program.scripts[k].HasOnActivityEnter && !Program.scripts[k].IsInActivity)
                    {
                        Program.scripts[k].CallModFunction(ModScript.ModMethod.OnActivityEnter, new object[0]);
                        Program.scripts[k].IsInActivity = true;
                    }
                }
                Program.inRace = true;
            }
            if (Program.gameMemory.ReadByte(Program.IS_ACTIVITY_MODE) == 0 && Program.inRace)
            {
                for (int l = 0; l < Program.scripts.Count; l++)
                {
                    if (Program.scripts[l].HasOnActivityExit && Program.scripts[l].IsInActivity)
                    {
                        Program.scripts[l].CallModFunction(ModScript.ModMethod.OnActivityExit, new object[0]);
                        Program.scripts[l].IsInActivity = false;
                    }
                }
                Program.inRace = false;
            }
        }

        private static void CallScriptsOnExit()
        {
            for (int i = 0; i < Program.scripts.Count; i++)
            {
                if (Program.scripts[i].HasOnExit)
                {
                    Program.scripts[i].CallModFunction(ModScript.ModMethod.OnExit, new object[0]);
                }
            }
        }

        private static void Update_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!NFS.IsGameRunning())
            {
                Log.Print("NFSScriptLoader INFO", "Game is not running.");
                Program.Terminate();
            }
            Program.CallScriptsEvents();
            for (int i = 0; i < Program.scripts.Count; i++)
            {
                if (Program.scripts[i].HasUpdate)
                {
                    Program.scripts[i].CallModFunction(ModScript.ModMethod.Update, new object[0]);
                }
            }
        }

        private static bool CompileScript(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            CodeDomProvider codeDomProvider = null;
            new CSharpCodeProvider(new Dictionary<string, string>
            {
                {
                    "CompilerVersion",
                    "v4.0"
                }
            });
            CompilerParameters compilerParameters = new CompilerParameters(new string[]
            {
                "mscorlib.dll",
                "System.Core.dll",
                "System.Windows.Forms.dll",
                "NFSScript.dll"
            });
            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = true;
            if (fileInfo.Extension.ToUpper(CultureInfo.InvariantCulture) == ".CS")
            {
                codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
            }
            else if (fileInfo.Extension.ToUpper(CultureInfo.InvariantCulture) == ".VB")
            {
                codeDomProvider = CodeDomProvider.CreateProvider("VisualBasic");
            }
            if (codeDomProvider == null)
            {
                return false;
            }
            CompilerResults compilerResults = codeDomProvider.CompileAssemblyFromFile(compilerParameters, new string[]
            {
                file
            });
            if (compilerResults.Errors.HasErrors)
            {
                foreach (object obj in compilerResults.Errors)
                {
                    CompilerError compilerError = (CompilerError)obj;
                    Log.Print("NFSScriptLoader ERROR", compilerError.ToString());
                }
                return false;
            }
            ModScript modScript = new ModScript(compilerResults.CompiledAssembly, Path.GetFileName(file));
            Program.scripts.Add(modScript);
            Log.Print("NFSScriptLoader INFO", string.Format("{0} {1}", "Loaded", modScript.File));
            return true;
        }

        private static void Restart()
        {
            Log.Print("NFSScriptLoader INFO", "Restarting...");
            Program.CallScriptsOnExit();
            Program.timer.Stop();
            Program.scripts = null;
            Program.inGameplay = false;
            Program.inRace = false;
            Program.LoadScripts();
            Program.CallPreScriptMethod();
            Program.CallInitScriptMethod();
            Program.CallMainScriptMethod();
            Program.timer.Start();
        }

        private static int FlexiableParse(string s)
        {
            int result;
            try
            {
                result = int.Parse(s);
            }
            catch
            {
                result = 0;
            }
            return result;
        }

        private static bool IsValidAssembly(string dll)
        {
            bool result;
            try
            {
                Assembly.LoadFrom(dll);
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public static bool IsRunning(Process process)
        {
            if (process == null)
            {
                throw new ArgumentNullException("process");
            }
            try
            {
                Process.GetProcessById(process.Id);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        private static void SetKeyboardHook()
        {
            Program._hookID = Program.SetHook(Program._proc);
            Application.Run();
            NativeMethods.UnhookWindowsHookEx(Program._hookID);
        }

        private static void PrintDebugMsg(string msg)
        {
            if (Program.settingDebug == 1)
            {
                Log.Print("NFSScriptLoader DEBUG", msg);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            IntPtr mainWindowHandle = Program.gameMemory.GetMainProcess().MainWindowHandle;
            if ((Program.currentNFSGame != NFSGame.Undetermined && !NFS.IsGameMinimized()) || (Program.currentNFSGame == NFSGame.Undetermined && NFS.IsGameFocused()))
            {
                if (nCode >= 0 && wParam == (IntPtr)257)
                {
                    int num = Marshal.ReadInt32(lParam);
                    if (num == Program.resetKey)
                    {
                        Program.Restart();
                    }
                    for (int i = 0; i < Program.scripts.Count; i++)
                    {
                        if (Program.scripts[i].HasOnKeyUp)
                        {
                            Program.scripts[i].CallModFunction(ModScript.ModMethod.OnKeyUp, new object[]
                            {
                                num
                            });
                        }
                    }
                }
                if (nCode >= 0 && wParam == (IntPtr)256)
                {
                    int num2 = Marshal.ReadInt32(lParam);
                    for (int j = 0; j < Program.scripts.Count; j++)
                    {
                        if (Program.scripts[j].HasOnKeyDown)
                        {
                            Program.scripts[j].CallModFunction(ModScript.ModMethod.OnKeyDown, new object[]
                            {
                                num2
                            });
                        }
                    }
                }
            }
            return NativeMethods.CallNextHookEx(Program._hookID, nCode, wParam, lParam);
        }

        private static void SetProcessVariables(Process p)
        {
            Program.gameProcess = p;
            Program.gameProcessName = p.ProcessName;
            Program.processNameTitle = p.MainWindowTitle;
            Log.Print("NFSScriptLoader INFO", "Game memory is loaded.");
        }

        private static IntPtr SetHook(Program.LowLevelKeyboardProc proc)
        {
            IntPtr result;
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule mainModule = currentProcess.MainModule)
                {
                    result = NativeMethods.SetWindowsHookEx(13, proc, NativeMethods.GetModuleHandle(mainModule.ModuleName), 0U);
                }
            }
            return result;
        }

        private static System.Timers.Timer timer;

        private static int settingShowConsole = 0;

        private static int settingDebug = 0;

        private static bool inGameplay = false;

        private static bool inRace = false;

        private static IntPtr GAMEPLAY_ACTIVE;

        private static IntPtr IS_ACTIVITY_MODE;

        private const int UPDATE_TICK = 1;

        private const int WAIT_BEFORE_LOAD = 5000;

        private static List<ModScript> scripts;

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYUP = 256;

        private const int WM_KEYDOWN = 257;

        private const int SW_HIDE = 0;

        private const int SW_SHOW = 5;

        private const string NFS_SCRIPT_INI_FILE_NAME = "NFSScriptSettings.ini";

        private static int resetKey = 45;

        private static Program.LowLevelKeyboardProc _proc = new Program.LowLevelKeyboardProc(Program.HookCallback);

        private static IntPtr _hookID = IntPtr.Zero;

        private static NFSGame currentNFSGame;

        public static VAMemory gameMemory;

        private static Process gameProcess;

        private static string gameProcessName = string.Empty;

        private static string processNameTitle = string.Empty;

        // (Invoke) Token: 0x06000053 RID: 83
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}
