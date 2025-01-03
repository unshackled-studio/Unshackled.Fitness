﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Kitchen.Core;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.Core.Data.Entities;
using Unshackled.Kitchen.Core.Enums;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Kitchen.My.Features.Products.Actions;

public class MergeProducts
{
	public class Command : IRequest<CommandResult>
	{
		public long MemberId { get; private set; }
		public long HouseholdId { get; private set; }
		public string KeptUId { get; private set; }
		public string DeletedUId { get; private set; }

		public Command(long memberId, long householdId, string keptSid, string deletedSid)
		{
			MemberId = memberId;
			HouseholdId = householdId;
			KeptUId = keptSid;
			DeletedUId = deletedSid;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(KitchenDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			if (!await db.HasHouseholdPermission(request.HouseholdId, request.MemberId, PermissionLevels.Write))
				return new CommandResult(false, KitchenGlobals.PermissionError);

			long keptId = request.KeptUId.DecodeLong();
			long deletedId = request.DeletedUId.DecodeLong();

			if (keptId == 0 || deletedId == 0)
				return new CommandResult(false, "Invalid product.");

			var keptProduct = await db.Products
				.AsNoTracking()
				.Where(x => x.Id == keptId)
				.SingleOrDefaultAsync(cancellationToken);

			var deletedProduct = await db.Products
				.AsNoTracking()
				.Where(x => x.Id == deletedId)
				.SingleOrDefaultAsync(cancellationToken);

			if (keptProduct != null && deletedProduct != null)
			{
				using (var transaction = db.Database.BeginTransaction())
				{
					try
					{
						/********************************************
						 * Product Substitutions
						 * *****************************************/

						var keptSubs = await db.ProductSubstitutions
							.Where(x => x.ProductId == keptProduct.Id)
							.ToListAsync(cancellationToken);

						var deletingSubs = await db.ProductSubstitutions
							.AsNoTracking()
							.Where(x => x.ProductId == deletedProduct.Id)
							.ToListAsync(cancellationToken);

						// Delete old substitutions
						await db.ProductSubstitutions
								.Where(x => x.ProductId == deletedProduct.Id)
								.DeleteFromQueryAsync(cancellationToken);

						foreach (var dSub in deletingSubs)
						{
							var kSub = keptSubs.Where(x => x.IngredientKey == dSub.IngredientKey)
								.SingleOrDefault();

							if (kSub != null) // Kept is already in substitutions
							{
								// Make primary if old is.
								if (dSub.IsPrimary && !kSub.IsPrimary)
									kSub.IsPrimary = true;
							}
							else // Not in substitutions
							{
								kSub = new()
								{
									HouseholdId = dSub.HouseholdId,
									IngredientKey = dSub.IngredientKey,
									IsPrimary = dSub.IsPrimary,
									ProductId = keptProduct.Id
								};
								db.ProductSubstitutions.Add(kSub);
							}
						}
						await db.SaveChangesAsync(cancellationToken);

						/********************************************
						 * Product Store Locations
						 * *****************************************/

						var keptLocs = await db.StoreProductLocations
							.Where(x => x.ProductId == keptProduct.Id)
							.ToListAsync(cancellationToken);

						var deletingLocs = await db.StoreProductLocations
							.AsNoTracking()
							.Where(x => x.ProductId == deletedProduct.Id)
							.ToListAsync(cancellationToken);

						// Delete old store locations
						await db.StoreProductLocations
							.Where(x => x.ProductId == deletedProduct.Id)
							.DeleteFromQueryAsync(cancellationToken);

						foreach (var dLoc in deletingLocs)
						{
							// Check is kept product is already in the store
							bool inStore = keptLocs.Where(x => x.StoreId == dLoc.StoreId).Any();

							if (!inStore) // Not in store, add the old location for the kept product
							{
								StoreProductLocationEntity kLoc = new()
								{
									ProductId = keptProduct.Id,
									SortOrder = dLoc.SortOrder,
									StoreId = dLoc.StoreId,
									StoreLocationId = dLoc.StoreLocationId
								};
								db.StoreProductLocations.Add(kLoc);
							}
						}
						await db.SaveChangesAsync(cancellationToken);

						/********************************************
						 * Shopping List Items
						 * *****************************************/

						var keptItems = await db.ShoppingListItems
							.Where(x => x.ProductId == keptProduct.Id)
							.ToListAsync(cancellationToken);

						var deletingItems = await db.ShoppingListItems
							.AsNoTracking()
							.Where(x => x.ProductId == deletedProduct.Id)
							.ToListAsync(cancellationToken);

						// Delete old shopping list items
						await db.ShoppingListItems
							.Where(x => x.ProductId == deletedProduct.Id)
							.DeleteFromQueryAsync(cancellationToken);

						foreach (var dItem in deletingItems)
						{
							var kItem = keptItems.Where(x => x.ShoppingListId == dItem.ShoppingListId)
								.SingleOrDefault();

							if (kItem == null) // Not in list
							{
								kItem = new()
								{
									IsInCart = false,
									ProductId = keptProduct.Id,
									Quantity = dItem.Quantity,
									ShoppingListId = dItem.ShoppingListId,
								};
								db.ShoppingListItems.Add(kItem);
							}
							else // In list
							{
								kItem.Quantity += dItem.Quantity;
							}
							await db.SaveChangesAsync(cancellationToken);
						}

						keptItems = null;
						deletingItems = null;

						/********************************************
						 * Shopping List Recipe Items
						 * *****************************************/
						var keptRecipeItems = await db.ShoppingListRecipeItems
							.Where(x => x.ProductId == keptProduct.Id)
							.ToListAsync(cancellationToken);

						var deletingRecipeItems = await db.ShoppingListRecipeItems
							.AsNoTracking()
							.Where(x => x.ProductId == deletedProduct.Id)
							.ToListAsync(cancellationToken);

						// Delete old shopping list items
						await db.ShoppingListRecipeItems
							.Where(x => x.ProductId == deletedProduct.Id)
							.DeleteFromQueryAsync(cancellationToken);

						foreach (var dItem in deletingRecipeItems)
						{
							var kItem = keptRecipeItems.Where(x => x.ShoppingListId == dItem.ShoppingListId && x.RecipeId == dItem.RecipeId)
								.SingleOrDefault();

							if (kItem == null) // Not in list
							{
								kItem = new()
								{
									Amount = dItem.Amount,
									IngredientKey = dItem.IngredientKey,
									PortionUsed = dItem.PortionUsed,
									ProductId = keptProduct.Id,
									RecipeId = dItem.RecipeId,
									ShoppingListId = dItem.ShoppingListId,
									UnitLabel = dItem.UnitLabel
								};
								db.ShoppingListRecipeItems.Add(kItem);
							}
							else // In list
							{
								kItem.Amount += dItem.Amount;
								kItem.PortionUsed += dItem.PortionUsed;
							}
							await db.SaveChangesAsync(cancellationToken);
						}

						/********************************************
						 * FINAL Remove Product
						 * *****************************************/
						await db.Products.Where(x => x.Id == deletedId)
							.DeleteFromQueryAsync(cancellationToken);

						// Commit transaction if all commands succeed, transaction will auto-rollback
						// when disposed if any command fails
						await transaction.CommitAsync(cancellationToken);

						return new CommandResult(true, "Products successfully merged.");
					}
					catch
					{
						await transaction.RollbackAsync(cancellationToken);
						return new CommandResult(false, "An error occurred while processing the merge.");
					}
				}
			}

			return new CommandResult(false, "Could not complete merge. One or more invalid products.");
		}
	}
}