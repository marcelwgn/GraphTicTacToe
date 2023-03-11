using Microsoft.Graph;
using Microsoft.UI.Xaml;
using GraphTicTacToe.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GraphTicTacToe.ViewModel
{
	public class ContactResult
	{
		public string Initials { get; init; }
		public string DisplayName { get; init; }
		public string Email { get; init; }
		public Contact RawContact { get; init; }
	}

	public class Graph : DependencyObject
	{
		public static Graph Instance { get; } = new Graph();

		public static string CurrentUser = null;


		public bool IsSignedIn
		{
			get { return (bool)GetValue(IsSignedInProperty); }
			set { SetValue(IsSignedInProperty, value); }
		}
		public static readonly DependencyProperty IsSignedInProperty =
			DependencyProperty.Register("IsSignedIn", typeof(bool), typeof(Graph), new PropertyMetadata(false));

		private GraphAPI GraphAPI { get; } = new GraphAPI();

		public static async Task<bool> SignIn()
		{
			CurrentUser = (await Instance.GraphAPI.Initialize()).UserPrincipalName;
			Instance.IsSignedIn = CurrentUser != null;
			return Instance.IsSignedIn;
		}

		public static void SignOut()
		{
			CurrentUser = "";
			Instance.GraphAPI.Clear();
			Instance.IsSignedIn = false;
		}

		public async static Task<IEnumerable<ContactResult>> GetContacts()
		{
			return (await Instance.GraphAPI.GetContacts()).Where(x => !string.IsNullOrEmpty(x.DisplayName) && x.EmailAddresses.Any()).Select(contact =>
			{
				return new ContactResult()
				{
					DisplayName = contact.DisplayName,
					Email = contact.EmailAddresses.ElementAt(0).Address,
					Initials = GetFirstCharacterSave(contact.GivenName) + GetFirstCharacterSave(contact.Surname),
					RawContact = contact,
				};
			});
		}

		public static async Task<GameState> InviteUserToGameFile(string gameName, string emailAddress)
		{
			var file = await Instance.GraphAPI.CreateFile(gameName);
			var gameState = new GameState(file);
			await Instance.GraphAPI.WriteToFile(file.Name, gameState.ToJsonString());
			await Instance.GraphAPI.ShareFileWithEmail(file.Name, emailAddress);
			return gameState;
		}

		private static string GetFirstCharacterSave(string value)
		{
			return value.Length > 0 ? value[0].ToString() : "";
		}

		public static async Task<List<DriveItem>> GetGames()
		{
			var result = await Task.WhenAll(new Task<List<DriveItem>>[]
			{
				Instance.GraphAPI.GetOwnGameFiles(),
				Instance.GraphAPI.GetSharedGameFiles()
			});
			result[0].AddRange(result[1]);
			return result[0];
		}

		public static async Task<string> GetContent(DriveItem item)
		{
			return await Instance.GraphAPI.GetContent(item);
		}

		public async static Task<bool> WriteFile(DriveItem item, string value)
		{
			await Instance.GraphAPI.WriteToFile(item, value);
			return true;
		}
	}
}
