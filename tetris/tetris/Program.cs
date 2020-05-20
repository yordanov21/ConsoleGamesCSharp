using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;
using System.Media;

namespace tetris
{
    class Program
    {
        // the code was made for education purpose with the support of the Software University and videos of Nikolay Kostov
        //Settings 
        static int TetrisRows = 21;
        static int TetrisCols = 10;
        static int InfoCols = 20;
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
        static string FigureSymbol = "■";
        static int FrameToMoveFigure = 16;
        static bool[,] CurrentFigure = null;
        static int CurrentFigureRow = 0;
        static int CurrentFigureCol = 0;
        static bool[,] NextFigure = null;
        static int NextFigureRow = 14;
        static int NextFigureCol = TetrisCols + 3;
        static bool[,] TetrisField = new bool[TetrisRows, TetrisCols];
        static Random Random = new Random();
        static bool PauseMode = false;
        static bool PlayGame = true;
        static int SongLevel = 2;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.Unicode;
            Console.WriteLine("Welcome to Tetris Console Game by y.yordanov21.");
            Console.WriteLine("");
            Console.WriteLine("All rights reserved!");
            Console.WriteLine("");
            Console.WriteLine("Please select music: Y/N");
            Console.Write("Play music:");
            string music = Console.ReadLine();
            if (music == "Y")
            {
                PlayGame = true;
            }
            else
            {
                PlayGame = false;
            }
            PlayConsoleMusic();

            if (File.Exists(ScoresFileName))
            {
                var allScores = File.ReadAllLines(ScoresFileName);
                foreach (var score in allScores)
                {
                    var match = Regex.Match(score, @" => (?<score>[0-9]+)");
                    HighScore = Math.Max(HighScore, int.Parse(match.Groups["score"].Value));
                }
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Title = "Tetris V1.0 by y.yordanov21";
            Console.CursorVisible = false;
            Console.WindowHeight = ConsoleRows;
            Console.WindowWidth = ConsoleCols;
            Console.BufferHeight = ConsoleRows;
            Console.BufferWidth = ConsoleCols;
            CurrentFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];

            while (true)
            {
                Frame++;
                UpdateLevel();

                // Read user input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Spacebar && PauseMode == false)
                    {
                        PauseMode = true;

                        Write("╔═══════════════╗", 5, 5);
                        Write("║               ║", 6, 5);
                        Write("║     Pause     ║", 7, 5);
                        Write("║               ║", 8, 5);
                        Write("╚═══════════════╝", 9, 5);
                        //TODO music don't STOP after pause 
                        PlayGame = false;
                        Console.ReadKey();
                    }

                    if (key.Key == ConsoleKey.Spacebar && PauseMode == true)
                    {
                        PlayGame = true;

                        PlayConsoleMusic();
                        PauseMode = false;
                    }

                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
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
                        if ((CurrentFigureCol < TetrisCols - CurrentFigure.GetLength(1)))
                        {
                            CurrentFigureCol++;
                        }
                    }

                    if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W)
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
                    //CurrentFigure = NextFigure;
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
                        PlayGame = false;
                        PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\gameover.wav");
                        Thread.Sleep(1000000);
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

        private static bool[,] GetNextFigure()
        {
            NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            return NextFigure;
        }

        private static void UpdateLevel()
        {
            if (Score <= 0)
            {
                Level = 1;
            }

            if (Score >= 1000 && SongLevel == 2)
            {
                Level = 2;
                SongLevel++;
                new Thread(() =>
                 {
                     PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                 }).Start();
            }
            else if (Score == 5000 && SongLevel == 3)
            {
                Level = 3;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score == 10000 && SongLevel == 4)
            {
                Level = 4;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score == 20000 && SongLevel == 5)
            {
                Level = 5;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score == 50000 && SongLevel == 6)
            {
                Level = 6;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score == 100000 && SongLevel == 7)
            {
                Level = 7;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score == 250000 && SongLevel == 8)
            {
                Level = 8;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score == 500000 && SongLevel == 9)
            {
                Level = 9;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score == 1000000 && SongLevel == 10)
            {
                Level = 10;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            //Level = (int)Math.Log10(Score) - 1;

            //if (Level < 1)
            //{
            //    Level = 1;
            //}
            //if (Level > 10)
            //{
            //    Level = 10;
            //}

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
            if (lines > 0)
            {
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\clear.wav");
                }).Start();

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

            CurrentFigure = NextFigure;
            GetNextFigure();
        }

        static bool Collision(bool[,] figure)
        {
            //CHEK for right outsite border 
            if (CurrentFigureCol > TetrisCols - figure.GetLength(1))
            {
                return true;
            }
            //CHEK for down outsite border 
            if (CurrentFigureRow + figure.GetLength(0) == TetrisRows)
            {
                return true;
            }
            //CHEK FOR COLLISUM DOWN
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
            Write(Level.ToString(), 3, TetrisCols + 3);
            Write("Score:", 5, TetrisCols + 3);
            Write(Score.ToString(), 7, TetrisCols + 3);
            Write("High Score:", 9, TetrisCols + 3);
            Write(HighScore.ToString(), 11, TetrisCols + 3);
            Write("Next figure:", 13, TetrisCols + 3);

            DrawNextFigure();

            //Write("Frame:", 13, TetrisCols + 3);
            //Write(Frame.ToString(), 14, TetrisCols + 3);
            //Write("Position:", 15, TetrisCols + 3);
            //Write($"{CurrentFigureCol}, {CurrentFigureCol}", 16, TetrisCols + 3);
            Write("Keys:", 18, TetrisCols + 3);
            Write("  ^  ", 19, TetrisCols + 3);
            Write("<   >", 20, TetrisCols + 3);
            Write("  v ", 21, TetrisCols + 3);
            Write("Pause:", 18, TetrisCols + 13);
            Write("space", 20, TetrisCols + 13);
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
                        line += $"{FigureSymbol}";
                    }
                    else
                    {
                        line += " ";
                    }
                }
                //+1 for the border
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
                        Write($"{FigureSymbol}", row + 1 + CurrentFigureRow, col + 1 + CurrentFigureCol);
                    }
                }
            }
        }

        static void DrawNextFigure()
        {

            for (int row = 0; row < NextFigure.GetLength(0); row++)
            {

                for (int col = 0; col < NextFigure.GetLength(1); col++)
                {
                    if (NextFigure[row, col])
                    {
                        Write($"{FigureSymbol}", row + 1 + NextFigureRow, col + 1 + NextFigureCol);
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

            string middleLine = "";
            for (int i = 0; i < TetrisRows; i++)
            {
                middleLine += "║";
                middleLine += new string(' ', TetrisCols) + "║" + new string(' ', InfoCols) + "║" + "\n";
            }

            string endLine = "╚";
            endLine += new string('═', TetrisCols);
            endLine += "╩";
            endLine += new string('═', InfoCols);
            endLine += "╝";

            string borderFrame = firstLine + "\n" + middleLine + endLine;
            Console.Write(borderFrame);
        }

        static void Write(string text, int row, int col)
        {
            Console.SetCursorPosition(col, row);
            Console.Write(text);
        }

        private static void PlayConsoleMusic()
        {
            if (PauseMode == false)
            {
                new Thread(() =>
                {
                    if (PlayGame)
                    {
                        var gameMusic = new SoundPlayer();
                        while (PlayGame)
                        {
                            gameMusic.SoundLocation = @"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\tetris-gameboy-02 (1).wav";
                            gameMusic.PlaySync();
                        }
                    }

                }).Start();
            }
        }

        static void PlayMusic(string songUrl)
        {
            var myPlayer = new SoundPlayer();
            myPlayer.SoundLocation = songUrl;
            myPlayer.Play();
        }
    }
}
