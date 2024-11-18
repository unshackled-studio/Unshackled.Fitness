﻿using Unshackled.Studio.Core.Client.Features;

namespace Unshackled.Food.My.Client.Features.Recipes.Models;

public class FormBulkAddStepModel
{
	public string RecipeSid { get; set; } = string.Empty;
	public string BulkText { get; set; } = string.Empty;

	public class Validator : BaseValidator<FormBulkAddStepModel>
	{
		public Validator()
		{
		}
	}
}