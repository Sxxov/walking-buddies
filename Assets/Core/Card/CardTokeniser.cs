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

		public static CardNode NodeAt(CardGrid grid, int x, int y)
		{
			var colI = 0;
			var currCol = grid.head;
			while (colI++ < x)
			{
				if (currCol is null)
				{
					break;
				}

				currCol = currCol.right;
			}

			if (currCol is null)
			{
				throw new IndexOutOfRangeException(
					$"Attempted to access column({x}) outside of grid column bounds({colI - 1})"
				);
			}

			var rowI = 0;
			var currRow = currCol;
			while (rowI++ < x)
			{
				if (currRow is null)
				{
					break;
				}

				var prevKind = currRow.kind;

				currRow = currRow.bottom;

				if (currRow?.kind == prevKind)
				{
					currRow = currRow.bottom;
				}
			}

			if (currRow is null)
			{
				throw new IndexOutOfRangeException(
					$"Attempted to access row({y}) outside of grid column's({colI}) row bounds({rowI - 1})"
				);
			}

			return currRow;
		}

		public static CardKinds KindAt(CardGrid grid, int x, int y) =>
			NodeAt(grid, x, y).kind;

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
