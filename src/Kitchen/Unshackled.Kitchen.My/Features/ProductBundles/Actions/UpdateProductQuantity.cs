﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Kitchen.Core;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.Core.Data.Entities;
using Unshackled.Kitchen.Core.Enums;
using Unshackled.Kitchen.My.Client.Features.ProductBundles.Models;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Kitchen.My.Features.ProductBundles.Actions;

public class UpdateProductQuantity
{
	public class Command : IRequest<CommandResult>
	{
		public long MemberId { get; private set; }
		public UpdateProductModel Model { get; private set; }

		public Command(long memberId, UpdateProductModel model)
		{
			MemberId = memberId;
			Model = model;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(KitchenDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			long productBundleId = request.Model.ProductBundleSid.DecodeLong();

			if (productBundleId == 0)
				return new CommandResult(false, "Invalid product bundle ID.");

			if (!await db.HasProductBundlePermission(productBundleId, request.MemberId, PermissionLevels.Write))
				return new CommandResult(false, KitchenGlobals.PermissionError);

			long productId = request.Model.ProductSid.DecodeLong();

			if (productId == 0)
				return new CommandResult(false, "Invalid product ID.");

			ProductBundleItemEntity? productBundleItem = await db.ProductBundleItems
				.Where(x => x.ProductBundleId == productBundleId && x.ProductId == productId)
				.SingleOrDefaultAsync();

			if (productBundleItem == null)
				return new CommandResult(false, "Invalid product.");

			// Update product
			productBundleItem.Quantity = Math.Max(request.Model.Quantity, 1);
			await db.SaveChangesAsync(cancellationToken);

			return new CommandResult(true, "Product quantity updated.");
		}
	}
}