using System;
using System.Collections.Generic;
using System.Threading;
using System.Media;
using System.IO;
using System.Text.RegularExpressions;

namespace snake
{
    
    class Program
    {
        //Settings 
        static int SnakeFieldRows = 20;
        static int SnakeFieldCols = 20;
        static int InfoCols = 15;
        static int ConsoleRows = 1 + SnakeFieldRows + 1;
        static int ConsoleCols = 1 + SnakeFieldCols + 1 + InfoCols + 1;       
        static string ScoresFileName = "scores.txt";
       
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
        static string FigureSymbol = "*";
        //static SnakeItems[,] SnakeFigure = new SnakeItems[SnakeFieldRows, SnakeFieldCols];

        static int SnakeFigureRow = 1;
        static int SnakeFigureCol = 12;       
        static SnakeItem[,] SnakeField = new SnakeItem[SnakeFieldRows, SnakeFieldCols];
        static List<OpstacleItem> SnakeObstaclesList = new List<OpstacleItem>();
        static Random Random = new Random();
        static bool PauseMode = false;
        static bool PlayGame = true;
        static int NextLevel = 2;
        static int SleepSec = 600;
        static int SleepSecLevel = 50;
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

            CurrentOpstacle = SnakeObstaclesList[SnakeObstaclesList.Count/2+2];
            while (true)
            {
               
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

                        PauseMode = false;
                    }

                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }

                    if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A)
                    {
                        ChangeDirection("DirectionLeft");
                    
                    }

                    if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)
                    {
                        ChangeDirection("DirectionRight");
                      
                    }

                    if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W)
                    {
                        ChangeDirection("DirectionUp");
                      
                    }

                    if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
                    {
                        ChangeDirection("DirectionDown");
                      
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
                    Score++;
                
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
                Score += 50 * Level;
                CurrentOpstacle = SnakeObstaclesList[Random.Next(0, SnakeObstaclesList.Count)];
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
                    PlayGame = false;
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
            firstLine += new string('═', SnakeFieldRows);
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
            Console.Write(borderFrame);
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
           
            //Write("Frame:", 13, SnakeFieldCols + 3);
            //Write(Frame.ToString(), 14, SnakeFieldCols + 3);
            //Write("Position:", 15, SnakeFieldCols + 3);
            //Write($"{CurrentFigureCol}, {CurrentFigureCol}", 16, SnakeFieldCols + 3);
            Write("Keys:", 15, SnakeFieldCols + 3);
            Write("  ^  ", 16, SnakeFieldCols + 3);
            Write("<   >", 17, SnakeFieldCols + 3);
            Write("  v  ", 18, SnakeFieldCols + 3);
            Write("Pause: space", 20, SnakeFieldCols +3);
            
        }
        static void DrawSnake()
        {

            for (int i = 0; i < SnakeEmenents.Count; i++)
            {
                Write($"{FigureSymbol}", SnakeEmenents[i].rowPossition, SnakeEmenents[i].colPossition);
            }
        }
        static void DrawOpstacle()
        {

            for (int i = 0; i < SnakeEmenents.Count; i++)
            {
                Write($"{FigureSymbol}", CurrentOpstacle.rowPossition, CurrentOpstacle.colPossition);
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

        static void Write(string text, int row, int col)
        {
            Console.SetCursorPosition(col, row);
            Console.Write(text);
        }

        private static void UpdateLevel()
        {
            if (Score <= 0)
            {
                Level = 1;
            }

            if (Score >= 500 && NextLevel == 2)
            {
                Level = 2;
                SleepSec -=SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 1000 && NextLevel == 3)
            {
                Level = 3;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 5000 && NextLevel == 4)
            {
                Level = 4;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 10000 && NextLevel == 5)
            {
                Level = 5;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 20000 && NextLevel == 6)
            {
                Level = 6;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score == 50000 && NextLevel == 7)
            {
                Level = 7;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 100000 && NextLevel == 8)
            {
                Level = 8;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 250000 && NextLevel == 9)
            {
                Level = 9;
                SleepSec -= SleepSecLevel;
                NextLevel++;
                new Thread(() =>
                {
                    PlayMusic(@"C:\Users\yyord\OneDrive\Desktop\My projects\Console_Games\ConsoleGamesCSharp\songs\success.wav");
                }).Start();
            }
            else if (Score >= 500000 && NextLevel == 10)
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
