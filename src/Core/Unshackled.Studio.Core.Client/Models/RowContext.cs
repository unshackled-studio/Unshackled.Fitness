﻿namespace Unshackled.Studio.Core.Client.Models;

public class RowContext<T> where T : new()
{
	public required T Item { get; set; }
	public int RowIndex { get; set; }
}