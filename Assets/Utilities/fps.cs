using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fps : MonoBehaviour
{
    public Text fpsText;

    private float fps_measure_interval = 1f; // Seconds
    private float elapsed_measure_time = 0f;
    private int renderedFrames = 0;
    private int fps1 = 0;
    private int fps2 = 0;

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
            fps1 = renderedFrames;
            //fpsText.text = "FPS: " +  renderedFrames.ToString();
            renderedFrames = 0;
            elapsed_measure_time = 0;
        }
        fps2 = Mathf.RoundToInt(1.0f / Time.deltaTime);
        fpsText.text = "FPS: " + fps1.ToString() + " | " + fps2.ToString(); //"FPS: " + fps1.ToString() + " | FPS (Crude): " + fps2.ToString();
    }
}
