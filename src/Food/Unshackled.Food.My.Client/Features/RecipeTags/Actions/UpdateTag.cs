﻿using MediatR;
using Unshackled.Food.My.Client.Features.RecipeTags.Models;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Food.My.Client.Features.RecipeTags.Actions;

public class UpdateTag
{
	public class Command : IRequest<CommandResult>
	{
		public FormTagModel Model { get; private set; }

		public Command(FormTagModel model)
		{
			Model = model;
		}
	}

	public class Handler : BaseRecipeTagHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			return await PostToCommandResultAsync($"{baseUrl}update", request.Model)
				?? new CommandResult(false, Globals.UnexpectedError);
		}
	}
}