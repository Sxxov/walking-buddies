using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using WalkingBuddies.Core.Ar;
using WalkingBuddies.Core.Card;
using WalkingBuddies.Core.Tweening;
using WalkingBuddies.Core.Utilty;

namespace WalkingBuddies.Core.Level
{
	public class LevelController : MonoBehaviour
	{
		[SerializeField]
		private TargetController targetController = null!;

		private GameObject[,]? tileField;

		private BuddiesStore<GameObject> buddies = new();

		private bool isHidden = true;

		private Coroutine? currTweenCoroutine;

		private readonly CancellationTokenSource cancellationTokenSource =
			new();

		void Start()
		{
			var field = LevelRepository.GetField(0);
			tileField = LevelTileFieldFactory.Create(field, gameObject);

			buddies.player = (GameObject)Instantiate(
				Resources.Load("Prefabs/Player", typeof(GameObject))
			);
			buddies.turtle = (GameObject)Instantiate(
				Resources.Load("Prefabs/Turtle", typeof(GameObject))
			);
			buddies.bird = (GameObject)Instantiate(
				Resources.Load("Prefabs/Bird", typeof(GameObject))
			);

			foreach (var buddy in buddies)
			{
				buddy.transform.SetParent(transform);
			}

			targetController.OnGridUpdate += async (cardGrid) =>
			{
				var levelField = new LevelField(field);
				var tokens = CardTokeniser.Tokenise(cardGrid);
				var result = await CardInterpreter.InterpretAsync(
					tokens,
					cancellationTokenSource.Token,
					levelField
				);

				var success = levelField.success;

				if (currTweenCoroutine is not null)
				{
					StopCoroutine(currTweenCoroutine);
				}

				currTweenCoroutine = StartCoroutine(Animate(result));
			};
		}

		void Update()
		{
			if (targetController.cardGrid.nodes.Length < 1)
			{
				if (!isHidden)
				{
					foreach (var tile in tileField!)
					{
						tile.SetActive(false);
					}

					foreach (var buddy in buddies)
					{
						buddy.SetActive(false);
					}

					isHidden = true;
				}

				return;
			}

			if (isHidden)
			{
				foreach (var tile in tileField!)
				{
					tile.SetActive(true);
				}

				foreach (var buddy in buddies)
				{
					buddy.SetActive(true);
				}

				isHidden = false;
			}

			var cardAveragePosition =
				targetController.cardGrid.nodes.Aggregate(
					Vector3.zero,
					(prev, curr) => prev + curr.transform.position
				) / targetController.cardGrid.nodes.Length;

			var cardAverageRotation = AverageUtility.AverageQuaternion(
				targetController.cardGrid.nodes
					.Select((v) => v.transform.rotation)
					.ToArray()
			);

			transform.SetPositionAndRotation(
				cardAveragePosition,
				cardAverageRotation
			);
		}

		void OnDestroy()
		{
			cancellationTokenSource.Cancel();
		}

		private IEnumerator Animate(
			List<BuddiesStore<Vector2Int>> buddyPositionsList,
			float y = 1.2f,
			float durationSeconds = .3f,
			Bezier? bezier = null
		)
		{
			bezier ??= new Bezier(0.22, 1, 0.36, 1);

			for (int i = 1, l = buddyPositionsList.Count; i < l; ++i)
			{
				var prevPositions = buddyPositionsList[i - 1];
				var currPositions = buddyPositionsList[i];

				var p1s = new BuddiesStore<Vector3>()
				{
					player = new Vector3(
						prevPositions.player.x,
						y,
						prevPositions.player.y
					),
					turtle = new Vector3(
						prevPositions.turtle.x,
						y,
						prevPositions.turtle.y
					),
					bird = new Vector3(
						prevPositions.bird.x,
						y,
						prevPositions.bird.y
					),
				};
				var p2s = new BuddiesStore<Vector3>()
				{
					player = new Vector3(
						currPositions.player.x,
						y,
						currPositions.player.y
					),
					turtle = new Vector3(
						currPositions.turtle.x,
						y,
						currPositions.turtle.y
					),
					bird = new Vector3(
						currPositions.bird.x,
						y,
						currPositions.bird.y
					),
				};

				var tweens = new BuddiesStore<IEnumerator<Vector3>>()
				{
					player = new Tween(
						p1s.player,
						p2s.player,
						durationSeconds,
						bezier
					).Run(),
					turtle = new Tween(
						p1s.turtle,
						p2s.turtle,
						durationSeconds,
						bezier
					).Run(),
					bird = new Tween(
						p1s.bird,
						p2s.bird,
						durationSeconds,
						bezier
					).Run(),
				};

				var shouldPlayerMove = tweens.player.MoveNext();
				var shouldTurtleMove = tweens.turtle.MoveNext();
				var shouldBirdMove = tweens.bird.MoveNext();

				while (shouldPlayerMove || shouldTurtleMove || shouldBirdMove)
				{
					if (shouldPlayerMove)
					{
						buddies.player.transform.localPosition = tweens
							.player
							.Current;
						shouldPlayerMove = tweens.player.MoveNext();
					}
					else
					{
						buddies.player.transform.localPosition = p2s.player;
					}

					if (shouldTurtleMove)
					{
						buddies.turtle.transform.localPosition = tweens
							.turtle
							.Current;
						shouldTurtleMove = tweens.turtle.MoveNext();
					}
					else
					{
						buddies.turtle.transform.localPosition = p2s.turtle;
					}

					if (shouldBirdMove)
					{
						buddies.bird.transform.localPosition = tweens
							.bird
							.Current;
						shouldBirdMove = tweens.bird.MoveNext();
					}
					else
					{
						buddies.bird.transform.localPosition = p2s.bird;
					}

					yield return null;
				}
			}

			var lastPositions = buddyPositionsList.Last();
			buddies.player.transform.localPosition = new Vector3(
				lastPositions.player.x,
				y,
				lastPositions.player.y
			);
			buddies.turtle.transform.localPosition = new Vector3(
				lastPositions.turtle.x,
				y,
				lastPositions.turtle.y
			);
			buddies.bird.transform.localPosition = new Vector3(
				lastPositions.bird.x,
				y,
				lastPositions.bird.y
			);
		}
	}
}
