using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;
using System.Text.Encodings;

namespace tetris
{
    class Program
    {
        //Settings 
        static int TetrisRows = 20;
        static int TetrisCols = 10;
        static int InfoCols = 12;
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
        static string ScoresFileName = "scores.txt";
        static int[] ScorePerLines = { 0, 40, 100, 300, 1200 };
        //State
        static int HighScore = 0;
        static int Score = 0;
        static int Frame = 0;
        static int Level = 1;
        static int FrameToMoveFigure = 16;
        static bool[,] CurrentFigure = null;
        static int CurrentFigureRow = 0;
        static int CurrentFigureCol = 0;
        static bool[,] TetrisField = new bool[TetrisRows, TetrisCols];
        static Random Random = new Random();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            PlayMusic();

            if (File.Exists(ScoresFileName))
            {
                var allScores = File.ReadAllLines(ScoresFileName);
                foreach (var score in allScores)
                {
                    var match = Regex.Match(score, @" => (?<score>[0-9]+)");
                    HighScore = Math.Max(HighScore, int.Parse(match.Groups["score"].Value));
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Title = "Tetris V1.0 by y.yordanov21";
            Console.CursorVisible = false;
            Console.WindowHeight = ConsoleRows + 1;
            Console.WindowWidth = ConsoleCols;
            Console.BufferHeight = ConsoleRows + 1;
            Console.BufferWidth = ConsoleCols;
            CurrentFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            //DrawBorder();
            //DrawInfo();
            while (true)
            {
                Frame++;
                UpdateLevel();
                // Read user input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape)
                    {
                        //Environment.Exit(0);
                        return; //bacause of Main()
                    }

                    if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A)
                    {
                        if (CurrentFigureCol >= 1)
                        {
                            CurrentFigureCol--;
                        }

                    }
                    if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)
                    {
                        if (CurrentFigureCol < TetrisCols - CurrentFigure.GetLength(1))
                        {
                            CurrentFigureCol++;
                        }

                    }
                    if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W || key.Key == ConsoleKey.Spacebar)
                    {
                        RotateCurrentFigure();
                    }
                    if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
                    {
                        Frame = 1;
                        Score += Level;
                        CurrentFigureRow++;

                    }
                }


                //Update the game state
                if (Frame % (FrameToMoveFigure - Level) == 0)
                {
                    CurrentFigureRow++;
                    Frame = 0;
                    Score++;
                }
                // user input
                // change state
                if (Collision(CurrentFigure))
                {
                    AddCurrentFigureToTetrisField();
                    int lines = CheckForFullLines();
                    //add points to score
                    Score += ScorePerLines[lines] * Level;
                    CurrentFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
                    CurrentFigureCol = 0;
                    CurrentFigureRow = 0;

                    //game over!
                    if (Collision(CurrentFigure))
                    {
                        File.AppendAllLines(ScoresFileName, new List<string>
                        {
                            $"[{DateTime.Now.ToString()}] {Environment.UserName} => {Score}"
                        });
                        var scoreAsString = Score.ToString();
                        scoreAsString += new string(' ', 7 - scoreAsString.Length);
                        Write("╔══════════════╗", 5, 5);
                        Write("║  Game        ║", 6, 5);
                        Write("║     over!    ║", 7, 5);
                        Write($"║      {scoreAsString} ║", 8, 5);
                        Write("╚══════════════╝", 9, 5);
                        Thread.Sleep(100000);
                        return;
                    }

                }

                //Redraw UI
                DrawBorder();
                DrawInfo();
                DrawTetrisField();
                DrawCurrentFigure();

                //wait 40 miliseconds
                Thread.Sleep(40);
            }
        }

        private static void UpdateLevel()
        {
            if (Score <= 0)
            {
                Level = 1;
            }
            Level = (int)Math.Log10(Score) - 1;
            if (Level < 1)
            {
                Level = 1;
            }
            if (Level > 10)
            {
                Level = 10;
            }

        }

        private static void RotateCurrentFigure()
        {
            var newFigure = new bool[CurrentFigure.GetLength(1), CurrentFigure.GetLength(0)];
            for (int row = 0; row < CurrentFigure.GetLength(0); row++)
            {
                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {
                    newFigure[col, CurrentFigure.GetLength(0) - row - 1] = CurrentFigure[row, col];
                }
            }

            if (!Collision(newFigure))
            {
                CurrentFigure = newFigure;
            }

        }

        private static int CheckForFullLines() //0,1,2,3,4
        {
            int lines = 0;

            for (int row = 0; row < TetrisField.GetLength(0); row++)
            {
                bool rowIsFull = true;
                for (int col = 0; col < TetrisField.GetLength(1); col++)
                {
                    if (!TetrisField[row, col])
                    {
                        rowIsFull = false;
                        break;
                    }
                }

                if (rowIsFull)
                {
                    for (int rowToMove = row; rowToMove >= 1; rowToMove--)
                    {
                        for (int col = 0; col < TetrisField.GetLength(1); col++)
                        {
                            TetrisField[rowToMove, col] = TetrisField[rowToMove - 1, col];
                        }
                    }

                    lines++;

                }
            }
            return lines;
        }

        private static void AddCurrentFigureToTetrisField()
        {
            for (int row = 0; row < CurrentFigure.GetLength(0); row++)
            {
                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {
                    if (CurrentFigure[row, col])
                    {
                        TetrisField[CurrentFigureRow + row, CurrentFigureCol + col] = true;
                    }
                }
            }
        }

        static bool Collision(bool[,] figure)
        {
            if (CurrentFigureCol > TetrisCols - figure.GetLength(1))
            {
                return true;
            }


            if (CurrentFigureRow + figure.GetLength(0) == TetrisRows)
            {
                return true;
            }

            for (int row = 0; row < figure.GetLength(0); row++)
            {
                for (int col = 0; col < figure.GetLength(1); col++)
                {
                    if (figure[row, col] && TetrisField[CurrentFigureRow + row + 1, CurrentFigureCol + col])
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        static void DrawInfo()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
            }
            Write("Level:", 1, TetrisCols + 3);
            Write(Level.ToString(), 2, TetrisCols + 3);
            Write("Score:", 4, TetrisCols + 3);
            Write(Score.ToString(), 5, TetrisCols + 3);
            Write("High Score:", 7, TetrisCols + 3);
            Write(HighScore.ToString(), 8, TetrisCols + 3);
            //Write("Frame:", 10, TetrisCols + 3);
            //Write(Frame.ToString(), 11, TetrisCols + 3);
            //Write("Position:", 13, TetrisCols + 3);
            //Write($"{CurrentFigureCol}, {CurrentFigureCol}", 14, TetrisCols + 3);
            Write("Keys:", 16, TetrisCols + 3);
            Write("  ^  ", 17, TetrisCols + 3);
            Write("< v >", 18, TetrisCols + 3);
        }

        static void DrawTetrisField()
        {
            for (int row = 0; row < TetrisField.GetLength(0); row++)
            {
                string line = "";
                for (int col = 0; col < TetrisField.GetLength(1); col++)
                {
                    if (TetrisField[row, col])
                    {
                        line += "■";
                    }
                    else
                    {
                        line += " ";
                    }
                }
                //+1 zaradi border-a 
                Write(line, row + 1, 1);
            }
        }

        static void DrawCurrentFigure()
        {

            for (int row = 0; row < CurrentFigure.GetLength(0); row++)
            {
          
                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {              
                    if (CurrentFigure[row, col])
                    {

                        Write("■", row + 1 + CurrentFigureRow,col+ 1 + CurrentFigureCol);
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

        static void Write(string text, int row, int col)
        {
            Console.SetCursorPosition(col, row);
            Console.Write(text);
        }

        private static void PlayMusic()
        {
            new Thread(() => {
                while (true)
                {
                    const int soundLenght = 100;
                    Console.Beep(1320, soundLenght * 4);
                    Console.Beep(990, soundLenght * 2);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(1188, soundLenght * 2);
                    Console.Beep(1320, soundLenght);
                    Console.Beep(1188, soundLenght);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(990, soundLenght * 2);
                    Console.Beep(880, soundLenght * 4);
                    Console.Beep(880, soundLenght * 2);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(1320, soundLenght * 4);
                    Console.Beep(1188, soundLenght * 2);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(990, soundLenght * 6);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(1188, soundLenght * 4);
                    Console.Beep(1320, soundLenght * 4);
                    Console.Beep(1056, soundLenght * 4);
                    Console.Beep(880, soundLenght * 4);
                    Console.Beep(880, soundLenght * 4);
                    Thread.Sleep(soundLenght * 2);
                    Console.Beep(1188, soundLenght * 4);
                    Console.Beep(1408, soundLenght * 2);
                    Console.Beep(1760, soundLenght * 4);
                    Console.Beep(1584, soundLenght * 2);
                    Console.Beep(1408, soundLenght * 2);
                    Console.Beep(1320, soundLenght * 6);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(1320, soundLenght * 4);
                    Console.Beep(1188, soundLenght * 2);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(990, soundLenght * 4);
                    Console.Beep(990, soundLenght * 2);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(1188, soundLenght * 4);
                    Console.Beep(1320, soundLenght * 4);
                    Console.Beep(1056, soundLenght * 4);
                    Console.Beep(880, soundLenght * 4);
                    Console.Beep(880, soundLenght * 4);
                    Thread.Sleep(soundLenght * 4);
                    Console.Beep(1320, soundLenght * 4);
                    Console.Beep(990, soundLenght * 2);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(1188, soundLenght * 2);
                    Console.Beep(1320, soundLenght);
                    Console.Beep(1188, soundLenght);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(990, soundLenght * 2);
                    Console.Beep(880, soundLenght * 4);
                    Console.Beep(880, soundLenght * 2);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(1320, soundLenght * 4);
                    Console.Beep(1188, soundLenght * 2);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(990, soundLenght * 6);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(1188, soundLenght * 4);
                    Console.Beep(1320, soundLenght * 4);
                    Console.Beep(1056, soundLenght * 4);
                    Console.Beep(880, soundLenght * 4);
                    Console.Beep(880, soundLenght * 4);
                    Thread.Sleep(soundLenght * 2);
                    Console.Beep(1188, soundLenght * 4);
                    Console.Beep(1408, soundLenght * 2);
                    Console.Beep(1760, soundLenght * 4);
                    Console.Beep(1584, soundLenght * 2);
                    Console.Beep(1408, soundLenght * 2);
                    Console.Beep(1320, soundLenght * 6);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(1320, soundLenght * 4);
                    Console.Beep(1188, soundLenght * 2);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(990, soundLenght * 4);
                    Console.Beep(990, soundLenght * 2);
                    Console.Beep(1056, soundLenght * 2);
                    Console.Beep(1188, soundLenght * 4);
                    Console.Beep(1320, soundLenght * 4);
                    Console.Beep(1056, soundLenght * 4);
                    Console.Beep(880, soundLenght * 4);
                    Console.Beep(880, soundLenght * 4);
                    Thread.Sleep(soundLenght * 4);
                    Console.Beep(660, soundLenght * 8);
                    Console.Beep(528, soundLenght * 8);
                    Console.Beep(594, soundLenght * 8);
                    Console.Beep(495, soundLenght * 8);
                    Console.Beep(528, soundLenght * 8);
                    Console.Beep(440, soundLenght * 8);
                    Console.Beep(419, soundLenght * 8);
                    Console.Beep(495, soundLenght * 8);
                    Console.Beep(660, soundLenght * 8);
                    Console.Beep(528, soundLenght * 8);
                    Console.Beep(594, soundLenght * 8);
                    Console.Beep(495, soundLenght * 8);
                    Console.Beep(528, soundLenght * 4);
                    Console.Beep(660, soundLenght * 4);
                    Console.Beep(880, soundLenght * 8);
                    Console.Beep(838, soundLenght * 16);
                    Console.Beep(660, soundLenght * 8);
                    Console.Beep(528, soundLenght * 8);
                    Console.Beep(594, soundLenght * 8);
                    Console.Beep(495, soundLenght * 8);
                    Console.Beep(528, soundLenght * 8);
                    Console.Beep(440, soundLenght * 8);
                    Console.Beep(419, soundLenght * 8);
                    Console.Beep(495, soundLenght * 8);
                    Console.Beep(660, soundLenght * 8);
                    Console.Beep(528, soundLenght * 8);
                    Console.Beep(594, soundLenght * 8);
                    Console.Beep(495, soundLenght * 8);
                    Console.Beep(528, soundLenght * 4);
                    Console.Beep(660, soundLenght * 4);
                    Console.Beep(880, soundLenght * 8);
                    Console.Beep(838, soundLenght * 16);
                }
            }).Start();
        }
    }
}
