using System;
using System.Linq;

namespace WalkingBuddies.Core.Card
{
	public class ParseException : Exception
	{
		public ParseException(
			CardKinds[] input,
			int i,
			params CardKinds[] expecteds
		)
			: base(
				string.Join(
					"\n",
					input.Select(
						(kind, kindI) =>
							$"{kindI, 2}| {(kindI == i ? $"{CardTokeniser.GetNotatedName(kind)} ← (!){(expecteds is null ? "" : $"Expected {string.Join(" | ", expecteds.Select((expected) => CardTokeniser.GetNotatedName(expected)))}")}" : CardTokeniser.GetNotatedName(kind))}"
					)
				) + (i >= input.Length ? "\n← (!) Unexpected end of input" : "")
			) { }

		public ParseException(
			CardKinds[] input,
			int i,
			Func<CardKinds, bool>? predicate = null
		)
			: base(
				string.Join(
					"\n",
					input.Select(
						(kind, kindI) =>
							$"{kindI, 2}| {(kindI == i ? $"{CardTokeniser.GetNotatedName(kind)} ← (!){(predicate is null ? "" : $"Expected to pass predicate({predicate}")})" : CardTokeniser.GetNotatedName(kind))}"
					)
				) + (i >= input.Length ? "\n← (!) Unexpected end of input" : "")
			) { }
	}
}
