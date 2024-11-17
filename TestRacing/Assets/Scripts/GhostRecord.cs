using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostRecord : MonoBehaviour
{
    public List<Vector3> recordedPositions = new List<Vector3>();
    public float recordInterval = 0.1f;
    private float timeSinceLastRecord = 0f;
    private bool isRecording = true;

    void Update()
    {
        if (isRecording)
        {
            timeSinceLastRecord += Time.deltaTime;
            if (timeSinceLastRecord >= recordInterval)
            {
                recordedPositions.Add(transform.position);
                timeSinceLastRecord = 0f;
            }
        }
    }

    public void StopRecording()
    {
        isRecording = false;
    }   
}
