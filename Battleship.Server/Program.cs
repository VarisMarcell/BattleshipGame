using Battleship.Core;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

internal static class Program
{
    private const int Port = 5000;

    // test board
    //private const TestBoard

    private static async Task Main()
    {
        Console.WriteLine("Battleship Server is starting...");

        var listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine($"Listening for connections on port {Port}...");

        var rng = new Random();

        // Accept player 1
        var p1 = await AcceptPlayerAsync(listener, playerNumber: 1, rng);
        await p1.SendAsync("WELCOME 1");
        await p1.SendAsync("WAITING_FOR_OPPONENT");

        // Accept player 2
        var p2 = await AcceptPlayerAsync(listener, playerNumber: 2, rng);
        await p2.SendAsync("WELCOME 2");

        await BroadcastAsync("START", p1, p2);
        Console.WriteLine("Two players connected. Game is starting...");

        await RunGameLoopAsync(p1, p2);
    }

    private static async Task<PlayerConnection> AcceptPlayerAsync(TcpListener listener, int playerNumber, Random rng)
    {
        Console.WriteLine($"Waiting for Player {playerNumber} to connect...");
        var tcpClient = await listener.AcceptTcpClientAsync();
        Console.WriteLine($"Player {playerNumber} connected: {tcpClient.Client.RemoteEndPoint}");

        var player = new PlayerConnection(tcpClient, playerNumber, rng);

        // Expect : HELLO <name>
        var line = await player.Reader.ReadLineAsync();
        if (!string.IsNullOrWhiteSpace(line) && line.StartsWith("HELLO "))
        {
            player.Name = line["HELLO ".Length..].Trim();
        }

        await player.SendAsync($"MESSAGE Hello {player.Name}, you are Player {playerNumber}.");
        return player;
    }

    private static Task BroadcastAsync(string line, params PlayerConnection[] players) => Task.WhenAll(players.Select(p => p.SendAsync(line)));

    private static async Task RunGameLoopAsync(PlayerConnection p1, PlayerConnection p2)
    {
        var current = p1;
        var opponent = p2;

        while (true)
        {
            await current.SendAsync("YOUR_TURN");
            await opponent.SendAsync("OPPONENT_TURN");


            string? cmd = await current.Reader.ReadLineAsync();
            if (cmd is null)
            {
                Console.WriteLine($"Player {current.Number} disconnected.");
                await opponent.SendAsync("MESSAGE Opponent disconnected. You win.");
                await opponent.SendAsync("GAME_OVER WIN");
                return;
            }

            if (!TryParseFire(cmd, out int x, out int y))
            {
                await current.SendAsync("MESSAGE Invalid command format. Use: FIRE x y");
                continue;
            }

            var result = opponent.Board.Shoot(x, y, out bool allSunk);

            // allow re-shooting same cell without changing turn
            if ( result == ShotResult.AlreadyShot)
            {
                await current.SendAsync("RESULT AlreadyShot");
                await current.SendAsync("MESSAGE You have already fired at this location. Try again.");
                continue; 
            }

            if (result == ShotResult.Hit || result == ShotResult.Sunk)
            {
                await current.SendAsync($"RESULT {result}");
                await opponent.SendAsync($"OPPONENT_RESULT {x} {y} {result}");

                Console.WriteLine($"{current.Name} fired at ({x},{y}) => {result}");

                if (allSunk)
                {
                    await current.SendAsync("GAME_OVER WIN");
                    await opponent.SendAsync("GAME_OVER LOSE");
                    Console.WriteLine($"Player {current.Number} wins!");
                    return;
                }

                continue;
            }

            // Notify both players of the result
            await current.SendAsync($"RESULT {result}");
            await opponent.SendAsync($"OPPONENT_RESULT {x} {y} {result}");

            Console.WriteLine($"{current.Name} fired at ({x},{y}) => {result}");

            //if (allSunk)
            //{
            //    await current.SendAsync("GAME_OVER WIN");
            //    await opponent.SendAsync("GAME_OVER LOSE");
            //    Console.WriteLine($"Player {current.Number} wins!");
            //    return;
            //}

            // Switch turns
            (current, opponent) = (opponent, current);
        }
    }

    private static bool TryParseFire(string cmd, out int x, out int y)
    {
        x = y = -1;

        if (!cmd.StartsWith("FIRE ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // analyze the command structure
        var parts = cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            return false;
        }

        return int.TryParse(parts[1], out x) && int.TryParse(parts[2], out y) && x is >= 0 and <= 9 && y is >= 0 and <= 9;
    }

    private sealed class PlayerConnection
    {
        public TcpClient Client { get; }
        public StreamReader Reader { get; }
        public StreamWriter Writer { get; }
        public Board Board { get; }
        public string Name { get; set; } = "Player";
        public int Number { get; }

        public PlayerConnection(TcpClient client, int number, Random rng)
        {
            Client = client;
            Number = number;

            var stream = client.GetStream();
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream) { AutoFlush = true };

            Board = new Board(rng);
            //Board.PlaceShipsRandomly();
            Board.PlaceShipsTesting();
        }

        public Task SendAsync(string line) => Writer.WriteLineAsync(line);
    }

}









// initial testing for the server side implementation of the Battleship game.
//internal static class Program
//{
//    private static void Main()
//    {
//        Console.WriteLine("Battleship Server is starting...");

//        // Create a game board and place ships randomly.
//        var board = new Board();
//        board.PlaceShipsRandomly();
//        Console.WriteLine("Ships have been placed on the board.");

//        // Example of firing a shot at 3 cells.
//        Fire(board, 0, 0);
//        Fire(board, 5, 5);
//        Fire(board, 9, 9);

//        // Try firing at the same cell again to see the AlreadyShot result.
//        Fire(board, 0, 0);

//        Console.WriteLine("Sanity check complete.");
//        Console.WriteLine("Press enter to exit...");
//        Console.ReadLine();
//    }


//    private static void Fire(Board board, int x, int y)
//    {
//        var result = board.Shoot(x, y, out bool allSunk);
//        Console.WriteLine($"FIRE ({x},{y}) => {result} | allShipsSunk={allSunk}");
//    }   
//}