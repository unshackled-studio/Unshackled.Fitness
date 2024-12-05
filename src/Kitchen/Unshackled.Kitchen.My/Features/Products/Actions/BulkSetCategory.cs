﻿using AutoMapper;
using MediatR;
using Unshackled.Kitchen.Core;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.Core.Data.Entities;
using Unshackled.Kitchen.Core.Enums;
using Unshackled.Kitchen.My.Client.Features.Products.Models;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Kitchen.My.Features.Products.Actions;

public class BulkSetCategory
{
	public class Command : IRequest<CommandResult>
	{
		public long MemberId { get; private set; }
		public long HouseholdId { get; private set; }
		public BulkCategoryModel Model { get; private set; }

		public Command(long memberId, long householdId, BulkCategoryModel model)
		{
			MemberId = memberId;
			HouseholdId = householdId;
			Model = model;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(KitchenDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			if (!await db.HasHouseholdPermission(request.HouseholdId, request.MemberId, PermissionLevels.Write))
				return new CommandResult(false, KitchenGlobals.PermissionError);

			List<long> productIds = request.Model.ProductSids.DecodeLong();

			if (!productIds.Any())
				return new CommandResult(false, "Invalid product IDs");

			long? newCatId = null;
			if (request.Model.CategorySid != KitchenGlobals.UncategorizedKey)
			{
				newCatId = request.Model.CategorySid.DecodeLong();
				if (newCatId == 0)
					return new CommandResult(false, "Invalid Category");
			}

			await db.Products
				.Where(x => productIds.Contains(x.Id))
				.UpdateFromQueryAsync(x => new ProductEntity() { ProductCategoryId = newCatId });

			return new CommandResult(true, "The new category was applied.");
		}
	}
}