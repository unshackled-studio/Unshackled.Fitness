﻿using Unshackled.Studio.Core.Client.Features;

namespace Unshackled.Food.My.Client.Features.Households.Actions;

public abstract class BaseHouseholdHandler : BaseHandler
{
	public BaseHouseholdHandler(HttpClient httpClient) : base(httpClient, "households") { }
}
