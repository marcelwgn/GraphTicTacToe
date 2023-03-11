// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using GraphTicTacToe.ViewModel;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;

namespace GraphTicTacToe.View
{
	public sealed partial class InviteToGameControl : UserControl
	{

		public ObservableCollection<ContactResult> FilteredContacts = new();
		public List<ContactResult> RawContacts = new();

		public event TypedEventHandler<InviteToGameControl, ContactResult> InviteUserRequested;
		public event TypedEventHandler<InviteToGameControl, object> CancelRequested;

		public InviteToGameControl()
		{
			this.InitializeComponent();

			Graph.GetContacts().ContinueWith((t) =>
			{
				DispatcherQueue.TryEnqueue(() =>
				{
					foreach (var contact in t.Result)
					{
						RawContacts.Add(contact);
						FilteredContacts.Add(contact);
					}
				});
			});
		}

		private void SearchContacts_TextChanged(object sender, TextChangedEventArgs e)
		{
			DispatcherQueue.TryEnqueue(() =>
			{
				FilteredContacts.Clear();
				foreach (var contact in RawContacts)
				{
					if (contact.DisplayName.Contains(SearchContacts.Text) || contact.Email.Contains(SearchContacts.Text))
					{
						FilteredContacts.Add(contact);
					}
				}
			});
			InviteUserButton.IsEnabled = ContactsGrid.SelectedItem != null;
		}

		private void ContactsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			InviteUserButton.IsEnabled = ContactsGrid.SelectedItem != null;
		}

		private void InviteUserButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			InviteUserRequested.Invoke(this, ContactsGrid.SelectedItem as ContactResult);
		}

		private void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			CancelRequested.Invoke(this, null);
		}
	}
}
