﻿using MediatR;
using Unshackled.Kitchen.Core;
using Unshackled.Kitchen.My.Client.Features.Recipes.Models;
using Unshackled.Studio.Core.Client.Services;

namespace Unshackled.Kitchen.My.Client.Features.Recipes.Actions;

public class UpdateIngredientTitles
{
	public class Command : IRequest<Unit>
	{
		public List<string> Titles { get; private set; }

		public Command(List<string> titles)
		{
			Titles = titles;
		}
	}

	public class Handler : BaseRecipeHandler, IRequestHandler<Command, Unit>
	{
		private readonly ILocalStorage localStorage;

		public Handler(HttpClient httpClient, ILocalStorage storage) : base(httpClient) 
		{
			this.localStorage = storage;
		}

		public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
		{
			var list = await localStorage.GetItemAsync<IngredientTitleList>(KitchenGlobals.LocalStorageKeys.IngredientTitles);

			if (list == null || list.DateRetrieved <= DateTime.UtcNow.AddMinutes(-LocalStorage.DefaultCacheDurationMinutes))
			{
				list = new() { DateRetrieved = DateTime.UtcNow };
				list.Titles = await GetResultAsync<List<string>>($"{baseUrl}list-ingredient-titles") ?? new();

				await localStorage.SetItemAsync(KitchenGlobals.LocalStorageKeys.IngredientTitles, list, cancellationToken);
			}
			else
			{
				bool updated = false;
				foreach (var title in request.Titles)
				{
					if (!list.Titles.Where(x => x == title).Any())
					{
						list.Titles.Add(title);
						list.Titles = list.Titles.OrderBy(x => x).ToList();
						list.DateRetrieved = DateTime.UtcNow;

						updated = true;
					}
				}

				if(updated)
					await localStorage.SetItemAsync(KitchenGlobals.LocalStorageKeys.IngredientTitles, list, cancellationToken);
			}

			return Unit.Value;
		}
	}
}
