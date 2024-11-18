﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Food.Core;
using Unshackled.Food.Core.Data;
using Unshackled.Food.Core.Data.Entities;
using Unshackled.Food.Core.Enums;
using Unshackled.Food.My.Client.Features.Stores.Models;
using Unshackled.Food.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Food.My.Features.Stores.Actions;

public class DeleteStore
{
	public class Command : IRequest<CommandResult>
	{
		public long MemberId { get; private set; }
		public string Sid { get; private set; }

		public Command(long memberId, string sid)
		{
			MemberId = memberId;
			Sid = sid;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(FoodDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			long storeId = request.Sid.DecodeLong();

			if (storeId == 0)
				return new CommandResult<StoreModel>(false, "Invalid store ID.");

			if (!await db.HasStorePermission(storeId, request.MemberId, PermissionLevels.Write))
				return new CommandResult<StoreModel>(false, FoodGlobals.PermissionError);

			var store = await db.Stores
				.Where(x => x.Id == storeId)
				.SingleOrDefaultAsync(cancellationToken);

			if (store == null)
				return new CommandResult(false, "Invalid store.");

			using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

			try
			{
				await db.StoreLocations
					.Where(x => x.StoreId == store.Id)
					.DeleteFromQueryAsync(cancellationToken);

				await db.StoreProductLocations
					.Where(x => x.StoreId == store.Id)
					.DeleteFromQueryAsync(cancellationToken);

				await db.ShoppingLists
					.Where(x => x.StoreId == store.Id)
					.UpdateFromQueryAsync(x => new ShoppingListEntity { StoreId = null });

				db.Stores.Remove(store);
				await db.SaveChangesAsync(cancellationToken);

				await transaction.CommitAsync(cancellationToken);

				return new CommandResult(true, "Store deleted.");
			}
			catch
			{
				await transaction.RollbackAsync(cancellationToken);
				return new CommandResult(false, "Database error. Store could not be deleted.");
			}
		}
	}
}