using Microsoft.AspNetCore.Components;
using Unshackled.Food.My.Client.Features.ProductCategories.Models;
using Unshackled.Studio.Core.Client.Components;

namespace Unshackled.Food.My.Client.Features.ProductCategories;

public class DrawerCategoryBase : BaseFormComponent<FormCategoryModel, FormCategoryModel.Validator>
{
	[Parameter] public bool IsAdding { get; set; }
	[Parameter] public EventCallback OnDeleted { get; set; }

	protected async Task HandleDeleteClicked()
	{
		await OnDeleted.InvokeAsync();
	}
}