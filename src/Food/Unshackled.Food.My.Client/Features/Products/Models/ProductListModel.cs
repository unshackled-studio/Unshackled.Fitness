﻿using Unshackled.Food.Core.Models;

namespace Unshackled.Food.My.Client.Features.Products.Models;

public class ProductListModel : BaseHouseholdObject
{
	public Guid? MatchId { get; set; }
	public string? Brand { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? Category { get; set; }
	public bool HasNutritionInfo { get; set; }
}
