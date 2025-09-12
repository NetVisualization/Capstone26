using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class VendorLookupScript : MonoBehaviour
{
    StreamReader reader;

    public string path;

    // if concerned about performance, remove this list; it doesn't do anything
    public List<string> testVendors = new();

    public Dictionary<string, string> MACVendors = new();

    String line; 
     
    // Start is called before the first frame update
    void Start()
    {
        // Path to the file
        path = Path.Combine(Application.streamingAssetsPath, "MAC_vendor.txt");

        // Because we're building on Android we gotta use a web request
        var loadRequest = UnityWebRequest.Get(path);
        loadRequest.SendWebRequest();

        // Load entire file
        while (!loadRequest.isDone) { }

        // Use this reader for the headest
        reader = new StreamReader(new MemoryStream(loadRequest.downloadHandler.data));
        // Use this reader for the computer
        //reader = new StreamReader(path);

        line = reader.ReadLine();

        // Process each line
        while ((line = reader.ReadLine()) != null)
        {
            // Splitting lines by tabs
            var parts = line.Split('\t');

            // Ensures the line has at least the 2 fields we are interested in (MAC prefix and vendor name)
            if (parts.Length >= 2)
            {
                string MACPrefix = parts[0].Trim().Replace(":", String.Empty); // first field  - MAC prefix
                string vendorName = parts[1].Trim();                           // second field - Vendor name

                // Add the values to the list for debugging purposes (just to make sure we are parsing/reading correctly)
                testVendors.Add($"{MACPrefix} - {vendorName} - {line.Replace(":", String.Empty)[0..6]}");

                try
                {
                    // Add to the dictionary (I think this is how a dictionary works but who knows)
                    MACVendors.Add(MACPrefix, vendorName);
                }
                catch (Exception e) 
                {
                    Debug.LogError("Error adding to dictionary: " + e.Message);    
                }
            }
            // Just to make sure it doesn't break in case of an empty or malformed line
            // but like being honest, the file won't change so lowkey we may not even need this
            else
            {
                Debug.LogWarning("evil line found, you're cooked my friend");
            }
        }
        // Imagine forgetting to close the file... embarrassing
        reader.Close();
    }

    public string GetVendor(string MAC)
    {
        // The MAC string should not have any colons in it
        // e.g. use 001223 instead of 00:12:23
        try
        {
            return MACVendors[MAC[0..6]];
        }
        catch (KeyNotFoundException)
        {
            return $"Not Found - {MAC}";
        }        
    }
}
