using System;
using System.Threading;

namespace tetris
{
    class Program
    {
        //Settings tetris frame
        static int TetrisRows = 20;
        static int TetrisCols = 20;
        static int InfoCols = 10;
        static int ConsoleRows = 1 + TetrisRows + 1;
        static int ConsoleCols = 1 + TetrisCols + 1 + InfoCols + 1;

        //State
        static int Score = 0;
        static void Main(string[] args)
        {
            Console.Title = "Tetris V1.0 by y.yordanov21";
            Console.CursorVisible = false;
            Console.WindowHeight = ConsoleRows + 1;
            Console.WindowWidth = ConsoleCols;
            Console.BufferHeight = ConsoleRows + 1;
            Console.BufferWidth = ConsoleCols;
            DrawBorder();
            DrawInfo();
            while (true)
            {

                if (Console.KeyAvailable)
                {
                  var key=  Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape)
                    {
                        //Environment.Exit(0);
                        return; //bacause of Main()
                    }
                }
               

                Score++;
                // user input
                // change state
                // redraw UI

                //wait 40 miliseconds
                DrawBorder();
                DrawInfo();
                Thread.Sleep(40);
            }
        }

        static void DrawInfo()
        {
            Write("Score:", 1, TetrisCols + 3);
            Write(Score.ToString(), 2, TetrisCols + 3);
        }

        static void DrawBorder()
        {
            //always start drawing border from point (0,0);
            Console.SetCursorPosition(0, 0);

            //drawing border
            string firstLine = "╔";
            firstLine += new string('═', TetrisCols);
            firstLine += "╦";
            firstLine += new string('═', InfoCols);
            firstLine += "╗";
            Console.Write(firstLine);

            for (int i = 0; i < TetrisRows; i++)
            {
                string middleLine = "║";
                middleLine += new string(' ', TetrisCols) + "║" + new string(' ', InfoCols) + "║";
                Console.Write(middleLine);
            }

            string endLine = "╚";
            endLine += new string('═', TetrisCols);
            endLine += "╩";
            endLine += new string('═', InfoCols);
            endLine += "╝";
            Console.Write(endLine);

        }

        static void Write(string text, int row, int col, ConsoleColor color = ConsoleColor.Yellow)
        {
            Console.ForegroundColor = color;
            Console.SetCursorPosition(col, row);
            Console.Write(text);
            Console.ResetColor();

        }
    }
}
