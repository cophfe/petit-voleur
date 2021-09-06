/*==========================================================
    
    Programmer: Dylan Smith
    LastUpdated: 4/09/2021
 
    Animation Manager
 
 ==========================================================*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnimationManager : MonoBehaviour
{
    //===========================================

    public Animator m_FerretAnimator;

    //===========================================
    void Start()
    {
    }

    //===========================================
    // Play the ferrets run animation. (Pass in the ferret controllers current speed)
    public void PlayFerretRun(float indexSpeed)
    {
        m_FerretAnimator.SetFloat("m_FerretSpeed", indexSpeed);
    }

    //===========================================

    public void PlayFerretDash()
    {
        m_FerretAnimator.SetTrigger("m_FerretDash");
    }

    //===========================================

    public void PlayFerretIdle()
    {

    }

    //===========================================

    public void PlayFerretJump(bool isTouchingGround)
    {

    }
}
