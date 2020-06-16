using System;
using System.Collections.Generic;
using System.Threading;
using System.Media;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace snake
{

    class Program
    {
        //Settings 
        static readonly int SnakeFieldRows = 15;
        static readonly int SnakeFieldCols = 40;
        static readonly int InfoRows = 12;
        static readonly int ConsoleRows = 1 + SnakeFieldRows + 1 + InfoRows + 1;
        static readonly int ConsoleCols = 1 + SnakeFieldCols + 1;
        static readonly string ScoresFileName = "scores.txt";

        //State
        static bool DirectionDown = false;
        static bool DirectionUp = false;
        static bool DirectionLeft = true;
        static bool DirectionRight = false;

        static bool OpposideDirectionDown = true;
        static bool OpposideDirectionUp = true;
        static bool OpposideDirectionLeft = true;
        static bool OpposideDirectionRight = false;

        static int HighScore = 0;
        static int Score = 0;
        static int Level = 1;
        //if use another special symbols you must change the font!!!  ◍ ■ ☯ ⚛ ⚙
        static readonly string FigureSymbol = "o";
        static readonly string SnakeHeadFigureSymbol = "O";
        static readonly string SnakeHeadFigureSymbolOpstacle = "@"; 
        static string OpstacleSymbol = "ψ";
        //static string[] OpstacleSymbolArray = { "⛄", "⚡", "⚓", "☆", "☀", "☃", "☕", "♠", "♡", "♣" };
        static string[] OpstacleSymbolArray = { "a", "s", "d", "8", "*", "+", "@", "&", "#", "$" };
        //start snake possition
        static int SnakeFigureRow = 1;
        static int SnakeFigureCol = 12;
        static SnakeItem[,] SnakeField = new SnakeItem[SnakeFieldRows, SnakeFieldCols];
        static List<OpstacleItem> SnakeObstaclesList = new List<OpstacleItem>();
        static Random Random = new Random();     
        //initial snake
        static readonly List<SnakeItem> SnakeEmenents = new List<SnakeItem>()
        {
            new SnakeItem{Id=0,rowPossition=SnakeFigureRow,colPossition=SnakeFigureCol},
            new SnakeItem{Id=1,rowPossition=SnakeFigureRow,colPossition=SnakeFigureCol+1},
            new SnakeItem{Id=2,rowPossition=SnakeFigureRow,colPossition=SnakeFigureCol+2},
            new SnakeItem{Id=3,rowPossition=SnakeFigureRow,colPossition=SnakeFigureCol+3},
        };

        static int NextLevel = 2;
        static int SleepSec = 350;
        static readonly int SleepSecLevel = SleepSec / 10;
        static OpstacleItem CurrentOpstacle = null;
        static bool PauseMode = false;
        static bool MusicPlayer = true;
        static readonly SoundPlayer player = new SoundPlayer();

        static void Main()
        {
            //console settings
            Console.Title = "Snake V1.0 by y.yordanov21";
            Console.OutputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.Unicode;
            Console.ForegroundColor = ConsoleColor.Gray;        
            Console.CursorVisible = false;
            Console.WindowHeight = ConsoleRows;
            Console.WindowWidth = ConsoleCols;
            Console.BufferHeight = ConsoleRows;
            Console.BufferWidth = ConsoleCols;

            if (File.Exists(ScoresFileName))
            {
                var allScores = File.ReadAllLines(ScoresFileName);
                foreach (var score in allScores)
                {
                    var match = Regex.Match(score, @" => (?<score>[0-9]+)");
                    HighScore = Math.Max(HighScore, int.Parse(match.Groups["score"].Value));
                }
            }

            for (int row = 1; row <= SnakeField.GetLength(0); row++)
            {
                var id = 0;
                for (int col = 1; col <= SnakeField.GetLength(1); col++)
                {
                    SnakeObstaclesList.Add(new OpstacleItem { Id = id, rowPossition = row, colPossition = col });
                    id++;
                }
            }
           
            if (MusicPlayer)
            {
                player.SoundLocation = @"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\01 - Super Mario Bros.wav";
                player.PlayLooping();
            }

            CurrentOpstacle = SnakeObstaclesList[SnakeObstaclesList.Count / 2 + 2];
            while (true)
            {
                UpdateLevel();
                // Read user input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();

                    if (key.Key == ConsoleKey.O && MusicPlayer == true)
                    {
                        player.Stop();
                        MusicPlayer = false;
                    }
                    if (key.Key == ConsoleKey.P && MusicPlayer == false)
                    {
                        player.PlayLooping();
                        MusicPlayer = true;
                    }
                    if (key.Key == ConsoleKey.Spacebar && PauseMode == false)
                    {
                        PauseMode = true;

                        Write("╔═══════════════╗", SnakeFieldRows / 2 - 1, SnakeFieldCols / 2 - 7);
                        Write("║               ║", SnakeFieldRows / 2, SnakeFieldCols / 2 - 7);
                        Write("║     Pause     ║", SnakeFieldRows / 2 + 1, SnakeFieldCols / 2 - 7);
                        Write("║               ║", SnakeFieldRows / 2 + 2, SnakeFieldCols / 2 - 7);
                        Write("╚═══════════════╝", SnakeFieldRows / 2 + 3, SnakeFieldCols / 2 - 7);
                        player.Stop();
                        Console.ReadKey();
                        if (MusicPlayer)
                        {
                            player.PlayLooping();
                        }                       
                    }

                    if (key.Key == ConsoleKey.Spacebar && PauseMode == true)
                    {
                        PauseMode = false;
                    }

                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }

                    if ((key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A) && OpposideDirectionLeft)
                    {
                        ChangeDirection("DirectionLeft");
                        OpposideDirectionLeft = true;
                        OpposideDirectionRight = false;
                        OpposideDirectionUp = true;
                        OpposideDirectionDown = true;
                    }

                    if ((key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D) && OpposideDirectionRight)
                    {
                        ChangeDirection("DirectionRight");
                        OpposideDirectionRight = true;
                        OpposideDirectionLeft = false;
                        OpposideDirectionUp = true;
                        OpposideDirectionDown = true;
                    }

                    if ((key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W) && OpposideDirectionUp)
                    {
                        ChangeDirection("DirectionUp");
                        OpposideDirectionUp = true;
                        OpposideDirectionDown = false;
                        OpposideDirectionLeft = true;
                        OpposideDirectionRight = true;
                    }

                    if ((key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S) && OpposideDirectionDown)
                    {
                        ChangeDirection("DirectionDown");
                        OpposideDirectionDown = true;
                        OpposideDirectionUp = false;
                        OpposideDirectionLeft = true;
                        OpposideDirectionRight = true;
                    }
                }

                //Update the game state              
                if (DirectionDown)
                {
                    SnakeFigureRow++;
                }
                else if (DirectionUp)
                {
                    SnakeFigureRow--;
                }
                else if (DirectionLeft)
                {
                    SnakeFigureCol--;
                }
                else if (DirectionRight)
                {
                    SnakeFigureCol++;
                }

                //Draw Game Units
                DrawBorder();
                DrawInfo();
                DrawSnake();
                MoveSnake();
                DrawOpstacle();
                Thread.Sleep(SleepSec);
            }
        }

        static void MoveSnake()
        {
            CheckForOpstacles();

            if (DirectionDown)
            {
                for (int i = SnakeEmenents.Count - 1; i >= 1; i--)
                {
                    SnakeEmenents[i].rowPossition = SnakeEmenents[i - 1].rowPossition;
                    SnakeEmenents[i].colPossition = SnakeEmenents[i - 1].colPossition;
                }

                if (SnakeEmenents[0].rowPossition == SnakeFieldRows)
                {
                    SnakeEmenents[0].rowPossition = 1;
                }
                else
                {
                    SnakeEmenents[0].rowPossition++;
                }
            }
            else if (DirectionUp)
            {
                for (int i = SnakeEmenents.Count - 1; i >= 1; i--)
                {
                    SnakeEmenents[i].rowPossition = SnakeEmenents[i - 1].rowPossition;
                    SnakeEmenents[i].colPossition = SnakeEmenents[i - 1].colPossition;
                }

                if (SnakeEmenents[0].rowPossition == 1)
                {
                    SnakeEmenents[0].rowPossition = SnakeFieldRows;
                }
                else
                {
                    SnakeEmenents[0].rowPossition--;
                }
            }
            else if (DirectionLeft)
            {
                for (int i = SnakeEmenents.Count - 1; i >= 1; i--)
                {
                    SnakeEmenents[i].rowPossition = SnakeEmenents[i - 1].rowPossition;
                    SnakeEmenents[i].colPossition = SnakeEmenents[i - 1].colPossition;
                }

                if (SnakeEmenents[0].colPossition == 1)
                {
                    SnakeEmenents[0].colPossition = SnakeFieldCols;
                }
                else
                {
                    SnakeEmenents[0].colPossition--;
                }
            }
            else if (DirectionRight)
            {
                for (int i = SnakeEmenents.Count - 1; i >= 1; i--)
                {
                    SnakeEmenents[i].rowPossition = SnakeEmenents[i - 1].rowPossition;
                    SnakeEmenents[i].colPossition = SnakeEmenents[i - 1].colPossition;
                }

                if (SnakeEmenents[0].colPossition == SnakeFieldCols)
                {
                    SnakeEmenents[0].colPossition = 1;
                }
                else
                {
                    SnakeEmenents[0].colPossition++;
                }
            }

            CheckForGameOver();
        }

        static void CheckForOpstacles()
        {
            var currentOpstacleRow = CurrentOpstacle.rowPossition;
            var currentOpstacleCol = CurrentOpstacle.colPossition;

            var lastSnakeEmenentId = SnakeEmenents[SnakeEmenents.Count - 1].Id;
            var lastSnakeEmenentRow = SnakeEmenents[SnakeEmenents.Count - 1].rowPossition;
            var lastSnakeEmenentCol = SnakeEmenents[SnakeEmenents.Count - 1].colPossition;
            if (SnakeEmenents[0].rowPossition == currentOpstacleRow && SnakeEmenents[0].colPossition == currentOpstacleCol)
            {
                bool validOpstacle = true;
                Score += 10 * Level;

                while (validOpstacle)
                {
                    bool checkedSnake = true;
                    CurrentOpstacle = SnakeObstaclesList[Random.Next(0, SnakeObstaclesList.Count)];

                    for (int i = 0; i < SnakeEmenents.Count; i++)
                    {
                        if (CurrentOpstacle.rowPossition == SnakeEmenents[i].rowPossition
                            && CurrentOpstacle.colPossition == SnakeEmenents[i].colPossition)
                        {
                            checkedSnake = false;
                        }
                    }

                    if (checkedSnake)
                    {
                        validOpstacle = false;
                    }
                }
            
                Write($"{SnakeHeadFigureSymbolOpstacle}", SnakeEmenents[0].rowPossition, SnakeEmenents[0].colPossition);
                Thread.Sleep(50);

                if (MusicPlayer == false)
                {
                    new Thread(() =>
                    {
                        PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\Minecraft-eat2.wav");
                    }).Start();
                }

                Write($"{SnakeHeadFigureSymbol}", SnakeEmenents[0].rowPossition, SnakeEmenents[0].colPossition);
                SnakeEmenents.Add(new SnakeItem { Id = lastSnakeEmenentId, rowPossition = lastSnakeEmenentRow, colPossition = lastSnakeEmenentCol });
            }
        }

        static void CheckForGameOver()
        {
            var currentSnakeHeadRowPossition = SnakeEmenents[0].rowPossition;
            var currentSnakeHeadColPossition = SnakeEmenents[0].colPossition;
            for (int i = 1; i < SnakeEmenents.Count; i++)
            {
                if (SnakeEmenents[i].rowPossition == currentSnakeHeadRowPossition
                    && SnakeEmenents[i].colPossition == currentSnakeHeadColPossition)
                {
                    File.AppendAllLines(ScoresFileName, new List<string>
                        {
                            $"[{DateTime.Now.ToString()}] {Environment.UserName} => {Score}"
                        });
                    var scoreAsString = Score.ToString();
                    scoreAsString += new string(' ', 7 - scoreAsString.Length);
                    Write("╔══════════════╗", SnakeFieldRows / 2 - 1, SnakeFieldCols / 2 - 7);
                    Write("║  Game        ║", SnakeFieldRows / 2, SnakeFieldCols / 2 - 7);
                    Write("║     over!    ║", SnakeFieldRows / 2 + 1, SnakeFieldCols / 2 - 7);
                    Write($"║      {scoreAsString} ║", SnakeFieldRows / 2 + 2, SnakeFieldCols / 2 - 7);
                    Write("╚══════════════╝", SnakeFieldRows / 2 + 3, SnakeFieldCols / 2 - 7);
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\gameover.wav");
                    Thread.Sleep(1000000);
                    return;
                }
            }
        }

        static void DrawBorder()
        {
            //always start drawing border from point (0,0);
            Console.SetCursorPosition(0, 0);

            //drawing border
            string firstLine = "╔" + new string('═', SnakeFieldCols) + "╗" + "\n";

            string middleLine = "";
            for (int i = 0; i < SnakeFieldRows; i++)
            {
                middleLine += "║" + new string(' ', SnakeFieldCols) + "║" + "\n";
            }
            middleLine += "╠" + new string('═', SnakeFieldCols) + "╣" + "\n";

            string infoSection = "";
            for (int i = 0; i < InfoRows; i++)
            {
                infoSection += "║" + new string(' ', SnakeFieldCols) + "║" + "\n"; ;

            }
            string endLine = "╚" + new string('═', SnakeFieldCols) + "╝";

            string borderFrame = firstLine + middleLine + infoSection + endLine;
            Write(borderFrame, 0, 0);
        }

        static void DrawInfo()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
            }

            Write("Level:", 1 + SnakeFieldRows + 2, 2);
            Write(Level.ToString(), 1 + SnakeFieldRows + 2, 10);
            Write("Score:", 1 + SnakeFieldRows + 4, 2);
            Write(Score.ToString(), 1 + SnakeFieldRows + 6, 2);
            Write("High Score:", 1 + SnakeFieldRows + 8, 2);
            Write(HighScore.ToString(), 1 + SnakeFieldRows + 10, 2);
            Write("Keys:", 1 + SnakeFieldRows + 2, 18);
            Write("  ^  ", 1 + SnakeFieldRows + 1, 24);
            Write("<   >", 1 + SnakeFieldRows + 2, 24);
            Write("  v  ", 1 + SnakeFieldRows + 3, 24);
            Write("Pause: space", 1 + SnakeFieldRows + 6, 18);
            string currentMusicMode = MusicPlayer == false ? "press P to start" : "press O to stop";
            Write($"Music: {currentMusicMode}", 1 + SnakeFieldRows + 8, 18);
            Write($"Snake Game by y.yordanov21", 1 + SnakeFieldRows + InfoRows, 2, ConsoleColor.Cyan);
            Write($"Enjoy!", 1 + SnakeFieldRows + InfoRows, 34, ConsoleColor.Yellow);
        }

        static void DrawSnake()
        {
            Write($"{SnakeHeadFigureSymbol}", SnakeEmenents[0].rowPossition, SnakeEmenents[0].colPossition);
            for (int i = 1; i < SnakeEmenents.Count; i++)
            {
                Write($"{FigureSymbol}", SnakeEmenents[i].rowPossition, SnakeEmenents[i].colPossition);
            }
        }

        static void DrawOpstacle()
        {
            for (int i = 0; i < SnakeEmenents.Count; i++)
            {
                //draw random OpstacleSymbol from  OpstacleSymbolArray
               // string currentOpstacleSymbol = OpstacleSymbolArray[Random.Next(0, OpstacleSymbolArray.Length - 1)];
               // Write($"{currentOpstacleSymbol}", CurrentOpstacle.rowPossition, CurrentOpstacle.colPossition, ConsoleColor.Yellow);
                
                //draw OpstacleSymbol
                 Write($"{OpstacleSymbol}", CurrentOpstacle.rowPossition, CurrentOpstacle.colPossition, ConsoleColor.Yellow);
            }
        }

        static void ChangeDirection(string direction)
        {
            if (direction == "DirectionDown")
            {
                DirectionDown = true;
                DirectionUp = false;
                DirectionLeft = false;
                DirectionRight = false;
            }
            else if (direction == "DirectionUp")
            {
                DirectionDown = false;
                DirectionUp = true;
                DirectionLeft = false;
                DirectionRight = false;
            }
            else if (direction == "DirectionLeft")
            {
                DirectionDown = false;
                DirectionUp = false;
                DirectionLeft = true;
                DirectionRight = false;
            }
            else if (direction == "DirectionRight")
            {
                DirectionDown = false;
                DirectionUp = false;
                DirectionLeft = false;
                DirectionRight = true;
            }
        }

        static void Write(string text, int row, int col, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.SetCursorPosition(col, row);
            Console.Write(text);
        }

        private static void UpdateLevel()
        {
            if (Score <= 0)
            {
                Level = 1;
            }

            if (Score >= 50 && NextLevel == 2)
            {
                Level = 2;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
                Thread.Sleep(1500);
                if (MusicPlayer)
                {
                    player.Play();
                }

            }
            else if (Score >= 200 && NextLevel == 3)
            {
                ChangeLevelValue(NextLevel);            
            }
            else if (Score >= 500 && NextLevel == 4)
            {
                ChangeLevelValue(NextLevel);
            }
            else if (Score >= 1000 && NextLevel == 5)
            {
                ChangeLevelValue(NextLevel);
            }
            else if (Score >= 2500 && NextLevel == 6)
            {
                ChangeLevelValue(NextLevel);
            }
            else if (Score == 5000 && NextLevel == 7)
            {
                ChangeLevelValue(NextLevel);
            }
            else if (Score >= 10000 && NextLevel == 8)
            {
                ChangeLevelValue(NextLevel);
            }
            else if (Score >= 20000 && NextLevel == 9)
            {
                ChangeLevelValue(NextLevel);
            }
            else if (Score >= 50000 && NextLevel == 10)
            {
                ChangeLevelValue(NextLevel);
            }
        }

        private static void ChangeLevelValue(int nextlevel)
        {
            Level = nextlevel;
            SleepSec -= SleepSecLevel;
            NextLevel++;
            new Thread(() =>
            {
                PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
            }).Start();
            Thread.Sleep(1500);
            if (MusicPlayer)
            {
                player.PlayLooping();
            }
        }

        static void PlayMusic(string songUrl, bool stopMusic = false)
        {
            var myPlayer = new SoundPlayer();
            myPlayer.SoundLocation = songUrl;
            myPlayer.Play();

            if (stopMusic == true)
            {
                myPlayer.Stop();
            }
        }
    }
    public class SnakeItem
    {
        public int Id { get; set; }
        public int rowPossition { get; set; }
        public int colPossition { get; set; }
    }
    public class OpstacleItem
    {
        public int Id { get; set; }
        public int rowPossition { get; set; }
        public int colPossition { get; set; }
    }
}
