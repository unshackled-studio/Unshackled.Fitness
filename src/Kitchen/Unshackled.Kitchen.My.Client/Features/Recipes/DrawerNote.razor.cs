using Microsoft.AspNetCore.Components;
using Unshackled.Kitchen.My.Client.Features.Recipes.Models;
using Unshackled.Studio.Core.Client.Components;

namespace Unshackled.Kitchen.My.Client.Features.Recipes;

public class DrawerNoteBase : BaseFormComponent<FormNoteModel, FormNoteModel.Validator>
{
	[Parameter] public bool IsAdding { get; set; }
	[Parameter] public EventCallback OnDeleted { get; set; }

	protected async Task HandleDeleteClicked()
	{
		await OnDeleted.InvokeAsync();
	}
}