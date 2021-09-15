/*=============================================================
   Programmer: Dylan Smith
   LastUpdated: 7/09/2021

    PointTracker Class.

    NOTE: 
    Some functions need to be called before others. The StampValues()
    function must be called before using the ClearAll() function to 
    avoid setting up the functions again.

    HardClear:
    The HardClear() function CANNOT be undone. Please use the clear functions carefully.
    HardClear will erase every index in the PointTracker Class. StampValues & ClearAll 
    functions are affected by this function, Only use a HardClear when starting a new level

    WatchScoreLimit:
    The score limit is watched by the tracker & this function should NOT be called outside this class.

    Bonus Points:
    will only affect the AddPoints() function. If enabled the points will get multiplied by the given amount (2 by default.) 
        

 ==============================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTracker : MonoBehaviour
{
    //=============================================
    
    private int m_PlayerScore;
	[SerializeField]
    private int m_ScoreMaxLimit;
    private int m_ScoreMinLimit;
    private int m_ScorePrevious;

    private int m_StampPrevious;
    private int m_StampMin;
    private int m_StampMax;
    private int m_StampBonus;
    public GameManager m_Game;

    private bool m_IsBonusPointsEnabled;
    private int m_BonusMultiplyAmount;
    //=============================================

    void Start()
    {
        m_ScoreMinLimit = 0; 
        m_PlayerScore = 0;
        m_ScorePrevious = 0;

        m_StampMax = 0;
        m_StampMin = 0;
        m_StampPrevious = 0;
        m_StampBonus = 0;

        m_Game = FindObjectOfType<GameManager>();
        m_IsBonusPointsEnabled = false;
        m_BonusMultiplyAmount = 2;
        StampValues();
    }

    //=============================================
    /// <summary>
    /// Get players current score.
    /// </summary>
    /// <returns> player score </returns>
    public int GetPoints()
    {
        return m_PlayerScore;
    }

    //=============================================
    /// <summary>
    /// Set players score to any index thats withing the min & max range
    /// </summary>
    /// <param name="index"></param>
    public void SetPoints(int index)
    {
        if (index <= m_ScoreMaxLimit && index >= m_ScoreMinLimit)
        {
            WatchScoreLimit();
            m_PlayerScore = index;
            m_Game.UpdatePointUI();
        }
        else
        {
            Debug.LogError("Tried to SetPoints() an invalid index.");
        }
    }

    //============================================
    /// <summary>
    /// Add an index to the players score 
    /// </summary>
    /// <param name="index"></param>
    public void AddPoints(int index)
    {
        if (index <= m_ScoreMaxLimit && index >= m_ScoreMinLimit)
        {
            if (!m_IsBonusPointsEnabled)
            {
                WatchScoreLimit();
                m_ScorePrevious = m_PlayerScore;
                m_PlayerScore += index;
                m_Game.UpdatePointUI();
            }
            else
            {
                WatchScoreLimit();
                m_ScorePrevious = m_PlayerScore;
                m_PlayerScore += index * m_BonusMultiplyAmount;
                m_Game.UpdatePointUI();
            }
        }
        else
        {
            Debug.LogError("Tried to AddPoints() an invalid index.");
        }
    }

    //============================================
    /// <summary>
    /// Subtract an index from the player score
    /// </summary>
    /// <param name="index"></param>
    public void SubtractPoints(int index)
    {
        if (index > m_StampMin)
        {
            WatchScoreLimit();
            m_PlayerScore -= index;
            m_Game.UpdatePointUI();
        }
        else
        {
            Debug.LogError("Points could not be subtracted. Index goes below minium.");
        }
    }

    //===========================================
    /// <summary>
    /// Set the Max amount of points the player can get
    /// </summary>
    /// <param name="index"></param>
    public void SetMaxLimit(int index)
    {

        m_ScoreMaxLimit = index;
    }

    //============================================
    /// <summary>
    /// Get the max amount of points the player will get.
    /// </summary>
    /// <returns> Max amount score can reach </returns>
    public int GetMaxLimit()
    {
        return m_ScoreMaxLimit;
    }

    //===========================================
    /// <summary>
    /// Set the min amount of score that player can get
    /// </summary>
    /// <param name="min"> Input the lowest amount the score can go.</param>
    public void SetMinLimit(int min)
    {
        m_ScoreMinLimit = min;
    }

    //===========================================
    /// <summary>
    /// Get the mininum amount of score the player can get.
    /// </summary>
    /// <returns> Mininum Score </returns>
    public int GetMinLimit()
    {
        return m_ScoreMinLimit;
    }

    //===========================================
    /// <summary>
    /// Get the previous number of the score before it was changed.
    /// </summary>
    /// <returns> previous score </returns>
    public int GetPreviousScore()
    {
        return m_ScorePrevious;
    }

    //===========================================
    /// <summary>
    /// Clear the players score to its default value (Zero)
    /// </summary>
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
    /// <summary>
    /// All values are set to Zero. Can't be undone unless a stamp has been done.
    /// </summary>
    public void ClearAll()
    {
        m_PlayerScore = 0;
        m_ScoreMinLimit = 0;
        m_ScoreMaxLimit = 0;
        m_ScorePrevious = 0;
        m_BonusMultiplyAmount = 2;
    }

    //===========================================
    /// <summary>
    /// Stamp all values & save them in a backup.
    /// </summary>
    public void StampValues()
    {
        m_StampMax = m_ScoreMaxLimit;
        m_StampMin = m_ScoreMinLimit;
        m_StampPrevious = m_PlayerScore;
        if (m_IsBonusPointsEnabled)
        {
            m_StampBonus = m_BonusMultiplyAmount;
        }
    }

    //===========================================
    /// <summary>
    /// Gets the stamped values & re-initalizes them back to what they were when stamped.
    /// </summary>
    public void RestoreStamp()
    {
        m_ScoreMinLimit = m_StampMin;
        m_ScoreMaxLimit = m_StampMax;
        m_PlayerScore = m_StampPrevious;
        if (m_IsBonusPointsEnabled)
        {
            m_BonusMultiplyAmount = m_StampBonus;
        }
        m_Game.UpdatePointUI();
    }

    //==========================================
    /// <summary>
    // Clear all values in the stamp. (Cannot be undone)
    /// </summary>
    public void ClearStamp()
    {
        m_StampMax = 0;
        m_StampMin = 0;
        m_StampPrevious = 0;
        if (m_IsBonusPointsEnabled)
        {
            m_StampBonus = 0;
        }
    }

    //==========================================
    /// <summary>
    /// HardClear will erase everything including Stamp values. Cannot be undone.
    /// </summary>
    public void HardClear()
    {
        ClearAll();
        ClearStamp();
    }

    
    //==========================================
    /// <summary>
    /// Watches for the score reaching the limit. (Do NOT call outside this class!)
    /// </summary>
    public void WatchScoreLimit()
    {
        if (m_PlayerScore >= m_ScoreMaxLimit)
        {
            m_Game.OnReachedPointThreshold();
        }
    }

    //=========================================
    /// <summary>
    /// Enables bonus points which multiplys the points added by a index. (2 by default)
    /// </summary>
    public void EnableBonusPoints()
    {
        m_IsBonusPointsEnabled = true;
    }

    //=========================================
    /// <summary>
    /// Disable the bonus points function. Points added will no longer multiply.
    /// </summary>
    public void DisableBonusPoints()
    {
        m_IsBonusPointsEnabled = false;
    }

    //=========================================
    /// <summary>
    /// Set the amount that will multiply during bonus points. (Bonus points must be enabled to set.)
    /// </summary>
    /// <param name="index"></param>
    public void SetMultiplyAmount(int index)
    {
        m_BonusMultiplyAmount = index;
    }

    //========================================
    /// <summary>
    /// Get the active status of bonus points.
    /// </summary>
    /// <returns> Bonus Points enabled/disabled </returns>
    public bool IsBonusPointsEnabled()
    {
        return m_IsBonusPointsEnabled;
    }
}

