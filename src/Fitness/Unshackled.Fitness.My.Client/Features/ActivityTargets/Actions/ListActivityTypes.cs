﻿using MediatR;
using Unshackled.Fitness.My.Client.Features.ActivityTargets.Models;

namespace Unshackled.Fitness.My.Client.Features.ActivityTargets.Actions;

public class ListActivityTypes
{
	public class Query : IRequest<List<ActivityTypeListModel>> { }

	public class Handler : BaseActivityTargetHandler, IRequestHandler<Query, List<ActivityTypeListModel>>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<List<ActivityTypeListModel>> Handle(Query request, CancellationToken cancellationToken)
		{
			return await GetResultAsync<List<ActivityTypeListModel>>($"{baseUrl}list-types") ?? [];
		}
	}
}
