using UnityEngine;
using WalkingBuddies.Core.Card;

namespace WalkingBuddies.Core.Level
{
	public class LevelTileFactory
	{
		private readonly TileCardKinds kind;

		public LevelTileFactory(TileCardKinds kind)
		{
			this.kind = kind;
		}

		public GameObject Create() => Create(kind);

		public static GameObject Create(TileCardKinds kind)
		{
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			var renderer = cube.GetComponent<Renderer>();

			cube.transform.localScale = new Vector3(1, .2f, 1);

			switch (kind)
			{
				case TileCardKinds.GRASS:
					renderer.material = (Material)
						Resources.Load("Materials/Grass", typeof(Material));
					break;
				case TileCardKinds.SEA:
					renderer.material = (Material)
						Resources.Load("Materials/Sea", typeof(Material));
					break;
				case TileCardKinds.HILL:
					renderer.material = (Material)
						Resources.Load("Materials/Hill", typeof(Material));
					break;
				case TileCardKinds.RAINCLOUD:
					renderer.material = (Material)
						Resources.Load("Materials/Raincloud", typeof(Material));
					break;
			}

			return cube;
		}
	}
}
