using System;
using System.Collections.Generic;
using System.Threading;

namespace tetris
{
    class Program
    {
        //Settings tetris frame
        static int TetrisRows = 20;
        static int TetrisCols = 10;
        static int InfoCols = 10;
        static int ConsoleRows = 1 + TetrisRows + 1;
        static int ConsoleCols = 1 + TetrisCols + 1 + InfoCols + 1;
        static List<bool[,]> TetrisFigures = new List<bool[,]>()
        {
            new bool [,] // I ----
            {
                {true, true, true, true }
            },
            new bool [,] // O
            {
                {true, true  },
                {true, true  }
            },
            new bool [,] // T
            {
                {false, true, false},
                {true, true ,true }
            },
            new bool [,] // S
            {
                {false, true, true},
                {true, true, false}
            },
            new bool[,] // Z
            {
                {true, true, false},
                {false, true, true}
            },
            new bool[,] // J
            {
                {false, false, true},
                {true, true, true}
            },
            new bool[,] // L
            {
                {true, false, false},
                {true, true, true}
            }
        };

        //State
        static int Score = 0;
        static int Frame = 0;
        static int FrameToMoveFigure = 15;
        static int CurrentFigureIndex = 2;
        static int CurrentFigureRow = 0;
        static int CurrentFigureCol = 0;
        static bool[,] TetrisField = new bool[TetrisRows, TetrisCols];

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
                Frame++;
                // Read user input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape)
                    {
                        //Environment.Exit(0);
                        return; //bacause of Main()
                    }

                    if (key.Key == ConsoleKey.Spacebar)
                    {
                        //TODO: Implement 90-degrees rotation of current figure
                    }

                    if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A)
                    {
                        //TODO: Move current figure to left
                        CurrentFigureCol--; //TODO: out of range
                    }
                    if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)
                    {
                        //TODO: Move current figure to right
                        CurrentFigureCol++; //TODO: out of range
                    }
                    if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W)
                    {
                        //TODO: Implement 90-degrees rotation of current figure
                    }
                    if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
                    {
                        Frame = 1;
                        Score++;
                        CurrentFigureRow++;
                        //TODO: Move current figure to down
                    }
                }


                //Update the game state
                if (Frame % FrameToMoveFigure == 0)
                {
                    CurrentFigureRow++;
                    Frame = 0;
                    Score++;
                }
                // user input
                // change state
                //if (Collision())
                //{
                //    AddCurrentFigureToTetrisField()
                //    CheckForFullLines()
                //    if(lines remove) score++  
                //}
                
                //Redraw UI
                DrawBorder();
                DrawInfo();
                //TODO: DrawTetrisField()
                DrawCurrentFigure();

                //wait 40 miliseconds
                Thread.Sleep(40);
            }
        }

       
        static void DrawInfo()
        {
            Write("Score:", 1, TetrisCols + 3);
            Write(Score.ToString(), 2, TetrisCols + 3);
            Write("Frame:", 4, TetrisCols + 3);
            Write(Frame.ToString(), 5, TetrisCols + 3);
        }

        static void DrawCurrentFigure()
        {
            var currentFigure = TetrisFigures[CurrentFigureIndex];
            for (int row = 0; row < currentFigure.GetLength(0); row++)
            {
                for (int col = 0; col < currentFigure.GetLength(1); col++)
                {
                    if (currentFigure[row, col])
                    {
                        Write("*", row+1+CurrentFigureRow, col+1+CurrentFigureCol);
                    } 
                }
            }
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
