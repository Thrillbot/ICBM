using Mirror;
using Rewired;
using System.Collections.Generic;
using UnityEngine;

public class Part : NetworkBehaviour
{
	[Header("Part")]
	public float mass;
	[Range(0,1)]
	public float drag;
	public Vector2 dimensions;

	[Header("Projection")]
	public LineRenderer lineRenderer;
	public LineRenderer mapLineRenderer;
	public int numPoints = 50; // Number of points to display in the line renderer
	public float timeStep = 0.1f; // Time step for each point in the simulation
	public LayerMask collisionMask; // Layer mask to check for collisions

	[Header("Other")]
	public GameObject explosion;
	public float explosionRadius;
	public float explosionDamage;
	public Builder builder;

	public Rigidbody rootRigidbody;
	public Collider attachedCollider;

	[SyncVar(hook = "SetArmed")]
	private bool armed;
	protected Player player;
	protected float _mass;
	protected float airPressure;
	protected float predictedAirPressure;
	protected float altitude;
	protected float predictedAltitude;

	private int playerId = 0;

	protected bool hasAuthority = false;

	private void Awake()
	{
		// Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
		player = ReInput.players.GetPlayer(playerId);
	}

	public virtual void Start()
	{
		_mass = mass;
	}

	public virtual void FixedUpdate()
	{
		if (!hasAuthority)
		{
			return;
		}

		if (transform.position.sqrMagnitude < Universe.killAltitude)
		{
			//CmdDestroyPart(transform.position + transform.position.normalized * 0.25f);
			DestroySelf();
			return;
		}

		if (armed && transform.position.sqrMagnitude < Universe.GetPointOnPlanet(transform.position).sqrMagnitude)
		{
			//CmdDestroyPart(transform.position + transform.position.normalized * 0.25f);
			DestroySelf();
			return;
		}

		if (!rootRigidbody)
		{
			if (transform.root == null)
				rootRigidbody = GetComponent<Rigidbody>();
			else
				rootRigidbody = transform.root.GetComponent<Rigidbody>();
			if (!rootRigidbody) return;
		}

		rootRigidbody.AddForce(transform.position.normalized * Universe.Gravity * _mass); // Apply gravity towards planet center.  There's not enough gravity fall off at these distances to include it

		Vector3 localUp = transform.position.normalized; // Local Up
		Vector3 localRight = Vector3.Cross(localUp, Vector3.forward); // Local Right

		if (transform.root == transform)
		{
			altitude = transform.position.magnitude - Universe.SeaLevel;
			airPressure = (Universe.KarmanLine - altitude) / Universe.KarmanLine; // Adjust based on your Universe class
			airPressure = Mathf.Clamp(airPressure, 0.0f, 1.0f); // Clamp to [0, 1]

			// Calculate angular velocity in radians per second
			float dayLengthInSeconds = Universe.dayLengthInMinutes * 60f;
			float angularVelocity = (2 * Mathf.PI) / dayLengthInSeconds;

			// Calculate desired surface velocity at the current position
			float surfaceSpeed = angularVelocity * Universe.SeaLevel * Mathf.Cos(Mathf.Asin(localUp.y));

			// Calculate the current velocity component along the tangent
			float currentTangentSpeed = Vector3.Dot(rootRigidbody.velocity, localRight);

			// Calculate the difference
			float speedDifference = surfaceSpeed - currentTangentSpeed;

			// Apply force based on the difference
			float forceMagnitude = airPressure * speedDifference * drag;
			rootRigidbody.AddForce(localRight * forceMagnitude);

			// Lift (Up/Down) based on alignment of forward vector and velocity times by air pressure
			rootRigidbody.AddForce(Vector3.Cross(transform.right, rootRigidbody.velocity) * Vector3.Dot(transform.forward, rootRigidbody.velocity) * airPressure * airPressure * 0.025f);
			ApplyTorqueToAlignWithVelocity();
		}

		rootRigidbody.AddForce(-rootRigidbody.velocity * airPressure * drag); // Apply drag

		if (!armed && transform.position.sqrMagnitude > Universe.GetPointOnPlanet(transform.position).sqrMagnitude)
		{
			armed = true;
			SetArmed(false, true);
		}
    }

	private void LateUpdate()
	{
		if (armed && lineRenderer)
		{
			DrawTrajectory();
		}
	}

	public void Abort ()
	{
		DestroySelf();
	}

	void DestroySelf ()
	{
		builder.DestroyPart(gameObject, armed, transform.position, explosionRadius, explosionDamage, GetComponent<ControlModule>() ? 1 : 0);
	}

	void ApplyTorqueToAlignWithVelocity()
	{
		Vector3 velocityDirection = rootRigidbody.velocity.normalized;
		Vector3 forwardDirection = transform.forward;

		// Calculate the angle between forward direction and velocity
		float angle = Vector3.Angle(forwardDirection, velocityDirection);

		// Calculate the rotation axis
		Vector3 rotationAxis = Vector3.Cross(forwardDirection, velocityDirection).normalized;

		// Include the magnitude of the velocity in the torque calculation
		float velocityMagnitude = rootRigidbody.velocity.magnitude;

		// Proportional torque
		Vector3 proportionalTorque = rotationAxis * angle * airPressure * airPressure * velocityMagnitude;

		// Calculate angular velocity around the right axis (for dampening)
		Vector3 angularVelocity = rootRigidbody.angularVelocity;
		float angularVelocityRight = Vector3.Dot(angularVelocity, transform.right);

		// Derivative torque (dampening)
		float dampeningCoefficient = 0.4f; // Adjust this value as needed
		Vector3 derivativeTorque = -angularVelocityRight * transform.right * dampeningCoefficient;

		// Total torque
		Vector3 totalTorque = proportionalTorque + derivativeTorque;

		// Apply torque
		rootRigidbody.AddTorque(totalTorque * 0.1f);
	}

	void DrawTrajectory()
	{
		List<Vector3> points = new List<Vector3>();
		Vector3 currentPosition = transform.position;
		Vector3 currentVelocity = rootRigidbody.velocity;
		points.Add(currentPosition);

		for (int i = 0; i < numPoints; i++)
		{
			// Create a temporary position and velocity for prediction
			Vector3 tempPosition = currentPosition;
			Vector3 tempVelocity = currentVelocity;

			foreach (Part p in transform.GetComponentsInChildren<Part>())
			{
				tempVelocity += tempPosition.normalized * Universe.Gravity * p._mass * timeStep; // Apply gravity towards the center
			}

			predictedAltitude = tempPosition.magnitude - Universe.SeaLevel;
			predictedAirPressure = (Universe.KarmanLine - predictedAltitude) / Universe.KarmanLine; // Adjust based on your Universe class
			predictedAirPressure = Mathf.Clamp(predictedAirPressure, 0.0f, 1.0f); // Clamp to [0, 1]
			foreach (Part p in transform.GetComponentsInChildren<Part>())
			{
				tempVelocity += -tempVelocity * predictedAirPressure * p.drag * timeStep; // Apply drag
			}

			// Update position based on new velocity
			currentPosition += tempVelocity * timeStep;
			currentVelocity = tempVelocity;

			points.Add(currentPosition);

			// Check for collision
			RaycastHit hit;
			if (Physics.Raycast(points[i], points[i + 1] - points[i], out hit, Vector3.Distance(points[i], points[i + 1]), collisionMask))
			{
				points.Add(hit.point);
				break;
			}
		}

		lineRenderer.positionCount = points.Count;
		lineRenderer.SetPositions(points.ToArray());

		mapLineRenderer.positionCount = points.Count;
		mapLineRenderer.SetPositions(points.ToArray());
	}

	public bool isArmed
	{
		get { return armed; }
	}

	void SetArmed (bool oldValue, bool newValue)
	{
		armed = newValue;
	}

	Vector3 FindPartOffset(Vector3 direction, Part part)
	{
		Vector3 offset;
		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
		{
			offset = transform.up * part.dimensions.x * 0.5f * Mathf.Sign(direction.x);
		}
		else
		{
			offset = transform.forward * part.dimensions.y * 0.5f * Mathf.Sign(direction.y);
		}
		return offset;
	}

	public override void OnStartAuthority ()
	{
		base.OnStartAuthority();

		hasAuthority = true;
		foreach (Builder b in FindObjectsByType<Builder>(FindObjectsSortMode.None))
		{
			if (b.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				builder = b;
				//b.InitializePart(gameObject);
				if (GetComponent<ControlModule>())
				{
					transform.parent = b.transform;
					transform.localEulerAngles = Vector3.zero;
					transform.localScale = Vector3.one * 10f;
					transform.localPosition = new Vector3(0, 0, -1.5f);
					GetComponent<ControlModule>().launchPad = b.launchPad;
					b.craftHead = gameObject;
				}
				else
				{
					transform.parent = b.currentHitCollider.transform.parent;
					transform.SetAsFirstSibling();
					transform.localPosition = b.ghostPart.transform.localPosition;
					transform.localRotation = b.ghostPart.transform.localRotation;

					foreach (SphereCollider s in transform.GetComponentsInChildren<SphereCollider>())
					{
						if (Vector3.Dot(s.transform.localPosition.normalized, b.currentHitCollider.transform.parent.position - s.transform.position) > 0.8f)
						{
							s.enabled = false;
							break;
						}
					}

					GetComponent<Part>().attachedCollider = b.currentHitCollider.GetComponent<Collider>();
				}
				break;
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(transform.position, new Vector3(dimensions.x, 0, dimensions.y));

		if (transform.parent == null)
		{
			Gizmos.color = Color.red;
			foreach (SphereCollider c in transform.GetComponentsInChildren<SphereCollider>())
			{
				if (c.enabled)
					Gizmos.DrawSphere(-c.transform.parent.localPosition + FindPartOffset(c.transform.localPosition, this), 0.01f);
			}
		}
	}
}
