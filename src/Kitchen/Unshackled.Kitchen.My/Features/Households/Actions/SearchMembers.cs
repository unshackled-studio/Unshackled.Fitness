﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.Core.Enums;
using Unshackled.Kitchen.My.Client.Features.Households.Models;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Data.Extensions;

namespace Unshackled.Kitchen.My.Features.Households.Actions;

public class SearchMembers
{
	public class Query : IRequest<SearchResult<MemberListModel>>
	{
		public long HouseholdId { get; private set; }
		public long MemberId { get; private set; }
		public MemberSearchModel Model { get; private set; }

		public Query(long memberId, long householdId, MemberSearchModel model)
		{
			HouseholdId = householdId;
			MemberId = memberId;
			Model = model;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Query, SearchResult<MemberListModel>>
	{
		public Handler(KitchenDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<SearchResult<MemberListModel>> Handle(Query request, CancellationToken cancellationToken)
		{
			if(!await db.HasHouseholdPermission(request.HouseholdId, request.MemberId, PermissionLevels.Read)) 
				return new SearchResult<MemberListModel>();

			var result = new SearchResult<MemberListModel>();
			var query = db.HouseholdMembers
					.Include(x => x.Member)
					.AsNoTracking()
					.Where(x => x.HouseholdId == request.HouseholdId);

			if (!string.IsNullOrEmpty(request.Model.Email))
			{
				query = query.Where(x => x.Member.Email.StartsWith(request.Model.Email));
			}

			result.Total = await query.CountAsync(cancellationToken);

			if (request.Model.Sorts.Any())
			{
				query = query.AddSorts(request.Model.Sorts);
			}
			else
			{
				query = query
					.OrderBy(x => x.Member.Email);
			}

			query = query
				.Skip(request.Model.Skip)
				.Take(request.Model.PageSize);

			result.Data = await mapper.ProjectTo<MemberListModel>(query)
				.ToListAsync(cancellationToken);

			return result;
		}
	}
}
