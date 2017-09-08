using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Helpers
{
    public static class ConsoleManager
    {
        public static ConsoleColor Foreground { get; } = Console.ForegroundColor;
        public static ConsoleColor Background { get; } = Console.BackgroundColor;

        public static void Clear()
        {
            Console.Clear();
        }

        public static void WriteError(string message)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = current;
        }

        public static string RunLoop(string heading, string[] items)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            ConsoleKey key;
            int currentPage = 0;
            int maxPage = items.Length / 9 + (items.Length % 9 == 0 ? 0 : 1);

            do
            {
                Clear(heading, items, currentPage, maxPage);

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

            return null;
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


    }
}
