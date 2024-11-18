﻿using MediatR;
using Unshackled.Food.My.Client.Features.Products.Models;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Food.My.Client.Features.Products.Actions;

public class MergeProducts
{
	public class Command : IRequest<CommandResult>
	{
		public MergeModel Model { get; }

		public Command(MergeModel model)
		{
			Model = model;
		}
	}

	public class Handler : BaseProductHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			return await PostToCommandResultAsync($"{baseUrl}merge", request.Model) ??
				new CommandResult(false, Globals.UnexpectedError);
		}
	}
}