using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class PathFinderController : Controller
{
	Thread t1, t2;
	int goalRow, goalCol, goalCell;
	int currentRow, currentCol, startCell;
	List<Node> openList, closedList;

	bool stopThread, foundGoal;

	private void Start()
	{
		//start the child threads.
		t1 = new Thread(NotifyMainThread);
		t1.Start();

		t2 = new Thread(CalculatePath);
		t2.Start();
	}

	#region Child Threads

	//This thread does the A* and calculates the path from source to destination.
	void CalculatePath()
	{
		//halt the thread at first
		ewh2.WaitOne();

		while (true)
		{
			//use a boolean variable to stop the thread.
			if (stopThread) break;

			//check if the point where the player clicked is walkable or not.
			if (nodes[rayPointZ, rayPointX].walkable)
			{
				SetRouteData(rayPointZ, rayPointX, playerPositionZ, playerPositionX);
				FindPath();
				index = path.Count;
			}

			//halt to let the other child thread know that this 
			//one is done with it's work and will remain halted until further notice.
			WaitHandle.SignalAndWait(ewh1, ewh2);
		}
	}

	//This thread communicates with the one that does A* and acts as a
	//bridge between the other thread and the main thread. 
	//This thread is used to notify the main thread.
	void NotifyMainThread()
	{
		//halt this thread at first.
		ewh1.WaitOne();

		while (true)
		{
			//use boolean variable to break out of this loop.
			if (stopThread) break;

			//let the thread (that will calculate the path) know that the user has clicked
			//somewhere on the map. Halt this thread until the other one signals it to 
			//let the main thread know that data has been collected.
			WaitHandle.SignalAndWait(ewh2, ewh1);

			//use this to notify the main thread that the path to the new
			//destination has been calculated.
			gotPathData = true;

			//halt this thread until the user clicks again on the map.
			//see "PlayerController" class for how this is unhalted.
			ewh1.WaitOne();
		}
	}
	#endregion

	#region A* Algorithm
	void SetRouteData(int row, int col, int cRow, int cCol)
	{
		//clear out all the lists from previous calcualtions.
		if (openList != null && closedList != null && (openList.Count > 0 || closedList.Count > 0))
		{
			openList.Clear();
			closedList.Clear();
			path.Clear();
		}
		else
		{
			//initialize all the lists when the very first destination is set.
			openList = new List<Node>();
			closedList = new List<Node>();
			path = new List<Vector3>();
		}
		//set the rows and columns of the source and destination cells.
		goalRow = row;
		goalCol = col;
		goalCell = nodes[goalRow, goalCol].cellNumber;
		currentRow = cRow;
		currentCol = cCol;
		startCell = nodes[currentRow, currentCol].cellNumber;
	}

	void FindPath()
	{
		//set all the costs to zero for the soruce node.
		nodes[currentRow, currentCol].gCost = 0;
		nodes[currentRow, currentCol].hCost = 0;
		nodes[currentRow, currentCol].fCost = 0;
		foundGoal = false;


		do
		{
			//cell: center-below
			if (IsCellWalkable(currentRow - 1, currentCol) && !foundGoal) ExploreNode(currentRow - 1, currentCol, 10);
			//cell: bottom-left-diagonal
			if (IsCellWalkable(currentRow - 1, currentCol - 1) && !foundGoal) ExploreNode(currentRow - 1, currentCol - 1, 14);
			//cell: center-left
			if (IsCellWalkable(currentRow, currentCol - 1) && !foundGoal) ExploreNode(currentRow, currentCol - 1, 10);
			//cell: top-left-diagonal
			if (IsCellWalkable(currentRow + 1, currentCol - 1) && !foundGoal) ExploreNode(currentRow + 1, currentCol - 1, 14);
			//cell: center-top
			if (IsCellWalkable(currentRow + 1, currentCol) && !foundGoal) ExploreNode(currentRow + 1, currentCol, 10);
			//cell: top-right-diagonal
			if (IsCellWalkable(currentRow + 1, currentCol + 1) && !foundGoal) ExploreNode(currentRow + 1, currentCol + 1, 14);
			//cell: center-right
			if (IsCellWalkable(currentRow, currentCol + 1) && !foundGoal) ExploreNode(currentRow, currentCol + 1, 10);
			//cell: bottom-right-diagonal
			if (IsCellWalkable(currentRow - 1, currentCol + 1) && !foundGoal) ExploreNode(currentRow - 1, currentCol + 1, 14);

			//get the next node from the openlist with the smallest hVal and repeat until the destination node is found.
			if (!foundGoal) GetNextNode();

		} while (!foundGoal);

		//add the goal node to the closed list and now we basically have our path.
		closedList.Add(nodes[goalRow, goalCol]);

		//Creating the path list..
		int p;
		path.Add(closedList[closedList.Count - 1].nodePosition);
		p = nodes[goalRow, goalCol].parent;

		//use the parents of the nodes in the closed list
		//to trace back to the source node and create the path.
		//start with the destination node's parent and that
		//will eventually lead back to the source node.
		while (p != startCell)
		{
			for (int i = 0; i < closedList.Count; i++)
			{
				if (p == closedList[i].cellNumber)
				{
					path.Add(closedList[i].nodePosition);
					p = closedList[i].parent;
					break;
				}
			}
		}
	}

	void ExploreNode(int r, int c, int gc)
	{
		//do a special check to see if the g cost of going to a node that
		//is already in the open list, from the node that is currently
		//being explored; is lesser or not. If it is, then re-parent that node
		//to the node that is currently being explored now.
		if (openList.Contains(nodes[r, c]))
		{
			//calculate new heuistics and reset parent.
			if (nodes[currentRow, currentCol].gCost + gc < nodes[r, c].gCost)
			{
				nodes[r, c].parent = nodes[currentRow, currentCol].cellNumber;
				nodes[r, c].gCost = nodes[currentRow, currentCol].gCost + gc;
				nodes[r, c].fCost = nodes[currentRow, currentCol].hCost + nodes[r, c].gCost;
			}
		}
		else if (!closedList.Contains(nodes[r, c]))
		{
			//calculate the g cost.
			nodes[r, c].gCost = nodes[currentRow, currentCol].gCost + gc;

			/*Heuristics calculation*/
			int rowDistance = Mathf.Abs(goalRow - r);
			int colDistance = Mathf.Abs(goalCol - c);
			if (rowDistance > colDistance) nodes[r, c].hCost = (colDistance * 14) + (rowDistance - colDistance) * 10;
			else nodes[r, c].hCost = (rowDistance * 14) + (colDistance - rowDistance) * 10;

			/*Heuristics calculation*/
			nodes[r, c].fCost = nodes[r, c].gCost + nodes[r, c].hCost;
			nodes[r, c].parent = nodes[currentRow, currentCol].cellNumber;

			//add this new found node to the open list.
			openList.Add(nodes[r, c]);

			//always check to see if the destination node is found.
			if (nodes[r, c].cellNumber == goalCell) foundGoal = true;
		}
	}

	//check to see if the node being explored is being covered by
	//an obstacle or not.
	bool IsCellWalkable(int r, int c)
	{
		try
		{
			if (nodes[r, c].walkable) return true;
			else return false;
		}
		catch
		{
			return false;
		}
	}

	//get the node with the next smallest h cost in the open list and 
	//explore it.
	void GetNextNode()
	{
		Node currentNode = openList[0];

		for (int i = 1; i < openList.Count; i++)
		{
			if (openList[i].fCost == currentNode.fCost)
			{
				if (openList[i].hCost < currentNode.hCost) currentNode = openList[i];
			}
			else if (openList[i].fCost < currentNode.fCost) currentNode = openList[i];
		}

		//remove the next node to be explored from the open list and 
		//add it to the closed list 
		//make sure no other node explores it again.
		openList.Remove(currentNode);
		closedList.Add(currentNode);
		currentRow = currentNode.row;
		currentCol = currentNode.col;
	}
	#endregion

	#region Stopping This Thread
	//stop the child threads whenever this program is not running
	private void OnDisable()
	{
		ThreadStopper();
	}

	private void OnDestroy()
	{
		ThreadStopper();
	}

	private void OnApplicationQuit()
	{
		ThreadStopper();
	}

	void ThreadStopper()
	{
		stopThread = true;
		ewh1.Set();
		ewh2.Set();
	}
	#endregion

	#region GUI Stuff
	//this shows if a path was found to the desired desitination of the user.
	//it also shows which cell the player is in and where it will go to next.
	private void OnGUI()
	{
		string x;
		if (nodes[rayPointZ, rayPointX].walkable) x = "\nRoute = Found!";
		else x = "\nRoute = NOT Found!";

		GUI.Box
			(
			new Rect(0, 0, 160, 60),
			"Source Cell: " + nodes[playerPositionZ, playerPositionX].cellNumber +
			"\nDestination Cell: " + nodes[rayPointZ, rayPointX].cellNumber + x
			);
	}
	#endregion
}
