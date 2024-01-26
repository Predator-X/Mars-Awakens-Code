using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI FpsText;
    private float pollinTime = 0.5f , time;
    private int frameCount;
    
    void Update()
    {
        time += Time.deltaTime;

        frameCount++;

        if(time>= pollinTime)
        {
            int framerate = Mathf.RoundToInt(frameCount / time);
            FpsText.text = framerate.ToString() + "FPS";

            time -= pollinTime;
            frameCount = 0;
        }
    }
}
