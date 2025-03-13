using System;
using UnityEngine;

[Serializable]
public class CustomTimer
{
    public float duration; // Duration is measured in seconds
    public float elapsedTime; // elapsed time
    public bool isRunning; // is the timer running
    public float timeRemaining; // time remaining on the timer

    // creates a new timer. Requires the new duration and if that duration is in minutes or not
    public CustomTimer(float newDuration, bool isInMinutes)
    {
        // Convert duration to seconds if it's provided in minutes
        duration = isInMinutes ? newDuration * 60 : newDuration;
        elapsedTime = 0;
        isRunning = false;
    }

    // starts timer, with last duration used. This does not set the duration!!!
    public void StartTimer()
    {
        elapsedTime = 0;
        isRunning = true;
    }

    /// <summary>
    /// If the timer isn't running this will come back false. If not, it will increment the elapsed time and then check if that time has reached the duration.
    /// If it's reached the duration, it will enter the statement and do what's required. 
    /// </summary>
    public bool UpdateTimer()
    {
        // If the timer isn't running, return false immediately.
        if (!isRunning) return false;

        // Increment the elapsed time passed since last frame
        elapsedTime += Time.deltaTime;
        
        // if the elapsed time has reached the duration, stop the timer and return that the timer has completed.
        if (elapsedTime >= duration)
        {
            isRunning = false;
            return true;
        }
        
        // if it's not finished, return false
        return false;
    }
    
    // Gets remaining time from timer in seconds.
    public float GetRemainingTime()
    {
        if (!isRunning)
            return 0;

        timeRemaining = Mathf.Max(duration - elapsedTime, 0);
        return timeRemaining;
    }

    public void StopTimer()
    {
        isRunning = false;
        elapsedTime = 0;
    }

    // Resets timer with the last duration that was used.
    public void ResetTimer()
    {
        StartTimer();
    }
}
