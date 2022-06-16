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
		private readonly List<TargetBehaviour> things = new();

		public readonly CardGrid cardGrid = new(new TargetBehaviour[] { });

		public event OnGridUpdate? OnGridUpdate;

		private float elapsed = 0f;

		void Update()
		{
			elapsed += Time.deltaTime;
			if (elapsed >= 1f)
			{
				elapsed %= 1f;
				UpdateCardGrid();
			}
		}

		public void Add(TargetBehaviour thing)
		{
			things.Add(thing);
			UpdateCardGrid();
		}

		public void Remove(TargetBehaviour thing)
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
				|| currNodes.Any((v) => prevNodes[i++].kind != v.kind)
			)
			{
				OnGridUpdate?.Invoke(cardGrid);
			}
		}
	}
}
