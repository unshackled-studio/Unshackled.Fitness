﻿using MediatR;
using Unshackled.Kitchen.My.Client.Features.Ingredients.Models;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Kitchen.My.Client.Features.Ingredients.Actions;

public class DeleteSubstitution
{
	public class Command : IRequest<CommandResult<string>>
	{
		public FormSubstitutionModel Model { get; private set; }

		public Command(FormSubstitutionModel model)
		{
			Model = model;
		}
	}

	public class Handler : BaseIngredientHandler, IRequestHandler<Command, CommandResult<string>>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<CommandResult<string>> Handle(Command request, CancellationToken cancellationToken)
		{
			return await PostToCommandResultAsync<FormSubstitutionModel, string>($"{baseUrl}delete-substitution", request.Model)
				?? new CommandResult<string>(false, Globals.UnexpectedError);
		}
	}
}