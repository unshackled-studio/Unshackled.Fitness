﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unshackled.Kitchen.Core.Enums;
using Unshackled.Studio.Core.Data.Entities;

namespace Unshackled.Kitchen.Core.Data.Entities;

public class CookbookMemberEntity
{
	public long CookbookId { get; set; }
	public virtual CookbookEntity Cookbook { get; set; } = null!;
	public long MemberId { get; set; }
	public virtual MemberEntity Member { get; set; } = null!;
	public PermissionLevels PermissionLevel { get; set; }

	public class TypeConfiguration : IEntityTypeConfiguration<CookbookMemberEntity>
	{
		public void Configure(EntityTypeBuilder<CookbookMemberEntity> config)
		{
			config.ToTable("CookbookMembers");
			config.HasKey(x => new { x.CookbookId, x.MemberId });

			config.HasOne(x => x.Cookbook)
				.WithMany(x => x.Memberships)
				.HasForeignKey(x => x.CookbookId);

			config.HasOne(x => x.Member)
				.WithMany()
				.HasForeignKey(x => x.MemberId);
		}
	}
}
