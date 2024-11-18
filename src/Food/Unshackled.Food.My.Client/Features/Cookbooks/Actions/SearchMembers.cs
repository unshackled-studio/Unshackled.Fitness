﻿using MediatR;
using Unshackled.Food.My.Client.Features.Cookbooks.Models;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Food.My.Client.Features.Cookbooks.Actions;

public class SearchMembers
{
	public class Query : IRequest<SearchResult<MemberListModel>> 
	{
		public string CookbookSid { get; private set; }
		public MemberSearchModel Model { get; private set; }

		public Query(string groupSid, MemberSearchModel model)
		{
			CookbookSid = groupSid;
			Model = model;
		}
	}

	public class Handler : BaseCookbookHandler, IRequestHandler<Query, SearchResult<MemberListModel>>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<SearchResult<MemberListModel>> Handle(Query request, CancellationToken cancellationToken)
		{
			return await PostToResultAsync<MemberSearchModel, SearchResult<MemberListModel>>($"{baseUrl}search/{request.CookbookSid}/members", request.Model) ??
				new SearchResult<MemberListModel>();
		}
	}
}
