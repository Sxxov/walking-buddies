using System.Collections.Generic;
using System;
using System.Linq;
using WalkingBuddies.Core.Level;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace WalkingBuddies.Core.Card
{
	public class CardInterpreter
	{
		public static async Task<List<BuddiesStore<Vector2Int>>> InterpretAsync(
			CardKinds[][] tokens,
			LevelField levelField
		) => await InterpretAsync(tokens, null, levelField);

		public static async Task<List<BuddiesStore<Vector2Int>>> InterpretAsync(
			CardKinds[][] tokens,
			CancellationToken? cancellationToken,
			LevelField levelField
		)
		{
			var source =
				new TaskCompletionSource<(List<
						BuddiesStore<Vector2Int>
					>?, ParseException?)>();
			var thread = new Thread(
				new ThreadStart(() =>
				{
					try
					{
						source.SetResult(
							(
								Interpret(
									tokens,
									cancellationToken,
									levelField
								),
								null
							)
						);
					}
					catch (ParseException e)
					{
						source.SetResult((null, e));
					}
				})
			);

			thread.Start();

			var (result, error) = await source.Task;

			if (error is not null)
			{
				throw error;
			}

			return result!;
		}

		public static List<BuddiesStore<Vector2Int>> Interpret(
			CardKinds[][] tokens,
			LevelField levelField
		) => Interpret(tokens, null, levelField);

		public static List<BuddiesStore<Vector2Int>> Interpret(
			CardKinds[][] tokens,
			CancellationToken? cancellationToken,
			LevelField levelField
		)
		{
			// check for repeating patterns in position as it interprets
			// returns if it detects repeating
			var seen = new List<BuddiesStore<Vector2Int>>();
			var patternStartIndexToCurrIncrementAndTargetIndex =
				new Dictionary<int, (int currIncrement, int targetIndex)>();

			while (
				!levelField.hasEnded
				&& !(
					cancellationToken is not null
					&& cancellationToken.Value.IsCancellationRequested
				)
			)
			{
				var currBuddyPositions = new BuddiesStore<Vector2Int>()
				{
					player = levelField.buddyPositions.player + Vector2Int.zero,
					turtle = levelField.buddyPositions.turtle + Vector2Int.zero,
					bird = levelField.buddyPositions.bird + Vector2Int.zero,
				};

				foreach (
					var (
						startI,
						(currInc, targetI)
					) in patternStartIndexToCurrIncrementAndTargetIndex.ToDictionary(
						entry => entry.Key,
						entry => entry.Value
					)
				)
				{
					var currI = startI + currInc + 1;

					if (
						currI < seen.Count
						&& seen[currI].player == currBuddyPositions.player
						&& seen[currI].turtle == currBuddyPositions.turtle
						&& seen[currI].bird == currBuddyPositions.bird
					)
					{
						// is repeating
						if (currI == targetI)
						{
							Debug.Log($"repeating: {currI} == {targetI}");
							levelField.End();

							goto ret;
						}
						else
						{
							patternStartIndexToCurrIncrementAndTargetIndex.Remove(
								startI
							);
						}

						patternStartIndexToCurrIncrementAndTargetIndex.Add(
							startI,
							(currInc + 1, targetI)
						);
					}
				}

				for (int i = 0, l = seen.Count; i < l; ++i)
				{
					var prevBuddyPositions = seen[i];

					if (
						currBuddyPositions.player == prevBuddyPositions.player
						&& currBuddyPositions.turtle
							== prevBuddyPositions.turtle
						&& currBuddyPositions.bird == prevBuddyPositions.bird
					)
					{
						patternStartIndexToCurrIncrementAndTargetIndex[i] = (
							0,
							seen.Count
						);
					}
				}

				seen.Add(currBuddyPositions);

				InterpretOnce(tokens, levelField);
			}

			ret:
			return levelField.buddyPositionsHistory!;
		}

		private static void InterpretOnce(
			CardKinds[][] tokens,
			LevelField levelField
		)
		{
			foreach (var input in tokens)
			{
				var i = 0;

				while (
					RunAction(input, ref i, levelField)
					|| RunSpecial(input, ref i, levelField)
				) { }

				if (i < input.Length)
				{
					throw new ParseException(
						input,
						i,
						CardKindRepository.kinds.action
							.Concat(CardKindRepository.kinds.special)
							.ToArray()
					);
				}
			}
		}

		private static (TileCardKinds tile, BuddyCardKinds? buddy) Tile(
			CardKinds[] input,
			ref int i
		) =>
			(
				tile: (TileCardKinds)Eat(
					input,
					ref i,
					CardKindRepository.kinds.tile
				),
				buddy: (BuddyCardKinds?)Eat(
					input,
					ref i,
					false,
					CardKindRepository.kinds.buddy
				)
			);

		private static void Swap(
			CardKinds[] input,
			ref int i,
			LevelField levelField
		)
		{
			// Debug.Log("swap");
			Eat(input, ref i, CardKinds.SWAP);

			var buddy = Eat(input, ref i, CardKindRepository.kinds.buddy);

			levelField.Swap((BuddyCardKinds)buddy);
		}

		private static void Walk(
			CardKinds[] input,
			ref int i,
			LevelField levelField
		)
		{
			// Debug.Log("walk");
			Eat(input, ref i, CardKinds.WALK);

			levelField.Walk();
		}

		private static void If(
			CardKinds[] input,
			ref int i,
			LevelField levelField
		)
		{
			Eat(input, ref i, CardKinds.IF);

			var (tile, buddy) = Tile(input, ref i);

			Eat(input, ref i, CardKinds.THEN);

			if (i >= input.Length)
			{
				throw new ParseException(
					input,
					i,
					CardKindRepository.kinds.action
						.Concat(CardKindRepository.kinds.special)
						.ToArray()
				);
			}

			if (
				buddy is null
					? tile == levelField.buddyNextTiles.player
					: tile
						== levelField.buddyNextTiles.Get((BuddyCardKinds)buddy!)
			)
			{
				while (
					RunAction(input, ref i, levelField)
					|| RunSpecial(input, ref i, levelField)
				) { }

				i = input.Length;
			}
			else
			{
				EatWhen(input, ref i, CardKinds.ELSE);

				++i;

				if (i >= input.Length)
				{
					throw new ParseException(
						input,
						i,
						CardKindRepository.kinds.action
							.Concat(CardKindRepository.kinds.special)
							.ToArray()
					);
				}

				while (
					RunAction(input, ref i, levelField)
					|| RunSpecial(input, ref i, levelField)
				) { }
			}
		}

		private static void Until(
			CardKinds[] input,
			ref int i,
			LevelField levelField
		)
		{
			var untilI = i;
			Eat(input, ref untilI, CardKinds.UNTIL);

			var (tile, buddy) = Tile(input, ref untilI);

			var actionI = untilI;
			EatWhen(input, ref actionI, -1, CardKindRepository.kinds.action);

			while (
				buddy is null
					? tile != levelField.buddyCurrTiles.player
					: tile
						!= levelField.buddyCurrTiles.Get((BuddyCardKinds)buddy!)
			)
			{
				RunAction(input, ref actionI, levelField);
			}
		}

		private static bool RunSpecial(
			CardKinds[] input,
			ref int i,
			LevelField levelField
		)
		{
			var hasRun = false;

			while (i < input.Length)
			{
				switch (input[i])
				{
					case CardKinds.IF:
						If(input, ref i, levelField);
						hasRun = true;
						break;
					case CardKinds.UNTIL:
						Until(input, ref i, levelField);
						hasRun = true;
						break;
					default:
						return hasRun;
				}
			}

			return hasRun;
		}

		private static bool RunAction(
			CardKinds[] input,
			ref int i,
			LevelField levelField
		)
		{
			var hasRun = false;

			while (i < input.Length)
			{
				switch (input[i])
				{
					case CardKinds.SWAP:
						Swap(input, ref i, levelField);
						hasRun = true;
						break;
					case CardKinds.WALK:
						Walk(input, ref i, levelField);
						hasRun = true;
						break;
					default:
						return hasRun;
				}
			}

			return hasRun;
		}

		private static CardKinds Eat(
			CardKinds[] input,
			ref int i,
			params CardKinds[] expecteds
		) => (CardKinds)Eat(input, ref i, true, expecteds)!;

		private static CardKinds? Eat(
			CardKinds[] input,
			ref int i,
			bool isRequired = true,
			params CardKinds[] expecteds
		)
		{
			if (!Is(input, i, isRequired, expecteds))
			{
				return null;
			}

			return input[i++];
		}

		private static CardKinds Eat(
			CardKinds[] input,
			ref int i,
			Func<CardKinds, bool>? predicate = null
		) => (CardKinds)Eat(input, ref i, predicate, true)!;

		private static CardKinds? Eat(
			CardKinds[] input,
			ref int i,
			Func<CardKinds, bool>? predicate = null,
			bool isRequired = true
		)
		{
			if (!Is(input, i, predicate, isRequired))
			{
				return null;
			}

			return input[i++];
		}

		private static CardKinds? EatWhen(
			CardKinds[] input,
			ref int i,
			params CardKinds[] expecteds
		) => EatWhen(input, ref i, 1, expecteds);

		private static CardKinds? EatWhen(
			CardKinds[] input,
			ref int i,
			int direction = 1,
			params CardKinds[] expecteds
		)
		{
			var ii = i;

			while (
				ii < input.Length
				&& ii >= 0
				&& !expecteds.Any((expected) => input[ii] == expected)
			)
			{
				ii += direction;
			}

			i = ii;

			if (ii >= input.Length && ii < 0)
			{
				return null;
			}

			return input[ii];
		}

		private static CardKinds? EatWhen(
			CardKinds[] input,
			ref int i,
			Func<CardKinds, bool> predicate,
			int direction = 1
		)
		{
			var ii = i;

			while (ii < input.Length && ii >= 0 && !predicate(input[ii]))
			{
				ii += direction;
			}

			i = ii;

			if (ii >= input.Length && ii < 0)
			{
				return null;
			}

			return input[ii];
		}

		private static bool Is(
			CardKinds[] input,
			int i,
			bool isRequired = true,
			params CardKinds[] expecteds
		)
		{
			if (i < 0 || i >= input.Length)
			{
				if (isRequired)
				{
					throw new ParseException(input, i, expecteds);
				}
				else
				{
					return false;
				}
			}

			if (!expecteds.Any((expected) => input[i] == expected))
			{
				if (isRequired)
				{
					throw new ParseException(input, i, expecteds);
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		private static bool Is(
			CardKinds[] input,
			int i,
			Func<CardKinds, bool>? predicate = null,
			bool isRequired = true
		)
		{
			if (i < 0 || i >= input.Length)
			{
				if (isRequired)
				{
					throw new ParseException(input, i, predicate);
				}
				else
				{
					return false;
				}
			}
			if (predicate is not null && !predicate(input[i]))
			{
				if (isRequired)
				{
					throw new ParseException(input, i, predicate);
				}
				else
				{
					return false;
				}
			}

			return true;
		}
	}
}
