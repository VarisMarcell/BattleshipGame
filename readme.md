# Battleship Game
This is a simple console-based Battleship game implemented in C#. The game allows two players to take turns guessing the locations of each other's ships.

## Prerequisites
- .NET Framework 4.7.2 or higher
- A C# development environment (e.g., Visual Studio)
- C# language version 7.3 or higher
- A terminal or command prompt to run the application
- Basic understanding of Battleship game rules
- Familiarity with console input/output in C#
- Ability to compile and run C# console applications

## Installation
1. Clone the repository: 
   ```bash
   git clone
2. Navigate to the project directory:
   ```bash
   cd battleship-game
3. Open the project in your C# development environment.
4. Build the solution to restore any dependencies.
5. Run the application.

---
For me, I have to open the solution explorer and right-click on Battleship.Client and select debug -> Start new instance to get the second player running.

## How to Play
1. Each player places their ships on a 10x10 grid by specifying the starting coordinates and orientation (horizontal or vertical).
2. Players take turns guessing the coordinates of the opponent's ships.
3. The game continues until one player sinks all of the opponent's ships.

## Protocol Details
The game uses a simple text-based protocol for communication between the two players. Each message consists of a command followed by parameters, separated by spaces. 

- `HELLO <name>` is sent by the client when joining the server. Name represents the name given by the player.
- `WELCOME` is sent by the server to the client upon a client connecting.
- `WAITING_FOR_OPPONENT` is sent by the server after player 1 joins, before player 2 connects.
- `START` announces that both players have connected and the game is ready to start.
- `MESSAGE <string>` a string of information typically sent from the server.
- `YOUR_TURN` when a client recieves this message, the server expects this player to take their turn.
- `OPPONENT_TURN` when a client recieves this message, the server is not listening to this player.
- `FIRE <x y>` is sent by the client to shoot at a cell (x, y).
- `RESULT <result>` sends one of the following: "Hit", "Miss", "Sunk", "AlreadyShot" to the current player.
- `OPPONENT_RESULT <x y result>` sends the coordinates with the result of the current player's shot to the opponent.
- `GAME_OVER <win?>` alerts clients that the game has ended, and whether they won or lost.

## Architecture Overview
This project uses a strict client–server architecture.

- The server is authoritative and owns all game state
- Clients never see opponent ship locations
- All actions are validated server-side
- The protocol is text-based over TCP

This design prevents cheating and desynchronization.

## Failure Handling

- Invalid commands are rejected by the server
- Out-of-turn actions are ignored
- If a client disconnects, the opponent wins by forfeit

## Enjoy the game!
~ Sam Tooley Jr.

