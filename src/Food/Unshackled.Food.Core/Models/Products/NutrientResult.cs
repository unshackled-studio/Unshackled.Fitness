﻿using Unshackled.Food.Core.Enums;

namespace Unshackled.Food.Core.Models.Products;

public class NutrientResult
{
	public decimal Amount { get; set; }
	public decimal AmountN { get; set; }
	public NutritionUnits Unit { get; set; }
}