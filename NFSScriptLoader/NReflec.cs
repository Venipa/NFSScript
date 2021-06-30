using System;
using System.Collections.Generic;
using System.Reflection;

namespace NFSScriptLoader
{
    internal static class NReflec
    {
        public static Type[] GetTypesFromDLL(string dllFilePath)
        {
            return Assembly.LoadFile(dllFilePath).GetTypes();
        }

        public static Type[] GetTypesFromAssembly(Assembly ass)
        {
            return ass.GetTypes();
        }

        public static MethodInfo[] GetMethodsFromDLL(string dllFilePath)
        {
            List<MethodInfo> list = new List<MethodInfo>();
            Type[] types = Assembly.LoadFile(dllFilePath).GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                list.AddRange(types[i].GetMethods());
            }
            return list.ToArray();
        }

        public static MethodInfo[] GetMethodsFromAssembly(Assembly ass)
        {
            List<MethodInfo> list = new List<MethodInfo>();
            Type[] types = ass.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                list.AddRange(types[i].GetMethods());
            }
            return list.ToArray();
        }

        public static object CallMethodFromType(Type t, string methodName, params object[] o)
        {
            MethodInfo method = t.GetMethod(methodName);
            ParameterInfo[] parameters = method.GetParameters();
            return method.Invoke(Activator.CreateInstance(t, null), (parameters.Length == 0) ? null : o);
        }

        public static object CallMethodFromFile(string dllFilePath, string typeName, string methodName, params object[] o)
        {
            Type type = Assembly.LoadFile(dllFilePath).GetType(typeName);
            MethodInfo method = type.GetMethod(methodName);
            ParameterInfo[] parameters = method.GetParameters();
            return method.Invoke(Activator.CreateInstance(type, null), (parameters.Length == 0) ? null : o);
        }
    }
}
