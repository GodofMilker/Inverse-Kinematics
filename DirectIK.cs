using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectIK: IterativeIK {

	public float totalLength = 5f;
	public int segmentCount = 1;

	public override void Start() {
		base.Start();
	}

	public override int getJointCount() {
		return segmentCount + 1;
	}

	public override float getSegmentLength() {
		return totalLength / segmentCount;
	}

	public override void CreateSegments() {
		AdjustChilds(segmentCount);
		for(int i = 0;i < segmentCount;i++) {
			Transform segment = segments[i];
			Vector3 scale = segment.localScale;
			scale.z = getSegmentLength();
			segment.localScale = scale;
		}
		UpdateSegments();
	}
	public override void UpdateSegments() {
		for(int i = 0;i < segments.Count;i++) {
			Transform segment = segments[i];
			Vector3 diff = joints[i + 1] - joints[i];
			segment.position = joints[i] + diff / 2 + transform.position;
			segment.LookAt(joints[i + 1] + transform.position);
		}
	}


}
