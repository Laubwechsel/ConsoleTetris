using System.Diagnostics.Metrics;
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
            { ' ', ConsoleColor.Gray },
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

            _scoreBoard[18, 0] = 'L';
            _scoreBoard[18, 1] = 'e';
            _scoreBoard[18, 2] = 'v';
            _scoreBoard[18, 3] = 'e';
            _scoreBoard[18, 4] = 'l';
            _scoreBoard[18, 5] = ':';

            _scoreBoard[17, 0] = 'L';
            _scoreBoard[17, 1] = 'i';
            _scoreBoard[17, 2] = 'n';
            _scoreBoard[17, 3] = 'e';
            _scoreBoard[17, 4] = 's';
            _scoreBoard[17, 5] = ':';

            _scoreBoard[16, 0] = 'S';
            _scoreBoard[16, 1] = 'c';
            _scoreBoard[16, 2] = 'o';
            _scoreBoard[16, 3] = 'r';
            _scoreBoard[16, 4] = 'e';
            _scoreBoard[16, 5] = ':';

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

            DrawLevel(1);
            DrawLinesCleared(0);
            DrawScore(0);
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
                    else if (x - 3 - numberString.Length >= 0 && x - 3 - numberString.Length < Globals.LeaderBoard[i].inital.Length)
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
                _scoreBoard[16, i + 6] = scoreString[i];
            }
        }
        public void DrawLevel(int level)
        {
            string levelString = FormatNumber(level, 3);
            for (int i = 0; i < s_scoreBoardWidth - 6 && i < levelString.Length; i++)
            {
                _scoreBoard[18, i + 6] = levelString[i];
            }

        }
        public void DrawLinesCleared(int lines)
        {
            string linesString = FormatNumber(lines, 4);

            for (int i = 0; i < s_scoreBoardWidth - 6 && i < linesString.Length; i++)
            {
                _scoreBoard[17, i + 6] = linesString[i];
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
        public string FormatNumber(int number, int leangth)
        {
            int mult = 1;
            for (int i = 0; i < leangth; i++)
            {
                mult *= 10;
            }
            number = number % mult;
            string scoreString = number.ToString();
            while (scoreString.Length < leangth)
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
                sb.AppendLine();
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
                    sb.AppendLine();
                }
                sb.Append('|');
                sb.Append("0123456789");
                sb.Append('-', s_totalWidht - 12);

                sb.Append('|');
                sb.AppendLine();

                return sb.ToString();
            }
        }
        public void DrawWithColor()
        {
            char[,] playAreaCopy;
            lock (this)
            {
                playAreaCopy = (char[,])_playArea.Clone();
            }
            StringBuilder sb = new();
            Console.ForegroundColor = ConsoleColor.Gray;
            ConsoleColor lastColor = ConsoleColor.Gray;
            sb.Append('|');
            sb.Append(Enumerable.Repeat('-', s_totalWidht - 2).ToArray());
            sb.Append('|');
            sb.Append(Environment.NewLine);
            for (int y = s_height - 1; y >= 0; y--)
            {
                sb.Append('|');
                for (int x = 0; x < s_playAreaWidth; x++)
                {
                    char next = playAreaCopy[y, x];
                    if (!_charColorMap.TryGetValue(next, out ConsoleColor color))
                    {
                        color = ConsoleColor.Gray;
                    }
                    if (lastColor != color)
                    {
                        Console.Write(sb.ToString());
                        lastColor = color;
                        Console.ForegroundColor = color;
                        sb.Clear();
                    }

                    sb.Append(next);
                }
                Console.Write(sb.ToString());
                sb.Clear();
                Console.ForegroundColor = ConsoleColor.Gray;
                lastColor = ConsoleColor.Gray;
                sb.Append('|');
                for (int x = 0; x < s_scoreBoardWidth; x++)
                {
                    char next = _scoreBoard[y, x];
                    if (y >= 3 && y < 7)
                    {
                        if (!_charColorMap.TryGetValue(next, out ConsoleColor color))
                        {
                            color = ConsoleColor.Gray;
                        }

                        if (lastColor != color)
                        {
                            Console.Write(sb.ToString());
                            Console.ForegroundColor = color;
                            lastColor = color;
                            sb.Clear();
                        }
                    }
                    if (y == 7)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;

                    }
                    sb.Append(next);
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                lastColor = ConsoleColor.Gray;
                sb.Append('|');
                sb.Append(Environment.NewLine);
            }
            sb.Append('|');
            sb.Append("0123456789");
            sb.Append(Enumerable.Repeat('-', s_totalWidht - 12).ToArray());

            sb.Append('|');
            sb.Append(Environment.NewLine);
            Console.Write(sb.ToString());


        }
        public void DrawWithoutColor()
        {

            Console.Write(GetBuffer());


        }
    }
}
