﻿using System.Text.Json.Serialization;

namespace Unshackled.Food.My.Client.Features.ProductBundles.Models;

public class FormProductModel
{
	public string ProductBundleSid { get; set; } = string.Empty;
	public string ProductSid { get; set; } = string.Empty;
	public string? Brand { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public int Quantity { get; set; }

	[JsonIgnore]
	public bool IsEditing { get; set; } = false;

	[JsonIgnore]
	public bool IsSaving { get; set; } = false;
}
