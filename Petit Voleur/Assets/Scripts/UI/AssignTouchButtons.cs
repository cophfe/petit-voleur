using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//this just exists so buttons work automatically across scenes
public class AssignTouchButtons : MonoBehaviour
{
	public EventTrigger BiteButton;
	public EventTrigger DashButton;
	public EventTrigger JumpButton;
	
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

		//add dash
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener((data) => { ferret.OnDash(); });
		DashButton.triggers.Add(entry);

		//add jump
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener((data) => { ferret.OnJump(); });
		JumpButton.triggers.Add(entry);
		//jump release too
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerUp;
		entry.callback.AddListener((data) => { ferret.OnJumpRelease(); });
		JumpButton.triggers.Add(entry);

		//add bite
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener((data) => { ferret.GetComponent<FerretPickup>().OnGrab(); });
		BiteButton.triggers.Add(entry);
	}
}
