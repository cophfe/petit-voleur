using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChefAI : MonoBehaviour
{
	public NavMeshAgent agent;
	public FerretController target;
	public Animator animator;
	public BoxCollider kickCollider;
	public bool animationPlaying = false;
	public float alertedDuration = 5.0f;
	public float inspectDuration = 1.0f;
	public float ferretVisibleDuration = 1.5f;
	public float inspectRange = 2.0f;
	public float wanderRange = 2.0f;
	public float kickRange = 3.0f;
	public float throwRange = 10.0f;
	public float kickRagdollDuration = 10.0f;
	public Vector3 kickVelocity = Vector3.forward;
	public int wanderIndex;

	public Transform wanderPointContainer;
	public Vector3[] wanderPoints;

	public float viewDistance = 50.0f;
	public float viewAngle = 60.0f;
	public LayerMask viewLaserMask;
	public Transform viewLaserPoint;

	private Transform targetTransform;
	private float inspectingTimer;
	private float alertedTimer;
	private float ferretVisibleTimer;
	private float throwTimer;
	private Vector3 soundPoint;
	private bool playerIsOnFloor = false;
	private Vector3 lastSeenPosition;

	// Start is called before the first frame update
	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		target = FindObjectOfType<FerretController>();
		targetTransform = target.transform;

		//Generate wander point array based on transforms in the container
		wanderPoints = new Vector3[wanderPointContainer.childCount];
		for (int i = 0; i < wanderPoints.Length; ++i)
			wanderPoints[i] = wanderPointContainer.GetChild(i).position;
	}

	// Update is called once per frame
	void Update()
	{
		if (animationPlaying)
			return;
		UpdateVisibility();

		BaseBehaviour();	
	}

	public void SetPlayerOnFloor(bool state)
	{
		playerIsOnFloor = state;
	}

	public void Kick()
	{
		//Checks if the player is in range
		if (Physics.CheckBox(kickCollider.transform.position, kickCollider.size / 2, kickCollider.transform.rotation, viewLaserMask))
		{
			target.velocity = Quaternion.LookRotation(transform.forward, Vector3.up) * kickVelocity;
			target.StartRagdoll(kickRagdollDuration);
		}
	}

	public void SetSoundPoint(Vector3 point)
	{
		soundPoint = point;
		inspectingTimer = inspectDuration;
	}

	void UpdateVisibility()
	{
		bool playerVisible = false;
		Vector3 eyeToPlayer = targetTransform.position - viewLaserPoint.position;
		float distance = eyeToPlayer.magnitude;
		//Check if the player is in view distance
		if (distance < viewDistance)
		{
			//Check if the player is within the horizontal view angle
			eyeToPlayer /= distance;
			Vector3 eyeToPlayerOnPlane = Vector3.ProjectOnPlane(eyeToPlayer, Vector3.up);
			if (Vector3.Angle(transform.forward, eyeToPlayerOnPlane) < viewAngle)
			{
				//Check line of sight
				if (!Physics.Raycast(viewLaserPoint.position, eyeToPlayer, distance, viewLaserMask))
				{
					playerVisible = true;
					lastSeenPosition = targetTransform.position;
				}
			}
		}
		
		if (playerVisible)
			ferretVisibleTimer = ferretVisibleDuration;
		else
			ferretVisibleTimer -= Time.deltaTime;
	}

	void BaseBehaviour()
	{
		if (inspectingTimer > 0)
		{
			inspectingTimer -= Time.deltaTime;
			DoInspect();
		}
		else
		{
			if (alertedTimer > 0)
			{
				alertedTimer -= Time.deltaTime;
				DoAlertState();
			}
			else
			{
				DoWander();
			}
		}
	}

	void DoInspect()
	{
		if (Vector3.Distance(soundPoint, transform.position) <= inspectRange)
			PlayLookAnim();
		else
			agent.SetDestination(soundPoint);
	}

	void DoWander()
	{
		if (wanderIndex >= 0)
		{
			if (Vector3.Distance(wanderPoints[wanderIndex], transform.position) < wanderRange)
			{
				PlayLookAnim();
				wanderIndex = -1;
			}
			else
			{
				agent.SetDestination(wanderPoints[wanderIndex]);
			}
		}
		else
		{
			SetWanderPoint();
		}
	}

	void DoAlertState()
	{
		if (ferretVisibleTimer > 0)
		{
			alertedTimer = alertedDuration;
		
			if (playerIsOnFloor)
				DoKicking();
			else
				DoThrowing();
		}
		else
		{
			agent.SetDestination(lastSeenPosition);
		}
	}

	void DoKicking()
	{
		if (Vector3.Distance(targetTransform.position, transform.position) < kickRange)
			PlayKickAnim();
		else
			agent.SetDestination(targetTransform.position);
	}

	void DoThrowing()
	{
		Vector3 aiToTarget = targetTransform.position - transform.position;
		aiToTarget.y = 0;
		if (aiToTarget.magnitude < throwRange)
		{
			if (throwTimer > 0)
				throwTimer -= Time.deltaTime;
			else
				ThrowItem();
		}
		else
		{
			agent.SetDestination(targetTransform.position);
		}
	}

	void PlayLookAnim()
	{
		agent.isStopped = true;
		animationPlaying = true;
	}

	void PlayKickAnim()
	{
		agent.isStopped = true;
		animationPlaying = true;
	}

	void ThrowItem()
	{
		agent.isStopped = true;
		print("THROWING");
	}

	void SetWanderPoint()
	{
		wanderIndex = Random.Range(0, wanderPoints.Length);
	}
}
