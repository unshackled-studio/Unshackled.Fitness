﻿namespace Unshackled.Kitchen.My.Client.Features.Ingredients.Models;

public class ProductSubstitutionModel
{
	public string ProductSid { get; set; } = string.Empty;
	public string? Brand { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public bool IsPrimary { get; set; }
}
