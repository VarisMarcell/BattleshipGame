using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Core;


// test with various game states

// Enums and classes representing the core logic of the Battleship game.
public enum CellState
{
    Empty,
    Ship,
    Hit,
    Miss
}

public enum ShotResult
{
    Miss,
    Hit,
    Sunk,
    AlreadyShot
}

public sealed class Ship
{
    public required List<(int X, int Y)> Cells { get; init; }
    public int Hits { get; private set; }
    public bool IsSunk => Hits >= Cells.Count;

    public void RegisterHit()
    {
        Hits++;
    }
}

public sealed class Board
{
    public const int Size = 10;

    private readonly CellState[,] _cells = new CellState[Size, Size];
    private readonly List<Ship> _ships = new();
    private readonly Random _rng;

    public Board(Random? random = null)
    {
        _rng = random ?? new Random();
    }

    public CellState GetCell(int x, int y) => _cells[x, y];

    // Clears the board and places ships randomly.
    public void PlaceShipsRandomly()
    {
        _ships.Clear();
        Array.Clear(_cells, 0, _cells.Length);

        // Example ship sizes: 5, 4, 3, 3, 2 - standard Battleship sizes.
        int[] lengths = { 5, 4, 3, 3, 2 };
        foreach (var length in lengths)
        {
            PlaceOneShip(length);
        }
    }

    // method for testing
    public void PlaceShipsTesting()
    {
        _ships.Clear();
        Array.Clear(_cells, 0, _cells.Length);

        int[] lengths = { 5, 4, 3 };
        foreach (var length in lengths)
        {
            PlaceOneShipTesting(length);
        }
    }

    public ShotResult Shoot(int x, int y, out bool allShipsSunk)
    {
        allShipsSunk = false;


        // maybe fix this later if time
        if (!InBounds(x, y))
        {
            return ShotResult.Miss;
        }

        var state = _cells[x, y];

        // maybe change to allow multiple shots on same cell later
        if (state is CellState.Hit or CellState.Miss)
        {
            return ShotResult.AlreadyShot;
        }

        if (state == CellState.Empty)
        {
            _cells[x, y] = CellState.Miss;
            return ShotResult.Miss;
        }

        // Ship hit
        _cells[x, y] = CellState.Hit;

        var ship = _ships.First(s => s.Cells.Any(c => c.X == x && c.Y == y));
        ship.RegisterHit();

        if (!ship.IsSunk)
        {
            return ShotResult.Hit;
        }

        allShipsSunk = _ships.All(s => s.IsSunk);
        return ShotResult.Sunk;
    }

    private void PlaceOneShip(int length)
    {
        while (true)
        {
            bool horizontal = _rng.Next(2) == 0;

            int startX = horizontal ? _rng.Next(0, Size - length + 1) : _rng.Next(0, Size);
            int startY = horizontal ? _rng.Next(0, Size) : _rng.Next(0, Size - length + 1);

            // Check if the ship can be placed without collisions or going out of bounds
            for (int i = 0; i < length; i++)
            {
                int x = horizontal ? startX + i : startX;
                int y = horizontal ? startY : startY + i;

                if (_cells[x, y] != CellState.Empty)
                {
                    goto retry;
                }
            }

            // Place the ship
            var cells = new List<(int X, int Y)>(capacity : length);
            for (int i = 0; i < length; i++)
            {
                int x = horizontal ? startX + i : startX;
                int y = horizontal ? startY : startY + i;

                _cells[x, y] = CellState.Ship;
                cells.Add((x, y));
            }

            _ships.Add(new Ship { Cells = cells });
            return;

            retry:
                continue;
        }
    }

    // sets the ships for testing in certain locations to make game testing easy
    private void PlaceOneShipTesting(int length)
    {
        while (true)
        {
            int startX;
            int startY;
            switch (length)
            {
                case 5:
                    startX = 0;
                    startY = 0;
                    break;

                case 4:
                    startX = 2;
                    startY = 0;
                    break;

                case 3:
                    startX = 4;
                    startY = 0;
                    break;

                default:
                    startX = _rng.Next(0, Size);
                    startY = _rng.Next(0, Size - length + 1);
                    break;
            }

            var cells = new List<(int X, int Y)>(capacity: length);
            for (int i = 0; i < length; i++)
            {
                int x = startX;
                int y = startY + i;

                _cells[x, y] = CellState.Ship;
                cells.Add((x, y));
            }

            _ships.Add(new Ship { Cells = cells });
            return;

        }
    }

    // Check if coordinates are within board bounds
    private static bool InBounds(int x, int y) => x >= 0 && x < Size && y >= 0 && y < Size;

}






