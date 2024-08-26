﻿using System.Diagnostics.Metrics;
using System.Text;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace ConsoleTetris
{
    internal class Display
    {
        public const int s_height = 20;
        public const int s_playAreaWidth = 10;
        public const int s_scoreBoardWidth = 15;

        public const int s_totalWidht = 1 + s_playAreaWidth + 1 + s_scoreBoardWidth + 1;
        public const int s_totalHeight = 1 + s_height + 1;

        private char[,] _playArea = new char[s_height, s_playAreaWidth];
        private char[,] _scoreBoard = new char[s_height, s_scoreBoardWidth];

        public char[] EmptyChars = [' ', '.'];

        public Dictionary<char, ConsoleColor> _charColorMap = new()
        {
            { 'o', ConsoleColor.Yellow },
            { 'j', ConsoleColor.Blue },
            { 'i', ConsoleColor.Cyan },
            { 'l', ConsoleColor.Red },
            { 's', ConsoleColor.Green },
            { 't', ConsoleColor.Magenta },
            { 'z', ConsoleColor.DarkRed },
            { '|', ConsoleColor.DarkGray },
            { '-', ConsoleColor.DarkGray },
            { '.', ConsoleColor.Gray },
            { ' ', ConsoleColor.Black },
        };
        public Display()
        {
            Reset();

        }
        public void ResetPlayArea()
        {
            for (int i = 0; i < s_height; i++)
            {
                for (int o = 0; o < s_playAreaWidth; o++)
                {
                    _playArea[i, o] = EmptyChars[0];
                }
            }
        }
        public void ResetScoreBoard()
        {
            ClearScoreBoard();

            _scoreBoard[15, 0] = 'S';
            _scoreBoard[15, 1] = 'c';
            _scoreBoard[15, 2] = 'o';
            _scoreBoard[15, 3] = 'r';
            _scoreBoard[15, 4] = 'e';
            _scoreBoard[15, 5] = ':';

            _scoreBoard[8, 1] = 'N';
            _scoreBoard[8, 2] = 'e';
            _scoreBoard[8, 3] = 'x';
            _scoreBoard[8, 4] = 't';

            _scoreBoard[8, 6] = 'S';
            _scoreBoard[8, 7] = 't';
            _scoreBoard[8, 8] = 'o';
            _scoreBoard[8, 9] = 'r';
            _scoreBoard[8, 10] = 'e';
            _scoreBoard[8, 11] = 'd';

        }
        public void DrawLeaderboard()
        {
            for (int i = 0; i < Globals.LeaderBoard.Count && i + 7 < 17; i++)
            {
                string numberString = FormatNumber(Globals.LeaderBoard[i].score);
                DrawOnScoreBoard(0, s_height - i - 7, (i + 1).ToString()[0]);
                for (int x = 2; x < s_scoreBoardWidth; x++)
                {
                    if (x - 2 < numberString.Length)
                    {
                        DrawOnScoreBoard(x, s_height - i - 7, numberString[x - 2]);

                    }
                    else if (x - 3 - numberString.Length>=0 && x - 3 - numberString.Length < Globals.LeaderBoard[i].inital.Length)
                    {
                        DrawOnScoreBoard(x, s_height - i - 7, Globals.LeaderBoard[i].inital[x - 3 - numberString.Length]);
                    }
                }
            }
        }
        public void ClearScoreBoard()
        {
            for (int i = 0; i < s_height; i++)
            {
                for (int o = 0; o < s_scoreBoardWidth; o++)
                {
                    _scoreBoard[i, o] = ' ';
                }
            }

        }
        public void Reset()
        {
            ResetScoreBoard();
            ResetPlayArea();
        }
        public void DrawOnPlayArea(int x, int y, char toDraw)
        {
            if (x < 0 || y < 0 || x >= s_playAreaWidth || y >= s_height)
            {
                return;
            }
            _playArea[y, x] = toDraw;
        }
        public void DrawOnScoreBoard(int x, int y, char toDraw)
        {
            if (x < 0 || y < 0 || x >= s_scoreBoardWidth || y >= s_height)
            {
                return;
            }
            _scoreBoard[y, x] = toDraw;
        }
        public void SetEmptyOnPlayArea(int x, int y)
        {
            if (x < 0 || y < 0 || x >= s_playAreaWidth || y >= s_height)
            {
                return;
            }
            _playArea[y, x] = EmptyChars[0];
        }
        public char GetPlayAreaChar((int X, int Y) point)
        {
            if (point.X < 0 || point.Y < 0 || point.X >= s_playAreaWidth || point.Y >= s_height)
            {
                return '@';
            }

            return _playArea[point.Y, point.X];
        }
        public bool IsPlayareaEmptyThere((int X, int Y) point)
        {
            return EmptyChars.Contains(GetPlayAreaChar(point));
        }
        public bool LineFilled(int y)
        {
            for (int i = 0; i < s_playAreaWidth; i++)
            {
                if (EmptyChars.Contains(_playArea[y, i]))
                {
                    return false;
                }
            }
            return true;
        }
        public void DrawScore(int score)
        {
            score = score % 1000000000;
            string scoreString = score.ToString();
            while (scoreString.Length < 9)
            {
                scoreString = '0' + scoreString;
            }
            for (int i = 0; i < s_scoreBoardWidth - 6; i++)
            {
                _scoreBoard[15, i + 6] = scoreString[i];
            }
        }
        public string FormatNumber(int number)
        {
            number = number % 1000000000;
            string scoreString = number.ToString();
            while (scoreString.Length < 9)
            {
                scoreString = '0' + scoreString;
            }
            return scoreString;
        }
        public void DrawNextPiece(Piece piece)
        {
            for (int y = 3; y < 7; y++)
            {
                for (int x = 2; x < 6; x++)
                {
                    _scoreBoard[y, x] = ' ';
                }
            }
            for (int i = 0; i < piece.Segments.Length; i++)
            {
                (int X, int Y) segment = piece.Segments[i];
                _scoreBoard[segment.Y - s_height + 7, segment.X + 2] = piece.DisplayChar;
            }
        }
        public void DrawStoredPiece(Piece piece)
        {
            for (int y = 3; y < 7; y++)
            {
                for (int x = 8; x < 12; x++)
                {
                    _scoreBoard[y, x] = ' ';
                }
            }
            for (int i = 0; i < piece.Segments.Length; i++)
            {
                (int X, int Y) segment = piece.Segments[i];
                _scoreBoard[segment.Y - s_height + 7, segment.X + 8] = piece.DisplayChar;
            }
        }
        public void WriteOnScoreBord(int height, string toWrite)
        {
            for (int x = 0; x < s_scoreBoardWidth; x++)
            {
                if (toWrite.Length > x)
                {
                    DrawOnScoreBoard(x, height, toWrite[x]);
                }
            }

        }
        public void WriteOnPlayArea(int height, string toWrite)
        {
            for (int x = 0; x < s_playAreaWidth; x++)
            {
                if (toWrite.Length > x)
                {
                    DrawOnPlayArea(x, height, toWrite[x]);
                }
            }

        }
        public string GetBuffer()
        {
            lock (this)
            {

                StringBuilder sb = new StringBuilder();
                sb.Append('|');
                sb.Append('-', s_totalWidht - 2);
                sb.Append('|');
                for (int i = s_height - 1; i >= 0; i--)
                {
                    sb.Append('|');
                    for (int o = 0; o < s_playAreaWidth; o++)
                    {
                        sb.Append(_playArea[i, o]);
                    }
                    sb.Append('|');
                    for (int o = 0; o < s_scoreBoardWidth; o++)
                    {
                        sb.Append(_scoreBoard[i, o]);
                    }
                    sb.Append('|');
                }
                sb.Append('|');
                //sb.Append('-', s_totalWidht - 2);
                sb.Append("0123456789");
                sb.Append('-', s_totalWidht - 12);

                sb.Append('|');

                return sb.ToString();
            }
        }
        public void DrawWithColor()
        {
            lock (this)
            {
                //Stopwatch sw1 = Stopwatch.StartNew();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write('|');
                Console.Write(Enumerable.Repeat('-', s_totalWidht - 2).ToArray());
                Console.Write('|');
                //sw1.Stop();
                //long[] times = new long[s_height];
                for (int y = s_height - 1; y >= 0; y--)
                {
                    //Stopwatch sw2 = Stopwatch.StartNew();
                    Console.Write('|');
                    for (int x = 0; x < s_playAreaWidth; x++)
                    {
                        char next = _playArea[y, x];
                        if (!_charColorMap.TryGetValue(next, out ConsoleColor color))
                        {
                            color = ConsoleColor.Gray;
                        }

                        Console.ForegroundColor = color;

                        Console.Write(next);
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write('|');
                    for (int x = 0; x < s_scoreBoardWidth; x++)
                    {
                        char next = _scoreBoard[y, x];
                        if (y >= 3 && y < 7)
                        {
                            if (!_charColorMap.TryGetValue(next, out ConsoleColor color))
                            {
                                color = ConsoleColor.Gray;
                            }

                            Console.ForegroundColor = color;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;

                        }
                        Console.Write(next);
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write('|');
                    //sw2.Stop();
                    //times[y] = sw2.ElapsedMilliseconds;
                }
                //Stopwatch sw3 = Stopwatch.StartNew();
                Console.Write('|');
                //Console.Write('-', s_totalWidht - 2);
                Console.Write("0123456789");
                Console.Write(Enumerable.Repeat('-', s_totalWidht - 12).ToArray());

                Console.Write('|');
                //sw3.Stop();
                int end = 0;
                end++;
            }

        }
        public void DrawWithoutColor()
        {
            lock (this)
            {
                Console.Write(GetBuffer());
            }

        }
    }
}
