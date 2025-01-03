﻿namespace Unshackled.Studio.Core.Client;

public class Globals
{
	public const string AccountUrlPrefix = "/account";
	public const int DefaultCacheDurationMinutes = 30;
	public const string LoginCallbackAction = "login-callback";
	public const string LinkLoginCallbackAction = "link-login-callback";
	public const string UnexpectedError = "An unexpected error occurred.";

	public static class ApiConstants
	{
		public const string LocalApi = "LocalApi";
	}

	public static class MiddlewareItemKeys
	{
		public const string Member = "member";
	}
}
