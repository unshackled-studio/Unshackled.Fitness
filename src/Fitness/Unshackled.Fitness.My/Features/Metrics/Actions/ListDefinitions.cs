﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unshackled.Fitness.Core.Data;
using Unshackled.Fitness.My.Client.Features.Metrics.Models;
using Unshackled.Studio.Core.Data;

namespace Unshackled.Fitness.My.Features.Metrics.Actions;

public class ListDefintions
{
	public class Query : IRequest<MetricListModel>
	{
		public long MemberId { get; private set; }

		public Query(long memberId)
		{
			MemberId = memberId;
		}
	}

	public class Handler : BaseHandler, IRequestHandler<Query, MetricListModel>
	{
		public Handler(FitnessDbContext db, IMapper mapper) : base(db, mapper) { }

		public async Task<MetricListModel> Handle(Query request, CancellationToken cancellationToken)
		{
			MetricListModel model = new();

			model.Groups = await mapper.ProjectTo<FormMetricDefinitionGroupModel>(db.MetricDefinitionGroups
				.Where(x => x.MemberId == request.MemberId)
				.OrderBy(x => x.SortOrder))
				.ToListAsync(cancellationToken);

			model.Metrics = await mapper.ProjectTo<FormMetricDefinitionModel>(db.MetricDefinitions
				.Where(x => x.MemberId == request.MemberId)
				.OrderBy(x => x.SortOrder))
				.ToListAsync(cancellationToken);

			return model;
		}
	}
}