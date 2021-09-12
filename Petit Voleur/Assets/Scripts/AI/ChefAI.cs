using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChefAI : MonoBehaviour
{
	public enum State : int
	{
		Wander,
		Inspect,
		Kick,
		Throw,
		LastSeenPos
	}
	
	public State currentState;
	private NavMeshAgent agent;
	private FerretController target;
	public Animator animator;
	public BoxCollider kickCollider;
	public float alertedDuration = 5.0f;
	public float inspectDuration = 1.0f;
	public float ferretVisibleDuration = 1.5f;
	public float ferretGroundedThreshold = -10.0f;
	public float inspectRange = 2.0f;
	public float wanderRange = 2.0f;
	public float kickRange = 3.0f;
	public float throwRange = 10.0f;
	public float kickRagdollDuration = 10.0f;
	public Vector3 kickVelocity = Vector3.forward;
	public LayerMask kickLayer;

	[Header("Throwing")]
	public GameObject[] throwablePrefabs;
	public Rigidbody currentThrowable;
	public Transform throwPoint;
	public float throwSpeed;
	public int deadReckoningSamples;
	
	[Header("Wander Points")]
	public int wanderIndex;
	public Transform wanderPointContainer;
	[SerializeField]
	private Vector3[] wanderPoints;

	[Header("Line of Sight")]
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
		if (animator.GetBool("animationPlaying"))
			return;

		animator.SetFloat("walkBlend", agent.velocity.magnitude / agent.speed);

		UpdateVisibility();

		//Start tree
		BaseBehaviour();	
	}

	public void Kick()
	{
		//Checks if the player is in range
		if (Physics.CheckBox(kickCollider.transform.TransformPoint(kickCollider.center), Vector3.Scale(kickCollider.size, kickCollider.transform.localScale) / 2, kickCollider.transform.rotation, kickLayer))
		{
			target.velocity = Quaternion.LookRotation(transform.forward, Vector3.up) * kickVelocity;
			target.StartRagdoll(kickRagdollDuration);
			target.rigidbody.AddTorque(transform.forward * 10, ForceMode.Impulse);
		}
	}

	public void Throw()
	{
		currentThrowable.transform.SetParent(null);
		//Calculate dead reckoning here

		Vector3 targetPoint = target.transform.position;
		Vector3 offset = targetPoint - currentThrowable.transform.position;
		float distance = offset.magnitude;
		float timeTaken = distance / throwSpeed;

		Vector3 initialVelocity = (offset - Physics.gravity * 0.5f * timeTaken * timeTaken) / timeTaken;

		currentThrowable.isKinematic = false;
		currentThrowable.velocity = initialVelocity;
		currentThrowable.angularVelocity = Vector3.right * 5.0f;
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
		{
			ferretVisibleTimer = ferretVisibleDuration;
			alertedTimer = alertedDuration;
		}
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
		currentState = State.Inspect;

		if (Vector3.Distance(soundPoint, transform.position) <= inspectRange)
			PlayLookAnim();
		else
			agent.SetDestination(soundPoint);
	}

	void DoWander()
	{
		currentState = State.Wander;

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
		
			if (target.transform.position.y < ferretGroundedThreshold)
				DoKicking();
			else
				DoThrowing();
		}
		else
		{
			currentState = State.LastSeenPos;
			agent.SetDestination(lastSeenPosition);
		}
	}

	void DoKicking()
	{
		currentState = State.Kick;
		if (Vector3.Distance(targetTransform.position, transform.position) < kickRange)
		{
			PlayKickAnim();
		}
		else
			agent.SetDestination(targetTransform.position);
	}

	void DoThrowing()
	{
		currentState = State.Throw;

		Vector3 aiToTarget = targetTransform.position - transform.position;
		aiToTarget.y = 0;
		if (aiToTarget.magnitude < throwRange)
		{
			if (throwTimer > 0)
				throwTimer -= Time.deltaTime;
			else
				PlayThrowAnim();
		}
		else
		{
			agent.SetDestination(targetTransform.position);
		}
	}

	void PlayLookAnim()
	{
		agent.SetDestination(transform.position);
		animator.SetBool("animationPlaying", true);
		animator.SetTrigger("lookAround");
	}

	void PlayKickAnim()
	{
		agent.SetDestination(transform.position);
		animator.SetBool("animationPlaying", true);
		animator.SetTrigger("kick");

	}

	void PlayThrowAnim()
	{
		agent.SetDestination(transform.position);
		animator.SetBool("animationPlaying", true);
		animator.SetTrigger("throw");

		currentThrowable = Instantiate(throwablePrefabs[Random.Range(0, throwablePrefabs.Length)], throwPoint, true).GetComponent<Rigidbody>();
		currentThrowable.transform.localPosition = Vector3.zero;
		currentThrowable.transform.localRotation = Quaternion.identity;
	}

	void SetWanderPoint()
	{
		wanderIndex = Random.Range(0, wanderPoints.Length);
	}
}
