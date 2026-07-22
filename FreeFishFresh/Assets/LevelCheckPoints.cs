using System.Collections.Generic;
using UnityEngine;

public class LevelCheckPoints : MonoBehaviour
{
    public List<CheckPontSingle> checkpoints = new List<CheckPontSingle>();
    public PlayerController playerController;
    public string StatusMessage = "";

    private int nextCheckpoint = 0;
    public float raceTimer = 0f;
    private bool isRaceRunning = false;

    void Awake()
    {
        checkpoints.Clear();

        foreach (Transform child in transform)
        {
            CheckPontSingle checkpoint = child.GetComponent<CheckPontSingle>();


            if (checkpoint != null)
            {
                Debug.Log("i registered this checkpoint" + checkpoint);
                checkpoints.Add(checkpoint);
                checkpoint.level = this;
            }
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

    public void CheckPointTriggered(CheckPontSingle checkpoint)
    {
        int checkpointIndex = checkpoints.IndexOf(checkpoint);

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
            isRaceRunning = false;
            Debug.Log("Finished in " + raceTimer.ToString("F2") + " seconds!");
            StatusMessage = "Finished in " + raceTimer.ToString("F2") + " seconds!";

            nextCheckpoint = 0;
        }
    }
}