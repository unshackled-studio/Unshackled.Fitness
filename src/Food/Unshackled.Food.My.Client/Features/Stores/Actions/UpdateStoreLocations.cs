﻿using MediatR;
using Unshackled.Food.My.Client.Features.Stores.Models;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Food.My.Client.Features.Stores.Actions;

public class UpdateStoreLocations
{
	public class Command : IRequest<CommandResult>
	{
		public string StoreSid { get; private set; }
		public UpdateLocationsModel Model { get; private set; }

		public Command(string storeSid, UpdateLocationsModel model)
		{
			StoreSid = storeSid;
			Model = model;
		}
	}

	public class Handler : BaseStoreHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			return await PostToCommandResultAsync($"{baseUrl}update/{request.StoreSid}/aisles", request.Model)
				?? new CommandResult(false, Globals.UnexpectedError);
		}
	}
}