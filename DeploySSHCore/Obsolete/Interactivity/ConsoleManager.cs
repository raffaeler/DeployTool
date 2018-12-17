using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH.Interactivity
{
    public class ConsoleManager
    {
        public ConsoleManager()
        {

        }


        public ConsoleColor Foreground { get; } = Console.ForegroundColor;
        public ConsoleColor Background { get; } = Console.BackgroundColor;

        public void Clear()
        {
            Console.Clear();
        }
    }
}
