﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.Core.Enums;
using Unshackled.Kitchen.My.Client.Features.Ingredients.Models;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Kitchen.My.Features.Ingredients.Actions;

public class SearchProducts
{
	public class Query : IRequest<SearchResult<ProductListModel>>
	{
		public long MemberId { get; private set; }
		public long HouseholdId { get; private set; }
		public SearchProductModel Model { get; private set; }

		public Query(long memberId, long householdId, SearchProductModel model)
		{
			HouseholdId = householdId;
			MemberId = memberId;
			Model = model;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Query, SearchResult<ProductListModel>>
	{
		public Handler(KitchenDbContext db, IMapper map) : base(db, map) { }

		public async Task<SearchResult<ProductListModel>> Handle(Query request, CancellationToken cancellationToken)
		{
			if (!await db.HasHouseholdPermission(request.HouseholdId, request.MemberId, PermissionLevels.Read))
				return new();

			var result = new SearchResult<ProductListModel>(request.Model.PageSize);

			var query = db.Products
				.AsNoTracking()
				.Where(x => x.HouseholdId == request.HouseholdId && x.IsArchived == false);

			if (!string.IsNullOrEmpty(request.Model.Title))
			{
				string[] keywords = request.Model.Title.Split(' ');
				if (keywords.Length > 1)
					query = query.TitleContains(keywords);
				else
					query = query.Where(x => x.Title.Contains(request.Model.Title));
			}

			result.Total = await query.CountAsync(cancellationToken);

			query = query
				.OrderBy(x => x.Brand)
				.ThenBy(x => x.Title);

			query = query.Skip(request.Model.Skip).Take(request.Model.PageSize);

			result.Data = await mapper.ProjectTo<ProductListModel>(query)
				.ToListAsync();

			return result;
		}
	}
}
