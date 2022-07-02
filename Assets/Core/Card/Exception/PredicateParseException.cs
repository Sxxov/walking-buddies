using System;
using System.Linq;

namespace WalkingBuddies.Core.Card
{
	public class PredicateParseException : AbstractParseException
	{
		public Func<CardKinds, bool>? predicate;

		public PredicateParseException(
			CardKinds[] input,
			int i,
			Func<CardKinds, bool>? predicate
		) : this("", input, i, predicate) { }

		public PredicateParseException(
			string message,
			CardKinds[] input,
			int i,
			Func<CardKinds, bool>? predicate
		)
			: base(
				i >= input.Length
					? "Unexpected end of cards"
					: $"Expected to pass predicate({predicate}), but got {CardTokeniser.GetNotatedName(input[i])}, which didn't pass{(message.Length > 0 ? $" — {message}" : "")}",
				input,
				i
			)
		{
			this.predicate = predicate;
		}
	}
}
