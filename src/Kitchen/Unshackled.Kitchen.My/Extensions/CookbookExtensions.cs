﻿using Microsoft.EntityFrameworkCore;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.Core.Data.Entities;
using Unshackled.Kitchen.Core.Enums;

namespace Unshackled.Kitchen.My.Extensions;

public static class CookbookExtensions
{
	public static async Task<bool> HasCookbookPermission(this KitchenDbContext db, long cookbookId, long memberId, PermissionLevels permission)
	{
		return await db.CookbookMembers
			.Where(x => x.CookbookId == cookbookId && x.MemberId == memberId && x.PermissionLevel >= permission)
			.AnyAsync();
	}

	public static async Task<bool> HasCookbookRecipePermission(this KitchenDbContext db, long recipeId, long memberId, PermissionLevels permission)
	{
		long cookbookId = await db.CookbookRecipes
			.Where(x => x.RecipeId == recipeId)
			.Select(x => x.CookbookId)
			.SingleOrDefaultAsync();

		if (cookbookId == 0)
			return false;

		return await db.CookbookMembers
			.Where(x => x.CookbookId == cookbookId && x.MemberId == memberId && x.PermissionLevel >= permission)
			.AnyAsync();
	}

	public static async Task<CookbookMemberEntity?> HasMember(this DbSet<CookbookMemberEntity> cookbookMembers, long memberId, long cookbookId)
	{
		return await cookbookMembers
			.Where(x => x.MemberId == memberId
				&& x.CookbookId == cookbookId)
			.SingleOrDefaultAsync();
	}

	public static async Task<CookbookMemberEntity?> HasMember(this DbSet<CookbookMemberEntity> cookbookMembers, string email, long cookbookId)
	{
		return await cookbookMembers
			.Include(x => x.Member)
			.Where(x => x.Member!.Email == email
				&& x.CookbookId == cookbookId)
			.SingleOrDefaultAsync();
	}
}
