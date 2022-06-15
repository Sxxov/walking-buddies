using System;
using System.Linq;

namespace WalkingBuddies.Core.Card
{
	public class CardKindRepository
	{
		public static readonly SpecificCardKindsStore specificKinds =
			new()
			{
				action = (ActionCardKinds[])
					Enum.GetValues(typeof(ActionCardKinds)),
				buddy = (BuddyCardKinds[])
					Enum.GetValues(typeof(BuddyCardKinds)),
				special = (SpecialCardKinds[])
					Enum.GetValues(typeof(SpecialCardKinds)),
				tile = (TileCardKinds[])Enum.GetValues(typeof(TileCardKinds))
			};

		public static readonly CardKindsStore kinds =
			new()
			{
				action = (CardKinds[])Enum.GetValues(typeof(ActionCardKinds)),
				buddy = (CardKinds[])Enum.GetValues(typeof(BuddyCardKinds)),
				special = (CardKinds[])Enum.GetValues(typeof(SpecialCardKinds)),
				tile = (CardKinds[])Enum.GetValues(typeof(TileCardKinds))
			};

		public static bool IsTile(CardKinds kind) => kinds.tile.Contains(kind);

		public static bool IsAction(CardKinds kind) =>
			kinds.action.Contains(kind);

		public static bool IsBuddy(CardKinds kind) =>
			kinds.buddy.Contains(kind);

		public static bool IsSpecial(CardKinds kind) =>
			kinds.special.Contains(kind);
	}
}
