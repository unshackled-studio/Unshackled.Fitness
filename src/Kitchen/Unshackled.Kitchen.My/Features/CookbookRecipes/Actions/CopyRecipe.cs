﻿using AutoMapper;
using MediatR;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.My.Client.Features.CookbookRecipes.Models;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Kitchen.My.Features.CookbookRecipes.Actions;

public class CopyRecipe
{
	public class Command : IRequest<CommandResult<string>>
	{
		public long MemberId { get; private set; }
		public FormCopyRecipeModel Model { get; private set; }

		public Command(long memberId, FormCopyRecipeModel model)
		{
			MemberId = memberId;
			Model = model;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult<string>>
	{
		public Handler(KitchenDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult<string>> Handle(Command request, CancellationToken cancellationToken)
		{
			long householdId = request.Model.HouseholdSid.DecodeLong();
			long recipeId = request.Model.RecipeSid.DecodeLong();

			return await db.CopyRecipe(householdId, recipeId, request.MemberId, request.Model.Title, request.Model.TagKeys, cancellationToken);
		}
	}
}