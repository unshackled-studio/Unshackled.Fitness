﻿using MediatR;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Kitchen.My.Client.Features.Stores.Actions;

public class DeleteProductLocation
{
	public class Command : IRequest<CommandResult>
	{
		public string StoreSid { get; private set; }
		public string ProductSid { get; private set; }

		public Command(string storeSid, string productSid)
		{
			StoreSid = storeSid;
			ProductSid = productSid;
		}
	}

	public class Handler : BaseStoreHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			return await PostToCommandResultAsync($"{baseUrl}delete/{request.StoreSid}/product", request.ProductSid)
				?? new CommandResult(false, Globals.UnexpectedError);
		}
	}
}