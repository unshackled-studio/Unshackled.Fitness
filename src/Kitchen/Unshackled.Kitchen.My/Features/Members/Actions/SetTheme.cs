﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.Core.Models;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Studio.Core.Client.Enums;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Kitchen.My.Features.Members.Actions;

public class SetTheme
{
	public class Command : IRequest<CommandResult<Member>>
	{
		public long Id { get; private set; }
		public Themes Theme { get; private set; }

		public Command(long id, Themes theme)
		{
			Id = id;
			Theme = theme;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult<Member>>
	{
		public Handler(KitchenDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult<Member>> Handle(Command request, CancellationToken cancellationToken)
		{
			var member = await db.Members
				.Where(x => x.Id == request.Id)
				.SingleOrDefaultAsync(cancellationToken);

			if (member == null)
				return new CommandResult<Member>(false, "Invalid member.");

			member.AppTheme = request.Theme;
			await db.SaveChangesAsync(cancellationToken);

			var m = await db.GetMember(member);

			return new CommandResult<Member>(true, "Theme updated.", m);
		}
	}
}
