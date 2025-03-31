using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro; // Para usar TextMeshPro

public class GetResponseWeatherEmotion : MonoBehaviour
{
    public TMP_Text responseText; // Arrastra aquí tu TextMeshPro en el Inspector
    private string url = "https://api-management-pet-production.up.railway.app/";

    void Start()
    {
        StartCoroutine(PostRequest());
    }

    IEnumerator PostRequest()
    {
        string jsonBody = "{\"weather\": \"soleado\", \"mood\": \"angry\"}";
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                responseText.text =  "" + request.downloadHandler.text; 
            }
            else
            {
                responseText.text = "Error: " + request.error; 
            }
        }
    }
}
