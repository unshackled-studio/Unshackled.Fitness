﻿using FluentValidation;
using Unshackled.Studio.Core.Client.Features;

namespace Unshackled.Kitchen.My.Client.Features.Stores.Models;

public class FormBulkAddLocationModel
{
	public string StoreSid { get; set; } = string.Empty;
	public string? Prefix { get; set; }
	public int NumberToAdd { get; set; } = 1;
	public bool SortDescending { get; set; } = false;

	public class Validator : BaseValidator<FormBulkAddLocationModel>
	{
		public Validator()
		{
			RuleFor(x => x.StoreSid)
				.NotEmpty().WithMessage("Invalid Store ID");
		}
	}
}
