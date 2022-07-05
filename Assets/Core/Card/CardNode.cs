using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using WalkingBuddies.Core.Ar;

namespace WalkingBuddies.Core.Card
{
	public class CardNode
	{
		public CardKinds kind;
		public Transform transform;
		public CardNode? top;
		public CardNode? bottom;
		public CardNode? left;
		public CardNode? right;
		public ReadOnlyDictionary<CardEdges, CardNode?> edgeToNode
		{
			get =>
				new(
					new Dictionary<CardEdges, CardNode?>()
					{
						{ CardEdges.TOP, top },
						{ CardEdges.BOTTOM, bottom },
						{ CardEdges.LEFT, left },
						{ CardEdges.RIGHT, right },
					}
				);
		}
		public ((CardEdges, CardNode?) top, (CardEdges, CardNode?) bottom, (CardEdges, CardNode?) left, (CardEdges, CardNode?) right) edgeAndNodes
		{
			get =>
				(
					(CardEdges.TOP, top),
					(CardEdges.BOTTOM, bottom),
					(CardEdges.LEFT, left),
					(CardEdges.RIGHT, right)
				);
		}

		public CardNode(TargetBehaviour thing)
		{
			kind = thing.kind;
			transform = thing.transform;
		}

		public CardNode? Get(CardEdges edge) =>
			edge switch
			{
				CardEdges.TOP => top,
				CardEdges.BOTTOM => bottom,
				CardEdges.LEFT => left,
				CardEdges.RIGHT => right,
				_ => null,
			};

		public void LinkTop(CardNode node)
		{
			top = node;
			node.bottom = this;
		}

		public void LinkBottom(CardNode node)
		{
			bottom = node;
			node.top = this;
		}

		public void LinkLeft(CardNode node)
		{
			left = node;
			node.right = this;
		}

		public void LinkRight(CardNode node)
		{
			right = node;
			node.left = this;
		}

		public void UnlinkTop()
		{
			if (top is not null)
			{
				top.bottom = null;
			}

			top = null;
		}

		public void UnlinkBottom()
		{
			if (bottom is not null)
			{
				bottom.top = null;
			}

			bottom = null;
		}

		public void UnlinkLeft()
		{
			if (left is not null)
			{
				left.right = null;
			}

			left = null;
		}

		public void UnlinkRight()
		{
			if (right is not null)
			{
				right.left = null;
			}

			right = null;
		}

		public void Unlink(CardEdges edge)
		{
			switch (edge)
			{
				case CardEdges.TOP:
					UnlinkTop();
					break;
				case CardEdges.BOTTOM:
					UnlinkBottom();
					break;
				case CardEdges.LEFT:
					UnlinkLeft();
					break;
				case CardEdges.RIGHT:
					UnlinkRight();
					break;
			}
		}

		public void Link(CardEdges edge, CardNode node)
		{
			switch (edge)
			{
				case CardEdges.TOP:
					LinkTop(node);
					break;
				case CardEdges.BOTTOM:
					LinkBottom(node);
					break;
				case CardEdges.LEFT:
					LinkLeft(node);
					break;
				case CardEdges.RIGHT:
					LinkRight(node);
					break;
			}
		}

		public override string ToString() =>
			$"{kind} — top: {top?.kind}, bottom: {bottom?.kind}, left: {left?.kind}, right: {right?.kind}";
	}
}
