﻿using Unshackled.Kitchen.Core.Models;

namespace Unshackled.Kitchen.My.Client.Features.Stores.Models;

public class StoreLocationModel : BaseHouseholdObject
{
	public string StoreSid { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;
	public List<FormProductLocationModel> ProductLocations { get; set; } = new();
}
