using System;
using System.Text;
using UnityEngine;
using Steamworks;
using Newtonsoft.Json;

public class AppsflyerSteamModule
{
    private CallResult<HTTPRequestCompleted_t> m_SteamAPICallCompleted_firstOpen;
    private CallResult<HTTPRequestCompleted_t> m_SteamAPICallCompleted_session;
    private CallResult<HTTPRequestCompleted_t> m_SteamAPICallCompleted_inapp;

    public string devkey { get; }
    public string appid { get; }
    public int af_counter { get; set; }
    public string af_device_id { get; }

    public AppsflyerSteamModule(string devkey, string appid)
    {
        this.devkey = devkey;
        this.appid = appid;

        this.af_counter = PlayerPrefs.GetInt("af_counter");
        // Debug.Log("af_counter: " + af_counter);

        this.af_device_id = PlayerPrefs.GetString("af_device_id");

        //in case there's no AF-ID yet
        if (String.IsNullOrEmpty(af_device_id))
        {
            af_device_id = GenerateGuid();
            PlayerPrefs.SetString("af_device_id", af_device_id);
        }

        // Debug.Log("af_device_id: " + af_device_id);

        // set listenered for steam callbacks
        if (SteamManager.Initialized)
        {
            m_SteamAPICallCompleted_firstOpen = CallResult<HTTPRequestCompleted_t>.Create(
                OnHTTPCallBack
            );
            m_SteamAPICallCompleted_session = CallResult<HTTPRequestCompleted_t>.Create(
                OnHTTPCallBack
            );
            m_SteamAPICallCompleted_inapp = CallResult<HTTPRequestCompleted_t>.Create(
                OnHTTPCallBack
            );
        }
    }

    private RequestData CreateRequestData()
    {
        // setting the device ids and request body
        DeviceIDs deviceid = new DeviceIDs { type = "custom", value = af_device_id };
        DeviceIDs[] deviceids = { deviceid };

        RequestData req = new RequestData
        {
            timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
            device_os_version = "1.0.0",
            device_model = SystemInfo.operatingSystem,
            app_version = SteamApps.GetAppBuildId().ToString(),
            device_ids = deviceids,
            request_id = GenerateGuid(),
            limit_ad_tracking = false
        };
        return req;
    }

    // report first open event to AppsFlyer (or session if counter > 2)
    public void Start()
    {
        // generating the request data
        RequestData req = CreateRequestData();

        // set request type
        AppsflyerRequestType REQ_TYPE =
            af_counter < 2
                ? AppsflyerRequestType.FIRST_OPEN_REQUEST
                : AppsflyerRequestType.SESSION_REQUEST;

        // post the request via steam http client
        SendSteamPostReq(req, REQ_TYPE);
    }

    // report inapp event to AppsFlyer
    public void LogEvent(string event_name, string event_values)
    {
        // generating the request data
        RequestData req = CreateRequestData();
        // setting the event name and value
        req.event_name = event_name;
        req.event_values = event_values;

        // set request type
        AppsflyerRequestType REQ_TYPE = AppsflyerRequestType.INAPP_EVENT_REQUEST;

        // post the request via steam http client
        SendSteamPostReq(req, REQ_TYPE);
    }

    // send post request with Steam HTTP Client
    private void SendSteamPostReq(RequestData req, AppsflyerRequestType REQ_TYPE)
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

        // create the steam api call handles
        SteamAPICall_t api_handle;
        HTTPRequestHandle handle = SteamHTTP.CreateHTTPRequest(EHTTPMethod.k_EHTTPMethodPOST, url);
        // set context (request type)
        SteamHTTP.SetHTTPRequestContextValue(handle, Convert.ToUInt64(REQ_TYPE));
        // set the authorization
        SteamHTTP.SetHTTPRequestHeaderValue(handle, "Authorization", auth);
        // set the request body
        byte[] bytes = Encoding.ASCII.GetBytes(json);
        uint size = (uint)bytes.Length;
        SteamHTTP.SetHTTPRequestRawPostBody(handle, "application/json", bytes, size);
        // sending the request
        SteamHTTP.SendHTTPRequest(handle, out api_handle);

        // attach the request to the handler based on the request type
        switch (REQ_TYPE)
        {
            case AppsflyerRequestType.FIRST_OPEN_REQUEST:
                m_SteamAPICallCompleted_firstOpen.Set(api_handle);
                break;
            case AppsflyerRequestType.SESSION_REQUEST:
                m_SteamAPICallCompleted_session.Set(api_handle);
                break;
            case AppsflyerRequestType.INAPP_EVENT_REQUEST:
                m_SteamAPICallCompleted_inapp.Set(api_handle);
                break;
        }
    }

    // generate GUID for post request and AF id
    private string GenerateGuid()
    {
        Guid myuuid = Guid.NewGuid();
        return myuuid.ToString();
    }

    // handle HTTP callback from steam
    private void OnHTTPCallBack(HTTPRequestCompleted_t pCallback, bool bIOFailure)
    {
        // handle error
        if (!pCallback.m_bRequestSuccessful || bIOFailure)
        {
            Debug.LogError("ERROR sending req of type: " + pCallback.m_ulContextValue);
            Debug.LogError("status code: " + pCallback.m_eStatusCode);
        } //handle success
        else if (
            pCallback.m_eStatusCode == EHTTPStatusCode.k_EHTTPStatusCode202Accepted
            || pCallback.m_eStatusCode == EHTTPStatusCode.k_EHTTPStatusCode200OK
        )
        {
            Debug.Log("Success sending req of type: " + pCallback.m_ulContextValue);
            Debug.Log("status code: " + pCallback.m_eStatusCode);
            switch ((AppsflyerRequestType)pCallback.m_ulContextValue)
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
    public string event_values;
}

[Serializable]
class DeviceIDs
{
    public string type;
    public string value;
}
