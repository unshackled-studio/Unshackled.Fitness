﻿using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Kitchen.Core;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.Core.Models;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Data.Extensions;

namespace Unshackled.Kitchen.My.Features.Members.Actions;

public class SaveSettings
{
	public class Command : IRequest<CommandResult<Member>>
	{
		public long Id { get; private set; }
		public AppSettings Settings { get; private set; }

		public Command(long id, AppSettings settings)
		{
			Id = id;
			Settings = settings;
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

			string settingsJson = JsonSerializer.Serialize(request.Settings);
			await db.SaveMeta(member.Id, KitchenGlobals.MetaKeys.AppSettings, settingsJson);

			var m = await db.GetMember(member);

			return new CommandResult<Member>(true, "Settings updated.", m);
		}
	}
}
