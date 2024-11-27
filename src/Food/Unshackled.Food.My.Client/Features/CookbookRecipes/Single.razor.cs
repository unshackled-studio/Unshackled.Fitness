using Microsoft.AspNetCore.Components;
using MudBlazor;
using Unshackled.Food.Core.Enums;
using Unshackled.Food.Core.Models;
using Unshackled.Food.My.Client.Extensions;
using Unshackled.Food.My.Client.Features.CookbookRecipes.Actions;
using Unshackled.Food.My.Client.Features.CookbookRecipes.Models;
using Unshackled.Studio.Core.Client.Components;

namespace Unshackled.Food.My.Client.Features.CookbookRecipes;

public class SingleBase : BaseComponent<Member>, IAsyncDisposable
{
	protected enum Views
	{
		None,
		Copy
	}

	[Inject] protected IDialogService DialogService { get; set; } = default!;
	[Parameter] public string RecipeSid { get; set; } = string.Empty;
	protected bool IsLoading { get; set; } = true;
	protected RecipeModel Recipe { get; set; } = new();
	protected bool IsWorking { get; set; } = false;
	protected bool DisableControls => IsWorking;
	protected decimal Scale { get; set; } = 1M;
	protected bool CanDelete => ActiveMember.HasCookbookPermissionLevel(PermissionLevels.Admin) || Recipe.IsOwner;
	protected bool DrawerOpen => DrawerView != Views.None;
	protected Views DrawerView { get; set; } = Views.None;
	protected string DrawerTitle => DrawerView switch
	{
		Views.Copy => "Copy To...",
		_ => string.Empty
	};

	protected override async Task OnParametersSetAsync()
	{
		await base.OnParametersSetAsync();

		IsLoading = true;
		Recipe = await Mediator.Send(new GetRecipe.Query(RecipeSid));
		DrawerView = Views.None;
		IsLoading = false;
	}

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		Breadcrumbs.Add(new BreadcrumbItem("Cookbook Recipes", "/cookbook-recipes", false));
		Breadcrumbs.Add(new BreadcrumbItem("Recipe", null, true));

		State.OnActiveMemberChange += StateHasChanged;
	}

	public override ValueTask DisposeAsync()
	{
		State.OnActiveMemberChange -= StateHasChanged;
		return base.DisposeAsync();
	}

	protected void HandleCancelClicked()
	{
		DrawerView = Views.None;
	}

	protected void HandleCopyToClicked()
	{
		DrawerView = Views.Copy;
	}

	protected async Task HandleDeleteClicked()
	{
		bool? confirm = await DialogService.ShowMessageBox(
				"Confirm Removal",
				"Are you sure you want to remove this recipe from the cookbook?",
				yesText: "Remove", cancelText: "Cancel");

		if (confirm.HasValue && confirm.Value)
		{
			IsWorking = true;

			var result = await Mediator.Send(new DeleteRecipe.Command(Recipe.Sid));
			ShowNotification(result);
			if (result.Success)
			{
				NavManager.NavigateTo("/cookbook-recipes");
			}

			IsWorking = false;
		}
	}
}