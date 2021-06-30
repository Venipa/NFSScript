using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace NFSScript.Utils
{
    /// <summary>
    /// Config File
    /// </summary>
    public class ConfigurationFile
    {
        private readonly string _path;
        /// <summary>
        /// 
        /// </summary>
        public string Path { get => _path; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get => System.IO.Path.GetFileName(_path); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public ConfigurationFile(string path)
        {
            _path = path;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, _path);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetValue(string section, string key)
        {
            var Builder = new StringBuilder(255);

            GetPrivateProfileString(section, key, "", Builder, 255, _path);

            var ReturnValue = Builder.ToString();

            return ReturnValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetString(string section, string key)
        {
            return GetValue(section, key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte GetByte(string section, string key)
        {
            return Convert.ToByte(GetValue(section, key));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public short GetInt16(string section, string key)
        {
            return Convert.ToInt16(GetValue(section, key));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetInt32(string section, string key)
        {
            return Convert.ToInt32(GetValue(section, key));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public long GetInt64(string section, string key)
        {
            return Convert.ToInt64(GetValue(section, key));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetFloat(string section, string key)
        {
            return Convert.ToSingle(GetValue(section, key), CultureInfo.InvariantCulture.NumberFormat);
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
    }
}
