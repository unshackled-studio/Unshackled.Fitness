﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Food.Core.Data;
using Unshackled.Food.Core.Enums;
using Unshackled.Food.My.Extensions;

namespace Unshackled.Food.My.Features.Recipes.Actions;

public class ListIngredientTitles
{
	public class Query : IRequest<List<string>>
	{
		public long HouseholdId { get; private set; }
		public long MemberId { get; private set; }

		public Query(long householdId, long memberId)
		{
			HouseholdId = householdId;
			MemberId = memberId;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Query, List<string>>
	{
		public Handler(FoodDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<List<string>> Handle(Query request, CancellationToken cancellationToken)
		{
			if (await db.HasHouseholdPermission(request.HouseholdId, request.MemberId, PermissionLevels.Read))
			{
				var result = new List<string>();
				var query = db.RecipeIngredients
					.AsNoTracking()
					.Where(x => x.HouseholdId == request.HouseholdId);

				return await query
					.Select(x => x.Title)
					.Distinct()
					.OrderBy(x => x)
					.ToListAsync(cancellationToken);
			}
			return new();
		}
	}
}
