﻿using Microsoft.AspNetCore.Components;
using Unshackled.Studio.Core.Client.Models;

namespace Unshackled.Studio.Core.Client.Components;

public abstract class BaseSearchComponent<TModel, TResults, TMember> : BaseComponent<TMember>
	where TModel : ISearchModel, new() 
	where TResults : class, new()
	where TMember: IMember
{
	protected SearchResult<TResults> SearchResults { get; set; }
	protected TModel SearchModel { get; set; }
	protected bool IsLoading { get; set; } = true;
	protected bool IsWorking { get; set; }
	protected virtual bool DisableControls => IsLoading || IsWorking;

	protected string SearchKey = string.Empty;

	public BaseSearchComponent() : base()
	{
		SearchModel = new();
		SearchResults = new(SearchModel.PageSize);
	}

	protected abstract Task DoSearch(int page);

	public async Task<TModel?> GetLocalSetting(string key)
	{
		return await localStorageService.GetItemAsync<TModel>(key);
	}

	protected async virtual Task HandlePageSelected(int page)
	{
		if(page != SearchModel.Page) {
			SearchModel.Page = page;
			await DoSearch(page);
		}
	}

	protected async virtual Task HandleResetClicked()
	{
		SearchModel = new();
		await DoSearch(1);
	}

	protected async Task HandleSortByClicked(int dir, params string[] members)
	{
		SearchModel.Sorts.Clear();
		foreach (var member in members)
		{
			SearchModel.Sorts.Add(new() { Member = member, SortDirection = dir });
		}
		await DoSearch(SearchModel.Page);
	}

	public async Task SaveLocalSetting(string key, TModel value)
	{
		await localStorageService.SetItemAsync(key, value, CancellationToken.None);
	}
}
