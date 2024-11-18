﻿using MediatR;
using Unshackled.Food.My.Client.Features.ProductBundles.Models;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Food.My.Client.Features.ProductBundles.Actions;

public class SearchProducts
{
	public class Query : IRequest<SearchResult<ProductListModel>>
	{
		public SearchProductsModel Model { get; private set; }

		public Query(SearchProductsModel model)
		{
			Model = model;
		}
	}

	public class Handler : BaseProductBundleHandler, IRequestHandler<Query, SearchResult<ProductListModel>>
	{
		public Handler(HttpClient httpClient) : base(httpClient) { }

		public async Task<SearchResult<ProductListModel>> Handle(Query request, CancellationToken cancellationToken)
		{
			return await PostToResultAsync<SearchProductsModel, SearchResult<ProductListModel>>($"{baseUrl}search-products", request.Model) ??
				new SearchResult<ProductListModel>();
		}
	}
}