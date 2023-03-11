using Microsoft.Graph;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GraphTicTacToe.ViewModel
{

	public enum GameStateEnum
	{
		X = 1,
		O = 2,
		Empty = 0
	}

	public class GameState
	{
		public GameStateEnum[][] Field { get; set; } = new GameStateEnum[3][];
		public string LastPlayer { get; set; } = null;
		public string WinningPlayer { get; set; } = null;
		public string StartPlayer { get; set; } = Graph.CurrentUser;

		[JsonIgnore]
		public DriveItem Item { get; set; }

		[JsonIgnore]
		public string GameName { get; set; }

		[JsonIgnore]
		public string Id => Item.Id;

		[JsonIgnore]
		public bool ItsOwnTurn => LastPlayer != Graph.CurrentUser;

		[JsonIgnore]
		public bool HasPlayerWon => WinningPlayer == Graph.CurrentUser;

		public GameState()
		{
			for (int i = 0; i < Field.Length; i++)
			{
				Field[i] = new GameStateEnum[3];
			}
		}

		public GameState(DriveItem item)
		{
			for (int i = 0; i < Field.Length; i++)
			{
				Field[i] = new GameStateEnum[3];
			}
			this.Item = item;
			try
			{
				this.GameName = item.Name.Split("_")[1];
			}
			catch (Exception)
			{
				this.GameName = "Unknown";
			}
		}

		public async void UpdateState(int xPos, int yPos)
		{
			LastPlayer = Graph.CurrentUser;
			var token = StartPlayer == Graph.CurrentUser ? GameStateEnum.X : GameStateEnum.O;

			// Set checked position
			if (Field[xPos][yPos] == GameStateEnum.Empty)
			{
				Field[xPos][yPos] = token;
			}

			// Check winning state
			if (GameHasEnded())
			{
				WinningPlayer = Graph.CurrentUser;
			}

			// Check if draw
			if (!Field.SelectMany(x => x).Contains(GameStateEnum.Empty))
			{
				WinningPlayer = "Draw";
			}

			// Write file to graph
			await Graph.WriteFile(this.Item, this.ToJsonString());
		}

		public bool GameHasEnded()
		{
			// Rows
			if (Field.Any(x => x.All(y => y == GameStateEnum.X) || x.All(y => y == GameStateEnum.O)))
			{
				return true;
			}

			// Columns
			for (int i = 0; i < 3; i++)
			{
				if (Field.All(x => x[i] == GameStateEnum.X) || Field.All(x => x[i] == GameStateEnum.O))
				{
					return true;
				}
			}

			// Diag Top Left
			var center = Field[1][1];
			var leftDiag = Field[0][0] == center && Field[2][2] == center && center != GameStateEnum.Empty;
			var rightDiag = Field[2][0] == center && Field[0][2] == center && center != GameStateEnum.Empty;

			return leftDiag || rightDiag;
		}

		public string ToJsonString()
		{
			return JsonSerializer.Serialize(this);
		}

		public async static Task<GameState> CreateFromDriveItem(DriveItem item)
		{
			var content = await Graph.GetContent(item);
			var state = JsonSerializer.Deserialize<GameState>(content);
			state.Item = item;
			return state;
		}
	}
}
