namespace NFSScript.Core
{
    /// <summary>
    /// Where the magic happens.
    /// </summary>
    public static class GameMemory
    {

        /// <summary>
        /// Where the magic happens.
        /// </summary>
        public static VAMemory memory;

        /// <summary>
        /// Check if Main WindowHandle is minimized
        /// </summary>
        /// <returns></returns>
        public static bool isMinimized()
        {
            return NativeMethods.IsIconic(memory.GetMainProcess().Handle);
        }
        /// <summary>
        /// Check if Main WindowHandle is focused
        /// </summary>
        /// <returns></returns>
        public static bool isFocused()
        {
            return NativeMethods.GetForegroundWindow() == memory.GetMainProcess().Handle;
        }
    }
}
