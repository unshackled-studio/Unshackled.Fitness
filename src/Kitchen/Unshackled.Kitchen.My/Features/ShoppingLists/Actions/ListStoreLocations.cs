﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.Core.Enums;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Kitchen.My.Features.ShoppingLists.Actions;

public class ListStoreLocations
{
	public class Query : IRequest<List<ListGroupModel>>
	{
		public long MemberId { get; private set; }
		public long StoreId { get; private set; }

		public Query(long memberId, long storeId)
		{
			MemberId = memberId;
			StoreId = storeId;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Query, List<ListGroupModel>>
	{
		public Handler(KitchenDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<List<ListGroupModel>> Handle(Query request, CancellationToken cancellationToken)
		{
			if (await db.HasStorePermission(request.StoreId, request.MemberId, PermissionLevels.Read))
			{
				return await db.StoreLocations
					.AsNoTracking()
					.Where(x => x.StoreId == request.StoreId)
					.OrderBy(x => x.SortOrder)
					.Select(x => new ListGroupModel
					{
						Sid = x.Id.Encode(),
						Title = x.Title,
						SortOrder = x.SortOrder
					})
					.ToListAsync();
			}
			return new();
		}
	}
}
