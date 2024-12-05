﻿using MediatR;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Kitchen.My.Client.Features.Cookbooks.Actions;

public class RejectInvite
{
	public class Command : IRequest<CommandResult>
	{
		public string CookbookSid { get; private set; }

		public Command(string cookbookSid)
		{
			CookbookSid = cookbookSid;
		}
	}

	public class Handler : BaseCookbookHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			return await PostToCommandResultAsync($"{baseUrl}reject-invite", request.CookbookSid)
				?? new CommandResult(false, Globals.UnexpectedError);
		}
	}
}