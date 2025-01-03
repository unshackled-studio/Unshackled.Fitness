﻿using FluentValidation;
using Unshackled.Studio.Core.Client.Features;

namespace Unshackled.Kitchen.My.Client.Features.ProductCategories.Models;

public class FormCategoryModel
{
	public string Sid { get; set; } = string.Empty;
	public string HouseholdSid { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;

	public class Validator : BaseValidator<FormCategoryModel>
	{
		public Validator()
		{
			RuleFor(x => x.Title)
				.NotEmpty().WithMessage("Title is required.")
				.MaximumLength(255).WithMessage("Title must not exceed 255 characters.");
		}
	}
}
