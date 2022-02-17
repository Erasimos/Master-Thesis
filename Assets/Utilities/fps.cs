using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class fps : MonoBehaviour
{
    public Text fpsText;
    private float fps_measure_interval = 1f; // Seconds
    private float elapsed_measure_time = 0f;
    private int renderedFrames = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        renderedFrames += 1;
        elapsed_measure_time += Time.deltaTime;

        if(elapsed_measure_time >= fps_measure_interval)
        {
            fpsText.text = "FPS: " +  renderedFrames.ToString();
            renderedFrames = 0;
            elapsed_measure_time = 0;
        }
    }
}
