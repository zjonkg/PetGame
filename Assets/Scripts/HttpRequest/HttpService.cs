using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using System;

public class HttpService : MonoBehaviour
{
    // Propiedad estática para acceder a la instancia global
    public static HttpService Instance { get; private set; }

    private void Awake()
    {
        // Verifica si ya existe una instancia de HttpService
        if (Instance == null)
        {
            // Si no existe, asigna esta instancia como la única
            Instance = this;

            // Asegúrate de que la instancia no sea destruida al cambiar de escena
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si ya existe una instancia, destruye esta para evitar duplicados
            Destroy(gameObject);
        }
    }

    public IEnumerator SendRequest<T>(string url, string method, object body, Action<T> callback, Action<string> errorCallback)
    {
        UnityWebRequest request;

        if (method == "GET")
        {
            request = UnityWebRequest.Get(url);
        }
        else if (method == "POST" || method == "PUT" || method == "DELETE")
        {
            string jsonBody = body != null ? JsonConvert.SerializeObject(body) : "";
            byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonBody);

            request = new UnityWebRequest(url, method);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.SetRequestHeader("Content-Type", "application/json");
        }
        else
        {
            errorCallback?.Invoke("Método HTTP no soportado: " + method);
            yield break;
        }

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("accept", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            errorCallback?.Invoke(request.error);
        }
        else
        {
            try
            {
                T response = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
                callback?.Invoke(response);
            }
            catch (Exception e)
            {
                errorCallback?.Invoke("Error al procesar respuesta: " + e.Message);
            }
        }
    }
}