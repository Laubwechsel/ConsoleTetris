using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleTetris
{
    internal class Game
    {
        Display _display;

        public int Score = 0;
        public int HighScore = 0;
        bool _lastScoreTetris = false;
        Piece _currentPiece;
        Piece _nextPiece;
        Piece? _holdPiece;
        bool _swapped = false;

        Queue<int> _nextPieceQueue = new();

        GhostPiece _ghost;

        Random _random = new Random((int)DateTime.Now.Ticks);
        private Thread _stepThread;
        private object _pieceLock = new object();

        public bool Running = false;
        private AutoResetEvent _gameReset = new AutoResetEvent(false);

        private float _gameSpeed = 1;
        private float _tickDelay = 1000;
        private int _totalLinesCleared = 0;
        private int _level = 1;

        Menu? _menu;
        public Game(Display display)
        {
            _display = display;
            _ghost = new GhostPiece(_display);
            SetNextPiece(GetNextPieceID());
            CreateNextPiece();
            _stepThread = new Thread(() =>
            {
                Stopwatch sw = new();
                int lastStepTime;
                int timeToSleep;
                while (true)
                {
                    while (Running)
                    {
                        sw.Restart();
                        Step();
                        sw.Stop();
                        lastStepTime = (int)sw.ElapsedMilliseconds;
#if DEBUG
                        string lastStepTimeString = lastStepTime.ToString();
                        for (int i = 0; i < lastStepTimeString.Length; i++)
                        {
                            _display.DrawOnScoreBoard(i, 0, lastStepTimeString[i]);

                        }
#endif
                        timeToSleep = (int)(_tickDelay / _gameSpeed);
                        Thread.Sleep(int.Clamp(timeToSleep - lastStepTime, 0, timeToSleep));
                    }
                    _menu!.DisplayGameOver();
                    _gameReset.WaitOne();
                }
            });
        }
        public void SetMenu(Menu menu)
        {
            _menu = menu;
        }
        public void Reset()
        {
            _display.ResetPlayArea();
            _nextPieceQueue.Clear();

            SetNextPiece(GetNextPieceID());
            CreateNextPiece();
            Running = true;
            _gameSpeed = 1;
            _totalLinesCleared = 0;
            _level = 1;
            _gameReset.Set();
        }
        public void Start()
        {
            Running = true;
            lock (_display)
            {
                _currentPiece.Show();
            }
            _stepThread.Start();
        }
        private void Step()
        {
            lock (_pieceLock)
            {

                if (!_currentPiece.Move((0, -1)))
                {
                    _swapped = false;
                    CreateNextPiece();
                    LineCheck();

                    for (int i = 0; i < _currentPiece.Segments.Length; i++)
                    {
                        if (!_display.EmptyChars.Contains(_display.GetPlayAreaChar(_currentPiece.Segments[i])))
                        {
                            Running = false;
                            _display.DrawScore(Score);
                            if (Score > HighScore)
                            {
                                HighScore = Score;
                            }
                            return;
                        }
                    }

                    _currentPiece.Show();

                }
                DisplayGhost();
            }
        }
        private void LineCheck()
        {
            int clearedLines = 0;
            lock (_display)
            {

                for (int i = Display.s_height - 1; i >= 0; i--)
                {
                    if (_display.LineFilled(i))
                    {
                        for (int y = i; y < Display.s_height - 1; y++)
                        {
                            for (int x = 0; x < Display.s_playAreaWidth; x++)
                            {
                                _display.DrawOnPlayArea(x, y, _display.GetPlayAreaChar((x, y + 1)));
                            }
                        }
                        for (int x = 0; x < Display.s_playAreaWidth; x++)
                        {
                            _display.SetEmptyOnPlayArea(x, Display.s_height - 1);
                        }
                        clearedLines++;
                    }
                }
                if (clearedLines < 4)
                {
                    switch (clearedLines)
                    {
                        case 1:
                            Score += 100;
                            break;
                        case 2:
                            Score += 250;
                            break;
                        case 3:
                            Score += 400;
                            break;
                        default:
                            break;
                    }
                    _lastScoreTetris = false;
                }
                else
                {
                    if (_lastScoreTetris)
                    {
                        Score += 1200;
                    }
                    else
                    {
                        Score += 800;
                    }
                    _lastScoreTetris = true;
                }
                _display.DrawScore(Score);

                _totalLinesCleared += clearedLines;
                _display.DrawLinesCleared(_totalLinesCleared);
                if (_totalLinesCleared >= LevelBarrier(_level))
                {
                    _level += 1;
                    _gameSpeed = GameSpeedForLevel(_level);
                    _display.DrawLevel(_level);
                }
            }
        }
        private float GameSpeedForLevel(int level)
        {
            return MathF.Pow(1.2f, level - 1);
        }
        private int LevelBarrier(int level)
        {
            return (int)(3f * MathF.Pow(level, 1.9f)) + 5;
        }
        [MemberNotNull(nameof(_currentPiece))]
        private void CreateNextPiece()
        {
            lock (_pieceLock)
            {
                _currentPiece = _nextPiece;
                for (int i = 0; i < _currentPiece.Segments.Length; i++)
                {
                    _currentPiece.Segments[i].X += 4;
                }
                int nextPieceInt = GetNextPieceID();
                SetNextPiece(nextPieceInt);
                _display.DrawNextPiece(_nextPiece);
            }
        }
        public void HandleInput(ConsoleKeyInfo input)
        {
            lock (_pieceLock)
            {

                switch (input.Key)
                {
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        _currentPiece.Move((-1, 0));
                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        _currentPiece.Move((1, 0));
                        break;
                    case ConsoleKey.Q:
                        _currentPiece.RotateCCW();
                        break;
                    case ConsoleKey.E:
                        _currentPiece.RotateCW();
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        lock (_display)
                        {
                            while (_currentPiece.Move((0, -1)))
                            {
                                Score += 5;
                            }
                            _display.DrawScore(Score);
                        }
                        break;
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        if (!_swapped)
                        {
                            SwapHoldPiece();
                            _swapped = true;
                        }
                        break;
                    default:
                        break;
                }
                DisplayGhost();
            }
        }
        private void FillQueue()
        {
            List<int> pieces = [0, 1, 2, 3, 4, 5, 6];
            while (pieces.Count > 0)
            {
                int next = _random.Next(pieces.Count);
                _nextPieceQueue.Enqueue(pieces[next]);
                pieces.RemoveAt(next);
            }
        }
        private int GetNextPieceID()
        {
            if (_nextPieceQueue.Count > 0)
            {
                return _nextPieceQueue.Dequeue();
            }
            FillQueue();
            return _nextPieceQueue.Dequeue();
        }
        [MemberNotNull(nameof(_nextPiece))]
        private void SetNextPiece(int id)
        {
            switch (id)
            {
                case 0://i
                    _nextPiece = new Piece(_display, 'i', Piece.IRotationDeltas, Piece.IInitialPositions);
                    break;
                case 1://o
                    _nextPiece = new Piece(_display, 'o', Piece.BRotationDeltas, Piece.BInitialPositions);
                    break;
                case 2://l
                    _nextPiece = new Piece(_display, 'l', Piece.LRotationDeltas, Piece.LInitialPositions);
                    break;
                case 3://j
                    _nextPiece = new Piece(_display, 'j', Piece.JRotationDeltas, Piece.JInitialPositions);
                    break;
                case 4://t
                    _nextPiece = new Piece(_display, 't', Piece.TRotationDeltas, Piece.TInitialPositions);
                    break;
                case 5://z
                    _nextPiece = new Piece(_display, 'z', Piece.ZRotationDeltas, Piece.ZInitialPositions);
                    break;
                case 6://s
                    _nextPiece = new Piece(_display, 's', Piece.SRotationDeltas, Piece.SInitialPositions);
                    break;
                default:
                    _nextPiece = new Piece(_display, 'i', Piece.IRotationDeltas, Piece.IInitialPositions);
                    break;
            }
        }
        public void DisplayGhost()
        {
            lock (_pieceLock)
            {
                lock (_display)
                {

                    _ghost.Hide();
                    _currentPiece.Hide();
                    for (int i = 0; i < _currentPiece.Segments.Length; i++)
                    {
                        _ghost.Segments[i].X = _currentPiece.Segments[i].X;
                        _ghost.Segments[i].Y = _currentPiece.Segments[i].Y;
                    }
                    while (_ghost.Move((0, -1))) ;
                    _currentPiece.Show();
                }
            }
        }
        private void SwapHoldPiece()
        {
            lock (_pieceLock)
            {
                lock (_display)
                {

                    _currentPiece.Hide();
                    if (_holdPiece == null)
                    {
                        _holdPiece = _currentPiece;
                        CreateNextPiece();
                    }
                    else
                    {
                        Piece buf = _holdPiece;
                        _holdPiece = _currentPiece;
                        _currentPiece = buf;
                        for (int i = 0; i < _currentPiece.Segments.Length; i++)
                        {
                            _currentPiece.Segments[i].X += 4;
                        }
                    }
                    _holdPiece.ResetPosition();
                    _display.DrawStoredPiece(_holdPiece);
                    _currentPiece.Show();
                }
            }
        }
        private void UpdateHighScore()
        {

        }
    }
}
