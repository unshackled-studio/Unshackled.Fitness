﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Kitchen.Core;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.Core.Enums;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Kitchen.My.Features.Households.Actions;

public class DeleteMember
{
	public class Command : IRequest<CommandResult>
	{
		public long AdminMemberId { get; private set; }
		public string HouseholdMemberSid { get; private set; }
		public long HouseholdId { get; private set; }

		public Command(long adminMemberId, string householdMemberId, long householdId)
		{
			AdminMemberId = adminMemberId;
			HouseholdMemberSid = householdMemberId;
			HouseholdId = householdId;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult>
	{
		public Handler(KitchenDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult> Handle(Command request, CancellationToken cancellationToken)
		{
			if (request.HouseholdId == 0)
				return new CommandResult(false, "Invalid household ID.");

			if (!await db.HasHouseholdPermission(request.HouseholdId, request.AdminMemberId, PermissionLevels.Admin))
				return new CommandResult(false, KitchenGlobals.PermissionError);

			long memberId = request.HouseholdMemberSid.DecodeLong();
			if (memberId == 0)
				return new CommandResult(false, "Invalid household member ID.");

			if (await db.Households
				.Where(x => x.Id == request.HouseholdId && x.MemberId == memberId)
				.AnyAsync(cancellationToken))
				return new CommandResult(false, "The household owner cannot be removed.");

			var householdMember = await db.HouseholdMembers
				.Where(x => x.HouseholdId == request.HouseholdId && x.MemberId == memberId)
				.SingleOrDefaultAsync(cancellationToken);

			if (householdMember == null)
				return new CommandResult(false, "Invalid household member.");

			// if active household for member in any app, remove it
			await db.MemberMeta
				.Where(x => x.Id == memberId && x.MetaKey == KitchenGlobals.MetaKeys.ActiveHouseholdId && x.MetaValue == request.HouseholdId.ToString())
				.DeleteFromQueryAsync(cancellationToken);
						
			// Remove membership
			db.HouseholdMembers.Remove(householdMember);
			await db.SaveChangesAsync(cancellationToken);

			return new CommandResult(true, "Member has been removed from the household.");
		}
	}
}