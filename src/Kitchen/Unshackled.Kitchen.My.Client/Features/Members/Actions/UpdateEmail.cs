﻿using MediatR;
using Unshackled.Kitchen.My.Client.Features.Members.Models;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Kitchen.My.Client.Features.Members.Actions;

public class UpdateEmail
{
	public class Command : IRequest<CommandResult>
	{
		public FormChangeEmailModel Model { get; private set; }

		public Command(FormChangeEmailModel model)
		{
			Model = model;
		}
	}

	public class Handler : BaseMemberHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			return await PostToCommandResultAsync($"{baseUrl}update-email", request.Model) 
				?? new CommandResult(false, Globals.UnexpectedError);
		}
	}
}
