using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaybackHandler : MonoBehaviour
{
    // Need a reference to the spawner cause that's where all the node/connection/packet data is stored #screwEvan
    public NodeSpawnerScript spawner;

    public Slider startslider;
    public Slider endslider;

    private bool playing = false;

    // %age of slider bar to move each tick (negative because 0 = latest)
    private float playbackSpeed = -.005f;
    public float skipAmount = 0.1f;

    // Update is called once per frame
    void Update()
    {
        if (playing == true)
        {
            endslider.value += playbackSpeed;
        }
    }

    public void PlayControl(string action)
    {
        if (action == "play")
        {
            if (endslider.value <= 0.05)
            {
                endslider.value = 1;
            }
            playing = true;
        }
        else if (action == "pause")
        {
            playing = false;
        }
        else if (action == "skip")
        {
            endslider.value += skipAmount;
        }
    }

    public void TogglePlaybackSpeed(GameObject buttonText)
    {
        const float slowSpeed = -0.0005f;
        const float normalSpeed = -0.005f;
        const float fastSpeed = -0.025f;

        switch (playbackSpeed)
        {
            // Normal -> Slow
            case normalSpeed:
                buttonText.GetComponent<TextMeshProUGUI>().text = "Playback Speed:\nSlow";
                playbackSpeed = slowSpeed;
                break;

            // Slow -> Fast
            case slowSpeed:
                buttonText.GetComponent<TextMeshProUGUI>().text = "Playback Speed:\nFast";
                playbackSpeed = fastSpeed;
                break;

            // Fast -> Normal
            case fastSpeed:
                buttonText.GetComponent<TextMeshProUGUI>().text = "Playback Speed:\nNormal";
                playbackSpeed = normalSpeed;
                break;
        }
    }
}
