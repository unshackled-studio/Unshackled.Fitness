﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Food.Core.Data;
using Unshackled.Food.Core.Models;
using Unshackled.Food.My.Extensions;
using Unshackled.Studio.Core.Client.Configuration;

namespace Unshackled.Food.My.Features.Members.Actions;

public class GetMemberByEmail
{
	public class Query : IRequest<Member?>
	{
		public string Email { get; private set; }

		public Query(string email)
		{
			Email = email;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Query, Member?>
	{
		private readonly SiteConfiguration siteConfig;

		public Handler(FoodDbContext db, IMapper mapper, SiteConfiguration siteConfig) : base(db, mapper) 
		{
			this.siteConfig = siteConfig;
		}

		public async Task<Member?> Handle(Query request, CancellationToken cancellationToken)
		{
			var memberEntity = await db.Members
				.AsNoTracking()
				.Where(s => s.Email == request.Email.ToLower())
				.SingleOrDefaultAsync(cancellationToken);

			if (memberEntity == null)
			{
				memberEntity = await db.AddMember(request.Email, siteConfig);
				if (memberEntity == null)
					return null;
			}

			return await db.GetMember(memberEntity);
		}
	}
}