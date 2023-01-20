using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.Serialization;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace GithubClient
{
    //JSON parsing methods
    struct LinkFields
    {
        public string self;
    }
    struct FileInfo
    {
        public string name;
        public string type;
        public string download_url;
        public LinkFields _links;
    }

    //Structs used to hold file data
    public struct FileData
    {
        public string name;
        public string contents;
    }
    public struct Directory
    {
        public string name;
        public List<Directory> subDirs;
        public List<FileData> files;
    }

    //Github classes
    public class Github
    {
        //Get all files from a repo
        public static void getRepo(string owner, string name, string access_token, MonoBehaviour client, Action<List<FileData>> callback)
        {
           // Directory root = 
             client.StartCoroutine( readDirectory("root", string.Format("https://api.github.com/repos/{0}/{1}/contents/", owner, name), access_token , callback));
            //return root;
        }

        //recursively get the contents of all files and subdirectories within a directory 
        private static IEnumerator readDirectory(string name,string uri, string access_token, Action<List<FileData>> callback)
        {
            //get the directory contents

            UnityWebRequest req = new UnityWebRequest(uri, "GET");
            req.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", access_token, "x-oauth-basic"))));
            req.SetRequestHeader("User-Agent", "lk-github-client");
            req.SendWebRequest();
            while (!req.isDone)
            {
               yield return null;
            }

            string jsonStr = req.downloadHandler.text;
            //parse result
            FileInfo[] dirContents = JsonConvert.DeserializeObject<FileInfo[]>(jsonStr);

            //read in data
            Directory result;
            result.name = name;
            result.subDirs = new List<Directory>();
            result.files = new List<FileData>();
            foreach (FileInfo file in dirContents)
            {
                if (file.type == "dir")
                { //read in the subdirectory
                    /*Directory sub = await readDirectory(file.name, file._links.self, access_token);
                    result.subDirs.Add(sub);
                    */
                }
                else
                {
                    /*//get the file contents;
                    req.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", access_token, "x-oauth-basic"))));
                    req.SetRequestHeader("User-Agent", "lk-github-client");
                    req.SendWebRequest();
                    while (!req.isDone)
                    {

                    }

                    while (!req.isDone)
                    {

                    }

                    string content = req.downloadHandler.text;

                    FileData data;
                    data.name = file.name;
                    data.contents = content;

                    result.files.Add(data);
                    */

                    FileData data = new FileData();
                    data.name = file.name;
                    result.files.Add(data);
                }
            }
            callback(result.files);

        }



    


    }
    
}


/*
 var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.UserAgent.Add(
    new ProductInfoHeaderValue("MyApplication", "1"));
var repo = "markheath/azure-deploy-manage-containers";
var contentsUrl = $"https://api.github.com/repos/{repo}/contents";
var contentsJson = await httpClient.GetStringAsync(contentsUrl);
var contents = (JArray)JsonConvert.DeserializeObject(contentsJson);
foreach(var file in contents)
{
    var fileType = (string)file["type"];
    if (fileType == "dir")
    {
        var directoryContentsUrl = (string)file["url"];
        // use this URL to list the contents of the folder
        Console.WriteLine($"DIR: {directoryContentsUrl}");
    }
    else if (fileType == "file")
    {
        var downloadUrl = (string)file["download_url"];
        // use this URL to download the contents of the file
        Console.WriteLine($"DOWNLOAD: {downloadUrl}");
    }
}
*/