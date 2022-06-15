using WalkingBuddies.Core.Card;

namespace WalkingBuddies.Core.Level
{
	public class LevelRepository
	{
		public static LevelHint[] hints = new LevelHint[]
		{
			new LevelHint()
			{
				heading =
					"turtle cannot pass through hills! find a way to swap turtle away when he hits one.",
				description =
					@"you can use the <Turtle> card to swap with turtle, or the <Player> card, as @Turtle, to swap with the player.

hint: 
@Turtle [If] |Hill| [Then] <…>",
				url = "https://youtu.be/dQw4w9WgXcQ",
			}
		};

		public static LevelInfo[] infos = new LevelInfo[]
		{
			new LevelInfo()
			{
				name = "silly hilly",
				story =
					@"the buddies managed to walk into a land with grass, sea, rainclouds, & hills!

unfortunately for turtle, he can't climb over hills...

find a way to get everyone across these lands to reach the land of oo!",
				firstEncounterKinds = new TileCardKinds[]
				{
					TileCardKinds.HILL,
				}
			}
		};

		public static TileCardKinds[][,] fields = new TileCardKinds[][,]
		{
			new TileCardKinds[,]
			{
				{
					TileCardKinds.GRASS,
					TileCardKinds.GRASS,
					TileCardKinds.SEA,
					TileCardKinds.GRASS,
					TileCardKinds.GRASS,
					TileCardKinds.GRASS,
					TileCardKinds.GRASS,
					TileCardKinds.GRASS,
					TileCardKinds.HILL
				},
				{
					TileCardKinds.RAINCLOUD,
					TileCardKinds.RAINCLOUD,
					TileCardKinds.GRASS,
					TileCardKinds.GRASS,
					TileCardKinds.GRASS,
					TileCardKinds.SEA,
					TileCardKinds.GRASS,
					TileCardKinds.RAINCLOUD,
					TileCardKinds.HILL
				},
				{
					TileCardKinds.GRASS,
					TileCardKinds.GRASS,
					TileCardKinds.GRASS,
					TileCardKinds.SEA,
					TileCardKinds.HILL,
					TileCardKinds.GRASS,
					TileCardKinds.GRASS,
					TileCardKinds.GRASS,
					TileCardKinds.GRASS
				},
			},
		};

		public static TileCardKinds[,] GetField(int levelIndex) =>
			fields[levelIndex];

		public static LevelInfo GetInfo(int levelIndex) => infos[levelIndex];

		public static LevelHint GetHint(int levelIndex) => hints[levelIndex];
	}
}
