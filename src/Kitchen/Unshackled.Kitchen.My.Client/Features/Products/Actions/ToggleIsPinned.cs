﻿using MediatR;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Kitchen.My.Client.Features.Products.Actions;

public class ToggleIsPinned
{
	public class Command : IRequest<CommandResult<bool>>
	{
		public string Sid { get; private set; }

		public Command(string sid)
		{
			Sid = sid;
		}
	}

	public class Handler : BaseProductHandler, IRequestHandler<Command, CommandResult<bool>>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<CommandResult<bool>> Handle(Command request, CancellationToken cancellationToken)
		{
			return await PostToCommandResultAsync<string, bool>($"{baseUrl}toggle/pinned", request.Sid) ??
				new CommandResult<bool>(false, Globals.UnexpectedError);
		}
	}
}
