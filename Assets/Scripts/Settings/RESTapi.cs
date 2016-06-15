using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
A class for providing functions for communication with the REST API server.
*/
public class RESTapi : MonoBehaviour{

    public static string server = "http://martinmihov.com:8888/";
    protected static string secret = "c1a78ef2ea7375fe3d1d08d7098c504ecd78e9cfd886f04170f53afd17a1c547";

    public delegate void OnComplete(string result);
    public delegate void OnError();

    // GET request, calls the OnComplete callback when completed.
    public WWW GET(string url, OnComplete onComplete, OnError onError)
    {
        WWW www = new WWW(url);
        StartCoroutine(WaitForRequest(www, onComplete, onError));
        return www;
    }

    // POST request, sends the data from the Dictionary as WWWForm and calls the OnComplete callback when completed.
    public WWW POST(string url, Dictionary<string, string> post, OnComplete onComplete, OnError onError)
    {
        WWWForm form = new WWWForm();

        foreach (KeyValuePair<string, string> post_arg in post)
        {
            form.AddField(post_arg.Key, post_arg.Value);
        }
        WWW www = new WWW(url, form);

        StartCoroutine(WaitForRequest(www, onComplete, onError));
        return www;
    }

    // Waits for request to be completed.
    private IEnumerator WaitForRequest(WWW www, OnComplete onComplete, OnError onError)
    {
        yield return www;
        // Check for errors
        if (www.error == null)
        {
            // If no errors call the callback.
            onComplete(www.text);
        }
        else
        {
            onError();
        }
    }
}
