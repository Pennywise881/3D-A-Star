using UnityEngine;

/*
 * Use this class to store information of all the nodes/cells on the map/plane.
 */

public class Node
{
	public bool walkable;
	public Vector3 nodePosition;
	public int cellNumber;
	public int gCost, hCost, fCost, parent;
	public int row, col;

	public Node(bool walkable, Vector3 nodePosition, int row, int col, int cellNumber)
	{
		this.walkable = walkable;
		this.nodePosition = nodePosition;
		this.row = row;
		this.col = col;
		this.cellNumber = cellNumber;
	}
}
