using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using WalkingBuddies.Core.Ar;

namespace WalkingBuddies.Core.Card
{
	public class CardGrid
	{
		public CardNode[] nodes { get; private set; } = new CardNode[] { };

		public CardNode? head
		{
			get
			{
				if (nodes.Length <= 0)
				{
					return null;
				}

				var currNode = nodes[0];

				for (; ; )
				{
					while (currNode.left is not null)
					{
						currNode = currNode.left;
					}

					if (currNode.top is not null)
					{
						currNode = currNode.top;
					}
					else
					{
						break;
					}
				}

				return currNode;
			}
		}

		public CardNode? tail
		{
			get
			{
				if (nodes.Length <= 0)
				{
					return null;
				}

				var currNode = nodes[0];

				do
				{
					while (currNode.right is not null)
					{
						currNode = currNode.right;
					}

					currNode = currNode.bottom;
				} while (currNode?.bottom is not null);

				return currNode;
			}
		}

		public CardGrid(TargetThing[] things)
		{
			Update(things);
		}

		public CardGrid Update(TargetThing[] things)
		{
			if (things.Length <= 0)
			{
				this.nodes = new CardNode[] { };

				return this;
			}

			if (things.Length == 1)
			{
				this.nodes = new CardNode[] { new CardNode(things[0]), };

				return this;
			}

			var nodes = things.Select((thing) => new CardNode(thing)).ToArray();
			var targetNodeToNearestNodeAndDistanceAndEdge =
				new Dictionary<
					CardNode,
					(CardNode node, float distance, CardEdges edge)
				>();
			var queue = new List<CardNode>() { nodes[0] };

			while (queue.Count > 0)
			{
				var targetNode = queue[0];
				queue.RemoveAt(0);
				var targetNodeInitialPosition = targetNode.transform.position;

				var edgeToNodeAndDistance =
					new Dictionary<CardEdges, (CardNode, float)>();

				var surroundingNodes = nodes
					.Where((node) => node != targetNode)
					.ToArray();
				var surroundingNodeToInitialDistance =
					new Dictionary<CardNode, float>();
				var surroundingNodesMinInitialDistance = float.PositiveInfinity;
				foreach (var surroundingNode in surroundingNodes)
				{
					var distance = Vector3.Distance(
						surroundingNode.transform.position,
						targetNode.transform.position
					);

					surroundingNodeToInitialDistance.Add(
						surroundingNode,
						distance
					);
					surroundingNodesMinInitialDistance = Math.Min(
						distance,
						surroundingNodesMinInitialDistance
					);
				}

				foreach (var (edge, edgeNode) in targetNode.edgeToNode)
				{
					if (edgeNode is not null)
					{
						continue;
					}

					CardNode? nearestNode = null;
					var nearestDistance = float.PositiveInfinity;
					foreach (var surroundingNode in surroundingNodes)
					{
						var preMoveDistance =
							surroundingNodeToInitialDistance.GetValueOrDefault(
								surroundingNode,
								float.PositiveInfinity
							);

						var postMoveDistance = Vector3.Distance(
							targetNodeInitialPosition
								+ GetAxisOfTransformAtEdge(
									targetNode.transform,
									edge
								) * surroundingNodesMinInitialDistance,
							surroundingNode.transform.position
						);

						if (
							postMoveDistance - preMoveDistance < 0
							&& postMoveDistance < nearestDistance
						)
						{
							nearestNode = surroundingNode;
							nearestDistance = postMoveDistance;
						}
					}

					if (nearestNode is null)
					{
						continue;
					}

					var isClosest = true;
					foreach (
						var (
							existingEdge,
							(existingNode, existingDistance)
						) in edgeToNodeAndDistance
					)
					{
						if (existingEdge == edge)
						{
							continue;
						}

						if (existingNode == nearestNode)
						{
							if (existingDistance < nearestDistance)
							{
								isClosest = false;
							}
							else
							{
								existingNode.Unlink(existingEdge);
							}
						}
					}

					if (isClosest)
					{
						targetNode.Link(edge, nearestNode);

						edgeToNodeAndDistance.Add(
							edge,
							(nearestNode, nearestDistance)
						);

						queue.Add(nearestNode);
					}
				}
			}

			this.nodes = nodes;

			return this;
		}

		private static Vector3 GetAxisOfTransformAtEdge(
			Transform transform,
			CardEdges edge
		) =>
			edge switch
			{
				CardEdges.TOP => transform.forward,
				CardEdges.BOTTOM => -transform.forward,
				CardEdges.LEFT => -transform.right,
				CardEdges.RIGHT => transform.right,
				_ => new Vector3(),
			};

  /*
  public CardGrid Update_(Transform[] transforms)
  {
      if (transforms.Length <= 0)
      {
          this.nodes = new CardNode[] { };

          return this;
      }

      if (transforms.Length == 1)
      {
          this.nodes = new CardNode[] { new CardNode(transforms[0]), };

          return this;
      }

      var nodes = transforms
          .Select((transform) => new CardNode(transform))
          .ToArray();
      var resolvedCount = 0;
      var cursor = new GameObject();
      var queue = new List<CardNode>() { nodes[0] };

      while (resolvedCount < transforms.Length)
      {
          if (queue.Count <= 0)
          {
              Debug.LogError("Empty queue with unsolved nodes");

              break;
          }

          var curr = queue[0];
          queue.RemoveAt(0);

          var nodesExceptCurr = nodes
              .Where((node) => node != curr)
              .ToArray();

          var nodeToDistance = new Dictionary<CardNode, float>();
          var nodesMinDistance = float.PositiveInfinity;

          // resolve nodesMinDistance
          foreach (var node in nodes)
          {
              var distance = Vector3.Distance(
                  node.transform.position,
                  curr.transform.position
              );

              nodeToDistance.Add(node, distance);

              if (distance < nodesMinDistance && distance > 0)
              {
                  nodesMinDistance = distance;
              }
          }

          // move cursor to curr node's transformation
          cursor.transform.SetPositionAndRotation(
              curr.transform.position,
              curr.transform.rotation
          );

          var originalPosition = curr.transform.position;

          // move cursor to resolve the
          if (curr.top is null)
          {
              cursor.transform.position = (
                  originalPosition
                  + cursor.transform.forward * nodesMinDistance
              );

              var (nearest, currDistance) = NearestNodeAndDistance(
                  cursor.transform.position,
                  nodesExceptCurr
              );
              var prevDistance = nodeToDistance.GetValueOrDefault(
                  nearest,
                  float.NegativeInfinity
              );
              var diffDistance = currDistance - prevDistance;

              // closer after moving
              if (diffDistance < 0)
              {
                  curr.LinkTop(nearest);

                  if (!queue.Contains(nearest))
                  {
                      queue.Add(nearest);
                  }
              }
          }

          if (curr.bottom is null)
          {
              cursor.transform.position = (
                  originalPosition
                  + -cursor.transform.forward * nodesMinDistance
              );

              var (nearest, currDistance) = NearestNodeAndDistance(
                  cursor.transform.position,
                  nodesExceptCurr
              );
              var prevDistance = nodeToDistance.GetValueOrDefault(
                  nearest,
                  float.NegativeInfinity
              );
              var diffDistance = currDistance - prevDistance;

              // closer after moving
              if (diffDistance < 0)
              {
                  curr.LinkBottom(nearest);

                  if (!queue.Contains(nearest))
                  {
                      queue.Add(nearest);
                  }
              }
          }

          if (curr.left is null)
          {
              cursor.transform.position = (
                  originalPosition
                  + -cursor.transform.right * nodesMinDistance
              );

              var (nearest, currDistance) = NearestNodeAndDistance(
                  cursor.transform.position,
                  nodesExceptCurr
              );
              var prevDistance = nodeToDistance.GetValueOrDefault(
                  nearest,
                  float.NegativeInfinity
              );
              var diffDistance = currDistance - prevDistance;

              // closer after moving
              if (diffDistance < 0)
              {
                  curr.LinkLeft(nearest);

                  if (!queue.Contains(nearest))
                  {
                      queue.Add(nearest);
                  }
              }
          }

          if (curr.right is null)
          {
              cursor.transform.position = (
                  originalPosition
                  + cursor.transform.right * nodesMinDistance
              );

              var (nearest, currDistance) = NearestNodeAndDistance(
                  cursor.transform.position,
                  nodesExceptCurr
              );
              var prevDistance = nodeToDistance.GetValueOrDefault(
                  nearest,
                  float.NegativeInfinity
              );
              var diffDistance = currDistance - prevDistance;

              // closer after moving
              if (diffDistance < 0)
              {
                  curr.LinkRight(nearest);

                  if (!queue.Contains(nearest))
                  {
                      queue.Add(nearest);
                  }
              }
          }

          cursor.transform.position = originalPosition;

          ++resolvedCount;
      }

      this.nodes = nodes;

      UnityEngine.Object.Destroy(cursor);

      return this;
  }

  private static (CardNode? node, float distance) NearestNodeAndDistance(
      Vector3 position,
      IList<CardNode> nodes,
      float threshold
  )
  {
      CardNode? nearestNode = null;
      var nearestDistance = float.PositiveInfinity;

      foreach (var node in nodes)
      {
          var distance = Vector3.Distance(
              node.transform.position,
              position
          );

          if (distance < nearestDistance && distance < threshold)
          {
              nearestNode = node;
              nearestDistance = distance;
          }
      }

      return (nearestNode, nearestDistance);
  }

  private static (CardNode node, float distance) NearestNodeAndDistance(
      Vector3 position,
      IList<CardNode> nodes
  )
  {
      return NearestNodeAndDistance(
          position,
          nodes,
          float.PositiveInfinity
      )!;
  }
  */
	}
}
