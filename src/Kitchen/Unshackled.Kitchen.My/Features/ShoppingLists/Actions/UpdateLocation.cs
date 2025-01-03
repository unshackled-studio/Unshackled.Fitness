﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Kitchen.Core;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.Core.Data.Entities;
using Unshackled.Kitchen.Core.Enums;
using Unshackled.Kitchen.My.Client.Features.ShoppingLists.Models;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Kitchen.My.Features.ShoppingLists.Actions;

public class UpdateLocation
{
	public class Command : IRequest<CommandResult<FormListItemModel>>
	{
		public long MemberId { get; private set; }
		public UpdateLocationModel Model { get; private set; }

		public Command(long memberId, UpdateLocationModel model)
		{
			MemberId = memberId;
			Model = model;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult<FormListItemModel>>
	{
		public Handler(KitchenDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult<FormListItemModel>> Handle(Command request, CancellationToken cancellationToken)
		{
			long storeId = request.Model.StoreSid.DecodeLong();

			if (storeId == 0)
				return new CommandResult<FormListItemModel>(false, "Invalid store ID.");

			long productId = request.Model.ProductSid.DecodeLong();

			if (productId == 0)
				return new CommandResult<FormListItemModel>(false, "Invalid product ID.");

			if (!await db.HasProductPermission(productId, request.MemberId, PermissionLevels.Write))
				return new CommandResult<FormListItemModel>(false, KitchenGlobals.PermissionError);

			long storeLocationId = request.Model.StoreLocationSid.DecodeLong();

			using var transaction = await db.Database.BeginTransactionAsync();

			try
			{
				var productLocation = await db.StoreProductLocations
					.Where(x => x.StoreId == storeId && x.ProductId == productId)
					.SingleOrDefaultAsync();

				if (storeLocationId == 0) // Location not set, so remove
				{
					if (productLocation != null)
					{
						// Update sort order of any products after the current product
						await db.StoreProductLocations
							.Where(x => x.StoreLocationId == productLocation.StoreLocationId && x.SortOrder > productLocation.SortOrder)
							.UpdateFromQueryAsync(x => new StoreProductLocationEntity { SortOrder = x.SortOrder - 1 });

						db.StoreProductLocations.Remove(productLocation);
						await db.SaveChangesAsync();
					}
					else
					{
						return new CommandResult<FormListItemModel>(false, "Product location not found");
					}
				}
				else // Set new location
				{
					bool exists = await db.StoreLocations
					.Where(x => x.StoreId == storeId && x.Id == storeLocationId)
					.AnyAsync();

					if (!exists)
						return new CommandResult<FormListItemModel>(false, "Location not found.");

					if (productLocation == null)
					{
						productLocation = new()
						{
							StoreId = storeId,
							ProductId = productId,
							StoreLocationId = storeLocationId,
							SortOrder = await db.StoreProductLocations.Where(x => x.StoreLocationId == storeLocationId).CountAsync(),
						};
						db.StoreProductLocations.Add(productLocation);
					}
					else
					{
						// Update sort order of any products after the current product
						await db.StoreProductLocations
							.Where(x => x.StoreLocationId == productLocation.StoreLocationId && x.SortOrder > productLocation.SortOrder)
							.UpdateFromQueryAsync(x => new StoreProductLocationEntity { SortOrder = x.SortOrder - 1 });

						productLocation.StoreLocationId = storeLocationId;
						productLocation.SortOrder = await db.StoreProductLocations.Where(x => x.StoreLocationId == storeLocationId).CountAsync();
					}
					await db.SaveChangesAsync();
				}

				await transaction.CommitAsync();
			}
			catch
			{
				await transaction.RollbackAsync();
				return new CommandResult<FormListItemModel>(false, Globals.UnexpectedError);
			}

			var product = await (from i in db.ShoppingListItems
									join l in db.ShoppingLists on i.ShoppingListId equals l.Id
									join p in db.Products on i.ProductId equals p.Id
									join pl in db.StoreProductLocations on new { i.ProductId, l.StoreId } equals new { pl.ProductId, StoreId = (long?)pl.StoreId } into locations
									from pl in locations.DefaultIfEmpty()
									join sl in db.StoreLocations on pl.StoreLocationId equals sl.Id into storeLocations
									from sl in storeLocations.DefaultIfEmpty()
									where i.ProductId == productId
									select new FormListItemModel
									{
										Brand = p.Brand,
										Description = p.Description,
										IsInCart = i.IsInCart,
										ListGroupSid = pl != null ? pl.StoreLocationId.Encode() : KitchenGlobals.UncategorizedKey,
										LocationSortOrder = sl != null ? sl.SortOrder : -1,
										ProductSid = p.Id.Encode(),
										Quantity = i.Quantity,
										ShoppingListSid = i.ShoppingListId.Encode(),
										SortOrder = pl != null ? pl.SortOrder : -1,
										StoreLocationSid = pl != null ? pl.StoreLocationId.Encode() : string.Empty,
										Title = p.Title
									}).SingleOrDefaultAsync();

			return new CommandResult<FormListItemModel>(true, "Location updated.", product);
		}
	}
}