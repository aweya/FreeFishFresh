using System.Collections.Generic;
using UnityEngine;

public class LevelCheckPoints : MonoBehaviour
{
    public List<CheckPontSingle> checkpoints = new List<CheckPontSingle>();
    public PlayerController playerController;
    public EndScreenManager endScreenManager;
    public string StatusMessage = "";

    private int nextCheckpoint = 0;
    public float raceTimer = 0f;
    private bool isRaceRunning = false;

    void Awake()
    {
        checkpoints.Clear();



        foreach (CheckPontSingle checkpoint in GetComponentsInChildren<CheckPontSingle>())
        {
            checkpoints.Add(checkpoint);
            checkpoint.level = this;

            Debug.Log("Registered " + checkpoint.name);
        }
    }

    public void Update()
    {
        if (isRaceRunning)
        {
            raceTimer += Time.deltaTime;
        }

        if (nextCheckpoint == 0)
        {
            raceTimer = 0f;
            isRaceRunning = true;
        }
    }

    public void CheckPointTriggered(CheckPontSingle checkpoint, bool debug)
    {
        int checkpointIndex = checkpoints.IndexOf(checkpoint);

        if (debug)
        {
            // Correct checkpoint
            checkpoint.Activate();


            playerController.resetPoint = checkpoint.CheckPointReset;
            StatusMessage = "";

            nextCheckpoint = checkpointIndex + 1;

            if (nextCheckpoint >= checkpoints.Count)
            {
                RaceFinished();
            }
        }


        // Already passed this checkpoint
        if (checkpointIndex < nextCheckpoint)
        {
            Debug.Log("Old checkpoint");
            return;
        }

        // Skipped a checkpoint
        if (checkpointIndex > nextCheckpoint)
        {
            Debug.Log("Checkpoint Skipped");
            StatusMessage = "Checkpoint Skipped";
            return;
        }

        // Correct checkpoint
        checkpoint.Activate();

        Debug.Log("Checkpoint " + nextCheckpoint + " reached");

        playerController.resetPoint = checkpoint.CheckPointReset;
        StatusMessage = "";

        nextCheckpoint++;

        if (nextCheckpoint >= checkpoints.Count)
        {
            RaceFinished();
        }
    }

    public void RaceFinished()
    {
        isRaceRunning = false;
        endScreenManager.ShowEndScreen(raceTimer);
        Debug.Log("Finished in " + raceTimer.ToString("F2") + " seconds!");
        StatusMessage = "Finished in " + raceTimer.ToString("F2") + " seconds!";



        nextCheckpoint = 0;
    }
}