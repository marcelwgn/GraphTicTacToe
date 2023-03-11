using GraphTicTacToe.ViewModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;

namespace GraphTicTacToe.View
{
	public sealed partial class GameView : UserControl
	{

		public GameState State
		{
			get { return (GameState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty =
			DependencyProperty.Register("State", typeof(GameState), typeof(GameView), new PropertyMetadata(null, new PropertyChangedCallback(PropertyChangedHandler)));

		public GameView()
		{
			this.InitializeComponent();
			VisualStateManager.GoToState(this, "NoGame", true);
		}

		private void UpdateGameField()
		{
			if (State == null)
			{
				VisualStateManager.GoToState(this, "NoGame", true);
				return;
			}

			foreach (var element in RunningGameView.Children)
			{
				if (element is Button button)
				{
					if (GetPosition(element) is (int, int) position)
					{
						UpdateButtonToState(button, position);
					}
				}
			}

			if (State.WinningPlayer == "Draw")
			{
				VisualStateManager.GoToState(this, "Draw", true);
				return;
			}

			if(State.WinningPlayer != null)
			{
				VisualStateManager.GoToState(this, "GameEnded", true);
				if (State.HasPlayerWon)
				{
					WinningText.Text = "You won!";
				}
				else
				{
					WinningText.Text = "You lost!";
				}
				return;
			}

			VisualStateManager.GoToState(this, "RunningGame", true);

		}

		private void UpdateButtonToState(Button button, (int,int) position)
		{
			var statePosition = State.Field[position.Item1][position.Item2];
			if (statePosition != GameStateEnum.Empty)
			{
				button.Content = statePosition.ToString();
				button.IsEnabled = false;
			}
			else
			{
				button.IsEnabled = State.ItsOwnTurn;
			}
		}

		public static void PropertyChangedHandler(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is GameView gameView)
			{
				if (e.Property == StateProperty)
				{
					gameView.UpdateGameField();
				}
			}
		}

		private void Cell_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (GetPosition(sender) is (int, int) position)
				{
					State.UpdateState(position.Item1, position.Item2);
					UpdateGameField();
				}
			}
			catch (Exception) { }
		}

		private static (int, int)? GetPosition(object element)
		{
			if (element is Button button)
			{
				var buttonPosition = button.Name.Split("_")[1..].Select(x => int.Parse(x)).ToArray();
				return new(buttonPosition[0], buttonPosition[1]);
			}
			return null;
		}
	}
}
