using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShard : MonoBehaviour
{
	public float destroyTime;
	public Rigidbody[] pieces;
	public float minExplodeVelocity;
	public float maxExplodeVelocity;
	
	// Start is called before the first frame update
	void Start()
	{
		Destroy(gameObject, destroyTime);
		foreach(Rigidbody piece in pieces)
		{
			Vector3 direction = piece.transform.localPosition.normalized;
			piece.velocity = direction * Random.Range(minExplodeVelocity, maxExplodeVelocity);
		}
	}
}
