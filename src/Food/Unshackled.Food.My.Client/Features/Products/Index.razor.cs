using Microsoft.AspNetCore.Components;
using MudBlazor;
using Unshackled.Food.Core;
using Unshackled.Food.My.Client.Features.Products.Actions;
using Unshackled.Food.My.Client.Features.Products.Models;
using Unshackled.Studio.Core.Client.Components;
using Unshackled.Studio.Core.Client.Configuration;

namespace Unshackled.Food.My.Client.Features.Products;

public class IndexBase : BaseSearchComponent<SearchProductModel, ProductListModel>
{
	[Inject] protected ClientConfiguration ClientConfig { get; set; } = default!;
	[Inject] protected IDialogService DialogService { get; set; } = default!;

	protected enum Views
	{
		None,
		Add,
		AddToList
	}

	protected const string FormId = "formAddProduct";
	public FormProductModel FormModel { get; set; } = new();
	protected List<ShoppingListModel> ShoppingLists { get; set; } = [];
	public bool IsSaving { get; set; } = false;
	protected bool MaxSelectionReached => SelectedSids.Count == FoodGlobals.MaxSelectionLimit;
	protected bool MergeAvailable => SelectedSids.Count == 2;
	protected List<string> SelectedSids { get; set; } = new();
	protected bool? SelectAll { get; set; } = false;
	protected bool IsBulkArchive { get; set; } = true;
	protected bool DrawerOpen => DrawerView != Views.None;
	protected Views DrawerView { get; set; } = Views.None;

	protected string DrawerTitle => DrawerView switch
	{
		Views.Add => "Add Product",
		Views.AddToList => "Add To List",
		_ => string.Empty
	};

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		Breadcrumbs.Add(new BreadcrumbItem("Products", null, true));

		SearchKey = "searchProducts";
		SearchModel = await GetLocalSetting(SearchKey) ?? new();

		await DoSearch(SearchModel.Page);

		ShoppingLists = await Mediator.Send(new ListShoppingLists.Query());
	}

	private void ClearSelected()
	{
		SelectedSids.Clear();
		SelectAll = false;
	}

	public bool DisableCheckbox(string sid)
	{
		return DisableControls
			|| (!SelectedSids.Contains(sid) && MaxSelectionReached);
	}

	protected override async Task DoSearch(int page)
	{
		SearchModel.Page = page;

		IsLoading = true;
		await SaveLocalSetting(SearchKey, SearchModel);
		SearchResults = await Mediator.Send(new SearchProducts.Query(SearchModel));
		IsBulkArchive = !SearchModel.IsArchived;
		IsLoading = false;
	}

	protected void HandleAddClicked()
	{
		FormModel = new();
		DrawerView = Views.Add;
	}

	protected void HandleAddToListClicked()
	{
		FormModel = new();
		DrawerView = Views.AddToList;
	}

	protected async void HandleAddToListSubmitted(AddToListModel model)
	{
		IsWorking = true;
		model.ProductSids = SelectedSids;
		var result = await Mediator.Send(new AddToList.Command(model));
		DrawerView = Views.None;
		ShowNotification(result);
		if (result.Success)
			ClearSelected();
		IsWorking = false;
		StateHasChanged();
	}

	protected async Task HandleBulkSetArchivedClicked()
	{
		if (SelectedSids.Any())
		{
			string action = IsBulkArchive ? "archive" : "restore";
			bool? confirm = await DialogService.ShowMessageBox(
					"Warning",
					$"Are you sure you want to {action} the selected products?",
					yesText: "Yes", cancelText: "Cancel");

			if (confirm.HasValue && confirm.Value)
			{
				IsWorking = true;
				BulkArchiveModel model = new()
				{
					IsArchiving = IsBulkArchive,
					ProductSids = SelectedSids
				};
				var result = await Mediator.Send(new BulkArchiveRestore.Command(model));
				if (result.Success)
				{
					ClearSelected();
					await DoSearch(SearchModel.Page);
				}
				ShowNotification(result);
				IsWorking = false;
			}
		}
	}

	protected void HandleCancelClicked()
	{
		DrawerView = Views.None;
	}

	protected void HandleCheckboxChanged(bool value, string sid)
	{
		if (value)
			SelectedSids.Add(sid);
		else
			SelectedSids.Remove(sid);

		if (MaxSelectionReached)
		{
			SelectAll = true;
		}
		else if (!SelectedSids.Any())
		{
			SelectAll = false;
		}
		else
		{
			SelectAll = null;
		}
	}

	protected async Task HandleFormAddSubmit(FormProductModel model)
	{
		IsSaving = true;
		var result = await Mediator.Send(new AddProduct.Command(model));
		ShowNotification(result);
		if (result.Success)
		{
			NavManager.NavigateTo($"/products/{result.Payload}");
		}
		DrawerView = Views.None;
		IsSaving = false;
	}

	protected async Task HandleMergeClicked()
	{
		if (MergeAvailable)
		{
			DialogOptions options = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

			var parameters = new DialogParameters();
			parameters.Add(DialogMerge.ParameterSids, SelectedSids);

			var dialog = DialogService.Show<DialogMerge>("Merge Products", parameters, options);
			DialogResult? confirm = await dialog.Result;

			if (confirm != null && !confirm.Canceled && confirm.Data != null)
			{
				IsWorking = true;
				string? keepId = confirm.Data.ToString();
				if (!string.IsNullOrEmpty(keepId))
				{
					string deleteId = SelectedSids
						.Where(x => x != keepId)
						.First();

					MergeModel merge = new()
					{
						KeptSid = keepId,
						DeletedSid = deleteId
					};

					var result = await Mediator.Send(new MergeProducts.Command(merge));
					if(result.Success)
					{
						await DoSearch(SearchModel.Page);
					}
					ClearSelected();
					ShowNotification(result);
				}
				IsWorking = false;
			}
		}
	}

	protected void HandleSelectAllNoneChanged(bool? value)
	{
		if (SelectAll == true) // Make false and clear
		{
			SelectedSids.Clear();
			SelectAll = false;
		}
		else if (SelectAll == false || SelectAll == null) // Make true and select all
		{
			SelectedSids = SearchResults.Data.Select(x => x.Sid).ToList();
			SelectAll = true;
		}
	}
}