using System;
using System.Text;
using UnityEngine;
using Steamworks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class AppsflyerSteamModule
{
    public string devkey { get; }
    public string appid { get; }
    public int af_counter { get; set; }
    public string af_device_id { get; }

    public MonoBehaviour mono { get; }

    public AppsflyerSteamModule(string devkey, string appid, MonoBehaviour mono)
    {
        this.devkey = devkey;
        this.appid = appid;
        this.mono = mono;

        this.af_counter = PlayerPrefs.GetInt("af_counter");
        // Debug.Log("af_counter: " + af_counter);

        this.af_device_id = PlayerPrefs.GetString("af_device_id");

        //in case there's no AF-ID yet
        if (String.IsNullOrEmpty(af_device_id))
        {
            af_device_id = GenerateGuid();
            PlayerPrefs.SetString("af_device_id", af_device_id);
        }
    }

    private RequestData CreateRequestData()
    {
        // setting the device ids and request body
        DeviceIDs deviceid = new DeviceIDs { type = "custom", value = af_device_id };
        DeviceIDs[] deviceids = { deviceid };
        string steamIDInt = SteamUser.GetSteamID().ToString();
        // DeviceIDs steamid = new DeviceIDs { type = "steamid" };
        // DeviceIDs[] deviceids = { deviceid, steamid };

        string device_os_ver = SystemInfo.operatingSystem;
        if (device_os_ver.IndexOf(" (") > -1)
            device_os_ver = device_os_ver.Replace(" (", "");
        if (device_os_ver.IndexOf("(") > -1)
            device_os_ver = device_os_ver.Replace("(", "");
        if (device_os_ver.IndexOf(")") > -1)
            device_os_ver = device_os_ver.Replace(")", "");
        if (device_os_ver.IndexOf("%20") > -1)
            device_os_ver = device_os_ver.Replace("%20", "-");
        if (device_os_ver.IndexOf(" ") > -1)
            device_os_ver = device_os_ver.Replace(" ", "-");
        device_os_ver = Regex.Replace(device_os_ver, "[^0-9.+-]", "");
        if (device_os_ver.IndexOf("-") == 0)
            device_os_ver = device_os_ver.Substring(1, device_os_ver.Length - 1);
        if (device_os_ver.Length > 23)
            device_os_ver = device_os_ver.Substring(0, 23);

        RequestData req = new RequestData
        {
            timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
            device_os_version = device_os_ver,
            device_model = SystemInfo.deviceModel,
            app_version = "1.0.0", //TODO: Insert your app version
            device_ids = deviceids,
            request_id = GenerateGuid(),
            limit_ad_tracking = false
        };
        return req;
    }

    // report first open event to AppsFlyer (or session if counter > 2)
    public void Start(bool skipFirst = false)
    {
        // generating the request data
        RequestData req = CreateRequestData();

        // set request type
        AppsflyerRequestType REQ_TYPE =
            af_counter < 2 && !skipFirst
                ? AppsflyerRequestType.FIRST_OPEN_REQUEST
                : AppsflyerRequestType.SESSION_REQUEST;

        // post the request via steam http client
        mono.StartCoroutine(SendSteamPostReq(req, REQ_TYPE));
    }

    // report inapp event to AppsFlyer
    public void LogEvent(string event_name, Dictionary<string, object> event_parameters)
    {
        // generating the request data
        RequestData req = CreateRequestData();
        // setting the event name and value
        req.event_name = event_name;
        req.event_parameters = event_parameters;

        // set request type
        AppsflyerRequestType REQ_TYPE = AppsflyerRequestType.INAPP_EVENT_REQUEST;

        // post the request via steam http client
        mono.StartCoroutine(SendSteamPostReq(req, REQ_TYPE));
    }

    public bool IsInstallOlderThanDate(string date)
    {
        bool isInstallOlder = false;

        AppId_t steamAppID = new AppId_t(uint.Parse(appid));
        string pchFolder;
        uint cchFolderBufferSize = 256;
        SteamApps.GetAppInstallDir(steamAppID, out pchFolder, cchFolderBufferSize);

        if (pchFolder == null)
        {
            Debug.LogWarning("could not find install folder");
            return isInstallOlder;
        }
        DateTime createdTime = Directory.GetCreationTime(pchFolder);
        DateTime checkDate = DateTime.Parse(date);

        if (createdTime != null)
        {
            isInstallOlder = DateTime.Compare(createdTime, checkDate) < 0;
        }

        return isInstallOlder;
    }

    // send post request with Steam HTTP Client
    private IEnumerator SendSteamPostReq(RequestData req, AppsflyerRequestType REQ_TYPE)
    {
        // serialize the json and remove empty fields
        string json = JsonConvert.SerializeObject(
            req,
            Newtonsoft.Json.Formatting.None,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
        );
        // Debug.Log(json);

        // create auth token
        string auth = HmacSha256Digest(json, devkey);

        // define the url based on the request type
        string url;
        switch (REQ_TYPE)
        {
            case AppsflyerRequestType.FIRST_OPEN_REQUEST:
                url = "https://events.appsflyer.com/v1.0/c2s/first_open/app/steam/" + appid;
                break;
            case AppsflyerRequestType.SESSION_REQUEST:
                url = "https://events.appsflyer.com/v1.0/c2s/session/app/steam/" + appid;
                break;
            case AppsflyerRequestType.INAPP_EVENT_REQUEST:
                url = "https://events.appsflyer.com/v1.0/c2s/inapp/app/steam/" + appid;
                break;
            default:
                url = null;
                break;
        }

        // set the request body
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        // set the request content type
        uwr.SetRequestHeader("Content-Type", "application/json");
        // set the authorization
        uwr.SetRequestHeader("Authorization", auth);
        uwr.SetRequestHeader(
            "user-agent",
            "Valve/Steam HTTP Client 1.0 ("
                + appid
                + ")/"
                + "("
                + SystemInfo.operatingSystem.Replace("(", "").Replace(")", "")
                + ")"
        );

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        switch (REQ_TYPE)
        {
            case AppsflyerRequestType.FIRST_OPEN_REQUEST:
                Debug.Log("Request type: FIRST_OPEN_REQUEST");
                break;
            case AppsflyerRequestType.SESSION_REQUEST:
                Debug.Log("Request type: SESSION_REQUEST");
                PlayerPrefs.SetInt("af_counter", af_counter);
                break;
            case AppsflyerRequestType.INAPP_EVENT_REQUEST:
                Debug.Log("Request type: INAPP_EVENT_REQUEST");
                break;
        }
        Debug.Log("Is success: " + uwr.result);
        Debug.Log("Response Code: " + uwr.responseCode);
        string resCode = uwr.responseCode.ToString();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
            // TODO: handle/log error
        }
        else if (resCode == "202" || resCode == "200")
        {
            switch (REQ_TYPE)
            {
                // increase the appsflyer counter on a first-open/session request
                case AppsflyerRequestType.FIRST_OPEN_REQUEST:
                case AppsflyerRequestType.SESSION_REQUEST:
                    af_counter++;
                    PlayerPrefs.SetInt("af_counter", af_counter);
                    break;
                case AppsflyerRequestType.INAPP_EVENT_REQUEST:
                    break;
            }
        }
        else
        {
            Debug.Log(
                "Please try to send the request to 'sandbox-events.appsflyer.com' instead of 'events.appsflyer.com' in order to debug."
            );
        }
    }

    // generate GUID for post request and AF id
    private string GenerateGuid()
    {
        Guid myuuid = Guid.NewGuid();
        return myuuid.ToString();
    }

    // generate hmac auth for post requests
    private string HmacSha256Digest(string message, string secret)
    {
        ASCIIEncoding encoding = new ASCIIEncoding();
        byte[] keyBytes = encoding.GetBytes(secret);
        byte[] messageBytes = encoding.GetBytes(message);
        System.Security.Cryptography.HMACSHA256 cryptographer =
            new System.Security.Cryptography.HMACSHA256(keyBytes);

        byte[] bytes = cryptographer.ComputeHash(messageBytes);

        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}

[Flags]
enum AppsflyerRequestType : ulong
{
    FIRST_OPEN_REQUEST = 100,
    SESSION_REQUEST = 101,
    INAPP_EVENT_REQUEST = 102
}

[Serializable]
class RequestData
{
    public string timestamp;
    public string device_os_version;
    public string device_model;
    public string app_version;
    public DeviceIDs[] device_ids;
    public string request_id;
    public bool limit_ad_tracking;
    public string event_name;
    public Dictionary<string, object> event_parameters;
}

[Serializable]
class DeviceIDs
{
    public string type;
    public string value;
}
