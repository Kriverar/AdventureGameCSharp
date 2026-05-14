using System;
using System.IO;
using System.Linq;
namespace AdventureGame;

public class AdventureGame
{
	public readonly string GO_NORTH = "W";
	public readonly string GO_SOUTH = "S";
	public readonly string GO_EAST = "D";
	public readonly string GO_WEST = "A";
	public readonly string GET_LAMP = "L";
	public readonly string GET_KEY = "K";
	public readonly string OPEN_CHEST = "O";
	public readonly string QUIT = "Q";

	private Adventurer adventurer;
	private Room[,] dungeon;
	private int aRow;
	private int aCol;
	private bool isChestOpen;
	private bool hasPlayerQuit;
	private bool isAdventureAlive;
	private string lastDirection;

	private int exitRow;
	private int exitCol;

	private int lampRow;
	private int lampCol;

	private int keyRow;
	private int keyCol;

	private int chestRow;
	private int chestCol;

	private int grueRow;
	private int grueCol;

	public AdventureGame()
	{

	}

	public void Start()
	{
		Init();

		ShowGameStartScreen();

		string input;

		do
		{
			ShowScene();

			do
			{
				ShowInputOptions();

				input = GetInput();
			}
			while(!IsValidInput(input));

			ProcessInput(input);

			UpdateGameState();
		}
		while(!IsGameOver());

		ShowGameOverScreen();
	}

	private void Init()
	{
		adventurer = new Adventurer();

		LoadDungeonFromFile("dungeon.txt");

		isChestOpen = false;
		hasPlayerQuit = false;
		isAdventureAlive = true;

		lastDirection = string.Empty;
	}

	private void LoadDungeonFromFile(string fileName)
	{
	 	string path = fileName;

		if(!File.Exists(path))
		{
			path = Path.Combine(AppContext.BaseDirectory, fileName);
		}

		if(!File.Exists(path))
		{
			path = Path.Combine("src", "AdventureGame", fileName);
		}

		string[] lines = File.ReadAllLines(path);

		int rows = 0;
		int cols = 0;

		int currentRow = -1;
		int currentCol = -1;

	for(int i = 0; i < lines.Length; i++)
		{
			string line = lines[i].Trim();

			if(line == "" || line.StartsWith("#"))
			{
				continue;
			}

			if(line.StartsWith("ROWS="))
			{
				rows = int.Parse(line.Substring("ROWS=".Length));
			}
			else if(line.StartsWith("COLS="))
			{
				cols = int.Parse(line.Substring("COLS=".Length));

				dungeon = new Room[rows, cols];

				for(int r = 0; r < rows; r++)
				{
					for(int c = 0; c < cols; c++)
					{
						dungeon[r, c] = new Room();
					}
				}
			}
			else if(line.StartsWith("EXIT="))
			{
				int[] coordinates = ParseCoordinates(line.Substring("EXIT=".Length));
				exitRow = coordinates[0];
				exitCol = coordinates[1];
			}
			else if(line.StartsWith("ADVENTURER="))
			{
				int[] coordinates = ParseCoordinates(line.Substring("ADVENTURER=".Length));
				aRow = coordinates[0];
				aCol = coordinates[1];
			}
			else if(line.StartsWith("LAMP="))
			{
				int[] coordinates = ParseCoordinates(line.Substring("LAMP=".Length));
				lampRow = coordinates[0];
				lampCol = coordinates[1];
				dungeon[lampRow, lampCol].SetLamp(true);
			}
			else if(line.StartsWith("KEY="))
			{
				int[] coordinates = ParseCoordinates(line.Substring("KEY=".Length));
				keyRow = coordinates[0];
				keyCol = coordinates[1];
				dungeon[keyRow, keyCol].SetKey(true);
			}
			else if(line.StartsWith("CHEST="))
			{
				int[] coordinates = ParseCoordinates(line.Substring("CHEST=".Length));
				chestRow = coordinates[0];
				chestCol = coordinates[1];
				dungeon[chestRow, chestCol].SetChest(true);
			}
			else if(line.StartsWith("GRUE="))
			{
				int[] coordinates = ParseCoordinates(line.Substring("GRUE=".Length));
				grueRow = coordinates[0];
				grueCol = coordinates[1];
			}
			else if(line.StartsWith("ROOM "))
			{
				string coordinateText = line.Substring("ROOM ".Length);
				int[] coordinates = ParseCoordinates(coordinateText);

				currentRow = coordinates[0];
				currentCol = coordinates[1];
			}
			else if(line.StartsWith("LIT="))
			{
				bool value = bool.Parse(line.Substring("LIT=".Length));
				dungeon[currentRow, currentCol].SetLit(value);
			}
			else if(line.StartsWith("DESC="))
			{
				string value = line.Substring("DESC=".Length);
				dungeon[currentRow, currentCol].SetDescription(value);
			}
			else if(line.StartsWith("N="))
			{
				bool value = bool.Parse(line.Substring("N=".Length));
				dungeon[currentRow, currentCol].SetNorth(value);
			}
			else if(line.StartsWith("S="))
			{
				bool value = bool.Parse(line.Substring("S=".Length));
				dungeon[currentRow, currentCol].SetSouth(value);
			}
			else if(line.StartsWith("E="))
			{
				bool value = bool.Parse(line.Substring("E=".Length));
				dungeon[currentRow, currentCol].SetEast(value);
			}
			else if(line.StartsWith("W="))
			{
				bool value = bool.Parse(line.Substring("W=".Length));
				dungeon[currentRow, currentCol].SetWest(value);
			}
		}
	}

	private int[] ParseCoordinates(string text)
	{
		string[] parts = text.Split(',');

		int row = int.Parse(parts[0]);
		int col = int.Parse(parts[1]);

		return new int[] { row, col };
	}

	private void ShowGameStartScreen()
	{
		Console.WriteLine("Welcome to Adventure Game!");
	}

	private void ShowScene()
	{
		var r = dungeon[aRow, aCol];

		if(adventurer.HasLamp() || r.IsLit())
		{
			Console.WriteLine(r.GetDescription());
		}
		else
		{
			Console.WriteLine("This room is pitch black!");
		}
	}

	private void ShowInputOptions()
	{
		string options = ""
		+ $"GO NORTH [{GO_NORTH}] | GO EAST [{GO_EAST}] | GET LAMP [{GET_LAMP}] | OPEN CHEST [{OPEN_CHEST}]\n"
		+ $"GO SOUTH [{GO_SOUTH}] | GO WEST [{GO_WEST}] | GET KEY  [{GET_KEY}] | QUIT       [{QUIT}]\n"
		+ $"> ";

		Console.Write(options);
	}

	private string GetInput()
	{
		return Console.ReadLine()!.ToUpper();
	}

	private bool IsValidInput(string input)
	{
		string[] validInputs = { GO_NORTH, GO_SOUTH, GO_EAST, GO_WEST, GET_LAMP, GET_KEY, OPEN_CHEST, QUIT };

		if(!validInputs.Contains(input))
		{
			Console.WriteLine("ERROR: Invalid input. Please try again.");
			return false;
		}

		return true;
	}

	private void ProcessInput(string input)
	{
		Room r = dungeon[aRow, aCol];

		if(!adventurer.HasLamp() && !r.IsLit() && input != lastDirection)
		{
			Console.WriteLine("You got eaten alive by the Grue!");
			isAdventureAlive = false;
		}
		else if(input == GO_NORTH)
		{
			GoNorth(r);
		}
		else if(input == GO_SOUTH)
		{
			GoSouth(r);
		}
		else if(input == GO_EAST)
		{
			GoEast(r);
		}
		else if(input == GO_WEST)
		{
			GoWest(r);
		}
		else if(input == GET_LAMP)
		{
			GetLamp(r);
		}
		else if(input == GET_KEY)
		{
			GetKey(r);
		}
		else if(input == OPEN_CHEST)
		{
			OpenChest(r);
		}
		else// if(input == QUIT)
		{
			Quit();
		}
	}

	private void UpdateGameState()
	{
		if(isChestOpen && aRow == exitRow && aCol == exitCol)
		{
			Console.WriteLine("You found the dungeon exit after getting the treasure!");
			Console.WriteLine("You escaped the dungeon!");
		}
	}

	private bool IsGameOver()
	{
		bool isAtExit = aRow == exitRow && aCol == exitCol;

		return hasPlayerQuit || !isAdventureAlive || (isChestOpen && isAtExit);
	}

	private void ShowGameOverScreen()
	{
		Console.WriteLine("Game Over!");
	}

	private void GoNorth(Room r)
	{
		if(r.HasNorth())
		{
			aRow -= 1;
			lastDirection = GO_SOUTH;
		}
		else
		{
			Console.WriteLine("You cannot go north!\a");
		}
	}

	private void GoSouth(Room r)
	{
		if(r.HasSouth())
		{
			aRow += 1;
			lastDirection = GO_NORTH;
		}
		else
		{
			Console.WriteLine("You cannot go south!\a");
		}
	}

	private void GoEast(Room r)
	{
		if(r.HasEast())
		{
			aCol += 1;
			lastDirection = GO_WEST;
		}
		else
		{
			Console.WriteLine("You cannot go east!\a");
		}
	}

	private void GoWest(Room r)
	{
		if(r.HasWest())
		{
			aCol -= 1;
			lastDirection = GO_EAST;
		}
		else
		{
			Console.WriteLine("You cannot go west!\a");
		}
	}

	private void GetLamp(Room r)
	{
		if(r.HasLamp())
		{
			Console.WriteLine("You got the lamp!");
			adventurer.SetLamp(true);
			r.SetLamp(false);
		}
		else
		{
			Console.WriteLine("There is no lamp in this room.");
		}
	}

	private void GetKey(Room r)
	{
		if(r.HasKey())
		{
			Console.WriteLine("You got the key!");
			adventurer.SetKey(true);
			r.SetKey(false);
		}
		else
		{
			Console.WriteLine("There is no key in this room.");
		}
	}

	private void OpenChest(Room r)
	{
		if(r.HasChest())
		{
		if(adventurer.HasKey())
		{
			Console.WriteLine("You opened the chest and got the treasure!");
			Console.WriteLine("Now find your way to the dungeon exit!");
			isChestOpen = true;
		}
			else
			{
				Console.WriteLine("You do not have the key!");
			}
		}
		else
		{
			Console.WriteLine("There is no chest in this room.");
		}
	}

	private void Quit()
	{
		Console.WriteLine("You quit the game!");
		hasPlayerQuit = true;
	}
}
