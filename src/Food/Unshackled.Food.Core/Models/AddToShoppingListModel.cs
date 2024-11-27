﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace Unshackled.Food.Core.Models;

public class AddToShoppingListModel
{
	public string ListSid { get; set; } = string.Empty;
	public string IngredientKey { get; set; } = string.Empty;
	public string IngredientTitle { get; set; } = string.Empty;
	public string ProductSid { get; set; } = string.Empty;
	public string ProductTitle { get; set; } = string.Empty;
	public decimal RequiredAmount { get; set; }
	public string RequiredAmountLabel { get; set; } = string.Empty;
	public int Quantity { get; set; } = 1;
	public int QuantityInList { get; set; }
	public decimal PortionUsed { get; set; }
	public bool IsUnitMismatch { get; set; } = false;

	public List<RecipeAmountListModel> RecipeAmounts { get; set; } = [];

	[JsonIgnore]
	public int TotalQuantity => Quantity + QuantityInList;
}