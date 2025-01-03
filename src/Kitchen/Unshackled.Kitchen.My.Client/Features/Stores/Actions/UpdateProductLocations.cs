﻿using MediatR;
using Unshackled.Kitchen.My.Client.Features.Stores.Models;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Kitchen.My.Client.Features.Stores.Actions;

public class UpdateProductLocations
{
	public class Command : IRequest<CommandResult>
	{
		public string StoreLocationSid { get; private set; }
		public List<FormProductLocationModel> Locations { get; private set; }

		public Command(string storeLocationSid, List<FormProductLocationModel> locations)
		{
			StoreLocationSid = storeLocationSid;
			Locations = locations;
		}
	}

	public class Handler : BaseStoreHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			return await PostToCommandResultAsync($"{baseUrl}update/{request.StoreLocationSid}/products", request.Locations)
				?? new CommandResult(false, Globals.UnexpectedError);
		}
	}
}
