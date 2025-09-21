using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;

public class PingIPHandler : MonoBehaviour
{
    /////// Should probably be made to run asynchronously
    
    public TextMeshProUGUI timeText;

    System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
    PingReply reply;

    public void testtest()
    {
        reply = ping.Send(System.Net.IPAddress.Parse("google.com"));
        timeText.text = "hello";

    }

    public void pingIP(GameObject IPinputField)
    {
        // Get IP address from GameObject
        //string IP = IPinputField.GetComponent<TextMeshProUGUI>().text;
        string IP = "8.8.8.8";

        // Ping IP
        reply = ping.Send(IP);

        if (reply.Status == IPStatus.Success)
        {
            // Set timeText text to the time it took
            timeText.text = $"Time: {reply.RoundtripTime}";
        }
        else
        {
            // Indicate error
            timeText.text = "Error";

        }

    }
}
