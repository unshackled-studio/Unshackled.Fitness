﻿using Microsoft.AspNetCore.Mvc;
using Unshackled.Kitchen.My.Features.ProductCategories.Actions;
using Unshackled.Kitchen.My.Client.Features.ProductCategories.Models;
using Unshackled.Studio.Core.Server.Features;

namespace Unshackled.Kitchen.My.Features.ProductCategories;

[ApiController]
[ApiRoute("product-categories")]
public class ProductCategoriesController : BaseController
{
	[HttpPost("add")]
	[ActiveMemberRequired]
	public async Task<IActionResult> Add([FromBody] FormCategoryModel model)
	{
		return Ok(await Mediator.Send(new AddCategory.Command(Member.Id, Member.ActiveHouseholdId, model)));
	}

	[HttpPost("delete")]
	[ActiveMemberRequired]
	public async Task<IActionResult> Delete([FromBody] string sid)
	{
		return Ok(await Mediator.Send(new DeleteCategory.Command(Member.Id, Member.ActiveHouseholdId, sid)));
	}

	[HttpGet("list")]
	public async Task<IActionResult> ListCategories()
	{
		return Ok(await Mediator.Send(new ListCategories.Query(Member.Id, Member.ActiveHouseholdId)));
	}

	[HttpPost("update")]
	[ActiveMemberRequired]
	public async Task<IActionResult> Update([FromBody] FormCategoryModel model)
	{
		return Ok(await Mediator.Send(new UpdateCategory.Command(Member.Id, Member.ActiveHouseholdId, model)));
	}
}
