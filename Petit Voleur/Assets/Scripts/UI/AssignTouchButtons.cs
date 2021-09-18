using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//this just exists so buttons work automatically across scenes
public class AssignTouchButtons : MonoBehaviour
{
	public EventTrigger biteTrigger;
	public EventTrigger dashTrigger;
	public EventTrigger jumpTrigger;
	
	Button dashButton;
	FerretController ferret;

    void Start()
    {
		ferret = FindObjectOfType<FerretController>();
		if (!ferret)
		{
			Debug.LogWarning("Button Assigner could not find ferret");
			//listen, some error checking is better than no error checking, right?
			return;
		}
		dashButton = dashTrigger.GetComponent<Button>();

		//add dash
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener((data) => { ferret.OnDash(); StartCoroutine(DashTimout()); });
		dashTrigger.triggers.Add(entry);

		//add jump
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener((data) => { ferret.OnJump(); });
		jumpTrigger.triggers.Add(entry);
		//jump release too
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerUp;
		entry.callback.AddListener((data) => { ferret.OnJumpRelease(); });
		jumpTrigger.triggers.Add(entry);
		//add bite
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener((data) => { ferret.GetComponent<FerretPickup>().OnGrab(); });
		biteTrigger.triggers.Add(entry);
	}

	IEnumerator DashTimout()
	{
		dashButton.interactable = false;
		yield return new WaitForSeconds(ferret.dashDuration);
		dashButton.interactable = true;
	}
}
