using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class AnalyticsManager
{
    private static readonly string apiUrl = "https://compucore.itcarlow.ie/Mole_Patrol_Analytics/upload_data";
    private static readonly string localHost = "http://localhost:8059/upload_data";

    public static IEnumerator PostMethod(string jsonData)
    {

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, jsonData))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data successfully sent to the server");
            }
            else
                Debug.LogError($"Error sending data: {request.responseCode} - {request.error}");
        }
    }
}
