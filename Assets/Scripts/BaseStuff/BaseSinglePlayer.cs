using Rewired;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Universe;

public class BaseSinglePlayer : MonoBehaviour
{
	[SerializeField]
	private float health = 100;
	[SerializeField]
	private Image healthbar;

	[SerializeField]
	private int playerId = 0;

	public string playerName;
	[SerializeField]
	private TMP_Text nameTag;
	public Texture2D playerIcon;
	[SerializeField]
	private RawImage playerFlag;
	[SerializeField]
	private GameObject destroyedBasePrefab;

	private Vector3 workerVec;
	private Player player;
	private bool initialized = false;

	private bool visible;
	private bool placed;

	private TMP_Text debugText;
	private List<Explosion> explosions;

	private Vector2 zoneAngles;
	private float workerFloat;

	public struct InitArgs
	{
		public int playerIndex;
		public Color playerColor;
	}

	void Awake()
	{
		player = ReInput.players.GetPlayer(playerId);

		transform.parent = Planet.transform;
	}

	public bool Visible
	{
		get { return visible; }
		set { visible = value; }
	}

	public void Initialize(InitArgs args)
	{
		int playerIndex = 0;
		//playerColor = args.playerColor;

		Planet.GetComponent<Worldificate>().GenerateWorld();

		zoneAngles = new Vector2(-360, 360);

		if (debugText == null)
		{
			foreach (TMP_Text t in FindObjectsOfType<TMP_Text>())
			{
				if (t.name.Contains("Debug"))
				{
					debugText = t;
					break;
				}
			}
		}

		Transform cam = FindObjectOfType<FreeCam>().transform;

		cam.LookAt(Quaternion.Euler(0, 0, (zoneAngles.y - zoneAngles.x) / 2 + zoneAngles.x) * Vector3.up);
		cam.localEulerAngles = new Vector3(0, cam.localEulerAngles.y, 0);

		transform.parent = Planet.transform;
		initialized = true;

		GetComponentInChildren<Builder>().controlButtons.SetActive(true);

		GameObject.FindWithTag("LoadingScreen").SetActive(false);
	}

	public void ApplyDamage(float damage)
	{
		health -= damage;
		if (health <= 0) BaseDeath();
	}

	void BaseDeath()
	{
		GameObject tempGo = Instantiate(destroyedBasePrefab);
		tempGo.transform.parent = GetComponentInChildren<Builder>().transform.parent;
		tempGo.transform.position = GetComponentInChildren<Builder>().transform.position;
		tempGo.transform.rotation = GetComponentInChildren<Builder>().transform.rotation;
		Destroy(GetComponentInChildren<Builder>().gameObject);
		nameTag.color = Color.red;

	}

	public void SetName(string oldValue, string newValue)
	{
		playerName = newValue;
		nameTag.text = playerName;
	}

	public void SetPlayerIcon(Texture2D oldValue, Texture2D newValue)
	{
		playerIcon = newValue;
		playerFlag.texture = newValue;
	}

	void SetHealth(float oldvalue, float newValue)
	{
		health = newValue;
		healthbar.fillAmount = health / 100f;
	}

	public void FocusBase()
	{
		GameObject.FindWithTag("FreeCam").GetComponent<FreeCam>().Focus(transform, 5);
	}

	public void AbortFlight()
	{
		if (GameObject.FindWithTag("FreeCam").GetComponent<FreeCam>().target.GetComponent<Part>())
			GameObject.FindWithTag("FreeCam").GetComponent<FreeCam>().target.GetComponent<Part>().Abort();
	}

	private void Update()
	{
		if (!initialized)
			return;

		if (placed)
		{
			explosions ??= new();
			foreach (Explosion e in FindObjectsOfType<Explosion>())
			{
				if (explosions.Contains(e)) continue;
				explosions.Add(e);

				if ((e.transform.position - transform.position).sqrMagnitude <= e.explosionRadius * e.explosionRadius)
				{
					ApplyDamage(e.explosionDamage);
				}
			}
		}

		if (!placed)
		{
			try
			{
				workerVec = GetPointOnPlanet(GetMousePointOnPlane());
			}
			catch
			{
				return;
			}

			transform.position = workerVec;
			transform.LookAt(transform.position * 2f);
			workerVec = transform.localEulerAngles;
			workerVec.z = 90;
			transform.localEulerAngles = workerVec;

			workerVec = GetPointOnPlanet(GetMousePointOnPlane());

			workerFloat = Vector3.SignedAngle(workerVec.normalized, Vector3.up, Vector3.forward);
			if (workerFloat < 0) workerFloat += 360f;

			visible = false;
			if (workerFloat > zoneAngles.x && workerFloat < zoneAngles.y)
			{
				visible = true;
			}

			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = visible;
			}

			if (visible && player.GetButtonUp("Interact"))
			{
				debugText.text = "";
				placed = true;
			}
		}
	}

	public void FixedUpdate()
	{
		if (!placed)
		{
			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = visible;
			}
		}
		else
		{
			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = true;
			}
		}
	}

	public void DestroyPart(GameObject part, bool armed, Vector3 pos, float explosionRadius, float explosionDamage, int explosionIndex)
	{
		if (part != null)
		{
			if (armed)
			{
				GameObject explosionGO = Instantiate(GetComponentInChildren<Builder>().explosions[explosionIndex], pos, Quaternion.identity);
			}

			Destroy(part);
		}
	}

	public void Stage(GameObject part)
	{
		if (part != null)
			Destroy(part.gameObject);
	}

	public void CmdSpawnPart(int index)
	{
		GameObject newPart;
		if (index == -1)
			newPart = Instantiate(GetComponentInChildren<Builder>().craftHeadPrefab);
		else
			newPart = Instantiate(GetComponentInChildren<Builder>().parts[index]);
	}

	public void CmdDestroyPart(GameObject netID)
	{
		if (netID != null)
			Destroy(netID.gameObject);
	}
}