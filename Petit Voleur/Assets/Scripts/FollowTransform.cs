using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
	public Transform target;
	public float maxDistance = 1.0f;
	public bool changeUp = false;
	[SerializeField]
	private Vector3 basePos;
	[SerializeField]
	private Quaternion baseRotation;
    // Start is called before the first frame update
    void Start()
    {
        basePos = transform.localPosition;
		baseRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
		transform.position = Vector3.MoveTowards(transform.parent.TransformPoint(basePos), target.position, maxDistance);

		if (changeUp)
			transform.up = (transform.position - transform.parent.position).normalized;
		else
			transform.rotation = target.rotation * baseRotation;
    }
}
