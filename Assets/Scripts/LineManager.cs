using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineManager : MonoBehaviour {
	[Header("Tweakers")]
	public float heightRatio;
	public float distanceRatio;

	public LineRenderer line;
	PlayerController pc;
	float mass;

	void Start () {
		line.useWorldSpace = true;
		pc = GetComponentInParent<PlayerController> ();
		mass = pc.GetComponent<Rigidbody> ().mass;
	}

	public void UpdateLineTrajectory (Vector2 direction)
	{
		float angle = Mathf.Atan2 (direction.x, direction.y) * Mathf.Rad2Deg;
		float x = direction.magnitude * pc.swipeForce;
		float y = x * pc.verticalFactor;
		Vector3 velocity = (new Vector3(0, y, x) / mass) * Time.fixedDeltaTime;

		int maxSegments = 30;
		float resolution = 10f; // factor to increase number of segments

		var positions = new List<Vector3> ();

		Vector3 currentPos = transform.position;
		Vector3 lastPos = currentPos;
			   
		while (positions.Count < maxSegments) {
			positions.Add (currentPos);

			//stop adding positions if we hit something
			if (hasHitSomethingBesidesPlayer (lastPos, currentPos)) {
				break;
			}

			lastPos = currentPos;

			currentPos += Quaternion.Euler(0f, angle, 0f) * (velocity / resolution);
			velocity += Physics.gravity / resolution;
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
		bool hasHitSomething = Physics.Linecast (pos1, pos2, out hitInfo, 1<<10);
		if (!hasHitSomething) {
			return false;
		} else {
			return true;
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