using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineManager : MonoBehaviour {
	[Header("Tweakers")]
	public float heightRatio;
	public float distanceRatio;

	public LineRenderer line;
	PlayerController pc;

	void Start () {
		line.useWorldSpace = true;
		pc = GetComponentInParent<PlayerController> ();
	}

	public void UpdateLineTrajectory (Vector2 direction)
	{
		int maxSegments = 20;
		float resolution = 10f; // factor to increase number of segments

		var positions = new List<Vector3> ();

		Vector3 gravity = new Vector3 (0f, Physics.gravity.y / (resolution * resolution), 0f);
		Vector3 velocity = (new Vector3(direction.x * distanceRatio, direction.magnitude * pc.verticalFactor * heightRatio, direction.y * distanceRatio)) / resolution;

		Vector3 currentPos = transform.position;
		Vector3 lastPos = currentPos;
			   
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

	public void SetLineColor(Color color) {
		line.startColor = color;
		line.endColor = color;
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