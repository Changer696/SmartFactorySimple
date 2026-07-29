using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartFactorySimple
{
    public static class Logging
    {
        private static readonly object _lock = new object();
        private static readonly StackRepository<string> _stack = new StackRepository<string>();
        private const string DefaultFileName = AppFileNames.OperationsFileName;

       
        public static string FilePath { get; set; } = AppFileNames.ResolvePath(DefaultFileName);

       
        public static void Log(string username, string description)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string line = $"{timestamp} | {username} | {description}";


                _stack.Push(line);

                File.AppendAllText(FilePath, line + Environment.NewLine);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Logging failed: {ex.Message}");
            }
        }

       
        public static void Log(string description)
        {
            Log(Environment.UserName, description);
           
        }

        
        public static bool TryPop(out string entry)
        {
            entry = null;
            if (_stack.IsEmpty)
                return false;

            entry = _stack.Pop();
            return entry != null;
        }

        
        public static bool TryPeek(out string entry)
        {
            entry = null;
            if (_stack.IsEmpty)
                return false;

            entry = _stack.Peek();
            return entry != null;
        }

        
        public static string[] GetAllEntries()
        {
            return _stack.GetAll().ToArray();
        }

       
        
    }
}
