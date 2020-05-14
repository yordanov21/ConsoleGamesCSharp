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
        static int SnakeFieldRows = 20;
        static int SnakeFieldCols = 30;
        static int InfoCols = 15;
        static int ConsoleRows = 1 + SnakeFieldRows + 1;
        static int ConsoleCols = 1 + SnakeFieldCols + 1 + InfoCols + 1;       
        static string ScoresFileName = "scores.txt";
       
        //State
        static bool DirectionDown = false;
        static bool DirectionUp = false;
        static bool DirectionLeft = true;
        static bool DirectionRight = false;

        static bool OpposideDirectionDown =true;
        static bool OpposideDirectionUp = true;
        static bool OpposideDirectionLeft = true;
        static bool OpposideDirectionRight =false;
        
        static int HighScore = 0;
        static int Score = 0;    
        static int Level = 1;
        static string FigureSymbol = "o";
        static string SnakeHeadFigureSymbol = "O";
        static string SnakeHeadFigureSymbolOpstacle = "@";
        //◍ ■ ☯ ⚛ ⚙
        static string OpstacleSymbol = "x";
        //static SnakeItems[,] SnakeFigure = new SnakeItems[SnakeFieldRows, SnakeFieldCols];

        static int SnakeFigureRow = 1;
        static int SnakeFigureCol = 12;       
        static SnakeItem[,] SnakeField = new SnakeItem[SnakeFieldRows, SnakeFieldCols];
        static List<OpstacleItem> SnakeObstaclesList = new List<OpstacleItem>();
        static Random Random = new Random();
        static bool PauseMode = false;
        static bool MusicPlayer = true;
        static int NextLevel = 2;
        static int SleepSec = 350;
        static int SleepSecLevel = SleepSec/10;
        static OpstacleItem CurrentOpstacle = null;
        static List<SnakeItem> SnakeEmenents = new List<SnakeItem>()
        {
            new SnakeItem{Id=0,rowPossition=SnakeFigureRow,colPossition=SnakeFigureCol},
            new SnakeItem{Id=1,rowPossition=SnakeFigureRow,colPossition=SnakeFigureCol+1},
            new SnakeItem{Id=2,rowPossition=SnakeFigureRow,colPossition=SnakeFigureCol+2},
            new SnakeItem{Id=3,rowPossition=SnakeFigureRow,colPossition=SnakeFigureCol+3},
        };
        
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

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Title = "Snake V1.0 by y.yordanov21";
            Console.CursorVisible = false;
            Console.WindowHeight = ConsoleRows ;
            Console.WindowWidth = ConsoleCols;
            Console.BufferHeight = ConsoleRows;
            Console.BufferWidth = ConsoleCols;

            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = @"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\tetris-gameboy-02 (1).wav";
            player.Play();

            CurrentOpstacle = SnakeObstaclesList[SnakeObstaclesList.Count/2+2];
            while (true)
            {             
                UpdateLevel();
                // Read user input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();

                    if (key.Key == ConsoleKey.P && MusicPlayer==true)
                    {
                        player.Stop();
                        MusicPlayer = false;                    
                    }
                    if (key.Key == ConsoleKey.O && MusicPlayer == false)
                    {
                        player.Play();
                        MusicPlayer = true;
                    }
                    if (key.Key == ConsoleKey.Spacebar && PauseMode == false)
                    {
                        PauseMode = true;

                        Write("╔═══════════════╗", 5, 5);
                        Write("║               ║", 6, 5);
                        Write("║     Pause     ║", 7, 5);
                        Write("║               ║", 8, 5);
                        Write("╚═══════════════╝", 9, 5);
                        Console.ReadKey();
                    }

                    if (key.Key == ConsoleKey.Spacebar && PauseMode == true)
                    {
                        PauseMode = false;
                    }

                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }

                    if ((key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A)&&OpposideDirectionLeft)
                    {
                        ChangeDirection("DirectionLeft");
                        OpposideDirectionLeft = true;
                        OpposideDirectionRight = false;
                        OpposideDirectionUp = true;
                        OpposideDirectionDown = true;
                    }

                    if ((key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)&& OpposideDirectionRight)
                    {
                        ChangeDirection("DirectionRight");                     
                        OpposideDirectionRight = true;
                        OpposideDirectionLeft = false;
                        OpposideDirectionUp = true;
                        OpposideDirectionDown = true;
                    }

                    if ((key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W)&&OpposideDirectionUp)
                    {
                        ChangeDirection("DirectionUp");
                        OpposideDirectionUp = true;
                        OpposideDirectionDown = false;
                        OpposideDirectionLeft = true;
                        OpposideDirectionRight = true;
                    }

                    if ((key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)&&OpposideDirectionDown)
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
                for (int i = SnakeEmenents.Count - 1; i >=1; i--)
                {
                    SnakeEmenents[i].rowPossition=SnakeEmenents[i-1].rowPossition;
                    SnakeEmenents[i].colPossition=SnakeEmenents[i-1].colPossition;
                }

                if (SnakeEmenents[0].rowPossition == SnakeFieldRows )
                {
                    SnakeEmenents[0].rowPossition=1;                  
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
                    SnakeEmenents[0].colPossition =1;
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

            var lastSnakeEmenentId = SnakeEmenents[SnakeEmenents.Count-1].Id;
            var lastSnakeEmenentRow = SnakeEmenents[SnakeEmenents.Count-1].rowPossition;
            var lastSnakeEmenentCol = SnakeEmenents[SnakeEmenents.Count-1].colPossition;
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

                // DrawOpstaclePassThroughSnake();
                Write($"{SnakeHeadFigureSymbolOpstacle}", SnakeEmenents[0].rowPossition, SnakeEmenents[0].colPossition);
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\selection.wav");
                }).Start();
                Thread.Sleep(100);
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
                    Write("╔══════════════╗", 5, 5);
                    Write("║  Game        ║", 6, 5);
                    Write("║     over!    ║", 7, 5);
                    Write($"║      {scoreAsString} ║", 8, 5);
                    Write("╚══════════════╝", 9, 5);
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
            string firstLine = "╔";
            firstLine += new string('═', SnakeFieldCols);
            firstLine += "╦";
            firstLine += new string('═', InfoCols);
            firstLine += "╗";

            string middleLine = "";
            for (int i = 0; i < SnakeFieldRows; i++)
            {
                middleLine += "║";
                middleLine += new string(' ', SnakeFieldCols) + "║" + new string(' ', InfoCols) + "║" + "\n";
            }

            string endLine = "╚";
            endLine += new string('═', SnakeFieldCols);
            endLine += "╩";
            endLine += new string('═', InfoCols);
            endLine += "╝";

            string borderFrame = firstLine + "\n" + middleLine + endLine;
            Write(borderFrame, 0, 0);
        }
        static void DrawInfo()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
            }
            Write("Level:", 1, SnakeFieldCols + 3);
            Write(Level.ToString(), 3, SnakeFieldCols + 3);
            Write("Score:", 5, SnakeFieldCols + 3);
            Write(Score.ToString(), 7, SnakeFieldCols + 3);
            Write("High Score:", 9, SnakeFieldCols + 3);
            Write(HighScore.ToString(), 11, SnakeFieldCols + 3);        
            Write("Keys:", 13, SnakeFieldCols + 3);
            Write("  ^  ", 14, SnakeFieldCols + 3);
            Write("<   >", 15, SnakeFieldCols + 3);
            Write("  v  ", 16, SnakeFieldCols + 3);
            Write("Pause: space", 17, SnakeFieldCols +3);
            Write($"Music: P -{MusicPlayer}", 19, SnakeFieldCols +3);
            
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
        static void Write(string text, int row, int col,ConsoleColor color= ConsoleColor.Gray)
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
                SleepSec -=SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 200 && NextLevel == 3)
            {
                Level = 3;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 500 && NextLevel == 4)
            {
                Level = 4;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 1000 && NextLevel == 5)
            {
                Level = 5;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 2500 && NextLevel == 6)
            {
                Level = 6;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score == 5000 && NextLevel == 7)
            {
                Level = 7;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 10000 && NextLevel == 8)
            {
                Level = 8;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 20000 && NextLevel == 9)
            {
                Level = 9;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 50000 && NextLevel == 10)
            {
                Level = 10;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
        }
        static void PlayMusic(string songUrl)
        {
            var myPlayer = new SoundPlayer();
            myPlayer.SoundLocation = songUrl;
            myPlayer.Play();
        }
        static void DrawOpstaclePassThroughSnake()
        {
            for (int i = 1; i < SnakeEmenents.Count; i++)
            {
                Write($"{OpstacleSymbol}", SnakeEmenents[i].rowPossition, SnakeEmenents[i].colPossition);
                Thread.Sleep(1);
                Write($"{FigureSymbol}", SnakeEmenents[i].rowPossition, SnakeEmenents[i].colPossition);
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
