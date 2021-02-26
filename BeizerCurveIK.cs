using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeizerCurveIK: IterativeIK {
	public float totalLength = 5f;
	public float desiredSegmentCount = 50;
	public int maxSegmentCount = 100;

	[Range(0.1f, 1f)]
	public float min = 0.7f;
	[Range(1f, 2f)]
	public float max = 1.4f;

	public override void CreateSegments() {
		AdjustChilds(maxSegmentCount);
		for(int i = 0;i < maxSegmentCount;i++) {
			Transform segment = segments[i];
			Vector3 scale = segment.localScale;
			scale.z = getBeizerSegmentLength();
			segment.localScale = scale;
		}
		UpdateSegments();
	}

	public override int getJointCount() {
		return 3;
	}

	public float getBeizerSegmentLength() {
		return totalLength / desiredSegmentCount;
	}

	public float BeizerSegmentLengthSquared {
		get {
			float length = getBeizerSegmentLength();

			return length * length;
		}
	}

	public override float getSegmentLength() {
		return totalLength / 2;
	}



	public override void UpdateSegments() {
		float tHalf = 0.5f / segments.Count;
		Vector3 curJoint = joints[0];
		float tCur = 0;
		int i = 0;
		float beizerSquared = BeizerSegmentLengthSquared;
		while(tCur < 1f && i < segments.Count) {
			float tNext = tHalf * 2f + tCur;
			Vector3 nextJoint = CubicBeizerCurve(tNext);
			Vector3 diff = nextJoint - curJoint;
			float calcDiff = diff.sqrMagnitude / beizerSquared;

			int a = 0;
			while(calcDiff > max && a < 10) {
				tNext -= tHalf * (calcDiff - max) / calcDiff;
				nextJoint = CubicBeizerCurve(tNext);
				diff = nextJoint - curJoint;
				calcDiff = diff.sqrMagnitude / beizerSquared;
				a++;
			}
			a = 0;
			while(calcDiff < min && a < 10) {
				tNext += tHalf * (min - calcDiff) / min;
				nextJoint = CubicBeizerCurve(tNext);
				diff = nextJoint - curJoint;
				a++;
				calcDiff = diff.sqrMagnitude / beizerSquared;
			}

			segments[i].position = transform.position + diff / 2f + curJoint;
			Vector3 scale = segments[i].localScale;
			scale.z = diff.magnitude;
			segments[i].localScale = scale;
			segments[i].LookAt(nextJoint + transform.position);
			segments[i].gameObject.SetActive(true);
			tCur = tNext;
			curJoint = nextJoint;
			i++;
		}
		for(;i < segments.Count;i++) {
			segments[i].gameObject.SetActive(false);
		}
	}

	public Vector3 CubicBeizerCurve(float t) {
		return joints[0] * Mathf.Pow(1 - t, 3) + 3 * joints[1] * Mathf.Pow(1 - t, 2) * t + 3 * getMaxPole() * (1 - t) * t * t + joints[2] * t * t * t;
	}
	public Vector3 getMaxPole() {
		Vector3 poleDirection = PoleLocal - joints[1];
		if(poleDirection.sqrMagnitude < getSegmentLength() * getSegmentLength())
			return PoleLocal;
		return (PoleLocal - joints[1]).normalized * getSegmentLength() + joints[1];
	}
}
