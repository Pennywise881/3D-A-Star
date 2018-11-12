using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

/*
 * Use this class to set up the shared resources of the two classes attached to the Player.
 */

public abstract class Controller : MonoBehaviour
{
	protected static Node[,] nodes;

	protected static EventWaitHandle ewh1 = new EventWaitHandle(false, EventResetMode.AutoReset);
	protected static EventWaitHandle ewh2 = new EventWaitHandle(false, EventResetMode.AutoReset);

	protected static int rayPointX, rayPointZ;
	protected static int playerPositionX, playerPositionZ;

	protected static List<Vector3> path;

	protected static bool gotPathData;

	protected static int index = -1;

	public Node[,] NodeInformation
	{
		get { return nodes; }
		set { nodes = value; }
	}
}
