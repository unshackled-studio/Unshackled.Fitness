﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Food.Core;
using Unshackled.Food.Core.Data;
using Unshackled.Food.Core.Data.Entities;
using Unshackled.Food.Core.Enums;
using Unshackled.Food.My.Client.Features.ProductBundles.Models;
using Unshackled.Food.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Food.My.Features.ProductBundles.Actions;

public class UpdateProductBundleProperties
{
	public class Command : IRequest<CommandResult<ProductBundleModel>>
	{
		public long MemberId { get; private set; }
		public FormProductBundleModel Model { get; private set; }

		public Command(long memberId, FormProductBundleModel model)
		{
			MemberId = memberId;
			Model = model;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult<ProductBundleModel>>
	{
		public Handler(FoodDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult<ProductBundleModel>> Handle(Command request, CancellationToken cancellationToken)
		{
			long productBundleId = request.Model.Sid.DecodeLong();

			if (productBundleId == 0)
				return new CommandResult<ProductBundleModel>(false, "Invalid product bundle ID.");

			if (!await db.HasProductBundlePermission(productBundleId, request.MemberId, PermissionLevels.Write))
				return new CommandResult<ProductBundleModel>(false, FoodGlobals.PermissionError);

			ProductBundleEntity? productBundle = await db.ProductBundles
				.Where(x => x.Id == productBundleId)
				.SingleOrDefaultAsync();

			if (productBundle == null)
				return new CommandResult<ProductBundleModel>(false, "Invalid product bundle.");

			// Update productBundle
			productBundle.Title = request.Model.Title.Trim();
			await db.SaveChangesAsync(cancellationToken);

			var pb = mapper.Map<ProductBundleModel>(productBundle);

			pb.Products = await mapper.ProjectTo<FormProductModel>(db.ProductBundleItems
						.AsNoTracking()
						.Include(x => x.Product)
						.Where(x => x.ProductBundleId == productBundle.Id)
						.OrderBy(x => x.Product.Title))
						.ToListAsync();

			return new CommandResult<ProductBundleModel>(true, "Product bundle updated.", pb);
		}
	}
}