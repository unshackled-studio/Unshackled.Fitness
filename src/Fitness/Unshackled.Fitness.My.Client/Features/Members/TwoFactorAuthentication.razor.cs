using MudBlazor;
using Unshackled.Fitness.Core.Models;
using Unshackled.Fitness.My.Client.Features.Members.Actions;
using Unshackled.Fitness.My.Client.Features.Members.Models;
using Unshackled.Studio.Core.Client.Components;

namespace Unshackled.Fitness.My.Client.Features.Members;

public class TwoFactorAuthenticationBase : BaseComponent<Member>
{
	protected bool IsLoading { get; set; } = true;
	protected bool IsWorking { get; set; }
	protected bool DisableControls => IsWorking;
	protected Current2faStatusModel CurrentStatus { get; set; } = new();

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		Breadcrumbs.Add(new BreadcrumbItem("Settings", "/member"));
		Breadcrumbs.Add(new BreadcrumbItem("Two-factor Authentication", null, true));

		CurrentStatus = await Mediator.Send(new GetCurrent2faStatus.Query());
		IsLoading = false;
	}
}