﻿using FluentValidation;
using Unshackled.Studio.Core.Client.Features;

namespace Unshackled.Food.My.Client.Features.Recipes.Models;

public class FormCopyRecipeModel
{
	public string RecipeSid { get; set; } = string.Empty;
	public string HouseholdSid { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;

	public class Validator : BaseValidator<FormCopyRecipeModel>
	{
		public Validator()
		{
			RuleFor(x => x.RecipeSid)
				.NotEmpty().WithMessage("Recipe is required.");

			RuleFor(x => x.HouseholdSid)
				.NotEmpty().WithMessage("Household is required.");

			RuleFor(x => x.Title)
				.NotEmpty().WithMessage("Title is required.");
		}
	}
}
