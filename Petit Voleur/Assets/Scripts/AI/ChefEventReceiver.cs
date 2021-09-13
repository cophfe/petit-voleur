using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefEventReceiver : MonoBehaviour
{
    public ChefAI chef;

	/// <summary>
	/// Animation has reached kick frame
	/// </summary>
    public void KickFrameReached()
    {
        chef.Kick();
    }

	/// <summary>
	/// Animation has reached throw frame
	/// </summary>
    public void ThrowFrameReached()
    {
        chef.Throw();
    }

	/// <summary>
	/// Animation has reached wield frame
	/// </summary>
	public void WieldFrameReached()
	{
		chef.WieldThrowable();
	}
}
