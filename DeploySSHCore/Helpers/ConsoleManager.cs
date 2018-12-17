using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeploySSH.Configuration;

namespace DeploySSH.Helpers
{
    public static class ConsoleManager
    {
        public static ConsoleColor Foreground { get; }
        public static ConsoleColor Background { get; }

        static ConsoleManager()
        {
            Foreground = Console.ForegroundColor;
            Background = Console.BackgroundColor;
        }

        public static void Clear()
        {
            Console.Clear();
        }

        public static string ReadLine(bool isEchoOff)
        {
            ConsoleKeyInfo keyInfo;
            var sb = new StringBuilder();
            while (true)
            {
                keyInfo = Console.ReadKey(isEchoOff);
                if (keyInfo.Key == ConsoleKey.Enter) break;
                sb.Append(keyInfo.KeyChar);
            }

            return sb.ToString();
        }

        public static ConsoleState GetConsoleState()
        {
            var left = Console.CursorLeft;
            var top = Console.CursorTop;
            return new ConsoleState(left, top);
        }

        public static void SetConsoleState(ConsoleState state)
        {
            Console.CursorLeft = state.Left;
            Console.CursorTop = state.Top;
        }

        public static void SetConsoleState(int left, int top)
        {
            Console.CursorLeft = left;
            Console.CursorTop = top;
        }

        public static void WriteAt(int left, int top, string message)
        {
            Console.CursorLeft = left;
            Console.CursorTop = top;
            Console.Write(message);
        }

        public static void ClearLine(int top)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = top;
            var width = Console.WindowWidth;
            Console.Write(new string(' ', width));
        }

        public static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(message);
            RestoreColors();
        }

        public static void WriteUnkOutput(string message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(message);
            RestoreColors();
        }

        public static void WriteError(string message, bool printcrlf = true)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            if(printcrlf)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
                Console.Write(" ");
            }

            RestoreColors();
        }

        public static void RestoreColors()
        {
            Console.ForegroundColor = Foreground;
            Console.BackgroundColor = Background;
        }

        public static FileInfo RunLoop(string heading, FileInfo[] items)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            ConsoleKey key;
            int currentPage = 0;
            int maxPage = items.Length / 9 + (items.Length % 9 == 0 ? 0 : 1);

            var texts = items
                .Select(f => GetTitle(f))
                .OrderBy(n => n)
                .ToArray();

            do
            {
                Clear(heading, texts, currentPage, maxPage);

                var keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                if (key == ConsoleKey.PageDown)
                {
                    if (currentPage < maxPage - 1)
                    {
                        currentPage++;
                    }

                    continue;
                }

                if (key == ConsoleKey.PageUp)
                {
                    if(currentPage >0)
                    {
                        currentPage--;
                    }

                    continue;
                }

                int index = GetKey(key) - 1;

                if (index < 0)
                {
                    continue;
                }

                index = currentPage * 9 + index;

                if (index >= items.Length)
                    continue;

                return items[index];
            }
            while (key != ConsoleKey.Q);

            Console.BackgroundColor = Background;
            Clear();

            return null;
        }

        private static string GetTitle(System.IO.FileInfo fileInfo)
        {
            var simpleName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name);
            var content = System.IO.File.ReadAllText(fileInfo.FullName);
            var config = JsonHelper.Deserialize(content);
            if (config == null || config.Description == null)
            {
                return simpleName;
            }

            var actions = string.Join(", ", config.Actions.Select(a => a.GetShortActionName()));
            if (string.IsNullOrEmpty(actions))
            {
                return $"{simpleName} ({config.Description}: no actions)";
            }

            return $"{simpleName} ({config.Description}: {actions})";
        }

        private static int GetKey(ConsoleKey key)
        {
            var str = Normalize(key);
            int.TryParse(str, out int number);
            return number;
        }

        private static string Normalize(ConsoleKey consoleKey)
        {
            var str = consoleKey.ToString().Trim('D');
            return str;
        }


        private static void Clear(string heading, string[] items, int currentPage, int maxPage)
        {
            Console.Clear();
            PrintHeading(heading, maxPage);
            PrintMenu(items, currentPage);
        }

        private static void PrintHeading(string heading, int pages)
        {
            Console.WriteLine(heading);
            if (pages > 1)
            {
                Console.WriteLine("(Use Page-Up and Page-Down to see the other files)");
            }

            Console.WriteLine("");
        }

        private static void PrintMenu(string[] items, int currentPage)
        {
            int index = 1;
            var max = Math.Min(currentPage * 9 + 9, items.Length);
            for(int i= currentPage * 9; i<max; i++)
            {
                var item = items[i];
                Console.WriteLine($"{index}. {item}");
                index++;
            }
        }

        public class ConsoleState
        {
            public ConsoleState(int left, int top)
            {
                this.Left = left;
                this.Top = top;
            }

            public int Left { get; set; }
            public int Top { get; set; }
        }
    }
}
