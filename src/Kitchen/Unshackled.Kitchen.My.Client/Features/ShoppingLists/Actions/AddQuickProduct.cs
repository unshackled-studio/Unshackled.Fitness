﻿using MediatR;
using Unshackled.Kitchen.My.Client.Features.ShoppingLists.Models;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Kitchen.My.Client.Features.ShoppingLists.Actions;

public class AddQuickProduct
{
	public class Command : IRequest<CommandResult>
	{
		public AddQuickProductModel Model { get; private set; }

		public Command(AddQuickProductModel model)
		{
			Model = model;
		}
	}

	public class Handler : BaseShoppingListHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			return await PostToCommandResultAsync($"{baseUrl}add-quick-product", request.Model)
				?? new CommandResult(false, Globals.UnexpectedError);
		}
	}
}