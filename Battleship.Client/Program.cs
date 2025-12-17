using System.Net.Sockets;
using System.Text;


internal static class Program
{
    private const int Port = 5000;

    // visualize a map for the client
    private static readonly char[,] AttackGrid = new char[10, 10];

    private static async Task Main()
    {
        Console.Write("Server IP (blank = localhost): ");
        var ip = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ip))
        {
            ip = "127.0.0.1";
        }

        Console.Write("Your name: ");
        var name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Player";
        }

        using var client = new TcpClient();
        Console.WriteLine($"Connecting to {ip}:{Port}...");
        await client.ConnectAsync(ip, Port);

        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        // Send hello
        await writer.WriteLineAsync($"HELLO {name}");

        Console.WriteLine("Connected. Waiting for server...");

        bool running = true;

        // initialize attack grid
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                AttackGrid[x, y] = '~'; 
            }
        }

        while (running)
        {
            string? line = await reader.ReadLineAsync();
            if (line == null)
            {
                Console.WriteLine("Disconnected from server.");
                break;
            }

            // parsing server messages
            var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0];
            var rest = parts.Length > 1 ? parts[1] : "";


            switch (cmd)
            {
                case "WELCOME":
                    Console.WriteLine($"You are player: {rest}");
                    break;

                case "WAITING_FOR_OPPONENT":
                    Console.WriteLine("Waiting for opponent to connect...");
                    break;

                case "START":
                    Console.WriteLine("Game started!");
                    break;

                case "MESSAGE":
                    Console.WriteLine(rest);
                    break;

                case "YOUR_TURN":
                    Console.WriteLine("=== Your turn! ===");
                    var (x, y) = ReadShot();

                    // store last shot for updating grid later
                    _lastShotX = x;
                    _lastShotY = y;

                    await writer.WriteLineAsync($"FIRE {x} {y}");
                    break;

                case "OPPONENT_TURN":
                    Console.WriteLine("=== Opponent's turn... ===");
                    break;

                case "RESULT":
                    Console.WriteLine($"Shot result: {rest}");

                    // Update attack grid
                    if (!rest.Trim().Equals("AlreadyShot", StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateAttackGridFromResult(rest);
                        PrintAttackGrid();
                    }

                    break;

                case "OPPONENT_RESULT":
                    Console.WriteLine($"Opponent's shot result: {rest}");
                    break;

                case "GAME_OVER":
                    Console.WriteLine($"GAME OVER: {rest}");
                    running = false;
                    break;

                default:
                    Console.WriteLine($"[Unknown] {line}");
                    break;
            }
        }

        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();

    }


    // might need to update this for when ships get sunk
    private static int _lastShotX;
    private static int _lastShotY;

    private static void UpdateAttackGridFromResult(string resultRaw)
    {
        string result = resultRaw.Trim();
        result = result.ToUpperInvariant();

        char symbol = result switch
        {
            "MISS" => 'O',
            "HIT" => 'X',
            "SUNK" => 'S',
            _ => AttackGrid[_lastShotX, _lastShotY] // no change for AlreadyShot or unknown
        };

        AttackGrid[_lastShotX, _lastShotY] = symbol;
    }

    private static void PrintAttackGrid()
    {   
        Console.WriteLine();
        Console.WriteLine("Attack Grid:");
        Console.WriteLine("  0 1 2 3 4 5 6 7 8 9");

        for (int y = 0; y < 10; y++)
        {
            Console.Write(y + " ");
            for (int x = 0; x < 10; x++)
            {
                Console.Write(AttackGrid[x, y] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }


    private static (int X, int Y) ReadShot()
    {
        while (true)
        {
            Console.Write("Enter shot as: x y (0-9): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                Console.WriteLine("Please enter two numbers seperated by a space.");
                continue;
            }

            if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y) &&
                x is >= 0 and <= 9 && y is >= 0 and <= 9)
            {
                return (x, y);
            }

            Console.WriteLine("Invalid coordinates. Please enter numbers between 0 and 9.");
        }
    }
}







