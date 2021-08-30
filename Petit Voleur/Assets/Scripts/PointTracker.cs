/*=============================================================
   Programmer: Dylan Smith
   LastUpdated: 30/08/2021

    PointTracker Class.

    NOTE: 
    Some functions need to be called before others. The StampValues()
    function must be called before using the ClearAll() function to 
    avoid setting up the functions again.

    HardClear:
    The HardClear() function CANNOT be undone. Please use the clear functions carefully.
    HardClear will erase every index in the PointTracker Class. StampValues & ClearAll 
    functions are affected by this function, Only use a HardClear when starting a new level
    or 

 ==============================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTracker : MonoBehaviour
{
    //=============================================
    
    private int m_PlayerScore;
    private int m_ScoreMaxLimit;
    private int m_ScoreMinLimit;
    private int m_ScorePrevious;

    private int m_StampPrevious;
    private int m_StampMin;
    private int m_StampMax;

    //=============================================

    void Start()
    {
        m_ScoreMaxLimit = 100; 
        m_ScoreMinLimit = 0; 
        m_PlayerScore = 0;
        m_ScorePrevious = 0;

        m_StampMax = 0;
        m_StampMin = 0;
        m_StampPrevious = 0;

        StampValues();
    }

    //=============================================
    // Get players current score.
    public int GetPoints()
    {
        return m_PlayerScore;
    }

    //=============================================
    //Set players score to any index thats withing the min & max range.
    public void SetPoints(int index)
    {
        if (index <= m_ScoreMaxLimit && index >= m_ScoreMinLimit)
        {
            m_PlayerScore = index;
        }
        else
        {
            Debug.LogError("Tried to SetPoints() an invalid index.");
        }
    }

    //============================================
    //Add an index to the players score 
    public void AddPoints(int index)
    {
        if (index <= m_ScoreMaxLimit && index >= m_ScoreMinLimit)
        {
            m_ScorePrevious = m_PlayerScore;
            m_PlayerScore += index;
        }
        else
        {
            Debug.LogError("Tried to AddPoints() an invalid index.");
        }
    }
    
    //============================================
    // Subtract an index from the player score
    public void SubtractPoints(int index)
    {
        if (index > m_StampMin)
        {
            m_PlayerScore -= index;
        }
        else
        {
            Debug.LogError("Points could not be subtracted. Index goes below minium.");
        }
    }


    //===========================================
    // Set the max amount of score the player can get.
    public void SetMaxLimit(int max)
    {
        m_ScoreMaxLimit = max;
    }

    //===========================================
    // Get the max amount that can be added to the players score.
    public int GetMaxLimit()
    {
        return m_ScoreMaxLimit;
    }

    //===========================================
    // Set the min amount of score that player can get
    public void SetMinLimit(int min)
    {
        m_ScoreMinLimit = min;
    }

    //===========================================
    // Get the mininum amount of score the player can get.
    public int GetMinLimit()
    {
        return m_ScoreMinLimit;
    }

    //===========================================
    // Get the previous number of the score before it was changed.
    public int GetPreviousScore()
    {
        return m_ScorePrevious;
    }

    //===========================================
    // Clear the players score to its default value (Zero)
    public void ClearPoints()
    {
        if (m_PlayerScore != 0)
        {
            m_ScorePrevious = 0;
            m_PlayerScore = 0;
        }
        else
        {
            Debug.LogError("Failed to clear points. Points already at default value.");
        }
    }

    //===========================================
    // All values are set to Zero. Can't be undone unless a stamp is 
    public void ClearAll()
    {
        m_PlayerScore = 0;
        m_ScoreMinLimit = 0;
        m_ScoreMaxLimit = 0;
        m_ScorePrevious = 0;
    }

    //===========================================
    //Stamp all values & save them in a backup.
    public void StampValues()
    {
        m_StampMax = m_ScoreMaxLimit;
        m_StampMin = m_ScoreMinLimit;
        m_StampPrevious = m_PlayerScore;
    }

    //===========================================
    // Gets the stamped values & re-initalizes them back to what they were when stamped.
    public void RestoreStamp()
    {
        m_ScoreMinLimit = m_StampMin;
        m_ScoreMaxLimit = m_StampMax;
        m_PlayerScore = m_StampPrevious;
    }

    //==========================================
    // Clear all values in the stamp. (Cannot be undone)
    public void ClearStamp()
    {
        m_StampMax = 0;
        m_StampMin = 0;
        m_StampPrevious = 0;
    }

    //==========================================
    // HardClear will erase everything including Stamp values. Cannot be undone.
    public void HardClear()
    {
        ClearAll();
        ClearStamp();
    }
}

