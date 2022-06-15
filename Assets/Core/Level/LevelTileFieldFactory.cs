using UnityEngine;
using WalkingBuddies.Core.Card;

namespace WalkingBuddies.Core.Level
{
	public class LevelTileFieldFactory
	{
		private readonly TileCardKinds[,] field;
		private readonly GameObject parent;
		private readonly float scale;

		public LevelTileFieldFactory(
			TileCardKinds[,] field,
			GameObject parent,
			float scale = 1
		)
		{
			this.field = field;
			this.parent = parent;
			this.scale = scale;
		}

		public GameObject[,] Create() => Create(field, parent, scale);

		public static GameObject[,] Create(
			TileCardKinds[,] field,
			GameObject parent,
			float scale = 1
		)
		{
			var (rowCount, colCount) = (field.GetLength(0), field.GetLength(1));

			var tiles = new GameObject[rowCount, colCount];

			for (int rowI = 0, l = rowCount; rowI < l; ++rowI)
			{
				for (int colI = 0, ll = colCount; colI < ll; ++colI)
				{
					var kind = field[rowI, colI];
					var tile = LevelTileFactory.Create(kind);

					tile.transform.SetParent(parent.transform);
					tile.transform.localScale *= scale;
					tile.transform.localPosition = new Vector3(
						colI * scale,
						1,
						rowI * scale
					);

					tiles[rowI, colI] = tile;
				}
			}

			return tiles;
		}
	}
}
