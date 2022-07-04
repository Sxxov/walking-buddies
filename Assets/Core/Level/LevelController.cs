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
	delegate void OnLevelControllerAnimateEnd();

	public class LevelController : MonoBehaviour
	{
		[SerializeField]
		private TargetController? targetController;

		[SerializeField]
		private ToastCanvasBehaviour? toastCanvasBehaviour;

		private event OnLevelControllerAnimateEnd? OnAnimateEnd;

		private TileCardKinds[,] field_ = LevelRepository.GetField(0);

		public TileCardKinds[,] field
		{
			get => field_;
		}

		private GameObject[,]? tileField;

		private BuddiesStore<GameObject> buddyObjects_ = new();

		public BuddiesStore<GameObject> buddyObjects
		{
			get => buddyObjects_;
		}

		private bool isHidden = false;

		private bool isInLevel = false;

		private Coroutine? currTweenCoroutine;

		private readonly CancellationTokenSource cancellationTokenSource =
			new();

		private FloatingPersistentToast? parseErrorToast;

		private BuddiesStore<FloatingPersistentToast?> buddiesStuckToasts;

		void Awake()
		{
			if (targetController is null)
			{
				throw new InvalidOperationException(
					"Attempted to construct LevelController without target controller"
				);
			}

			if (toastCanvasBehaviour is null)
			{
				throw new InvalidOperationException(
					"Attempted to construct LevelController without toast canvas"
				);
			}

			tileField = LevelTileFieldFactory.Create(field, gameObject);

			buddyObjects_.player = (GameObject)Instantiate(
				Resources.Load("@Prefabs/@Buddies/@Player", typeof(GameObject))
			);
			buddyObjects_.turtle = (GameObject)Instantiate(
				Resources.Load("@Prefabs/@Buddies/@Turtle", typeof(GameObject))
			);
			buddyObjects_.bird = (GameObject)Instantiate(
				Resources.Load("@Prefabs/@Buddies/@Bird", typeof(GameObject))
			);

			foreach (var buddy in buddyObjects)
			{
				buddy.transform.SetParent(transform);
			}

			ScheduleAnimate(new List<BuddiesStore<Vector2Int>>());

			isInLevel = true;

			targetController.OnGridUpdate += OnGridUpdate;
		}

		IEnumerator Start()
		{
			var toastManager = new ToastManager(toastCanvasBehaviour!);

			toastManager
				.SetHeading("18 grass\n3 seas\n3 rainclouds\n*3 hills*")
				.SetParagraph(
					"the buddies managed to walk into a land with grass, sea, rainclouds, & hills!\n\nunfortunately for turtle, he can't climb over hills…\n\nfind a way to get everyone across these lands to reach the land of oo!"
				)
				.SetIcon("\ue16e")
				.SetButtonIcon("\ue5c8")
				.SetButtonText("start!")
				.Show();

			yield return new WaitUntil(() => !toastManager.isShown);

			toastManager
				.SetHeading("oh, a freshie!")
				.SetParagraph(
					"seems like this is the first time you're trying to instruct the buddies, let's get you started!"
				)
				.SetIcon("\ue16e")
				.SetButtonIcon("\ue5c8")
				.SetButtonText("continue!")
				.SetIsCloseFabActive(false)
				.Show();

			yield return new WaitUntil(() => !toastManager.isShown);

			toastManager
				.SetHeading("step 1: allocate…")
				.SetParagraph(
					"find a blank surface large enough to compose cards on"
				)
				.SetIcon("\ue16e")
				.SetButtonIcon("\ue5c8")
				.SetButtonText("found it!")
				.SetIsCloseFabActive(false)
				.Show();

			yield return new WaitUntil(() => !toastManager.isShown);

			toastManager
				.SetHeading("step 2: compose…")
				.SetParagraph(
					$"arrange your cards & point your camera at them!\n\ncards are interpreted as instructions for the buddies. you can form a full instruction by stacking cards on top of each other, ensuring each of their top halves are visible. you can then chain instructions together by placing the next set of cards to the right of the last one!\n\nfor example, you can form a full instruction by sequencing the following cards:\n\n{CardTokeniser.GetNotatedName(CardKinds.IF)}\n{CardTokeniser.GetNotatedName(CardKinds.GRASS)}\n{CardTokeniser.GetNotatedName(CardKinds.THEN)}\n*{CardTokeniser.GetNotatedName(CardKinds.WALK)}"
				)
				.SetIcon("\ue16e")
				.SetButtonIcon("\ue5c8")
				.SetButtonText("got it!")
				.SetIsCloseFabActive(false)
				.Show();

			yield return new WaitUntil(() => !toastManager.isShown);

			toastManager
				.SetHeading("step 3: profit!")
				.SetParagraph(
					"you're ready to go!\n\nget creative & get the buddies to the land of oo!"
				)
				.SetIcon("\ue16e")
				.SetButtonIcon("\ue5c8")
				.SetButtonText("let's go!")
				.SetIsCloseFabActive(false)
				.Show();
		}

		void Update()
		{
			if (targetController!.cardGrid.nodes.Length < 1)
			{
				if (!isHidden)
				{
					foreach (var tile in tileField!)
					{
						tile.SetActive(false);
					}

					foreach (var buddy in buddyObjects)
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

				foreach (var buddy in buddyObjects)
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

			var levelField = new LevelField(field);
			var tokens = CardTokeniser.Tokenise(cardGrid);
			// var tokens = new CardKinds[][]
			// {
			// 	new CardKinds[]
			// 	{
			// 		CardKinds.IF,
			// 		CardKinds.SEA,
			// 		CardKinds.THEN,
			// 		CardKinds.SWAP,
			// 		CardKinds.BIRD,
			// 		CardKinds.SWAP,
			// 		CardKinds.TURTLE,
			// 		CardKinds.WALK,
			// 		CardKinds.ELSE,
			// 		CardKinds.WALK,
			// 	},
			// };

			parseErrorToast?.Destroy();
			foreach (var toast in buddiesStuckToasts)
			{
				toast?.Destroy();
			}

			try
			{
				var result = await CardInterpreter.InterpretAsync(
					tokens,
					cancellationTokenSource.Token,
					levelField
				);

				ScheduleAnimate(result);

				OnAnimateEnd = GenerateOnAnimateEnd(levelField.success);
			}
			catch (AbstractParseException e)
			{
				var rowI = e.i;
				var colI = Array.IndexOf(tokens, e.input);

				if (colI < 0)
				{
					throw new InvalidOperationException(
						"Attempted to generate a persistent toast for an input that was not in the list of tokens"
					);
				}

				var node = CardTokeniser.NodeAt(
					cardGrid,
					colI,
					rowI >= e.input.Length ? e.input.Length - 1 : rowI
				);

				parseErrorToast?.Destroy();
				parseErrorToast = new FloatingPersistentToast()
					.SetHeading("uh oh…")
					.SetParagraph(e.Message)
					.SetIcon("\ue001")
					.Show();

				parseErrorToast.canvasObject.transform.SetParent(
					node.transform,
					false
				);
			}
		}

		private OnLevelControllerAnimateEnd GenerateOnAnimateEnd(
			BuddiesStore<bool> success
		) =>
			() =>
			{
				if (success.player && success.turtle && success.bird)
				{
					var toast = new ToastManager(toastCanvasBehaviour!)
						.SetHeading("hooray!")
						.SetParagraph(
							"you helped the buddies through the level & got them to the land of oo"
						)
						.SetIcon("\ue876")
						.SetButtonIcon("\ue5c8")
						.SetButtonText("continue")
						.SetIsCloseFabActive(false)
						.Show();

					void onVisibilityChange(bool isShown)
					{
						if (!isShown)
						{
							SceneManager.LoadScene(
								SceneManager.GetActiveScene().buildIndex
							);

							toast.OnVisibilityChange -= onVisibilityChange;
						}
					}

					toast.OnVisibilityChange += onVisibilityChange;
				}
				else
				{
					targetController!.OnGridUpdate -= OnGridUpdate;

					parseErrorToast?.Destroy();
					foreach (var toast in buddiesStuckToasts)
					{
						toast?.Destroy();
					}

					FloatingPersistentToast createToast() =>
						new FloatingPersistentToast()
							.SetHeading("stuck!")
							.SetParagraph(
								"I seem to be stuck here… i can't walk through this next tile!"
							)
							.SetIcon("\ue001")
							.Show();

					if (!success.player)
					{
						buddiesStuckToasts.player?.Destroy();
						buddiesStuckToasts.player = createToast();
						buddiesStuckToasts
							.player
							.canvasObject
							.transform
							.position = buddyObjects.player.transform.position;
					}

					if (!success.turtle)
					{
						buddiesStuckToasts.turtle?.Destroy();
						buddiesStuckToasts.turtle = createToast();
						buddiesStuckToasts
							.turtle
							.canvasObject
							.transform
							.position = buddyObjects.turtle.transform.position;
					}

					if (!success.bird)
					{
						buddiesStuckToasts.bird?.Destroy();
						buddiesStuckToasts.bird = createToast();
						buddiesStuckToasts
							.bird
							.canvasObject
							.transform
							.position = buddyObjects.bird.transform.position;
					}
				}
			};

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

				buddyObjects_.player.transform.localPosition = new(
					init.player.x,
					y,
					init.player.y
				);
				buddyObjects_.turtle.transform.localPosition = new(
					init.turtle.x,
					y,
					init.turtle.y
				);
				buddyObjects_.bird.transform.localPosition = new(
					init.bird.x,
					y,
					init.bird.y
				);
			}
			else if (buddyPositionsList.Count == 1)
			{
				buddyObjects_.player.transform.localPosition = new(
					buddyPositionsList[0].player.x,
					y,
					buddyPositionsList[0].player.y
				);
				buddyObjects_.turtle.transform.localPosition = new(
					buddyPositionsList[0].turtle.x,
					y,
					buddyPositionsList[0].turtle.y
				);
				buddyObjects_.bird.transform.localPosition = new(
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
							buddyObjects_.player.transform.localPosition =
								tweens.player.Current;
							shouldPlayerMove = tweens.player.MoveNext();
						}
						else
						{
							buddyObjects_.player.transform.localPosition =
								p2s.player;
						}

						if (shouldTurtleMove)
						{
							buddyObjects_.turtle.transform.localPosition =
								tweens.turtle.Current;
							shouldTurtleMove = tweens.turtle.MoveNext();
						}
						else
						{
							buddyObjects_.turtle.transform.localPosition =
								p2s.turtle;
						}

						if (shouldBirdMove)
						{
							buddyObjects_.bird.transform.localPosition = tweens
								.bird
								.Current;
							shouldBirdMove = tweens.bird.MoveNext();
						}
						else
						{
							buddyObjects_.bird.transform.localPosition =
								p2s.bird;
						}

						yield return null;
					}
				}

				var lastPositions = buddyPositionsList.Last();
				buddyObjects_.player.transform.localPosition = new(
					lastPositions.player.x,
					y,
					lastPositions.player.y
				);
				buddyObjects_.turtle.transform.localPosition = new(
					lastPositions.turtle.x,
					y,
					lastPositions.turtle.y
				);
				buddyObjects_.bird.transform.localPosition = new(
					lastPositions.bird.x,
					y,
					lastPositions.bird.y
				);
			}

			OnAnimateEnd?.Invoke();
		}
	}
}
