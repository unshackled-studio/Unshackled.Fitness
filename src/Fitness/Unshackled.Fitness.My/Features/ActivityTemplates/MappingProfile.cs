﻿using AutoMapper;
using Unshackled.Fitness.Core.Data.Entities;
using Unshackled.Fitness.My.Client.Features.ActivityTemplates.Models;
using Unshackled.Studio.Core.Server.Extensions;

namespace Unshackled.Fitness.My.Features.ActivityTemplates;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<ActivityTemplateEntity, ActivityTemplateModel>()
			.ForMember(d => d.ActivityTypeColor, m => m.MapFrom(s => s.ActivityType != null ? s.ActivityType.Color : string.Empty))
			.ForMember(d => d.ActivityTypeSid, m => m.MapFrom(s => s.ActivityTypeId.Encode()))
			.ForMember(d => d.ActivityTypeTitle, m => m.MapFrom(s => s.ActivityType != null ? s.ActivityType.Title : string.Empty))
			.ForMember(d => d.MemberSid, m => m.MapFrom(s => s.MemberId.Encode()))
			.ForMember(d => d.Sid, m => m.MapFrom(s => s.Id.Encode()));

		CreateMap<ActivityTemplateEntity, TemplateListItem>()
			.ForMember(d => d.Sid, m => m.MapFrom(s => s.Id.Encode()))
			.ForMember(d => d.ActivityTypeSid, m => m.MapFrom(s => s.ActivityTypeId.Encode()))
			.ForMember(d => d.ActivityTypeName, m => m.MapFrom(s => s.ActivityType != null ? s.ActivityType.Title : string.Empty));

		CreateMap<ActivityTypeEntity, ActivityTypeListModel>()
			.ForMember(d => d.MemberSid, m => m.MapFrom(s => s.MemberId.Encode()))
			.ForMember(d => d.Sid, m => m.MapFrom(s => s.Id.Encode()));
	}
}