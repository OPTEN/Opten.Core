using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Opten.Core.Extensions
{
	/// <summary>
	/// The HTML Extensions.
	/// </summary>
	public static class HtmlExtensions
	{

		/// <summary>
		/// Converts the line breaks to array.
		/// </summary>
		/// <param name="html">The HTML.</param>
		/// <returns></returns>
		public static string[] ConvertLineBreaksToArray(this string html)
		{
			if (string.IsNullOrWhiteSpace(html)) return new string[0];

			return html.Split(new string[] { "\n\r", "\n" }, StringSplitOptions.None); // None => empty are <br/>
		}

		/// <summary>
		/// Truncates the text and checks for white space.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="startIndex">The start index.</param>
		/// <param name="length">The length.</param>
		/// <param name="suffix">The suffix.</param>
		/// <returns></returns>
		public static string TruncateAndCheckForWhiteSpace(this string text, int startIndex = 0, int length = 50, string suffix = "...")
		{
			length = length - suffix.Length;

			text = text.NullCheckTrim();
			if (string.IsNullOrWhiteSpace(text)) return string.Empty;

			if (text.Length <= length) return text;

			string truncated = text.Substring(startIndex, length);
			if (Char.IsWhiteSpace(truncated[truncated.Length - 1]) == false)
				truncated = text.TruncateAndCheckForWhiteSpace(startIndex, length + 1, string.Empty);

			return truncated.Trim() + suffix;
		}

		/// <summary>
		/// Removes the HTML tags.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		public static string RemoveHtmlTags(this string text)
		{
			return Regex.Replace(text, "<.*?>", string.Empty);
		}

	}
}