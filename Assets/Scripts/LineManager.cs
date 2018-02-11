using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineManager : MonoBehaviour {

	//SOURCE: http://forum.unity3d.com/threads/projectile-prediction-line.143636/

	private float flickFactor = 1;
	public LineRenderer line;
	//the maximum valid distance of the line
	private float maxDragDistance = 21;
	//the factor by which the player moves up, based on the the line distance
	public float verticalFactor = 3;

	void Start () {
		line.useWorldSpace = true;
	}

	public Vector3 LaunchVelocity (Vector3 startPos, Vector3 mousePos)
	{
//		var heading = mousePos - startPos;
//		var direction = heading.normalized;
//		var distance = heading.magnitude;

		float xDifference = mousePos.x - startPos.x;
		float zDifference = mousePos.z - startPos.z;

		float verticalVelocity = (Mathf.Sqrt(Mathf.Abs((xDifference * xDifference) + (zDifference * zDifference)))) / verticalFactor;
		Vector3 launchVelocity = new Vector3 (xDifference * flickFactor * 1.5f, verticalVelocity * Mathf.Abs (flickFactor), zDifference * flickFactor * 1.5f);

		// limit magnitude of launch
		if (Mathf.Abs (launchVelocity.magnitude) > maxDragDistance)
		{
			float overflowFactor = Mathf.Abs (launchVelocity.magnitude / maxDragDistance);
			launchVelocity /= overflowFactor;
		}

		return launchVelocity;
	}

	public void UpdateLineTrajectory (Vector3 startPos, Vector3 mousePos)
	{

//		var heading = mousePos - startPos;
//		var direction = heading.normalized;
//		var distance = heading.magnitude;
//		print(distance);

//		float forwardStep = 5f;
//		float upStep = 1.5f;
//		float gravityStep = 1f;
		int maxSegments = 20;
		float resolution = 10f; // factor to increase number of segments

		var positions = new List<Vector3> ();
		Vector3 currentPos = new Vector3 ();
		Vector3 lastPos = new Vector3 ();

		Vector3 gravity = new Vector3 (0f, Physics.gravity.y / (resolution * resolution), 0f);
		Vector3 velocity = LaunchVelocity (startPos, mousePos) / resolution;

		currentPos = startPos;
		lastPos = currentPos;
			   
		while (positions.Count < maxSegments) {
			positions.Add (currentPos);

			//stop adding positions if we hit something
//			if (hasHitSomethingBesidesPlayer (lastPos, currentPos)) {
//				break;
//			}

			lastPos = currentPos;

	    	currentPos += velocity;
	    	velocity += gravity;
	    }

	    BuildLine(positions);
	}

	bool hasHitSomethingBesidesPlayer (Vector3 pos1, Vector3 pos2)
	{
		RaycastHit hitInfo;
		bool hasHitSomething = Physics.Linecast (pos1, pos2, out hitInfo);
		if (!hasHitSomething) {
			return false;
		} else {
			print (hitInfo.collider.gameObject.tag);
//			may have hit player and it doesn't count
			if (hitInfo.collider.gameObject.tag == "Untagged" || hitInfo.collider.gameObject.tag == "Enemy") {
				return true;
			} else {
				return false;

			}
//			return true;
		}
	}
	 
//	bool TravelTrajectorySegment(Vector3 startPos, Vector3 direction, float speed, float timePerSegmentInSeconds, List<Vector3> positions)
//	{
//	    var newPos = startPos + direction * speed * timePerSegmentInSeconds + Physics.gravity * timePerSegmentInSeconds;
//	   
//	    RaycastHit hitInfo;
//	    var hasHitSomething = Physics.Linecast(startPos, newPos, out hitInfo);
//	    if (hasHitSomething)
//	    {
//	        newPos = hitInfo.position;
//	    }
//	    positions.Add(newPos);
//	   
//	    return hasHitSomething;
//	}
	 
	void BuildLine(List<Vector3> positions) {
	    line.SetVertexCount(positions.Count);
	    for (var i = 0; i < positions.Count; ++i)
	    {
	        line.SetPosition(i, positions[i]);
	    }
	}
}