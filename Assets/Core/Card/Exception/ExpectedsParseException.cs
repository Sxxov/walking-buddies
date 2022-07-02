using System;
using System.Linq;

namespace WalkingBuddies.Core.Card
{
	public class ExpectedsParseException : AbstractParseException
	{
		public CardKinds[] expecteds;

		public ExpectedsParseException(
			CardKinds[] input,
			int i,
			params CardKinds[] expecteds
		) : this("", input, i, expecteds) { }

		public ExpectedsParseException(
			string message,
			CardKinds[] input,
			int i,
			params CardKinds[] expecteds
		)
			: base(
				i >= input.Length
					? "Unexpected end of cards"
					: $"Expected {string.Join(" | ", expecteds.Select((expected) => CardTokeniser.GetNotatedName(expected)))} but got {CardTokeniser.GetNotatedName(input[i])}{(message.Length > 0 ? $" — {message}" : "")}",
				input,
				i
			)
		{
			this.expecteds = expecteds;
		}
	}
}
