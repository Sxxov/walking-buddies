using System.Collections.Generic;
using System;
using UnityEngine;
using WalkingBuddies.Core.Card;

namespace WalkingBuddies.Core.Level
{
	public delegate void OnLevelBuddyWalk(
		BuddiesStore<bool> didWalk,
		LevelField levelField
	);

	public delegate void OnLevelBuddySwap(
		BuddyCardKinds buddy,
		LevelField levelField
	);

	public delegate void OnLevelEnd(
		BuddiesStore<bool> success,
		LevelField levelField
	);

	public class LevelField
	{
		public event OnLevelBuddyWalk? OnBuddyWalk;

		public event OnLevelBuddySwap? OnBuddySwap;

		public event OnLevelEnd? OnEnd;

		private readonly TileCardKinds[,] field;

		public bool hasEnded { get; private set; }

		public static BuddiesStore<Vector2Int> buddyPositionsInit
		{
			get =>
				new()
				{
					player = new(0, 1),
					turtle = new(0, 0),
					bird = new(0, 2),
				};
		}

		public BuddiesStore<bool> success
		{
			get =>
				new()
				{
					bird = buddyCurrTiles.bird == null,
					player = buddyCurrTiles.player == null,
					turtle = buddyCurrTiles.turtle == null,
				};
		}

		public BuddiesStore<TileCardKinds?> buddyCurrTiles = new();

		public BuddiesStore<TileCardKinds?> buddyNextTiles = new();

		public BuddiesStore<Vector2Int> buddyPositions = buddyPositionsInit;

		public readonly List<BuddiesStore<Vector2Int>> buddyPositionsHistory =
			new();

		public LevelField(TileCardKinds[,] field)
		{
			if (field.GetLength(0) != 3)
			{
				throw new ArgumentOutOfRangeException(
					nameof(field),
					$"Attempted to instantiate LevelField with levelField containing {field.GetLength(0)} rows. LevelField expects 3 rows"
				);
			}

			this.field = field;

			buddyPositionsHistory.Add(CloneBuddiesStoreVector2(buddyPositions));

			UpdateTile();
		}

		public void Swap(BuddyCardKinds buddy)
		{
			Debug.Log($"swap: {buddy}");
			Debug.Log(
				$"buddyPositions: player = [{buddyPositions.player.x}, {buddyPositions.player.y}], turtle = [{buddyPositions.turtle.x}, {buddyPositions.turtle.y}], bird = [{buddyPositions.bird.x}, {buddyPositions.bird.y}]"
			);

			var (prevPlayerX, prevPlayerY) = (
				buddyPositions.player.x,
				buddyPositions.player.y
			);

			switch (buddy)
			{
				case BuddyCardKinds.TURTLE:

					{
						(buddyPositions.player.x, buddyPositions.player.y) = (
							buddyPositions.turtle.x,
							buddyPositions.turtle.y
						);
						(buddyPositions.turtle.x, buddyPositions.turtle.y) = (
							prevPlayerX,
							prevPlayerY
						);
					}
					break;
				case BuddyCardKinds.BIRD:

					{
						(buddyPositions.player.x, buddyPositions.player.y) = (
							buddyPositions.bird.x,
							buddyPositions.bird.y
						);
						(buddyPositions.bird.x, buddyPositions.bird.y) = (
							prevPlayerX,
							prevPlayerY
						);
					}
					break;
			}

			buddyPositionsHistory.Add(CloneBuddiesStoreVector2(buddyPositions));

			if (OnBuddySwap is not null)
			{
				OnBuddySwap.Invoke(buddy, this);
			}
		}

		public void Walk()
		{
			var didWalk = new BuddiesStore<bool>();
			var colCount = field.GetLength(1);

			if (
				buddyPositions.player.x + 1 == colCount
				|| (
					buddyPositions.player.x + 1 < colCount
					&& field[
						buddyPositions.player.y,
						buddyPositions.player.x + 1
					] != TileCardKinds.SEA
				)
			)
			{
				buddyPositions.player.x += 1;
				didWalk.player = true;
			}

			if (
				buddyPositions.turtle.x + 1 == colCount
				|| (
					buddyPositions.turtle.x + 1 < colCount
					&& field[
						buddyPositions.turtle.y,
						buddyPositions.turtle.x + 1
					] != TileCardKinds.HILL
				)
			)
			{
				buddyPositions.turtle.x += 1;
				didWalk.turtle = true;
			}

			if (
				buddyPositions.bird.x + 1 == colCount
				|| (
					buddyPositions.bird.x + 1 < colCount
					&& field[buddyPositions.bird.y, buddyPositions.bird.x + 1]
						!= TileCardKinds.RAINCLOUD
				)
			)
			{
				buddyPositions.bird.x += 1;
				didWalk.bird = true;
			}

			UpdateTile();

			Debug.Log(
				$"didWalk: player = {didWalk.player}, turtle = {didWalk.turtle}, bird = {didWalk.bird}"
			);

			if (didWalk.player || didWalk.turtle || didWalk.bird)
			{
				buddyPositionsHistory.Add(
					CloneBuddiesStoreVector2(buddyPositions)
				);
				OnBuddyWalk?.Invoke(didWalk, this);
			}
			else
			{
				End();
			}
		}

		public void End()
		{
			hasEnded = true;
			OnEnd?.Invoke(success, this);
		}

		private void UpdateTile()
		{
			var colCount = field.GetLength(1);

			buddyCurrTiles.player =
				buddyPositions.player.x >= colCount
					? null
					: field[buddyPositions.player.y, buddyPositions.player.x];
			buddyCurrTiles.turtle =
				buddyPositions.turtle.x >= colCount
					? null
					: field[buddyPositions.turtle.y, buddyPositions.turtle.x];
			buddyCurrTiles.bird =
				buddyPositions.bird.x >= colCount
					? null
					: field[buddyPositions.bird.y, buddyPositions.bird.x];

			buddyNextTiles.player =
				buddyPositions.player.x + 1 >= colCount
					? null
					: field[
						buddyPositions.player.y,
						buddyPositions.player.x + 1
					];
			buddyNextTiles.turtle =
				buddyPositions.turtle.x + 1 >= colCount
					? null
					: field[
						buddyPositions.turtle.y,
						buddyPositions.turtle.x + 1
					];
			buddyNextTiles.bird =
				buddyPositions.bird.x + 1 >= colCount
					? null
					: field[buddyPositions.bird.y, buddyPositions.bird.x + 1];
		}

		private static BuddiesStore<Vector2Int> CloneBuddiesStoreVector2(
			BuddiesStore<Vector2Int> store
		)
		{
			return new()
			{
				player = store.player + Vector2Int.zero,
				turtle = store.turtle + Vector2Int.zero,
				bird = store.bird + Vector2Int.zero,
			};
		}
	}
}
