﻿namespace Unshackled.Food.My.Client.Features.ProductBundles.Models;

public class UpdateProductModel
{
	public string ProductBundleSid { get; set; } = string.Empty;
	public string ProductSid { get; set; } = string.Empty;
	public int Quantity { get; set; }
}
