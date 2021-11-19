using SimpleJSON;
using Sketchfab;
using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * Sketchfab login, model searching and downloading integration
 */
public class SketchfabIntegration : MonoBehaviour
{
    private string KEY_USERNAME = "KEY_USERNAME";

    [SerializeField]
    private int maxFaces = 25000;
    [SerializeField]
    private float maxSizeMB = 5;

    private SketchfabLogger auth;
    private SketchfabAPI api;
    private SketchfabImporter importer;

    private List<string> requestedObjects;
    private string modelName;
    private bool active;

    public Action<string> onFailed;
    public Action<GameObject, string> onFinished;

    /**
     * Getter of authentication status
     */
    public SketchfabLogger GetAuth()
    {
        return auth;
    }

    /**
     * Get the last used username
     */
    public string GetUsername()
    {
        return PlayerPrefs.GetString(KEY_USERNAME, "");
    }

    /**
     * Login into Sketchfab and save username
     */
    public void Login(string username, string password)
    {
        auth.requestAccessToken(username, password);

        PlayerPrefs.SetString(KEY_USERNAME, username);
        PlayerPrefs.Save();
    }

    /**
     * Add a download 3D model request by a string
     */
    public void RequestObject(string name)
    {
        requestedObjects.Add(name);
    }

    /**
     * Init class objects
     */
    void Awake()
    {
        auth = new SketchfabLogger();
        api = SketchfabPlugin.getAPI();
        importer = new SketchfabImporter();
        requestedObjects = new List<string>();

        active = true;
        importer.onFinished += OnFinished;
    }

    /**
     * Process 3D model requests and authentication
     */
    void Update()
    {
        if (!api.Update())
        {
            onFailed?.Invoke("Communication failed");
        }

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

    /**
     * Callback receiving search results, selecting the best match and requsting model download
     */
    void HandleSearch(string response)
    {
        int length = int.MaxValue;
        string uid = null;
        string name = null;
        JSONNode responseJson = Utils.JSONParse(response)["results"]["models"];
        foreach (JSONNode model in responseJson.AsArray)
        {
            if ((model["archives"] == null) || (model["archives"]["gltf"] == null) || (model["archives"]["gltf"]["size"] == null))
            {
                continue;
            }
            float sizeMB = int.Parse(model["archives"]["gltf"]["size"]) / (float)(1024 * 1024);
            if (sizeMB > maxSizeMB)
            {
                continue;
            }

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
            onFailed?.Invoke("Model not found");
            active = true;
        }
    }

    /**
     * Callback receiving 3D model metadata, requesting download of 3D data
     */
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
            onFailed?.Invoke("Model not valid");
            active = true;
        }
    }

    /**
     * Callback receiving 3D model data, importing it into scene
     */
    void HandleArchive(byte[] data)
    {
        importer.loadFromBuffer(data);
    }

    /**
     * Callback forward 3D model import success to other objects
     */
    void OnFinished(GameObject model)
    {
        Debug.Log("Import finished");
        active = true;
        onFinished?.Invoke(model, modelName);
    }
}
