using TMPro;
using UnityEngine;

public class OnlineRoom : MonoBehaviour
{
	public TextMeshProUGUI text;

	public TextMeshProUGUI text2;

	public CodeStateAnimation anim;

	public CodeStateAnimation cameraAnim;

	private bool doorIsOpen;

	private bool playersInOnlineRoom;

	private Vector3 doorStart;

	private GameManager manager;

	private float sinceSwitch = 1f;

	public GameObject[] enable;

	public GameObject[] disable;

	private bool mIsInsideMatch;

	public bool hasStarted;

	private int playersAlive;

	private int playersCloseEnough;

	private bool peopleAreOnLeftSide;

	private bool peopleAreOnRightSide;

	private bool peopleAreOnBothSides;

	private void Start()
	{
		manager = GameManager.Instance;
		doorStart = anim.transform.position;
	}

	private void Update()
	{
		if (hasStarted)
		{
			cameraAnim.state1 = true;
			return;
		}
		sinceSwitch += Time.deltaTime;
		CheckSides();
		CheckDoor();
	}

	public void GoBack()
	{
		GameObject[] array = enable;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(true);
			}
		}
		GameObject[] array2 = disable;
		foreach (GameObject gameObject2 in array2)
		{
			if ((bool)gameObject2)
			{
				gameObject2.SetActive(false);
			}
		}
		if ((bool)cameraAnim)
		{
			cameraAnim.state1 = true;
		}
		base.enabled = false;
	}

	private void CheckSides()
	{
		peopleAreOnLeftSide = false;
		peopleAreOnRightSide = false;
		peopleAreOnBothSides = false;
		foreach (Controller item in manager.playersAlive)
		{
			if (item.GetComponentInChildren<Torso>().transform.position.z < -18.5f)
			{
				peopleAreOnRightSide = true;
			}
			else
			{
				peopleAreOnLeftSide = true;
			}
		}
		if (peopleAreOnLeftSide && peopleAreOnRightSide)
		{
			peopleAreOnBothSides = true;
		}
	}

	private void CheckDoor()
	{
		playersAlive = manager.playersAlive.Count;
		playersCloseEnough = 0;
		foreach (Controller item in manager.playersAlive)
		{
			if (Vector3.Distance(item.GetComponentInChildren<Torso>().transform.position, doorStart) < 3f)
			{
				playersCloseEnough++;
			}
		}
		text.text = playersCloseEnough + "/" + playersAlive;
		text2.text = playersCloseEnough + "/" + playersAlive;
		if ((playersAlive > 0 && playersCloseEnough == playersAlive) || peopleAreOnBothSides)
		{
			doorIsOpen = true;
			anim.state1 = false;
		}
		else
		{
			doorIsOpen = false;
			anim.state1 = true;
		}
		if (!playersInOnlineRoom)
		{
			foreach (Controller item2 in manager.playersAlive)
			{
				if (item2.GetComponentInChildren<Torso>().transform.position.z < -18.5f && sinceSwitch > 1f)
				{
					playersInOnlineRoom = true;
					sinceSwitch = 0f;
					cameraAnim.state1 = false;
					{
						foreach (Controller item3 in manager.playersAlive)
						{
							Vector3 vector = new Vector3(0f, -8f + Random.Range(-1f, 1f), -22f + Random.Range(-1f, 1f));
							Vector3 vector2 = vector - item3.GetComponentInChildren<Torso>().transform.position;
							item3.GetComponentInChildren<Blink>(true).LeaveTrail(item3.gameObject);
							item3.transform.position += vector2;
						}
						break;
					}
				}
			}
			return;
		}
		foreach (Controller item4 in manager.playersAlive)
		{
			if (!(item4.GetComponentInChildren<Torso>().transform.position.z > -18.5f) || !(sinceSwitch > 1f))
			{
				continue;
			}
			playersInOnlineRoom = false;
			sinceSwitch = 0f;
			cameraAnim.state1 = true;
			{
				foreach (Controller item5 in manager.playersAlive)
				{
					Vector3 vector3 = new Vector3(0f, -8f + Random.Range(-1f, 1f), -15f + Random.Range(-1f, 1f));
					Vector3 vector4 = vector3 - item5.GetComponentInChildren<Torso>().transform.position;
					item5.GetComponentInChildren<Blink>(true).LeaveTrail(item5.gameObject);
					item5.transform.position += vector4;
				}
				break;
			}
		}
	}
}
