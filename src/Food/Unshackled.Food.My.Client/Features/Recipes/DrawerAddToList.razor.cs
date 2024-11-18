using Microsoft.AspNetCore.Components;
using Unshackled.Food.Core.Models.ShoppingLists;
using Unshackled.Food.My.Client.Features.Recipes.Actions;
using Unshackled.Food.My.Client.Features.Recipes.Models;
using Unshackled.Studio.Core.Client.Components;

namespace Unshackled.Food.My.Client.Features.Recipes;

public class DrawerAddToListBase : BaseComponent
{
	[Parameter] public RecipeModel Recipe { get; set; } = new();
	[Parameter] public decimal Scale { get; set; } = 1M;
	[Parameter] public EventCallback OnAddedComplete { get; set; }
	[Parameter] public EventCallback OnCancelClicked { get; set; }

	public bool IsLoading { get; set; } = true;
	public bool IsSelecting { get; set; } = true;
	protected bool IsWorking { get; set; } = false;
	protected List<ShoppingListModel> ShoppingLists { get; set; } = new();
	protected List<AddToListModel> Items { get; set; } = new();
	protected string SelectedShoppingListSid {  get; set; } = string.Empty;

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		ShoppingLists = await Mediator.Send(new ListShoppingLists.Query());
		IsLoading = false;
	}

	protected async Task HandleAddClicked(string sid)
	{
		IsWorking = true;

		SelectedShoppingListSid = sid;
		SelectListModel model = new()
		{
			ListSid = sid,
			RecipeSid = Recipe.Sid,
			Scale = Scale
		};

		Items = await Mediator.Send(new GetAddToListItems.Query(model));
		IsSelecting = false;
		IsWorking = false;
		StateHasChanged();
	}

	protected async Task HandleAddToListClicked()
	{
		IsWorking = true;
		var addedItems = Items.Where(x => x.Quantity > 0).ToList();
		AddRecipeToListModel model = new()
		{
			List = addedItems,
			RecipeSid = Recipe.Sid,
			RecipeTitle = Recipe.Title,
			ShoppingListSid = SelectedShoppingListSid
		};
		var result = await Mediator.Send(new AddToList.Command(model));
		ShowNotification(result);
		await OnAddedComplete.InvokeAsync();
		IsWorking = false;
	}

	protected async Task HandleCancelClicked()
	{
		await OnCancelClicked.InvokeAsync();
	}
}