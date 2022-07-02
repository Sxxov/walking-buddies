using System;
using UnityEngine;
using WalkingBuddies.Core.Level;
using UnityEngine.UI;
using WalkingBuddies.Core.Card;
using TMPro;

namespace WalkingBuddies.Core.Ui
{
	public class MinimapBehaviour : MonoBehaviour
	{
		[SerializeField]
		private int tileSize = 224;

		[SerializeField]
		private int tileMargin = 8;

		[SerializeField]
		private LevelController? controller;

		[SerializeField]
		private TMP_FontAsset? emojiFont;

		private TileCardKinds[,]? currField;

		private GameObject? tilesObject;

		private GameObject? buddiesObject;

		void Awake()
		{
			if (controller is null)
			{
				throw new InvalidOperationException(
					"Attempted to construct MinimapBehaviour without controller"
				);
			}

			if (emojiFont is null)
			{
				throw new InvalidOperationException(
					"Attempted to construct MinimapBehaviour without emojiFont"
				);
			}

			tilesObject = new GameObject();
			tilesObject.AddComponent<RectTransform>();
			tilesObject.AddComponent<CanvasRenderer>();
			tilesObject.transform.SetParent(transform, false);
			((RectTransform)tilesObject.transform).anchorMin = new Vector2(
				0.5f,
				0.5f
			);
			((RectTransform)tilesObject.transform).anchorMax = new Vector2(
				0.5f,
				0.5f
			);
			((RectTransform)tilesObject!.transform).sizeDelta = Vector2.zero;

			buddiesObject = new GameObject();
			buddiesObject.AddComponent<RectTransform>();
			buddiesObject.AddComponent<CanvasRenderer>();
			buddiesObject.transform.SetParent(transform, false);
			buddiesObject.transform.localPosition = new Vector3(
				-(tileSize + tileMargin),
				0,
				0
			);
			((RectTransform)buddiesObject.transform).anchorMin = new Vector2(
				0.5f,
				0.5f
			);
			((RectTransform)buddiesObject.transform).anchorMax = new Vector2(
				0.5f,
				0.5f
			);
			((RectTransform)buddiesObject!.transform).sizeDelta = Vector2.zero;
		}

		void Update()
		{
			if (currField != controller!.field)
			{
				UpdateTiles(controller!.field, tilesObject!);

				var size = new Vector2(
					(controller!.field.GetLength(0) - 1)
						* (tileSize + tileMargin),
					(controller!.field.GetLength(1) - 1)
						* (tileSize + tileMargin)
				);

				var offset = new Vector2(-size.x / 2, size.y / 2);
				((RectTransform)tilesObject!.transform).localPosition = offset;
				((RectTransform)buddiesObject!.transform).localPosition =
					offset;

				currField = controller!.field;
			}

			UpdateBuddies(
				new BuddiesStore<Vector2>()
				{
					player = new Vector2(
						controller!
							.buddyObjects
							.player
							.transform
							.localPosition
							.x,
						controller!
							.buddyObjects
							.player
							.transform
							.localPosition
							.z
					),
					turtle = new Vector2(
						controller!
							.buddyObjects
							.turtle
							.transform
							.localPosition
							.x,
						controller!
							.buddyObjects
							.turtle
							.transform
							.localPosition
							.z
					),
					bird = new Vector2(
						controller!.buddyObjects.bird.transform.localPosition.x,
						controller!.buddyObjects.bird.transform.localPosition.z
					)
				},
				buddiesObject!
			);
		}

		private void UpdateBuddies(
			BuddiesStore<Vector2> buddiesPositions,
			GameObject parent
		)
		{
			GameObject player;
			GameObject turtle;
			GameObject bird;

			if (parent.transform.childCount < 3)
			{
				(player, turtle, bird) = (
					new GameObject(),
					new GameObject(),
					new GameObject()
				);

				player.AddComponent<CanvasRenderer>();
				turtle.AddComponent<CanvasRenderer>();
				bird.AddComponent<CanvasRenderer>();

				player.AddComponent<RectTransform>();
				turtle.AddComponent<RectTransform>();
				bird.AddComponent<RectTransform>();

				var (playerText, turtleText, birdText) = (
					player.AddComponent<TextMeshProUGUI>(),
					turtle.AddComponent<TextMeshProUGUI>(),
					bird.AddComponent<TextMeshProUGUI>()
				);

				(playerText.text, turtleText.text, birdText.text) = (
					"🧑",
					"🐢",
					"🐦"
				);

				(playerText.font, turtleText.font, birdText.font) = (
					emojiFont!,
					emojiFont!,
					emojiFont!
				);

				(playerText.fontSize, turtleText.fontSize, birdText.fontSize) =
					(tileSize / 2, tileSize / 2, tileSize / 2);

				(
					((RectTransform)player.transform).sizeDelta,
					((RectTransform)turtle.transform).sizeDelta,
					((RectTransform)bird.transform).sizeDelta
				) = (
					new Vector2(playerText.fontSize, playerText.fontSize),
					new Vector2(turtleText.fontSize, birdText.fontSize),
					new Vector2(birdText.fontSize, birdText.fontSize)
				);

				player.transform.SetParent(parent.transform, false);
				turtle.transform.SetParent(parent.transform, false);
				bird.transform.SetParent(parent.transform, false);
			}
			else
			{
				(player, turtle, bird) = (
					parent.transform.GetChild(0).gameObject,
					parent.transform.GetChild(1).gameObject,
					parent.transform.GetChild(2).gameObject
				);
			}

			(
				player.transform.localPosition,
				turtle.transform.localPosition,
				bird.transform.localPosition
			) = (
				new Vector2(
					buddiesPositions.player.y * (tileSize + tileMargin),
					-buddiesPositions.player.x * (tileSize + tileMargin)
				),
				new Vector2(
					buddiesPositions.turtle.y * (tileSize + tileMargin),
					-buddiesPositions.turtle.x * (tileSize + tileMargin)
				),
				new Vector2(
					buddiesPositions.bird.y * (tileSize + tileMargin),
					-buddiesPositions.bird.x * (tileSize + tileMargin)
				)
			);
		}

		private void UpdateTiles(TileCardKinds[,] field, GameObject parent)
		{
			foreach (Transform child in parent.transform)
			{
				Destroy(child.gameObject);
			}

			for (int rowI = 0, rowL = field.GetLength(0); rowI < rowL; ++rowI)
			{
				for (
					int colI = 0, colL = field.GetLength(1);
					colI < colL;
					++colI
				)
				{
					var tile = field[rowI, colI];

					var panel = new GameObject();
					panel.AddComponent<CanvasRenderer>();
					panel.AddComponent<RectTransform>();
					var image = panel.AddComponent<Image>();

					image.color = tile switch
					{
						TileCardKinds.GRASS
							=> new Color(
								120f / 255f,
								224f / 255f,
								143f / 255f,
								.5f
							),
						TileCardKinds.HILL
							=> new Color(
								229f / 255f,
								142f / 255f,
								38f / 255f,
								.5f
							),
						TileCardKinds.RAINCLOUD
							=> new Color(
								241f / 255f,
								245f / 255f,
								249f / 255f,
								.5f
							),
						TileCardKinds.SEA
							=> new Color(
								130f / 255f,
								204f / 255f,
								221f / 255f,
								.5f
							),
						_
							=> throw new InvalidOperationException(
								"Unknown tile kind"
							),
					};

					((RectTransform)panel.transform).sizeDelta = new Vector2(
						tileSize,
						tileSize
					);
					panel.transform.localPosition = new Vector2(
						rowI * (tileSize + tileMargin),
						-colI * (tileSize + tileMargin)
					);
					panel.transform.SetParent(parent.transform, false);
				}
			}
		}
	}
}
