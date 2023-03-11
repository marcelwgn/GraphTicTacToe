using Microsoft.UI.Xaml.Controls;
using GraphTicTacToe.ViewModel;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.UI.Xaml;
using System.Linq;

namespace GraphTicTacToe.View
{
	public sealed partial class HomePage : Page
	{
		public ObservableCollection<GameState> GameFiles = new();

		public GameState OpenGame
		{
			get { return (GameState)GetValue(OpenGameProperty); }
			set { SetValue(OpenGameProperty, value); }
		}
		public static readonly DependencyProperty OpenGameProperty =
			DependencyProperty.Register("OpenGame", typeof(GameState), typeof(GameState), new PropertyMetadata(null));


		public Thread SyncerThread;
		public bool IsSyncing = false;

		public HomePage()
		{
			this.InitializeComponent();
			RefreshList();

			SyncerThread = new Thread(() =>
			{
				try
				{
					while (IsSyncing)
					{
						RefreshList();
						Thread.Sleep(10_000);
					}
				}
				catch (ThreadInterruptedException) { }
			});
			IsSyncing = true;
			SyncerThread.Start();

			Unloaded += (s, e) =>
			{
				IsSyncing = false;
			};
		}

		private void CreateNewGame_Click(object sender, RoutedEventArgs e)
		{
			var contentDialog = new ContentDialog()
			{
				XamlRoot = this.XamlRoot,
			};
			var control = new InviteToGameControl();
			control.InviteUserRequested += (s, e) =>
			{
				InviteUserRequested(e);
				contentDialog.Hide();
				contentDialog = null;
			};
			control.CancelRequested += (s, e) =>
			{
				contentDialog.Hide();
				contentDialog = null;
			};
			contentDialog.Content = control;

			var _ = contentDialog.ShowAsync();
		}

		private void InviteUserRequested(ContactResult contact)
		{
			Graph.InviteUserToGameFile(contact.DisplayName, contact.Email).ContinueWith((result) =>
			{
				DispatcherQueue.TryEnqueue(() =>
				{
					GameFiles.Add(result.Result);
				});
			});
		}

		private void RefreshList()
		{
			Graph.GetGames().ContinueWith((t) =>
			{
				DispatcherQueue.TryEnqueue(async () =>
				{
					if (OpenGame != null)
					{
						var item = t.Result.Where(x => x.Id == OpenGame.Id).First();
						OpenGame = await GameState.CreateFromDriveItem(item);
					}
					
					if (t.Result.Count != GameFiles.Count)
					{
						var selectedItem = GameList.SelectedItem;
						GameFiles.Clear();
						foreach (var item in t.Result)
						{
							GameFiles.Add(new GameState(item));
						}
						GameList.SelectedItem = selectedItem;
					}
				});
			});
		}

		private async void GameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(GameList.SelectedItem != null)
			{
				OpenGame = await GameState.CreateFromDriveItem((GameList.SelectedItem as GameState).Item);
			}
		}

		private void SignOutButton_Click(object sender, RoutedEventArgs e)
		{
			IsSyncing = false;
			SyncerThread.Interrupt();
			while (SyncerThread.IsAlive)
			{
				// Wait
			}
			Graph.SignOut();
		}
	}
}
