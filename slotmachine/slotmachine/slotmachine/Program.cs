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

        //Settings 
        static int GameFieldRows = 24;
        static int GameFieldCols = 14;
        static int InfoCols = 25;
        static int InfoBottonRows = 10;
        static int ConsoleRows = 1 + GameFieldRows + InfoBottonRows + 1;
        static int ConsoleCols = 1 + GameFieldCols + 1 + InfoCols + 1;
        static bool[,] SevenFig = new bool[,] // 7 ----
            {
                {true, true, true, true },
                {false, false, true, false },
                {false,  true,  false,false },
                {true, false, false, false },
            };
        static bool[,] TemplateFig = new bool[,] // 7 ----
     {
                {true, true, true, true },
                {true, false, true, true },
                {true,  true,  false,true},
                {true, true, true,true },
     };
        static bool[,] EmptyTemplateFig = new bool[,] // 7 ----
   {
                {false, false, false, false },
                {false, false, false, false},
                {true, true, true, true },
                {false, false, false, false },
   };
        static List<bool[,]> SlotMachineFigures = new List<bool[,]>()
        {
            SevenFig,
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
                {false, false, false, false },
                {true, true, true, true },
                {true, true, true, true},
                {false, false, false, false},
            },
            TemplateFig,
        };

        static bool[,] MiddleFigure1;
        static bool[,] MiddleFigure2;
        static bool[,] MiddleFigure3;
        static readonly string ScoresFileName = "scores.txt";

        //State
        static int MoneyRecord = 0;
        static int Money = 100;
        static int Bet = 10;
        static string FigureSymbol = "#";
        static bool[,] NextFigure = null;
        static Random Random = new Random();
        static bool PauseMode = false;
        static bool PlayGame = true;
        static bool MusicPlayer = true;
        static readonly SoundPlayer player = new SoundPlayer();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.Unicode;

            if (File.Exists(ScoresFileName))
            {
                var allScores = File.ReadAllLines(ScoresFileName);
                foreach (var score in allScores)
                {
                    var match = Regex.Match(score, @" => (?<score>[0-9]+)");
                    MoneyRecord = Math.Max(MoneyRecord, int.Parse(match.Groups["score"].Value));
                }

                if (MusicPlayer)
                {
                    player.SoundLocation = @"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\slotMachine.wav";
                    player.PlayLooping();
                }
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Title = "Slot Machine V1.0 by y.yordanov21";
            Console.CursorVisible = false;
            Console.WindowHeight = ConsoleRows;
            Console.WindowWidth = ConsoleCols;
            Console.BufferHeight = ConsoleRows;
            Console.BufferWidth = ConsoleCols;
            
            NextFigure = SlotMachineFigures[Random.Next(0, SlotMachineFigures.Count)];

            while (true)
            {     
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

                        player.Stop();
                        Console.ReadKey();
                        if (MusicPlayer)
                        {
                            player.PlayLooping();
                        }
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
                        //777
                        if (MiddleFigure1 == MiddleFigure2 && MiddleFigure2 == MiddleFigure3 && MiddleFigure3 == SevenFig)
                        {
                            currentWin += 1000 * Bet;
                            DrawFigure(MiddleFigure1, 10, 0, ConsoleColor.DarkRed);
                            DrawFigure(MiddleFigure2, 10, 5, ConsoleColor.DarkRed);
                            DrawFigure(MiddleFigure3, 10, 10, ConsoleColor.DarkRed);
                        }
                        //77
                        else if (MiddleFigure1 == MiddleFigure2 && MiddleFigure2 == SevenFig)
                        {
                            currentWin += 100 * Bet;
                            DrawFigure(MiddleFigure1, 10, 0, ConsoleColor.DarkRed);
                            DrawFigure(MiddleFigure2, 10, 5, ConsoleColor.DarkRed);

                        }
                        //###
                        else if (MiddleFigure1 == MiddleFigure2 && MiddleFigure2 == MiddleFigure3 && MiddleFigure1 == MiddleFigure3)
                        {
                            currentWin += 100 * Bet;
                            DrawFigure(MiddleFigure1, 10, 0, ConsoleColor.DarkRed);
                            DrawFigure(MiddleFigure2, 10, 5, ConsoleColor.DarkRed);
                            DrawFigure(MiddleFigure3, 10, 10, ConsoleColor.DarkRed);
                        }
                        //##-
                        else if (MiddleFigure1 == MiddleFigure2)
                        {
                            currentWin += 10 * Bet;
                            DrawFigure(MiddleFigure1, 10, 0, ConsoleColor.DarkRed);
                            DrawFigure(MiddleFigure2, 10, 5, ConsoleColor.DarkRed);

                        }
                        //7
                        else if (MiddleFigure1 == SevenFig || MiddleFigure2 == SevenFig || MiddleFigure3 == SevenFig)
                        {
                            currentWin += 1 * Bet;
                            if (MiddleFigure1 == SevenFig)
                            {
                                DrawFigure(MiddleFigure1, 10, 0, ConsoleColor.DarkRed);
                            }
                            else if (MiddleFigure2 == SevenFig)
                            {
                                DrawFigure(MiddleFigure2, 10, 5, ConsoleColor.DarkRed);
                            }
                            else if (MiddleFigure3 == SevenFig)
                            {
                                DrawFigure(MiddleFigure3, 10, 10, ConsoleColor.DarkRed);
                            }

                        }
                        //-##
                        else if (MiddleFigure2 == MiddleFigure3)
                        {
                            currentWin += 1 * Bet;

                            DrawFigure(MiddleFigure2, 10, 5, ConsoleColor.DarkRed);
                            DrawFigure(MiddleFigure3, 10, 10, ConsoleColor.DarkRed);
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

                                new Thread(() =>
                                {
                                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\YouWon.wav");
                                }).Start();
                                Thread.Sleep(1500);
                                if (MusicPlayer)
                                {
                                    player.Play();
                                }
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

                        PlayGame = false;
                        Console.ReadKey();
                    }
                    if (key.Key == ConsoleKey.Enter && PauseMode == true)
                    {
                        PlayGame = true;

                        PlayConsoleMusic();
                        PauseMode = false;
                    }

                    if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W)
                    {
                        if (Money - 10 >= Bet)
                        {
                            Bet += 10;
                        }

                    }

                    if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
                    {
                        if (Bet > 0)
                        {
                            Bet -= 10;
                        }

                    }
                }

                //Redraw UI
                DrawBorder();
                DrawInfo();

                DrawFigureLineOne(0, 0);
                DrawFigureLineTwo(0, 5);
                DrawFigureLineThree(0, 10);

                //wait 300 miliseconds
                Thread.Sleep(300);
            }
        }



        private static bool[,] GetNextFigure()
        {
            NextFigure = SlotMachineFigures[Random.Next(0, SlotMachineFigures.Count)];
            return NextFigure;
        }

        static void DrawInfo()
        {
            if (Money > MoneyRecord)
            {
                MoneyRecord = Money;
            }

            Write("BET:", GameFieldRows + 2, GameFieldCols + 3);
            Write(Bet.ToString(), GameFieldRows + 2, GameFieldCols + 8);
            Write("Money:", GameFieldRows + 4, GameFieldCols + 3);
            Write(Money.ToString(), GameFieldRows + 4, GameFieldCols + 10);
            Write("Record:", GameFieldRows + 6, GameFieldCols + 3);
            Write(MoneyRecord.ToString(), GameFieldRows + 6, GameFieldCols + 10);

            Write("Keys:", GameFieldRows + 2, 2);
            Write("BET + : ^", GameFieldRows + 3, 2);
            Write("BET - : v", GameFieldRows + 4, 2);
            Write("ENTER:", GameFieldRows + 6, 2);
            Write("for Gaming", GameFieldRows + 7, 2);

            Write("Pause:", GameFieldRows + 9, 2);
            Write("space", GameFieldRows + 10, 2);

            Write("- 1BET", 3, GameFieldCols + 18);
            Write("- 100BET", 7, GameFieldCols + 18);
            Write("- 1000BET", 11, GameFieldCols + 18);
            Write("- 1BET", 15, GameFieldCols + 18);
            Write("- 10BET", 19, GameFieldCols + 18);
            Write("- 100BET", 23, GameFieldCols + 18);

            DrawFigure(SevenFig, 0, GameFieldCols + 2);
            DrawFigure(SevenFig, 4, GameFieldCols + 2);
            DrawFigure(SevenFig, 4, GameFieldCols + 2 + 5);
            DrawFigure(SevenFig, 8, GameFieldCols + 2);
            DrawFigure(SevenFig, 8, GameFieldCols + 2 + 5);
            DrawFigure(SevenFig, 8, GameFieldCols + 2 + 10);
            DrawFigure(EmptyTemplateFig, 12, GameFieldCols + 2);
            DrawFigure(TemplateFig, 12, GameFieldCols + 2 + 5);
            DrawFigure(TemplateFig, 12, GameFieldCols + 2 + 10);
            DrawFigure(TemplateFig, 16, GameFieldCols + 2);
            DrawFigure(TemplateFig, 16, GameFieldCols + 2 + 5);
            DrawFigure(EmptyTemplateFig, 16, GameFieldCols + 2 + 10);
            DrawFigure(TemplateFig, 20, GameFieldCols + 2);
            DrawFigure(TemplateFig, 20, GameFieldCols + 2 + 5);
            DrawFigure(TemplateFig, 20, GameFieldCols + 2 + 10);
        }

        static void DrawFigureLineOne(int row, int col)
        {

            bool[,] nextFig1 = GetNextFigure();
            bool[,] nextFig2 = GetNextFigure();
            MiddleFigure1 = GetNextFigure();
            bool[,] nextFig4 = GetNextFigure();
            bool[,] nextFig5 = GetNextFigure();

            DrawFigure(nextFig1, row, col);
            DrawFigure(nextFig2, row + 5, col);
            DrawFigure(MiddleFigure1, row + 10, col);
            DrawFigure(nextFig4, row + 15, col);
            DrawFigure(nextFig5, row + 20, col);


        }
        static void DrawFigureLineTwo(int row, int col)
        {

            bool[,] nextFig1 = GetNextFigure();
            bool[,] nextFig2 = GetNextFigure();
            MiddleFigure2 = GetNextFigure();
            bool[,] nextFig4 = GetNextFigure();
            bool[,] nextFig5 = GetNextFigure();

            DrawFigure(nextFig1, row, col);
            DrawFigure(nextFig2, row + 5, col);
            DrawFigure(MiddleFigure2, row + 10, col);
            DrawFigure(nextFig4, row + 15, col);
            DrawFigure(nextFig5, row + 20, col);
        }
        static void DrawFigureLineThree(int row, int col)
        {
            bool[,] nextFig1 = GetNextFigure();
            bool[,] nextFig2 = GetNextFigure();
            MiddleFigure3 = GetNextFigure();
            bool[,] nextFig4 = GetNextFigure();
            bool[,] nextFig5 = GetNextFigure();

            DrawFigure(nextFig1, row, col);
            DrawFigure(nextFig2, row + 5, col);
            DrawFigure(MiddleFigure3, row + 10, col);
            DrawFigure(nextFig4, row + 15, col);
            DrawFigure(nextFig5, row + 20, col);
        }
        static void DrawFigure(bool[,] nextFigure, int NextFigureRow, int NextFigureCol, ConsoleColor color = ConsoleColor.Cyan)
        {

            for (int row = 0; row < nextFigure.GetLength(0); row++)
            {

                for (int col = 0; col < nextFigure.GetLength(1); col++)
                {
                    if (nextFigure[row, col])
                    {
                        Write($"{FigureSymbol}", row + 1 + NextFigureRow, col + 1 + NextFigureCol, color);
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
            firstLine += new string('═', GameFieldCols);
            firstLine += "╦";
            firstLine += new string('═', InfoCols);
            firstLine += "╗";

            string middleLine = "";
            for (int i = 0; i < GameFieldRows; i++)
            {
                if (i == 9 || i == 14)
                {
                    middleLine += "║";
                    middleLine += new string('-', GameFieldCols) + "║" + new string(' ', InfoCols) + "║" + "\n";
                }
                else
                {
                    middleLine += "║";
                    middleLine += new string(' ', GameFieldCols) + "║" + new string(' ', InfoCols) + "║" + "\n";
                }

            }

            string endLine = "║";
            endLine += new string('═', GameFieldCols);
            endLine += "╬";
            endLine += new string('═', InfoCols);
            endLine += "║";

            string bottonSection = "";
            for (int i = 0; i < InfoBottonRows - 1; i++)
            {
                bottonSection += "║" + new string(' ', GameFieldCols) + "║" + new string(' ', InfoCols) + "║" + "\n";
            }

            bottonSection += "╚" + new string('═', GameFieldCols) + "╩" + new string('═', InfoCols) + "╝";

            string borderFrame = firstLine + "\n" + middleLine + endLine + bottonSection;
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
            if (PauseMode == false)
            {
                new Thread(() =>
                {
                    if (PlayGame)
                    {
                        var gameMusic = new SoundPlayer();
                        while (PlayGame)
                        {
                            gameMusic.SoundLocation = @"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\slotMachine.wav";
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