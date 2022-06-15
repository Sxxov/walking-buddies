using System.Collections.Generic;
using System;

namespace WalkingBuddies.Core.Card
{
	public class CardTokeniser
	{
		public static CardKinds[][] Tokenise(CardGrid grid)
		{
			var cols = new List<CardKinds[]>();
			var currCol = grid.head;

			while (currCol is not null)
			{
				var rows = new List<CardKinds>();
				var currRow = currCol;

				while (currRow is not null)
				{
					rows.Add(currRow.kind);

					var prevKind = currRow.kind;

					currRow = currRow.bottom;

					if (currRow?.kind == prevKind)
					{
						currRow = currRow.bottom;
					}
				}

				cols.Add(rows.ToArray());

				currCol = currCol.right;
			}

			return cols.ToArray();
		}

		public static CardKinds ConvertNameToKind(string name)
		{
			var nameUpper = name.ToUpper();

			foreach (var enumName in Enum.GetNames(typeof(CardKinds)))
			{
				if (nameUpper.StartsWith(enumName))
				{
					return (CardKinds)Enum.Parse(typeof(CardKinds), enumName);
				}
			}

			throw new ArgumentException(
				$"Name({name}) is not a kind of card",
				nameof(name)
			);
		}

		public static string ConvertKindToName(CardKinds kind) =>
			kind.ToString();

		public static string GetKindPrefix(CardKinds kind) =>
			kind switch
			{
				{ } when CardKindRepository.IsAction(kind) => "<",
				{ } when CardKindRepository.IsBuddy(kind) => "@",
				{ } when CardKindRepository.IsSpecial(kind) => "[",
				{ } when CardKindRepository.IsTile(kind) => "|",
				_ => throw new ArgumentException($"Unknown kind({kind})"),
			};

		public static string GetKindSuffix(CardKinds kind) =>
			kind switch
			{
				{ } when CardKindRepository.IsAction(kind) => ">",
				{ } when CardKindRepository.IsBuddy(kind) => "",
				{ } when CardKindRepository.IsSpecial(kind) => "]",
				{ } when CardKindRepository.IsTile(kind) => "|",
				_ => throw new ArgumentException($"Unknown kind({kind})"),
			};

		public static string GetNotatedName(string name) =>
			$"{GetKindPrefix(ConvertNameToKind(name))}{name}{GetKindSuffix(ConvertNameToKind(name))}";

		public static string GetNotatedName(CardKinds kind) =>
			$"{GetKindPrefix(kind)}{ConvertKindToName(kind)}{GetKindSuffix(kind)}";
	}
}
