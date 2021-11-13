﻿using SimpleJSON;
using Sketchfab;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SketchfabIntegration : MonoBehaviour
{
    [SerializeField]
    private int maxFaces = 50000;

    private SketchfabLogger auth;
    private SketchfabAPI api;
    private SketchfabImporter importer;

    private List<string> requestedObjects;
    private bool active;
    
    public Action<GameObject> onFinished;
    
    public void Login(string username, string password)
    {
        auth.requestAccessToken(username, password);
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
            
            string url = SketchfabPlugin.Urls.modelEndPoint + "/" + uid + "/download";
            SketchfabRequest request = new SketchfabRequest(url, auth.getHeader());
            request.setCallback(HandleDownloadAPIResponse);
            api.registerRequest(request);
        }
        else
        {
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
            active = true;
        }
    }

    void HandleArchive(byte[] data)
    {
        importer.configure("", true);
        importer.loadFromBuffer(data);
        importer.onFinished += OnFinished;
    }
    
    void OnFinished(GameObject model)
    {
        Debug.Log("Download finished");
        active = true;
        onFinished?.Invoke(model);
    }
}
