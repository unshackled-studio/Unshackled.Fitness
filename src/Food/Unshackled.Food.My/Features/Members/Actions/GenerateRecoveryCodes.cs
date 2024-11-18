﻿using System.Security.Claims;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Unshackled.Food.Core.Data;
using Unshackled.Food.My.Client.Features.Members.Models;
using Unshackled.Studio.Core.Client.Models;
using Unshackled.Studio.Core.Data.Entities;

namespace Unshackled.Food.My.Features.Members.Actions;

public class GenerateRecoveryCodes
{
	public class Command : IRequest<CommandResult<RecoveryCodesModel>>
	{
		public ClaimsPrincipal User { get; private set; }

		public Command(ClaimsPrincipal user)
		{
			User = user;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Command, CommandResult<RecoveryCodesModel>>
	{
		private readonly UserManager<UserEntity> userManager;

		public Handler(FoodDbContext db, IMapper mapper, UserManager<UserEntity> userManager) : base(db, mapper)
		{
			this.userManager = userManager;
		}

		public async Task<CommandResult<RecoveryCodesModel>> Handle(Command request, CancellationToken cancellationToken)
		{
			var user = await userManager.GetUserAsync(request.User);

			if (user == null)
				return new CommandResult<RecoveryCodesModel>(false, "Invalid user.");

			RecoveryCodesModel model = new()
			{
				RecoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)
			};

			return new CommandResult<RecoveryCodesModel>(true, "You have generated new recovery codes.", model);
		}
	}
}
