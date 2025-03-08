
using System.Diagnostics;

namespace ConsoleTetris
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.Clear();
            Console.CursorVisible = false;
            Console.SetWindowSize(42, Display.s_totalHeight + 2);
            if (OperatingSystem.IsWindows())
            {
                Console.SetBufferSize(42, Display.s_totalHeight + 2);
            }
            if (File.Exists(Globals.HighscoresPath))
            {
                using (StreamReader stream = File.OpenText(Globals.HighscoresPath))
                {
                    string? line;
                    while ((line = stream.ReadLine()) != null)
                    {
                        string[] parts = line.Split(' ');
                        if (parts.Length == 2)
                        {
                            Globals.LeaderBoard.Add((int.Parse(parts[0]), parts[1].ToCharArray()));
                        }
                    }
                }
            }
            Display display = new Display();
            Game game = new Game(display);
            Menu menu = new Menu(display, game);
            game.SetMenu(menu);
            Thread inputThread = new Thread(() =>
            {
                while (true)
                {
                    ConsoleKeyInfo input = Console.ReadKey(true);
                    if (game.Running) game.HandleInput(input);
                    else menu.HandleIndput(input);
                }
            });
            Tetris();
            Console.WriteLine("type 'nc' for no Color\n(better performance)");
            if (OperatingSystem.IsWindows())
            {
                Console.WriteLine("use ctr + mouse wheel to change the games size");
            }
            if (OperatingSystem.IsLinux())
            {
                Console.WriteLine("use ctr + '+' or '-' to change the games size");
            }
            if (OperatingSystem.IsIOS())
            {
                Console.WriteLine("use cmd + '+' or '-' to change the games size");
            }
            Console.WriteLine();
            Console.WriteLine("a/d or left/right move piece");
            Console.WriteLine("q/e rotate piece");
            Console.WriteLine("w or up store piece");
            Console.WriteLine("s or down hard drop piece");
            Console.WriteLine();
            Console.WriteLine("Press Enter To Start");

            string? inp = Console.ReadLine();
            if (inp == "nc")
            {
                Globals.DrawFunction = display.DrawWithoutColor;
            }
            else
            {
                Globals.DrawFunction = display.DrawWithColor;
            }
            Console.Clear();
            Console.SetWindowSize(Display.s_totalWidht + 1, Display.s_totalHeight + 2);
            if (OperatingSystem.IsWindows())
            {
                Console.SetBufferSize(Display.s_totalWidht + 1, Display.s_totalHeight + 2);
            }
            Globals.DrawFunction.Invoke();
            game.Start();
            inputThread.Start();
            Thread.Sleep(30);
#if DEBUG
            int frameCounter = 0;
#endif
            Stopwatch stopwatch = new Stopwatch();
            while (true)
            {
                Console.SetCursorPosition(0, 0);
#if DEBUG
#endif
                stopwatch.Restart();
                Globals.DrawFunction.Invoke();
                stopwatch.Stop();
                Thread.Sleep(int.Clamp(10 - (int)stopwatch.ElapsedMilliseconds, 0, 10));
#if DEBUG
                Console.WriteLine(frameCounter);
                Console.Write(stopwatch.ElapsedMilliseconds);
                Console.Write("   ");
                frameCounter++;
#endif
            }
        }
        static void Tetris()
        {
            string text = "TTTTTT  EEEEE  TTTTTT  RRRRR   II   SSSS  \n" +
                          "TTTTTT  EE     TTTTTT  RR   R  II  SS   S \n" +
                          "  TT    EE       TT    RR   R  II   SS    \n" +
                          "  TT    EEEEE    TT    RRRRR   II    SSS  \n" +
                          "  TT    EE       TT    RRRR    II      SS \n" +
                          "  TT    EE       TT    RR RR   II   S   SS\n" +
                          "  TT    EEEEE    TT    RR  RR  II    SSSS \n";
            for (int i = 0; i < text.Length; i++)
            {
                char cur = text[i];
                switch (cur)
                {
                    case 'T':
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case 'E':
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case 'R':
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case 'I':
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case 'S':
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                }
                Console.Write(cur);
            }

        }

    }

}
