﻿using AutoMapper;
using MediatR;
using Unshackled.Fitness.Core.Data;
using Unshackled.Fitness.My.Extensions;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Data;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Fitness.My.Features.Dashboard.Actions;

public class AddWorkout
{
	public class Command : IRequest<CommandResult<string>>
	{
		public long MemberId { get; private set; }
		public string TemplateSid { get; private set; }

		public Command(long memberId, string templateSid)
		{
			MemberId = memberId;
			TemplateSid = templateSid;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult<string>>
	{
		public Handler(FitnessDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<CommandResult<string>> Handle(Command request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(request.TemplateSid))
				return new CommandResult<string>(false, "Template ID missing.");

			long templateId = request.TemplateSid.DecodeLong();

			if (templateId == 0)
				return new CommandResult<string>(false, "Invalid template ID.");

			return await db.AddWorkoutFromTemplate(request.MemberId, templateId, cancellationToken);
		}
	}
}