using SimpleJSON;
using Sketchfab;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SketchfabIntegration : MonoBehaviour
{
    private string KEY_USERNAME = "KEY_USERNAME";

    [SerializeField]
    private int maxFaces = 25000;

    private SketchfabLogger auth;
    private SketchfabAPI api;
    private SketchfabImporter importer;

    private List<string> requestedObjects;
    private string modelName;
    private bool active;

    public Action onFailed;
    public Action<GameObject, string> onFinished;

    public SketchfabLogger GetAuth()
    {
        return auth;
    }

    public string GetUsername()
    {
        return PlayerPrefs.GetString(KEY_USERNAME, "");
    }

    public void Login(string username, string password)
    {
        auth.requestAccessToken(username, password);

        PlayerPrefs.SetString(KEY_USERNAME, username);
        PlayerPrefs.Save();
    }

    public void RequestObject(string name)
    {
        requestedObjects.Add(name);
    }

    void Awake()
    {
        auth = new SketchfabLogger();
        api = SketchfabPlugin.getAPI();
        importer = new SketchfabImporter();
        requestedObjects = new List<string>();

        active = true;
        importer.onFinished += OnFinished;
    }

    void Update()
    {
        api.Update();

        if (auth.isUserLogged() && active && (requestedObjects.Count > 0))
        {
            string searchQuery = SketchfabPlugin.Urls.searchEndpoint;
            searchQuery = searchQuery + "downloadable=true&sort_by=likeCount&q=" + requestedObjects[0];
            searchQuery = searchQuery + "&max_face_count=" + maxFaces;

            SketchfabRequest request = new SketchfabRequest(searchQuery, auth.getHeader());
            request.setCallback(HandleSearch);
            api.registerRequest(request);

            requestedObjects.RemoveAt(0);
            active = false;
        }
    }

    void HandleSearch(string response)
    {
        int length = int.MaxValue;
        string uid = null;
        string name = null;
        JSONNode responseJson = Utils.JSONParse(response)["results"]["models"];
        foreach (JSONNode model in responseJson.AsArray)
        {
            if (length > model["name"].ToString().Length)
            {
                length = model["name"].ToString().Length;
                name = model["name"];
                uid = model["uid"];
            }
        }

        if (uid != null)
        {
            Debug.Log("Downloading " + name);
            modelName = name;

            string url = SketchfabPlugin.Urls.modelEndPoint + "/" + uid + "/download";
            SketchfabRequest request = new SketchfabRequest(url, auth.getHeader());
            request.setCallback(HandleDownloadAPIResponse);
            api.registerRequest(request);
        }
        else
        {
            onFailed?.Invoke();
            active = true;
        }
    }

    void HandleDownloadAPIResponse(string response)
    {
        JSONNode responseJson = Utils.JSONParse(response);
        if (responseJson["gltf"] != null)
        {
            SketchfabRequest request = new SketchfabRequest(responseJson["gltf"]["url"]);
            request.setCallback(HandleArchive);
            api.registerRequest(request);
        }
        else
        {
            onFailed?.Invoke();
            active = true;
        }
    }

    void HandleArchive(byte[] data)
    {
        importer.configure("", true);
        importer.loadFromBuffer(data);
    }

    void OnFinished(GameObject model)
    {
        Debug.Log("Download finished");
        active = true;
        onFinished?.Invoke(model, modelName);
    }
}
