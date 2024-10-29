﻿using Unshackled.Fitness.Core.Enums;
using Unshackled.Fitness.Core.Models;

namespace Unshackled.Fitness.My.Client.Features.Activities.Models;

public class ActivityListModel : BaseMemberObject
{
	public string Title { get; set; } = string.Empty;
	public DateTime DateEvent { get; set; }
	public DateTime DateEventUtc { get; set; }
	public EventTypes EventType { get; set; }
	public string ActivityTypeSid { get; set; } = string.Empty;
	public string ActivityTypeTitle { get; set; } = string.Empty;
	public string ActivityTypeColor { get; set; } = string.Empty;
	public double TotalDistanceMeters { get; set; }
	public int TotalTimeSeconds { get; set; }
	public int TotalCalories { get; set; }
}
