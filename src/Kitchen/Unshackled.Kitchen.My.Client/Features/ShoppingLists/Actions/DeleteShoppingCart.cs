﻿using MediatR;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Kitchen.My.Client.Features.ShoppingLists.Actions;

public class DeleteShoppingCart
{
	public class Command : IRequest<CommandResult>
	{
		public string ShoppingListSid { get; private set; }

		public Command(string shoppingListSid)
		{
			ShoppingListSid = shoppingListSid;
		}
	}

	public class Handler : BaseShoppingListHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			return await PostToCommandResultAsync($"{baseUrl}delete-cart", request.ShoppingListSid)
				?? new CommandResult(false, Globals.UnexpectedError);
		}
	}
}