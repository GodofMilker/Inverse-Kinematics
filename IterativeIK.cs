using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IterativeIK: MonoBehaviour {


	protected int jointCount = 2;
	public float segmentLength = 1f;
	public Transform target;
	public Transform pole;
	public bool adjustForPole = false;
	public bool optimizedPole = false;

	public GameObject segmentPrefab;
	public Transform segmentsParent;

	public Vector3 TargetLocal {
		get {
			return target.position - transform.position;
		}
	}
	public Vector3 PoleLocal {
		get {
			return pole.position - transform.position;
		}
	}

	protected float decidedSegmentLength = 1f;
	protected List<Vector3> joints = new List<Vector3>();
	protected List<Transform> segments = new List<Transform>();


	// Start is called before the first frame update
	public virtual void Start() {
		AdjustChilds(0);
		CreateJoints();
	}

	// Update is called once per frame
	public virtual void Update() {
		UpdateJoints(10);

	}

	public virtual int getJointCount() {
		return Mathf.Max(jointCount, 2);
	}
	public virtual float getSegmentLength() {
		return segmentLength;
	}

	public virtual void CreateJoints() {
		joints = new List<Vector3>();
		joints.Add(Vector3.zero);
		Vector3 poleSegment = PoleLocal.normalized * getSegmentLength();
		for(int i = 1;i < getJointCount();i++) {
			Vector3 newJoint = joints[i - 1] + poleSegment;
			joints.Add(newJoint);
		}
		decidedSegmentLength = getSegmentLength();
		CreateSegments();
	}




	public void UpdateJoints(int iterateCount) {
		for(int iteration = 0;iteration < iterateCount - 1;iteration++) {
			if((joints[joints.Count - 1] - TargetLocal).sqrMagnitude < 0.001f)
				break;
			UpdateJointsOnce();
		}
		if(adjustForPole)
			AdjustToPole();
		UpdateJointsOnce();
		UpdateSegments();
	}

	public void UpdateJointsOnce() {
		joints[joints.Count - 1] = TargetLocal;
		for(int i = joints.Count - 2;i > 0;i--) {
			Vector3 jointTarget = joints[i] - joints[i + 1];
			joints[i] = jointTarget.normalized * decidedSegmentLength + joints[i + 1];
		}

		for(int i = 1;i < joints.Count;i++) {
			Vector3 jointTarget = joints[i] - joints[i - 1];
			joints[i] = jointTarget.normalized * decidedSegmentLength + joints[i - 1];
		}
	}

	public void AdjustToPole() {
		for(int i = 1;i < joints.Count - 1;i++) {
			Vector3 midPoint;
			Vector3 difference;
			Vector3 direction;
			Vector3 delta;
			Vector3 deltaPlane;
			float radius;

			difference = joints[i + 1] - joints[i - 1];
			direction = difference.normalized;
			midPoint = (joints[i + 1] + joints[i - 1]) / 2.0f;
			radius = (joints[i] - midPoint).magnitude;
			delta = PoleLocal - midPoint;
			deltaPlane = delta - Vector3.Dot(direction, delta) * direction;
			Vector3 newJointPos = midPoint + radius * deltaPlane.normalized;
			if(!optimizedPole || (newJointPos - joints[i]).sqrMagnitude / radius >= 0.02f)
				joints[i] = newJointPos;
		}
	}

	public virtual void CreateSegments() {

	}

	public virtual void UpdateSegments() {

	}

	private void OnDrawGizmos() {
		/*foreach(Vector3 joint in joints) {
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(joint + transform.position, 0.2f);
		}*/
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(target.position, 0.3f);
		Gizmos.color = Color.gray;
		Gizmos.DrawSphere(pole.position, 0.3f);
	}

	public void AdjustChilds(int segmentCount) {
		segments.Clear();
		int childCount = segmentsParent.childCount;
		if(childCount != segmentCount) {

			if(childCount > segmentCount) {
				for(int i = 0;i < childCount - segmentCount;i++) {
					DestroyImmediate(segmentsParent.GetChild(0).gameObject);
				}
			} else {
				for(int i = 0;i < segmentCount - childCount;i++) {
					Instantiate(segmentPrefab, segmentsParent);
				}
			}
		}
		for(int i = 0;i < segmentCount;i++) {
			segments.Add(segmentsParent.GetChild(i));
		}
	}

}
