// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GraphTicTacToe.ViewModel;

namespace GraphTicTacToe.View
{
	public sealed partial class SignInPage : Page
	{
		public SignInPage()
		{
			this.InitializeComponent();
		}

		private async void SignInButton_Click(object sender, RoutedEventArgs e)
		{
			await Graph.SignIn();
		}
	}
}
