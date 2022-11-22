﻿using System.Text;

namespace CodeChops.SourceGeneration.Utilities.Extensions;

public static class StringBuilderExtensions
{
	public static StringBuilder TrimEnd(this StringBuilder sb)
	{
		if (sb.Length == 0) return sb;

		var i = sb.Length - 1;

		for (; i >= 0; i--)
			if (!Char.IsWhiteSpace(sb[i]))
				break;

		if (i < sb.Length - 1)
			sb.Length = i + 1;

		return sb;
	}

	/// <summary>
	/// Appends the string builder with text and a newline if the result of the provided method is not null.
	/// </summary>
	public static StringBuilder AppendLine(this StringBuilder sb, Func<string?> textRetriever, bool trimEnd = false)
	{
		var text = textRetriever();

		if (text is not null)
		{
			sb.AppendLine(text);
			if (trimEnd) sb.TrimEnd();
		}

		return sb;
	}
}