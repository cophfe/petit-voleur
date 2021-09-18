using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChefAI : MonoBehaviour
{
	public State currentState;
	private NavMeshAgent agent;
	private FerretController target;
	public Animator animator;
	public BoxCollider kickCollider;
	public float alertedBeginDuration = 1.0f;
	public float alertedDuration = 5.0f;
	public float inspectDuration = 1.0f;
	public float ferretVisibleDuration = 1.5f;
	public float ferretGroundedThreshold = -10.0f;
	public float inspectRange = 2.0f;
	public float wanderRange = 2.0f;

	[Header("Kicking")]
	public float kickRange = 3.0f;
	public int kickDamage = 1;
	public float kickRagdollDuration = 10.0f;
	public float kickCooldown = 1.0f;
	public Vector3 kickVelocity = Vector3.forward;
	public LayerMask kickLayer;

	[Header("Throwing")]
	public GameObject[] throwablePrefabs;
	public Rigidbody currentThrowable;
	public Transform throwPoint;
	public float throwRange = 10.0f;
	public float throwDelay = 4.0f;
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
	[Tooltip("Controls the multiplier applied to the alert by LoS timer, based on distance.")]
	public AnimationCurve distanceToViewAlertCurve;

	private Transform targetTransform;
	public float inspectingTimer;
	[HideInInspector]
	public float alertedTimer;
	[HideInInspector]
	public float ferretAlertVisibleTimer;
	[HideInInspector]
	public float ferretStartAlertTimer;
	private float throwTimer;
	private float kickCooldownTimer;
	private Vector3 soundPoint;
	private Vector3 lastSeenPosition;
	private Vector3 desiredLookDir;

	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		target = FindObjectOfType<FerretController>();
		targetTransform = target.transform;

		agent.updateRotation = false;

		//Generate wander point array based on transforms in the container
		wanderPoints = new Vector3[wanderPointContainer.childCount];
		for (int i = 0; i < wanderPoints.Length; ++i)
			wanderPoints[i] = wanderPointContainer.GetChild(i).position;
	}

	void Update()
	{
		//Update blend float
		animator.SetFloat("walkBlend", agent.velocity.magnitude / agent.speed);
		
		UpdateVisibility();
		
		//Decrement inspection timer
		if (inspectingTimer > 0)
			inspectingTimer -= Time.deltaTime;

		//Don't run functionality when animations are playing, but still rotate when throwing
		if (animator.GetBool("animationPlaying"))
		{
			if (currentState == State.Throw)
				UpdateRotation();

			return;
		}

		UpdateRotation();

		//Decrement kick timer
		if (kickCooldownTimer > 0)
			kickCooldownTimer -= Time.deltaTime;
		
		//Start tree
		BaseBehaviour();
	}


	// ====================================================== //
	// ================== Public methods   ================== //
	// ====================================================== //
	
	/// <summary>
	/// Kicks all objects in the kick box, including player
	/// </summary>
	public void Kick()
	{
		//Checks if the player is in range
		Collider[] colliders = Physics.OverlapBox(kickCollider.transform.TransformPoint(kickCollider.center), Vector3.Scale(kickCollider.size, kickCollider.transform.localScale) / 2, kickCollider.transform.rotation, kickLayer);
		Rigidbody rb;
		bool playerKicked = false;
		Vector3 velocity = Quaternion.LookRotation(transform.forward, Vector3.up) * kickVelocity;
		for (int i = 0; i < colliders.Length; ++i)
		{
			//Yeet all rigidbodies
			rb = colliders[i].attachedRigidbody;
			if (rb)
			{
				//Turn the player into a ragdoll then kick, but only once and not per collider
				if (!playerKicked && rb.GetComponent<FerretController>())
				{
					target.health.Damage(kickDamage);
					target.StartRagdoll(kickRagdollDuration);
					Vector3 shakeDirection = target.cameraController.transform.InverseTransformDirection(velocity.normalized * 3);
					target.cameraController.AddCameraShake(shakeDirection);
					target.ferretAudio.PlayFerretKicked();
					playerKicked = true;
					kickCooldownTimer = kickCooldown;
				}
				
				rb.velocity = velocity;
				rb.AddTorque(transform.forward * 10, ForceMode.Impulse);
			}
		}
	}

	/// <summary>
	/// Throws an object if it's equipped
	/// </summary>
	public void Throw()
	{
		if (currentThrowable)
		{
			currentThrowable.transform.SetParent(null);
			//Calculate dead reckoning here

			Vector3 targetPoint = target.transform.position;
			Vector3 offset = targetPoint - currentThrowable.transform.position;
			float distance = offset.magnitude;
			float timeTaken = distance / throwSpeed;

			//zubat equations
			Vector3 initialVelocity = (offset - Physics.gravity * 0.5f * timeTaken * timeTaken) / timeTaken;

			currentThrowable.isKinematic = false;
			currentThrowable.velocity = initialVelocity;
			//currentThrowable.angularVelocity = Vector3.right * 5.0f;

			currentThrowable = null;
		}
	}

	/// <summary>
	/// Equips a throwable
	/// </summary>
	public void WieldThrowable()
	{
		if (!currentThrowable)
		{
			//Create the throwable and reset its values
			currentThrowable = Instantiate(throwablePrefabs[Random.Range(0, throwablePrefabs.Length)], throwPoint, true).GetComponent<Rigidbody>();
			currentThrowable.transform.localPosition = Vector3.zero;
			currentThrowable.transform.localRotation = Quaternion.identity;
		}
	}

	/// <summary>
	/// Set the point that the chef inspects and starts inspecting
	/// </summary>
	/// <param name="point">Point in world space</param>
	public void SetSoundPoint(Vector3 point)
	{
		soundPoint = point;
		inspectingTimer = inspectDuration;
	}

	// ====================================================== //
	// =================== Private Methods ================== //
	// ====================================================== //

	/// <summary>
	/// Does full visibility check on ferret from chef's eye point
	/// </summary>
	void UpdateVisibility()
	{
		//Assume the player isn't in view
		bool playerVisible = false;
		Vector3 eyeToPlayer = targetTransform.position - viewLaserPoint.position;
		float distance = eyeToPlayer.magnitude;
		//Check if the player is in view distance
		if (distance < viewDistance)
		{
			eyeToPlayer /= distance;
			//Check if the player is within the horizontal view angle
			Vector3 eyeToPlayerOnPlane = Vector3.ProjectOnPlane(eyeToPlayer, Vector3.up).normalized;
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
		
		// ~~~~~~ Update visibility timers ~~~~~ //
		if (playerVisible)
		{
			//Count up start alert timer
			ferretStartAlertTimer += Time.deltaTime * distanceToViewAlertCurve.Evaluate(distance / viewDistance);
			//Reset alert visibility timer
			ferretAlertVisibleTimer = ferretVisibleDuration;
		}
		else
		{
			//Tick down start alert timer
			if (ferretStartAlertTimer > 0)
				ferretStartAlertTimer -= Time.deltaTime;
			//Count down alert visibility timer
			ferretAlertVisibleTimer -= Time.deltaTime;
		}

		//Player was seen for long enough, start the hunt!
		if (ferretStartAlertTimer >= alertedBeginDuration)
			alertedTimer = alertedDuration;
	}

	/// <summary>
	/// Entry node for the "decision" tree
	/// </summary>
	void BaseBehaviour()
	{
		//Chef is alerted
		if (alertedTimer > 0)
		{
			alertedTimer -= Time.deltaTime;
			DoAlertState();
		}
		else if (inspectingTimer > 0)
		{
			DoInspect();
		}
		else
		{
			DoWander();
		}
	}

	/// <summary>
	/// Travels to inspection point and looks around
	/// </summary>
	void DoInspect()
	{
		currentState = State.Inspect;

		if (Vector3.Distance(soundPoint, transform.position) <= inspectRange)
			PlayLookAnim();
		else
			agent.SetDestination(soundPoint);
	}

	/// <summary>
	/// Picks a random point, travels to it and looks around
	/// </summary>
	void DoWander()
	{
		currentState = State.Wander;

		//Travels to point at wander index if an index is valid, otherwise find a new index
		if (wanderIndex >= 0)
		{
			//Look around if at wander point, otherwise keep going
			if (Vector3.Distance(wanderPoints[wanderIndex], transform.position) < wanderRange)
			{
				PlayLookAnim();
				SetWanderPoint();
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

	/// <summary>
	/// Attacks if the ferret's current position is known, or travels to last seen position
	/// </summary>
	void DoAlertState()
	{
		if (ferretAlertVisibleTimer > 0)
		{
			alertedTimer = alertedDuration;

			//Kick or throw based on how close to the floor the player is
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

	/// <summary>
	/// Travel to player and play the kick animation
	/// </summary>
	void DoKicking()
	{
		currentState = State.Kick;

		//Get distance to player ignoring the y pos
		Vector3 aiToTarget = targetTransform.position - transform.position;
		aiToTarget.y = 0;
		if (aiToTarget.sqrMagnitude < kickRange * kickRange)
		{
			if (kickCooldownTimer <= 0 && Vector3.Angle(transform.forward, aiToTarget) < 15.0f)
				PlayKickAnim();
		}
		else
			agent.SetDestination(targetTransform.position);
	}

	/// <summary>
	/// Travel close enough to the player, then start playing the throw animation with a delay between throws
	/// </summary>
	void DoThrowing()
	{
		currentState = State.Throw;

		Vector3 aiToTarget = targetTransform.position - transform.position;
		aiToTarget.y = 0;
		if (aiToTarget.magnitude < throwRange)
		{
			agent.SetDestination(transform.position);
			if (throwTimer > 0)
				throwTimer -= Time.deltaTime;
			else
			{
				throwTimer = throwDelay;
				PlayThrowAnim();
			}
		}
		else
		{
			agent.SetDestination(targetTransform.position);
		}
	}

	/// <summary>
	/// Plays look animation and stops in place
	/// </summary>
	void PlayLookAnim()
	{
		agent.SetDestination(transform.position);
		animator.SetBool("animationPlaying", true);
		animator.SetTrigger("lookAround");
	}

	/// <summary>
	/// Plays kick animation and stops in place
	/// </summary>
	void PlayKickAnim()
	{
		agent.SetDestination(transform.position);
		animator.SetBool("animationPlaying", true);
		animator.SetTrigger("kick");

	}

	/// <summary>
	/// Plays throw animation and stops in place
	/// </summary>
	void PlayThrowAnim()
	{
		agent.SetDestination(transform.position);
		animator.SetBool("animationPlaying", true);
		animator.SetTrigger("throw");
	}

	/// <summary>
	/// Picks a random wander point in the wander point array
	/// </summary>
	void SetWanderPoint()
	{
		wanderIndex = (wanderIndex + 1) % wanderPoints.Length;
	}

	/// <summary>
	/// Rotate the chef based on the current state, uses delta time to limit rotation each frame
	/// </summary>
	void UpdateRotation()
	{
		//Update rotation
		switch(currentState)
		{
			//Look at the player
			case State.Throw:
				desiredLookDir = Vector3.ProjectOnPlane(targetTransform.position - transform.position, Vector3.up).normalized;
				break;
			
			//Look at the player
			case State.Kick:
				desiredLookDir = Vector3.ProjectOnPlane(targetTransform.position - transform.position, Vector3.up).normalized;
				break;
			
			//Look toward velocity
			default:
				if (agent.velocity.sqrMagnitude > Mathf.Epsilon * Mathf.Epsilon)
					desiredLookDir = Vector3.ProjectOnPlane(agent.velocity, Vector3.up).normalized;
			break;
		}

		//Rotate towards the desired direction
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(desiredLookDir, Vector3.up), agent.angularSpeed * Time.deltaTime);
	}

	/// <summary>
	/// All states that the AI can be in
	/// </summary>
	public enum State : int
	{
		Wander,
		Inspect,
		Kick,
		Throw,
		LastSeenPos
	}
}
