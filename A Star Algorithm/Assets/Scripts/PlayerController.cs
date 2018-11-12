using UnityEngine;

public class PlayerController : Controller
{
	//3D quads to represent the source node/cell and the destination node/cell.
	[SerializeField]
	Transform destinationIndicator, sourceIndicator;

	Vector3 nextPos;

	private void Update()
	{
		//use the right mouse button to click on anywhere on the MAP/PLANE.
		//use raycast to collect data on the clicked position.
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
			{
				if (hit.collider.tag == "Map")
				{
					gotPathData = false;

					//get the point where the ray hit and the position of the player.
					//casting the values to ints will help in determining the
					//source and destination nodes. The variables below act as the
					//rows and columns of the Node array.
					rayPointZ = (int)hit.point.z;
					rayPointX = (int)hit.point.x;
					playerPositionZ = (int)transform.position.z;
					playerPositionX = (int)transform.position.x;

					//set the position of the indicators based on the above calculated values
					//see how those values correspond to knowing the appropriate nodes/cells on the map.
					destinationIndicator.position = new Vector3(nodes[rayPointZ, rayPointX].nodePosition.x, 0.1f, nodes[rayPointZ, rayPointX].nodePosition.z);
					sourceIndicator.position = new Vector3(nodes[playerPositionZ, playerPositionX].nodePosition.x, 0.1f, nodes[playerPositionZ, playerPositionX].nodePosition.z);

					//restart the other thread to calculate the path.
					ewh1.Set();
				}
			}
		}

		//if a new path is found, set the current position to where the player is in the current frame.
		if (gotPathData)
		{
			nextPos = transform.position;
			gotPathData = false;
		}

		//get the next position that the player has to move towards.
		if (transform.position == nextPos && index > 0) nextPos = path[--index];

		//code for moving the player.
		if (index >= 0)
		{
			transform.position = Vector3.MoveTowards(transform.position, nextPos, 5 * Time.deltaTime);
			//clear out the path list to stop the player when it reaches it's destination.
			if (index == 0) path.Clear();
		}
	}
}