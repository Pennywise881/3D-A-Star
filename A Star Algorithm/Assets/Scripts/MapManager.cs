using UnityEngine;
using UnityEditor;

public class MapManager : MonoBehaviour
{
	[SerializeField]
	Controller controller;

	int rowCount, colCount;

	float colliderRadius;

	Node[,] nodes;

	void Start()
	{
		//set the radius of the collider that is used to check for obstacles.
		colliderRadius = 0.49f;

		//calculate the number of rows and columns based on the map/plane's x and z scales.
		rowCount = (int)(transform.localScale.z) * 10;
		colCount = (int)(transform.localScale.x) * 10;

		//initialize the Node array
		nodes = new Node[rowCount, colCount];

		//adjust the position of the map/plane based on it's scale
		transform.position = new Vector3(5 * colCount / 10, 0, 5 * rowCount / 10);


		SetupMap();

		//feed the information setup in the function above to the 'Controller' class' nodes variable.
		controller.NodeInformation = nodes;
	}

	private void SetupMap()
	{
		//These two variables are used to keep track of the position of each
		//individual cell on the map/plane.
		float xPos, zPos;
		xPos = 0.5f;
		zPos = 0.5f;

		//variable to record the cell number of each cell on the map.
		int cellNumber = 1;

		for (int i = 0; i < rowCount; i++)
		{
			for (int j = 0; j < colCount; j++)
			{
				//get the position each cell in one row
				Vector3 cellPosition = new Vector3(xPos + j, 1, zPos);
				//use this to check if the collider collided with any obstacle.
				bool walkable = !(Physics.CheckSphere(cellPosition, colliderRadius, 1 << 8));

				//initialize a new Node object for every cell on the map.
				nodes[i, j] = new Node(walkable, cellPosition, i, j, cellNumber++);
			}
			//go to the next row.
			//This determines the position along the Z axis of each cell in one row
			zPos += 1;
		}
	}

	#region Gizmos Stuff
	//Display the cell number of each node.
	//Indicate which nodes are unreachable
	void OnDrawGizmos()
	{
		if (nodes != null)
		{
			foreach (Node n in nodes)
			{
				//if (n.walkable == false)
				//{
				//	Gizmos.color = Color.red;
				//	Gizmos.DrawCube(new Vector3(n.nodePosition.x, 0, n.nodePosition.z), new Vector3(1, 0.1f, 1));
				//}
				Handles.Label(new Vector3(n.nodePosition.x, 0.1f, n.nodePosition.z), n.cellNumber.ToString());
			}
		}
	}
	#endregion
}