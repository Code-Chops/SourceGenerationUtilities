﻿namespace CodeChops.SourceGeneration.Utilities.Extensions;

/// <summary>
/// Provides extensions on <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
	/// <summary>
	/// <see cref="Environment.NewLine"/>, but escaped for matching that from a <see cref="Regex"/>.
	/// </summary>
	private static readonly string RegexNewLine = Regex.Escape(Environment.NewLine);

	private static ImmutableArray<char> Base32Alphabet { get; } = "0123456789ABCDEFGHJKMNPQRSTVWXYZ".ToImmutableArray();

	/// <summary>
	/// Returns the input with the first character converted to uppercase.
	/// </summary>
	public static string ToTitleCase(this string source)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));

		if (source.Length == 0 || Char.IsUpper(source[0]))
			return source;

		var chars = new Char[source.Length];
		chars[0] = Char.ToUpperInvariant(source[0]);
		source.CopyTo(1, chars, 1, source.Length - 1);

		return new String(chars);
	}

	/// <summary>
	/// Normalizes the whitespace for the given C# source code as much as possible.
	/// </summary>
	public static string NormalizeWhitespace(this string source)
	{
		source = source.TrimStart(); // Remove starting whitespace
		source = Regex.Replace(source, @"\r?\n", Environment.NewLine); // Normalize line endings for the executing OS
		source = Regex.Replace(source, @"\n[ \t]+(?=[\r\n])", "\n"); // Remove needless tabs from empty lines
		source = Regex.Replace(source, $"(?:{RegexNewLine}){{3,}}", $"{Environment.NewLine}{Environment.NewLine}"); // Remove needless whitespace between paragraphs
		source = Regex.Replace(source, $"{{(?:{RegexNewLine}\t* *)+({RegexNewLine}\t* *)(?=\\S)", $"{{$1"); // Remove needless whitespace after opening braces
		source = Regex.Replace(source, $"(\\S)(?:{RegexNewLine}\t* *)+({RegexNewLine}\t* *)(?=}})", $"$1$2"); // Remove needless whitespace before closing braces
		source = Regex.Replace(source, $"(</summary>)(?:{RegexNewLine})+({RegexNewLine})", $"$1$2"); // Remove needless whitespace after summaries
		source = Regex.Replace(source, $"](?:{RegexNewLine})+({RegexNewLine}\t* *)\\[", $"]$1["); // Remove needless whitespace between attributes

		return source;
	}

	/// <summary>
	/// Gets a stable hash code (int). 
	/// </summary>
	// ReSharper disable once MemberCanBePrivate.Global
	public static int GetStableHashCode32(this string source)
	{
		var span = source.AsSpan();

		// FNV-1a
		// For its performance, collision resistance, and outstanding distribution:
		// https://softwareengineering.stackexchange.com/a/145633
		unchecked
		{
			// Inspiration: https://gist.github.com/RobThree/25d764ea6d4849fdd0c79d15cda27d61
			// Confirmation: https://gist.github.com/StephenCleary/4f6568e5ab5bee7845943fdaef8426d2

			const uint fnv32Offset = 2166136261;
			const uint fnv32Prime = 16777619;

			var result = fnv32Offset;

			foreach (var t in span)
				result = (result ^ t) * fnv32Prime;

			return (int)result;
		}
	}

	/// <summary>
	/// Gets a stable hash code (ulong). 
	/// </summary>
	// ReSharper disable once MemberCanBePrivate.Global
	public static ulong GetStableHashCode64(this string source)
	{
		var span = source.AsSpan();

		// FNV-1a
		// For its performance, collision resistance, and outstanding distribution:
		// https://softwareengineering.stackexchange.com/a/145633
		unchecked
		{
			// Inspiration: https://gist.github.com/RobThree/25d764ea6d4849fdd0c79d15cda27d61

			const ulong fnv64Offset = 14695981039346656037UL;
			const ulong fnv64Prime = 1099511628211UL;

			var result = fnv64Offset;

			foreach (var t in span)
				result = (result ^ t) * fnv64Prime;

			return result;
		}
	}

	/// <summary>
	/// Gets a stable int hash code as string.
	/// </summary>
	public static string GetStableStringHashCode32(this string source)
	{
		var hashCode = source.GetStableHashCode32();

		var bytes = new byte[8];

		for (var i = 0; i < 4; i++)
			bytes[i] = (byte)(hashCode >> 8 * i);

		var chars = new char[13];
		ToBase32Chars8(bytes, chars.AsSpan());
		var result = new string(chars, 0, 7);

		return result;
	}

	/// <summary>
	/// Gets a stable ulong hash code as string.
	/// </summary>
	public static string GetStableStringHashCode64(this string source)
	{
		var hashCode = source.GetStableHashCode64();

		var bytes = new byte[8];

		for (var i = 0; i < 8; i++)
			bytes[i] = (byte)(hashCode >> 8 * i);

		var chars = new char[13];
		ToBase32Chars8(bytes, chars.AsSpan());
		var result = new string(chars);

		return result;
	}

	/// <summary>
	/// Converts the given 8 bytes to 13 base32 chars.
	/// </summary>
	private static void ToBase32Chars8(ReadOnlySpan<byte> bytes, Span<char> chars)
	{
		System.Diagnostics.Debug.Assert(Base32Alphabet.Length == 32);
		System.Diagnostics.Debug.Assert(bytes.Length >= 8);
		System.Diagnostics.Debug.Assert(chars.Length >= 13);

		var ulongValue = 0UL;
		for (var i = 0; i < 8; i++) ulongValue = (ulongValue << 8) | bytes[i];

		// Can encode 8 bytes as 13 chars
		for (var i = 13 - 1; i >= 0; i--)
		{
			var quotient = ulongValue / 32UL;
			var remainder = ulongValue - 32UL * quotient;
			ulongValue = quotient;
			chars[i] = Base32Alphabet[(int)remainder];
		}
	}
	
	/// <summary>
	/// Only returns the value with a leading space when the provided value is not null.
	/// </summary>
	public static string? Write<T>(this T value)
		=> value is null ? null : $" {value}";

	/// <summary>
	/// Only returns the text when the provided value is not null. 
	/// </summary>
	public static string? Write<T>(this T value, string text)
		=> value is null ? null : text;

	/// <summary>
	/// Only returns the text when the provided value is not null. 
	/// </summary>
	public static string? Write<T>(this T value, Func<T, string> text)
		=> value is null ? null : text.Invoke(value);
}
