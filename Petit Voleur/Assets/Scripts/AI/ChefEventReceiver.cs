using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefEventReceiver : MonoBehaviour
{
    public ChefAI chef;

    public void KickFrameReached()
    {
        chef.Kick();
    }

    public void ThrowFrameReached()
    {
        chef.Throw();
    }

	public void WieldFrameReached()
	{
		chef.WieldThrowable();
	}
}
