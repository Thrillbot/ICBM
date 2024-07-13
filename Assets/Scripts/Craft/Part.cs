using Mirror;
using Rewired;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Part : NetworkBehaviour
{
	[Header("Part")]
	public float mass;
	[Range(0,1)]
	public float drag;
	public Vector2 dimensions;

	public GameObject explosion;
	public Player player;
	public Rigidbody rootRigidbody;

	public Collider attachedCollider;

	private bool armed;
	protected float _mass;
	protected float airPressure;

	private int playerId = 0;
	private Transform networkTransform;

	/*
	[SyncVar(hook = "SyncPosition")]
	public Vector3 position = Vector3.zero;

	[SyncVar(hook = "SyncRotation")]
	public Quaternion rotation = Quaternion.identity;
	*/

	private bool hasAuthority = false;

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
			//transform.position = position;
			//transform.rotation = rotation;
			return;
		}

		if (armed && transform.position.sqrMagnitude < Universe.GetPointOnPlanet(transform.position).sqrMagnitude)
		{
			Instantiate(explosion, transform.position + transform.position.normalized * 0.25f, Quaternion.identity);
			Destroy(gameObject);
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

		//workerVec = transform.position.normalized; // Local Up
		//workerVec = Vector3.Cross(workerVec, Vector3.forward); // Local Right

		Vector3 localUp = transform.position.normalized; // Local Up
		Vector3 localRight = Vector3.Cross(localUp, Vector3.forward); // Local Right

		if (transform.root == transform)
		{
			float altitude = transform.position.magnitude - Universe.SeaLevel;
			airPressure = (Universe.KarmanLine - altitude) / Universe.KarmanLine; // Adjust based on your Universe class
			airPressure = Mathf.Clamp(airPressure, 0.0f, 1.0f); // Clamp to [0, 1]

			// Calculate angular velocity in radians per second
			float dayLengthInSeconds = Universe.dayLengthInMinutes * 60f;
			float angularVelocity = (2 * Mathf.PI) / dayLengthInSeconds;

			// Calculate desired surface velocity at the current position
			float surfaceSpeed = angularVelocity * Universe.SeaLevel * Mathf.Cos(Mathf.Asin(localUp.y));
			Vector3 desiredSurfaceVelocity = localRight * surfaceSpeed;

			// Calculate the current velocity component along the tangent
			float currentTangentSpeed = Vector3.Dot(rootRigidbody.velocity, localRight);

			// Calculate the difference
			float speedDifference = surfaceSpeed - currentTangentSpeed;

			// Apply force based on the difference
			float forceMagnitude = airPressure * speedDifference * drag;
			rootRigidbody.AddForce(localRight * forceMagnitude);
		}

		rootRigidbody.AddForce(-rootRigidbody.velocity * airPressure * drag); // Apply drag

		if (!armed && transform.position.sqrMagnitude > Universe.GetPointOnPlanet(transform.position).sqrMagnitude)
		{
			armed = true;
		}


		//position = transform.position;
		//rotation = transform.rotation;
	}

	/*
	private void LateUpdate()
	{
		if (networkTransform)
		{
			networkTransform.transform.position = transform.position;
			networkTransform.transform.rotation = transform.rotation;
		}
	}
	*/

	public bool isArmed
	{
		get { return armed; }
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
		hasAuthority = true;
		foreach (Builder b in FindObjectsByType<Builder>(FindObjectsSortMode.None))
		{
			if (b.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
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
				//GetComponent<Part>().position = transform.position;
				//GetComponent<Part>().rotation = transform.rotation;

				StartCoroutine(Initialize(b));
				break;
			}
		}
	}

	IEnumerator Initialize (Builder b)
	{
		yield return null;

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
		//GetComponent<Part>().position = transform.position;
		//GetComponent<Part>().rotation = transform.rotation;
	}

	/*
	void SyncPosition(Vector3 oldValue, Vector3 newValue)
	{
		if (!isServer)
			return;

		position = newValue;
	}

	void SyncRotation(Quaternion oldValue, Quaternion newValue)
	{
		if (!isServer)
			return;

		rotation = newValue;
	}
	*/

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
