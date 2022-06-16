using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WalkingBuddies.Core.Ar;
using WalkingBuddies.Core.Card;
using WalkingBuddies.Core.Tweening;
using WalkingBuddies.Core.Ui;
using WalkingBuddies.Core.Utilty;

namespace WalkingBuddies.Core.Level
{
	public class LevelController : MonoBehaviour
	{
		[SerializeField]
		private TargetController targetController = null!;

		[SerializeField]
		private CanvasBehaviour canvasBehaviour = null!;

		private TileCardKinds[,]? field;

		private GameObject[,]? tileField;

		private BuddiesStore<GameObject> buddies = new();

		private bool isHidden = false;

		private bool isInLevel = false;

		private Coroutine? currTweenCoroutine;

		private readonly CancellationTokenSource cancellationTokenSource =
			new();

		IEnumerator Start()
		{
			field = LevelRepository.GetField(0);
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

			ScheduleAnimate(new List<BuddiesStore<Vector2Int>>());

			isInLevel = true;

			targetController.OnGridUpdate += OnGridUpdate;

			yield return null;

			new Toast(canvasBehaviour)
				.SetHeading("18 grass\n3 seas\n3 rainclouds\n*3 hills*")
				.SetParagraph(
					"the buddies managed to walk into a land with grass, sea, rainclouds, & hills!\n\nunfortunately for turtle, he can't climb over hills…\n\nfind a way to get everyone across these lands to reach the land of oo!"
				)
				.SetIcon("\ue16e")
				.SetButtonIcon("\ue5c8")
				.SetButtonText("start!")
				.Show();
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

		private async void OnGridUpdate(CardGrid cardGrid)
		{
			if (!isInLevel || field is null)
			{
				return;
			}

			try
			{
				var levelField = new LevelField(field);
				var tokens = CardTokeniser.Tokenise(cardGrid);
				var result = await CardInterpreter.InterpretAsync(
					tokens,
					cancellationTokenSource.Token,
					levelField
				);

				var success = levelField.success;

				ScheduleAnimate(result);

				if (success.player && success.turtle && success.bird)
				{
					var toast = new Toast(canvasBehaviour)
						.SetHeading("hooray!")
						.SetParagraph(
							"you helped the buddies through the level & got them to the land of oo"
						)
						.SetIcon("\ue876")
						.SetButtonIcon("\ue5c8")
						.SetButtonText("continue")
						.Show();

					toast.OnVisibilityChange += (isShown) =>
					{
						if (!isShown)
						{
							var scene = SceneManager.GetActiveScene();
							SceneManager.LoadScene(scene.name);
						}
					};
				}
			}
			catch (ParseException e)
			{
				new Toast(canvasBehaviour)
					.SetHeading("uh oh…")
					.SetParagraph(e.Message)
					.SetIcon("\ue001")
					.SetButtonIcon("\ue5cd")
					.SetButtonText("dismiss")
					.Show();
			}
		}

		private void ScheduleAnimate(
			List<BuddiesStore<Vector2Int>> buddyPositionsList,
			float y = 1.2f,
			float durationSeconds = .3f,
			Bezier? bezier = null
		)
		{
			if (currTweenCoroutine is not null)
			{
				StopCoroutine(currTweenCoroutine);
			}

			currTweenCoroutine = StartCoroutine(
				Animate(buddyPositionsList, y, durationSeconds, bezier)
			);
		}

		private IEnumerator Animate(
			List<BuddiesStore<Vector2Int>> buddyPositionsList,
			float y = 1.2f,
			float durationSeconds = .3f,
			Bezier? bezier = null
		)
		{
			if (buddyPositionsList.Count <= 0)
			{
				var init = LevelField.buddyPositionsInit;

				buddies.player.transform.localPosition = new(
					init.player.x,
					y,
					init.player.y
				);
				buddies.turtle.transform.localPosition = new(
					init.turtle.x,
					y,
					init.turtle.y
				);
				buddies.bird.transform.localPosition = new(
					init.bird.x,
					y,
					init.bird.y
				);
			}
			else if (buddyPositionsList.Count == 1)
			{
				buddies.player.transform.localPosition = new(
					buddyPositionsList[0].player.x,
					y,
					buddyPositionsList[0].player.y
				);
				buddies.turtle.transform.localPosition = new(
					buddyPositionsList[0].turtle.x,
					y,
					buddyPositionsList[0].turtle.y
				);
				buddies.bird.transform.localPosition = new(
					buddyPositionsList[0].bird.x,
					y,
					buddyPositionsList[0].bird.y
				);
			}
			else
			{
				bezier ??= new Bezier(0.22, 1, 0.36, 1);

				for (int i = 1, l = buddyPositionsList.Count; i < l; ++i)
				{
					var prevPositions = buddyPositionsList[i - 1];
					var currPositions = buddyPositionsList[i];

					var p1s = new BuddiesStore<Vector3>()
					{
						player = new(
							prevPositions.player.x,
							y,
							prevPositions.player.y
						),
						turtle = new(
							prevPositions.turtle.x,
							y,
							prevPositions.turtle.y
						),
						bird = new(
							prevPositions.bird.x,
							y,
							prevPositions.bird.y
						),
					};
					var p2s = new BuddiesStore<Vector3>()
					{
						player = new(
							currPositions.player.x,
							y,
							currPositions.player.y
						),
						turtle = new(
							currPositions.turtle.x,
							y,
							currPositions.turtle.y
						),
						bird = new(
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

					while (
						shouldPlayerMove || shouldTurtleMove || shouldBirdMove
					)
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
				buddies.player.transform.localPosition = new(
					lastPositions.player.x,
					y,
					lastPositions.player.y
				);
				buddies.turtle.transform.localPosition = new(
					lastPositions.turtle.x,
					y,
					lastPositions.turtle.y
				);
				buddies.bird.transform.localPosition = new(
					lastPositions.bird.x,
					y,
					lastPositions.bird.y
				);
			}
		}
	}
}
