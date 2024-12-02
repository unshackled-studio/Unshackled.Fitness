﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Food.Core;
using Unshackled.Food.Core.Data;
using Unshackled.Food.Core.Enums;
using Unshackled.Food.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Food.My.Features.Households.Actions;

public class DeleteHousehold
{
	public class Command : IRequest<CommandResult>
	{
		public long MemberId { get; private set; }
		public string HouseholdSid { get; private set; }

		public Command(long memberId, string householdSid)
		{
			MemberId = memberId;
			HouseholdSid = householdSid;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(FoodDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			long householdId = request.HouseholdSid.DecodeLong();

			if (householdId == 0)
				return new CommandResult(false, "Invalid household ID.");

			if (!await db.HasHouseholdPermission(householdId, request.MemberId, PermissionLevels.Admin))
				return new CommandResult(false, FoodGlobals.PermissionError);

			var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

			try
			{
				// if active household for any member, remove it
				await db.MemberMeta
					.Where(x => x.MetaKey == FoodGlobals.MetaKeys.ActiveHouseholdId && x.MetaValue == householdId.ToString())
					.DeleteFromQueryAsync(cancellationToken);

				await db.HouseholdInvites
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.HouseholdMembers
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.ShoppingListRecipeItems
					.Include(x => x.ShoppingList)
					.Where(x => x.ShoppingList.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.ShoppingListItems
					.Include(x => x.ShoppingList)
					.Where(x => x.ShoppingList.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.ShoppingLists
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.StoreProductLocations
					.Include(x => x.Store)
					.Where(x => x.Store.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.StoreLocations
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.Stores
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.ProductBundleItems
					.Include(x => x.ProductBundle)
					.Where(x => x.ProductBundle.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.ProductBundles
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.ProductSubstitutions
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.Products
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.ProductCategories
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.RecipeTags
					.Include(x => x.Recipe)
					.Where(x => x.Recipe.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.Tags
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.CookbookRecipes
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.RecipeNotes
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.RecipeStepIngredients
					.Include(x => x.RecipeStep)
					.Where(x => x.RecipeStep.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.RecipeSteps
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.RecipeIngredients
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.RecipeIngredientGroups
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.Recipes
					.Where(x => x.HouseholdId == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await db.Households
					.Where(x => x.Id == householdId)
					.DeleteFromQueryAsync(cancellationToken);

				await transaction.CommitAsync(cancellationToken);

				return new CommandResult(true, "The household has been deleted.");
			}
			catch
			{
				await transaction.RollbackAsync(cancellationToken);
				return new CommandResult(false, "The household could not be deleted.");
			}
		}
	}
}