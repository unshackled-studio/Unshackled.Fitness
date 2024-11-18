﻿using AutoMapper;
using MediatR;
using Unshackled.Food.Core;
using Unshackled.Food.Core.Data;
using Unshackled.Food.Core.Data.Entities;
using Unshackled.Food.Core.Enums;
using Unshackled.Food.My.Client.Features.ProductBundles.Models;
using Unshackled.Food.My.Extensions;
using Unshackled.Studio.Core.Client;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Food.My.Features.ProductBundles.Actions;

public class AddProducts
{
	public class Command : IRequest<CommandResult>
	{
		public long MemberId { get; private set; }
		public AddProductsModel Model { get; private set; }

		public Command(long memberId, AddProductsModel model)
		{
			MemberId = memberId;
			Model = model;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(FoodDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			long productBundleId = request.Model.ProductBundleSid.DecodeLong();

			if (productBundleId == 0)
				return new CommandResult(false, "Invalid shopping list ID.");

			if (!await db.HasProductBundlePermission(productBundleId, request.MemberId, PermissionLevels.Write))
				return new CommandResult(false, FoodGlobals.PermissionError);

			using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

			try
			{
				List<ProductBundleItemEntity> newItems = new();
				foreach (var productSid in request.Model.Products.Keys)
				{
					long productId = productSid.DecodeLong();

					// invalid product ID, skip and continue
					if (productId == 0) continue;

					ProductBundleItemEntity item = new()
					{
						ProductId = productId,
						Quantity = request.Model.Products[productSid],
						ProductBundleId = productBundleId
					};
					newItems.Add(item);
				}

				if (newItems.Count > 0)
				{
					db.ProductBundleItems.AddRange(newItems);
					await db.SaveChangesAsync(cancellationToken);
					await transaction.CommitAsync(cancellationToken);

					return new CommandResult(true, "Products successfully added to list.");
				}

				return new CommandResult(false, "Nothing to add.");
			}
			catch
			{
				await transaction.RollbackAsync(cancellationToken);
				return new CommandResult(false, Globals.UnexpectedError);
			}
		}
	}
}