using System;

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
					? $"Expected the next card to pass predicate({predicate}), but there were no more cards at the end of the input"
					: $"Expected to pass predicate({predicate}), but got {CardTokeniser.GetNotatedName(input[i])}, which didn't pass{(message.Length > 0 ? $" — {message}" : "")}",
				input,
				i
			)
		{
			this.predicate = predicate;
		}
	}
}
