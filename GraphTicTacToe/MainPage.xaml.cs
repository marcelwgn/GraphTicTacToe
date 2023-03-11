// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Identity.Client;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GraphTicTacToe.Model;
using GraphTicTacToe.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GraphTicTacToe
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
			NavigationFrame.Navigate(typeof(View.SignInPage));

			Graph.Instance.RegisterPropertyChangedCallback(Graph.IsSignedInProperty, (s, e) =>
			{
				if (Graph.Instance.IsSignedIn)
				{
					NavigationFrame.Navigate(typeof(View.HomePage));
				}
				else
				{
					NavigationFrame.Navigate(typeof(View.SignInPage), null);
				}
			});
		}
	}
}
