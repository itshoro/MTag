using System;
using System.Collections.Generic;
using System.Text;

namespace MusicMetaData.Util
{
    public static class Logger
    {
        public static void Log(LogLevel level, string message)
        {
            Console.WriteLine($"[{level.ToString()}] {DateTime.UtcNow} - {message}");
        }
    }

    public enum LogLevel
    {
        Notice,
        Warning,
        Error,
        Debug
    }
}
