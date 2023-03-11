using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphTicTacToe.Model
{
	public class GraphAPI
	{
		const string GameFilePrefix = "GraphTicTacToeGameFile";
		const string GameFolderName = "GraphTicTacToe";
		private string GameFolderId = "";

		public static readonly string[] _scopes = new[] { "User.Read", "Files.ReadWrite.AppFolder", "Files.ReadWrite.All", "Contacts.Read" };
		private string ClientId = "d2f0a522-d572-4628-87d5-8e1395f5715c";
		private string TenantId = "common";
		private GraphServiceClient _client;
		private IDriveItemRequestBuilder DefaultFolder => _client.Me.Drive.Items[GameFolderId];

		public async Task<User> Initialize()
		{
			var options = new InteractiveBrowserCredentialOptions
			{
				ClientId = ClientId,
				TenantId = TenantId,
				AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
				RedirectUri = new Uri("https://login.microsoftonline.com/common/oauth2/nativeclient"),
			};

			var interactiveCredential = new InteractiveBrowserCredential(options);

			var context = new TokenRequestContext(_scopes);
			interactiveCredential.GetToken(context);

			_client = new GraphServiceClient(interactiveCredential);

			var driveItem = new DriveItem
			{
				Name = GameFolderName,
				Folder = new Folder
				{
				},
				AdditionalData = new Dictionary<string, object>()
				{
					{ "@microsoft.graph.conflictBehavior", "replace" }
				}
			};
			await _client.Me.Drive.Root.Children.Request().AddAsync(driveItem);
			GameFolderId = (await _client.Me.Drive.Root.ItemWithPath(GameFolderName).Request().GetAsync()).Id;
			return await _client.Me.Request().GetAsync();
		}

		public void Clear()
		{
			this._client = null;
		}

		public async Task<Contact[]> GetContacts()
		{
			var data = new List<Contact>();
			var result = await _client.Me.Contacts.Request().GetAsync();
			while (result.NextPageRequest != null)
			{
				data.AddRange(result.CurrentPage);
				result = await result.NextPageRequest.GetAsync();
			}
			return data.ToArray();
		}

		public async Task<DriveItem> CreateFile(string name)
		{
			var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(""));
			var file = await DefaultFolder.ItemWithPath(GameFilePrefix + "_" + name + "_" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "_game.txt").Content.Request().PutAsync<DriveItem>(stream);
			return file;
		}

		public async Task<bool> WriteToFile(string name, string content)
		{
			var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
			await DefaultFolder.ItemWithPath(name).Content.Request().PutAsync<DriveItem>(stream);
			return true;
		}

		public async Task<bool> WriteToFile(DriveItem item, string content)
		{
			var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
			var file = await GetFileRequestBuilder(item).Content.Request().PutAsync<DriveItem>(stream);
			return true;
		}

		public async Task<bool> ShareFileWithEmail(string name, string email)
		{
			try
			{
				var recipients = new List<DriveRecipient>()
				{
					new DriveRecipient()
					{
						Email = email
					}
				};
				var request = await DefaultFolder.ItemWithPath(name).Invite(recipients, true, new string[] { "read", "write" }, false, "Game session access").Request().PostAsync();
				return request.Count > 0;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task<bool> CheckIfFileExists(string name)
		{
			var result = await DefaultFolder.Search(name).Request().GetAsync();
			return result.Count > 0;
		}

		public async Task<List<DriveItem>> GetOwnGameFiles()
		{
			var items = new List<DriveItem>();
			var result = await DefaultFolder.Children.Request().GetAsync();
			items.AddRange(result.CurrentPage);
			while (result.NextPageRequest != null)
			{
				items.AddRange(result.CurrentPage);
				result = await result.NextPageRequest.GetAsync();
			}

			return items;
		}

		public async Task<List<DriveItem>> GetSharedGameFiles()
		{
			var items = new List<DriveItem>();
			var result = await _client.Me.Drive
				.SharedWithMe()
				.Request()
				.GetAsync();
			items.AddRange(result.CurrentPage.Where(x => x.Name.Contains(GameFilePrefix)));
			while (result.NextPageRequest != null)
			{
				items.AddRange(result.CurrentPage.Where(x => x.Name.Contains(GameFilePrefix)));
				result = await result.NextPageRequest.GetAsync();
			}

			return items;
		}

		public async Task<string> GetContent(DriveItem item)
		{
			var data = await GetFileRequestBuilder(item).Content.Request().GetAsync();
			var reader = new System.IO.StreamReader(data);
			return await reader.ReadToEndAsync();
		}

		private IDriveItemRequestBuilder GetFileRequestBuilder(DriveItem item)
		{
			if (item.RemoteItem != null)
			{
				return _client.Drives[item.RemoteItem.ParentReference.DriveId].Items[item.RemoteItem.Id];
			}
			return DefaultFolder.ItemWithPath(item.Name);
		}
	}
}
