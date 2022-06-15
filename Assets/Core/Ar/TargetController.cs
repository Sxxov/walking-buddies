using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using WalkingBuddies.Core.Card;
using WalkingBuddies.Core.Level;

namespace WalkingBuddies.Core.Ar
{
	public delegate void OnGridUpdate(CardGrid cardGrid);

	public class TargetController : MonoBehaviour
	{
		private readonly List<TargetThing> things = new();

		public readonly CardGrid cardGrid = new(new TargetThing[] { });

		public event OnGridUpdate? OnGridUpdate;

		// private float elapsed = 0f;

		// void Update()
		// {
		// 	elapsed += Time.deltaTime;
		// 	if (elapsed >= 1f)
		// 	{
		// 		elapsed %= 1f;
		// 		UpdateCardGrid();
		// 	}
		// }

		void Start()
		{
			OnGridUpdate?.Invoke(cardGrid);
		}

		public void Add(TargetThing thing)
		{
			things.Add(thing);
			UpdateCardGrid();
		}

		public void Remove(TargetThing thing)
		{
			things.Remove(thing);
			UpdateCardGrid();
		}

		private void UpdateCardGrid()
		{
			var prevNodes = cardGrid.nodes;

			cardGrid.Update(things.ToArray());

			var currNodes = cardGrid.nodes;

			var i = 0;
			if (
				prevNodes.Length != currNodes.Length
				|| currNodes.Any((v) => prevNodes[i++] != v)
			)
			{
				OnGridUpdate?.Invoke(cardGrid);
			}
		}
	}
}
