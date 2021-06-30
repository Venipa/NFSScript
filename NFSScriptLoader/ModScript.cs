using System;
using System.IO;
using System.Reflection;
using System.Text;
using NFSScript;

namespace NFSScriptLoader
{
    public class ModScript
    {
        public string File { get; private set; }

        public bool HasInitialize { get; private set; }

        public bool HasPre { get; private set; }

        public bool HasMain { get; private set; }

        public bool HasUpdate { get; private set; }

        public bool HasOnKeyUp { get; private set; }

        public bool HasOnKeyDown { get; private set; }

        public bool HasOnGameplayStart { get; private set; }

        public bool HasOnGameplayExit { get; private set; }

        public bool HasOnActivityEnter { get; private set; }

        public bool HasOnActivityExit { get; private set; }

        public bool HasOnExit { get; private set; }

        public bool IsInGameplay { get; set; }

        public bool IsInActivity { get; set; }

        public ModScript(string dllFile)
        {
            this.IsInGameplay = false;
            this.IsInActivity = false;
            this.File = dllFile;
            string dllFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.File);
            this.t = NReflec.GetTypesFromDLL(dllFilePath);
            this.methods = NReflec.GetMethodsFromDLL(dllFilePath);
            this.CheckMethods();
        }

        public ModScript(Assembly ass, string fileName)
        {
            this.IsInGameplay = false;
            this.IsInActivity = false;
            this.File = fileName;
            this.t = NReflec.GetTypesFromAssembly(ass);
            this.methods = NReflec.GetMethodsFromAssembly(ass);
            this.CheckMethods();
        }

        private void CheckMethods()
        {
            for (int i = 0; i < this.methods.Length; i++)
            {
                if (this.methods[i].Name.Equals("Pre"))
                {
                    this.HasPre = true;
                }
                if (this.methods[i].Name.Equals("Initialize"))
                {
                    this.HasInitialize = true;
                }
                if (this.methods[i].Name.Equals("Main"))
                {
                    this.HasMain = true;
                }
                if (this.methods[i].Name.Equals("Update"))
                {
                    this.HasUpdate = true;
                }
                if (this.methods[i].Name.Equals("OnKeyUp"))
                {
                    this.HasOnKeyUp = true;
                }
                if (this.methods[i].Name.Equals("OnKeyDown"))
                {
                    this.HasOnKeyDown = true;
                }
                if (this.methods[i].Name.Equals("OnGameplayStart"))
                {
                    this.HasOnGameplayStart = true;
                }
                if (this.methods[i].Name.Equals("OnGameplayExit"))
                {
                    this.HasOnGameplayExit = true;
                }
                if (this.methods[i].Name.Equals("OnActivityEnter"))
                {
                    this.HasOnActivityEnter = true;
                }
                if (this.methods[i].Name.Equals("OnActivityExit"))
                {
                    this.HasOnActivityExit = true;
                }
                if (this.methods[i].Name.Equals("OnExit"))
                {
                    this.HasOnExit = true;
                }
            }
        }

        public void CallModFunction(ModScript.ModMethod modMethod, params object[] o)
        {
            string methodName = string.Empty;
            switch (modMethod)
            {
                case ModScript.ModMethod.None:
                    methodName = string.Empty;
                    break;
                case ModScript.ModMethod.Initialize:
                    methodName = "Initialize";
                    break;
                case ModScript.ModMethod.Main:
                    methodName = "Main";
                    break;
                case ModScript.ModMethod.Update:
                    methodName = "Update";
                    break;
                case ModScript.ModMethod.OnKeyDown:
                    methodName = "OnKeyDown";
                    break;
                case ModScript.ModMethod.OnKeyUp:
                    methodName = "OnKeyUp";
                    break;
                case ModScript.ModMethod.OnGameplayStart:
                    methodName = "OnGameplayStart";
                    break;
                case ModScript.ModMethod.OnGameplayExit:
                    methodName = "OnGameplayExit";
                    break;
                case ModScript.ModMethod.OnActivityEnter:
                    methodName = "OnActivityEnter";
                    break;
                case ModScript.ModMethod.OnActivityExit:
                    methodName = "OnActivityExit";
                    break;
                case ModScript.ModMethod.OnExit:
                    methodName = "OnExit";
                    break;
                case ModScript.ModMethod.Pre:
                    methodName = "Pre";
                    break;
            }
            for (int i = 0; i < this.t.Length; i++)
            {
                if (this.t[i].IsSubclassOf(typeof(Mod)))
                {
                    try
                    {
                        NReflec.CallMethodFromType(this.t[i], methodName, o);
                    }
                    catch (Exception ex)
                    {
                        Log.Print("EXCEPTION", ex.ToString());
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("ModScript: File = ");
            stringBuilder.Append(this.File);
            stringBuilder.Append(" Types: ");
            stringBuilder.Append(this.t.Length);
            stringBuilder.Append(" Methods: ");
            for (int i = 0; i < this.methods.Length; i++)
            {
                stringBuilder.AppendLine(string.Format("{0} {1}: {2} ", "Method", i, this.methods[i].Name));
            }
            return stringBuilder.ToString();
        }

        private Type[] t;

        private MethodInfo[] methods;

        public enum ModMethod : byte
        {
            None,
            Initialize,
            Main,
            Update,
            OnKeyDown,
            OnKeyUp,
            OnGameplayStart,
            OnGameplayExit,
            OnActivityEnter,
            OnActivityExit,
            OnExit,
            Pre
        }
    }
}
