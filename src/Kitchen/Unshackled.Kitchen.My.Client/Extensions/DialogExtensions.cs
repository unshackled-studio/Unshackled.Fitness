﻿using MudBlazor;
using Unshackled.Kitchen.My.Client.Components;
using Unshackled.Studio.Core.Client.Services;

namespace Unshackled.Kitchen.My.Client.Extensions;

public static class DialogExtensions
{
	public static async Task OpenMakeItClicked(this IDialogService dialogService, IScreenWakeLockService screenLockService)
	{
		var options = new DialogOptions
		{
			BackgroundClass = "bg-blur",
			FullScreen = true,
			FullWidth = true,
			CloseButton = true
		};

		var dialog = await dialogService.ShowAsync<DialogMakeRecipe>("Make It", options);
		var result = await dialog.Result;

		// Make sure screen lock is released when dialog is closed.
		if (result != null && screenLockService.HasWakeLock())
		{
			await screenLockService.ReleaseWakeLock();
		}
	}
}
