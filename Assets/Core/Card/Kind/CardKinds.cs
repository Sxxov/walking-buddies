namespace WalkingBuddies.Core.Card
{
	public enum CardKinds
	{
		WALK = ActionCardKinds.WALK,
		SWAP = ActionCardKinds.SWAP,
		TURTLE = BuddyCardKinds.TURTLE,
		BIRD = BuddyCardKinds.BIRD,
		IF = SpecialCardKinds.IF,
		THEN = SpecialCardKinds.THEN,
		ELSE = SpecialCardKinds.ELSE,
		UNTIL = SpecialCardKinds.UNTIL,
		GRASS = TileCardKinds.GRASS,
		SEA = TileCardKinds.SEA,
		RAINCLOUD = TileCardKinds.RAINCLOUD,
		HILL = TileCardKinds.HILL,
	}
}
