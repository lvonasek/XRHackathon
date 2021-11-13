using SimpleJSON;
using Sketchfab;
using UnityEngine;

public class ModelConjuring : MonoBehaviour
{
    [SerializeField]
    private string username;
    [SerializeField]
    private string password;
    [SerializeField]
    private string requestedObject;
    [SerializeField]
    private bool doSearch;

    private SketchfabLogger auth;

    void Start()
    {
        auth = new SketchfabLogger();
        auth.requestAccessToken(username, password);
    }

    void Update()
    {
        SketchfabPlugin.getAPI().Update();

        if (auth.isUserLogged() && doSearch)
        {
			string searchQuery = SketchfabPlugin.Urls.searchEndpoint;
            searchQuery = searchQuery + "downloadable=true&q=" + requestedObject;
            searchQuery = searchQuery + "&max_face_count=" + 20000;

            SketchfabRequest request = new SketchfabRequest(searchQuery, auth.getHeader());
			request.setCallback(handleSearch);
            SketchfabPlugin.getAPI().registerRequest(request);

            doSearch = false;
        }
    }

	void handleSearch(string response)
	{
        int length = int.MaxValue;
        string uid = null;
        JSONNode responseJson = Utils.JSONParse(response)["results"]["models"];
        foreach (JSONNode model in responseJson.AsArray)
        {
            if (length > model["name"].ToString().Length)
            {
                length = model["name"].ToString().Length;
                uid = model["uid"];
            }
        }

        if (uid != null)
        {
            string url = SketchfabPlugin.Urls.modelEndPoint + "/" + uid + "/download";
            SketchfabRequest request = new SketchfabRequest(url, auth.getHeader());
            request.setCallback(handleDownloadAPIResponse);
            SketchfabPlugin.getAPI().registerRequest(request);
        }
    }

    void handleDownloadAPIResponse(string response)
    {
        JSONNode responseJson = Utils.JSONParse(response);
        if (responseJson["gltf"] != null)
        {
            SketchfabRequest request = new SketchfabRequest(responseJson["gltf"]["url"]);
            request.setCallback(handleArchive);
            SketchfabPlugin.getAPI().registerRequest(request);
        }
    }

    void handleArchive(byte[] data)
    {
        Debug.Log("Download finished");
        SketchfabImporter importer = new SketchfabImporter();
        importer.configure("", true);
        importer.loadFromBuffer(data);
    }
}
