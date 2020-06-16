using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;
using System.Media;
using System.Xml.Resolvers;

namespace slotmachine
{
    class Program
    {
        // the code was made for education purpose with the support of the Software University and videos of Nikolay Kostov
        //Settings 
        static int TetrisRows = 24;
        static int TetrisCols = 14;
        static int InfoCols = 20;
        static int InfoBottonRows = 10;
        static int ConsoleRows = 1 + TetrisRows + InfoBottonRows + 1;
        static int ConsoleCols = 1 + TetrisCols + 1 + InfoCols + 1;
        static bool[,] Seven = new bool[,] // 7 ----
            {
                {true, true, true, true },
                {false, false, true, false },
                {false,  true,  false,false },
                {true, false, false, false },
            };
        static List<bool[,]> TetrisFigures = new List<bool[,]>()
        {
            Seven,
            new bool [,] // O
            {
               {false, true, true, false },
                {true, false, false, true },
                {true,  false,  false,true },
                {false, true, true, false },
            },
            new bool [,] // X
            {
                {true, false, false, true },
                {false, true, true, false },
                {false,true, true, false },
                {true, false, false, true },
            },
            new bool [,] // #
            {
                {true, false, true, false },
                {false, true,false, true },
                {true, false, true, false},
                {false, true,false, true },
            },
            new bool[,] // #2
            {
                {false, true,false, true },
                {true, false, true, false },
                {false, true,false, true },
                {true, false, true, false },
            },
            new bool[,] // line1
            {
                {true, true, true, true },
                {false, false, false, false },
                {false,  false,  false,false },
                {true, true, true, true},
            },
            new bool[,] // Line2
            {
                {true, true, true, true },
                {false, false, false, false },
                {false, false, false, false },
                {true, true, true, true},
            }
        };

        static bool[,] MiddleFigure1;
        static bool[,] MiddleFigure2;
        static bool[,] MiddleFigure3;
        static string ScoresFileName = "scores.txt";

        //State
        static int MoneyRecord = 0;
        static int Money = 100;
        static int Frame = 0;
        static int Bet = 10;
        static string FigureSymbol = "#";
        static int FrameToMoveFigure = 16;
        static bool[,] CurrentFigureOne = null;
        static int CurrentFigureRowOne = 0;
        static int CurrentFigureColOne = 0;
        static bool[,] CurrentFigureTwo = null;
        static int CurrentFigureRowTwo = 0;
        static int CurrentFigureColTwo = 4;
        static bool[,] CurrentFigureThree = null;
        static int CurrentFigureRowThree = 0;
        static int CurrentFigureColThree = 8;
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
                    MoneyRecord = Math.Max(MoneyRecord, int.Parse(match.Groups["score"].Value));
                }
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Title = "Slot Machine V1.0 by y.yordanov21";
            Console.CursorVisible = false;
            Console.WindowHeight = ConsoleRows;
            Console.WindowWidth = ConsoleCols;
            Console.BufferHeight = ConsoleRows;
            Console.BufferWidth = ConsoleCols;
            CurrentFigureOne = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            CurrentFigureTwo = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            CurrentFigureThree = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];

            while (true)
            {
                Frame++;
                // UpdateLevel();

                // Read user input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Spacebar && PauseMode == false)
                    {
                        PauseMode = true;

                        Write("╔═══════════════╗", 5, 15);
                        Write("║               ║", 6, 15);
                        Write("║     Pause     ║", 7, 15);
                        Write("║               ║", 8, 15);
                        Write("╚═══════════════╝", 9, 15);
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

                    if (key.Key == ConsoleKey.Enter && PauseMode == false)
                    {
                        PauseMode = true;
                        int currentWin = 0;
                        if (MiddleFigure1 == MiddleFigure2 && MiddleFigure2 == MiddleFigure3)
                        {
                            currentWin += 10 * Bet;
                        }
                        else if (MiddleFigure1 == MiddleFigure2)
                        {
                            currentWin += 1 * Bet;
                        }
                        else if (MiddleFigure2 == MiddleFigure3)
                        {
                            currentWin += 1 * Bet;
                        }
                        else if (MiddleFigure1 == Seven || MiddleFigure2 == Seven || MiddleFigure3 == Seven)
                        {
                            currentWin += 1 * Bet;
                        }
                        else if (MiddleFigure1 == MiddleFigure2 && MiddleFigure2 == Seven)
                        {
                            currentWin += 5 * Bet;
                        }
                        else
                        {
                            currentWin = -Bet;
                        }
                        Money += currentWin;
                        if (Money > 0)
                        {
                            if (currentWin >= 0)
                            {
                                File.AppendAllLines(ScoresFileName, new List<string>
                     {
                         $"[{DateTime.Now.ToString()}] {Environment.UserName} => {Money}"
                     });
                                var currentWinAsString = currentWin.ToString();
                                currentWinAsString += new string(' ', 7 - currentWinAsString.Length);
                                Write("╔══════════════╗", 5, 17);
                                Write("║  You         ║", 6, 17);
                                Write("║     Win:     ║", 7, 17);
                                Write($"║      {currentWinAsString} ║", 8, 17);
                                Write("╚══════════════╝", 9, 17);
                                PlayGame = false;
                                // PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\gameover.wav");
                            }
                            else
                            {
                                var currentBetAsString = Bet.ToString();
                                currentBetAsString += new string(' ', 7 - currentBetAsString.Length);
                                Write("╔══════════════╗", 5, 17);
                                Write("╔══════════════╗", 5, 17);
                                Write("║  You         ║", 6, 17);
                                Write("║     Lost:    ║", 7, 17);
                                Write($"║      {currentBetAsString} ║", 8, 17);
                                Write("╚══════════════╝", 9, 17);
                                PlayGame = false;
                            }
                            Thread.Sleep(500);
                        }
                        else
                        {
                                             
                            Write("╔══════════════╗", 5, 5);
                            Write("║  Game        ║", 6, 5);
                            Write("║     over!    ║", 7, 5);
                            Write("║  You win: 0 $║", 8, 5);
                            Write("╚══════════════╝", 9, 5);
                            PlayGame = false;
                            PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\gameover.wav");
                            Thread.Sleep(1000000);
                            return;
                        }


                        //  PlayGame = false;
                        Console.ReadKey();
                    }
                    if (key.Key == ConsoleKey.Enter && PauseMode == true)
                    {
                        PlayGame = true;

                        // PlayConsoleMusic();
                        PauseMode = false;
                    }

                    //if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A)
                    //{

                    //    if (CurrentFigureColOne >= 1)
                    //    {
                    //        CurrentFigureColOne--;
                    //    }

                    //}

                    //if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)
                    //{
                    //    if ((CurrentFigureColOne < TetrisCols - CurrentFigureOne.GetLength(1)))
                    //    {
                    //        CurrentFigureColOne++;
                    //    }
                    //}

                    if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W)
                    {
                        Bet += 10;
                    }

                    if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
                    {
                        if (Bet > 0)
                        {
                            Bet -= 10;
                        }

                    }
                }

                //Update the game state
                if (Frame % (FrameToMoveFigure) == 0)
                {
                    //CurrentFigureRowOne++;
                    CurrentFigureRowTwo++;
                    CurrentFigureRowThree++;
                    Frame = 0;

                }
                // user input
                // change state
                //if (Collision(CurrentFigure))
                //{
                //    AddCurrentFigureToTetrisField();
                //    int lines = CheckForFullLines();
                //    //add points to score
                //    Score += ScorePerLines[lines] * Level;
                //    //CurrentFigure = NextFigure;
                //    CurrentFigureCol = 0;
                //    CurrentFigureRow = 0;

                //    //game over!
                //    if (Collision(CurrentFigure))
                //    {
                //        File.AppendAllLines(ScoresFileName, new List<string>
                //        {
                //            $"[{DateTime.Now.ToString()}] {Environment.UserName} => {Score}"
                //        });
                //        var scoreAsString = Score.ToString();
                //        scoreAsString += new string(' ', 7 - scoreAsString.Length);
                //        Write("╔══════════════╗", 5, 5);
                //        Write("║  Game        ║", 6, 5);
                //        Write("║     over!    ║", 7, 5);
                //        Write($"║      {scoreAsString} ║", 8, 5);
                //        Write("╚══════════════╝", 9, 5);
                //        PlayGame = false;
                //        PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\gameover.wav");
                //        Thread.Sleep(1000000);
                //        return;
                //    }
                //}



                //Redraw UI
                DrawBorder();
                DrawInfo();

                DrawCurrentFigureOne(0, 0);
                DrawCurrentFigureTwo(0, 5);
                DrawCurrentFigureThree(0, 10);

                //wait 300 miliseconds
                Thread.Sleep(300);
            }
        }



        private static bool[,] GetNextFigure()
        {
            NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            return NextFigure;
        }

        private static void UpdateLevel()
        {
            if (Money <= 0)
            {
                Bet = 1;
            }

            if (Money >= 1000 && SongLevel == 2)
            {
                Bet = 2;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Money == 5000 && SongLevel == 3)
            {
                Bet = 3;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Money == 10000 && SongLevel == 4)
            {
                Bet = 4;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Money == 20000 && SongLevel == 5)
            {
                Bet = 5;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Money == 50000 && SongLevel == 6)
            {
                Bet = 6;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Money == 100000 && SongLevel == 7)
            {
                Bet = 7;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Money == 250000 && SongLevel == 8)
            {
                Bet = 8;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Money == 500000 && SongLevel == 9)
            {
                Bet = 9;
                SongLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Money == 1000000 && SongLevel == 10)
            {
                Bet = 10;
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


        static void DrawInfo()
        {
            if (Money > MoneyRecord)
            {
                MoneyRecord = Money;
            }

            Write("BET:", 1, TetrisCols + 3);
            Write(Bet.ToString(), 1, TetrisCols + 8);
            Write("Money:", 3, TetrisCols + 3);
            Write(Money.ToString(), 3, TetrisCols + 10);
            Write("Record:", 5, TetrisCols + 3);
            Write(MoneyRecord.ToString(), 5, TetrisCols + 10);

            Write("Keys:", TetrisRows + 2, 2);
            Write("BET + : ^", TetrisRows + 3, 2);
            Write("BET - : v", TetrisRows + 4, 2);
            Write("ENTER:", TetrisRows + 6, 2);
            Write("for Gaming", TetrisRows + 7, 2);

            Write("Pause:", TetrisRows + 9, 2);
            Write("space", TetrisRows + 10, 2);
        }


        static void DrawCurrentFigureOne(int row, int col)
        {

            bool[,] nextFig1 = GetNextFigure();
            bool[,] nextFig2 = GetNextFigure();
            MiddleFigure1 = GetNextFigure();
            bool[,] nextFig4 = GetNextFigure();
            bool[,] nextFig5 = GetNextFigure();

            DrawNextFigure(nextFig1, row, col);
            DrawNextFigure(nextFig2, row + 5, col);
            DrawNextFigure(MiddleFigure1, row + 10, col);
            DrawNextFigure(nextFig4, row + 15, col);
            DrawNextFigure(nextFig5, row + 20, col);


        }
        static void DrawCurrentFigureTwo(int row, int col)
        {

            bool[,] nextFig1 = GetNextFigure();
            bool[,] nextFig2 = GetNextFigure();
            MiddleFigure2 = GetNextFigure();
            bool[,] nextFig4 = GetNextFigure();
            bool[,] nextFig5 = GetNextFigure();

            DrawNextFigure(nextFig1, row, col);
            DrawNextFigure(nextFig2, row + 5, col);
            DrawNextFigure(MiddleFigure2, row + 10, col);
            DrawNextFigure(nextFig4, row + 15, col);
            DrawNextFigure(nextFig5, row + 20, col);
        }
        static void DrawCurrentFigureThree(int row, int col)
        {
            bool[,] nextFig1 = GetNextFigure();
            bool[,] nextFig2 = GetNextFigure();
            MiddleFigure3 = GetNextFigure();
            bool[,] nextFig4 = GetNextFigure();
            bool[,] nextFig5 = GetNextFigure();

            DrawNextFigure(nextFig1, row, col);
            DrawNextFigure(nextFig2, row + 5, col);
            DrawNextFigure(MiddleFigure3, row + 10, col);
            DrawNextFigure(nextFig4, row + 15, col);
            DrawNextFigure(nextFig5, row + 20, col);
        }
        static void DrawNextFigure(bool[,] nextFigure, int NextFigureRow, int NextFigureCol)
        {

            for (int row = 0; row < nextFigure.GetLength(0); row++)
            {

                for (int col = 0; col < nextFigure.GetLength(1); col++)
                {
                    if (nextFigure[row, col])
                    {
                        Write($"{FigureSymbol}", row + 1 + NextFigureRow, col + 1 + NextFigureCol, ConsoleColor.Cyan);
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
                if (i == 9 || i == 14)
                {
                    middleLine += "║";
                    middleLine += new string('-', TetrisCols) + "║" + new string(' ', InfoCols) + "║" + "\n";
                }
                else
                {
                    middleLine += "║";
                    middleLine += new string(' ', TetrisCols) + "║" + new string(' ', InfoCols) + "║" + "\n";
                }

            }

            string endLine = "╚";
            endLine += new string('═', TetrisCols);
            endLine += "╩";
            endLine += new string('═', InfoCols);
            endLine += "╝";

            string borderFrame = firstLine + "\n" + middleLine + endLine;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(borderFrame);
        }

        static void Write(string text, int row, int col, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.SetCursorPosition(col, row);
            Console.Write(text);
        }

        private static void PlayConsoleMusic()
        {
            //if (PauseMode == false)
            //{
            //    new Thread(() =>
            //    {
            //        if (PlayGame)
            //        {
            //            var gameMusic = new SoundPlayer();
            //            while (PlayGame)
            //            {
            //                gameMusic.SoundLocation = @"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\tetris-gameboy-02 (1).wav";
            //                gameMusic.PlaySync();
            //            }
            //        }

            //    }).Start();
            //}
        }

        static void PlayMusic(string songUrl)
        {
            //var myPlayer = new SoundPlayer();
            //myPlayer.SoundLocation = songUrl;
            //myPlayer.Play();
        }
    }
}