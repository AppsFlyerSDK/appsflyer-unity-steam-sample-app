using System;
using System.Text;
using UnityEngine;
using Steamworks;
using Newtonsoft.Json;

public class AppsflyerSteamModule
{
    private CallResult<HTTPRequestCompleted_t> m_SteamAPICallCompleted;
    private CallResult<HTTPRequestCompleted_t> m_SteamAPICallCompleted_inapp;

    public string devkey { get; }
    public string appid { get; }    
    public int af_counter { get; set; }
    public string af_device_id { get; }

    public AppsflyerSteamModule(string appid, string devkey) {
        // PlayerPrefs.DeleteAll();
        this.devkey = devkey;
        this.appid = appid;
        
        this.af_counter = PlayerPrefs.GetInt("af_counter");
        Debug.Log("af_counter: " + af_counter);

        this.af_device_id = PlayerPrefs.GetString("af_device_id");
        //in case there's no ID yet
        if (String.IsNullOrEmpty(af_device_id)) {
            af_device_id = GenerateGuid();
            PlayerPrefs.SetString("af_device_id", af_device_id);
        }

        Debug.Log("af_device_id: " + af_device_id);

        if(SteamManager.Initialized) {
            m_SteamAPICallCompleted = CallResult<HTTPRequestCompleted_t>.Create(OnHTTPCallBack);
            m_SteamAPICallCompleted_inapp = CallResult<HTTPRequestCompleted_t>.Create(OnHTTPCallBack);
        }
    }

    public void Start() {
        DeviceIDs deviceid = new DeviceIDs{type = "custom", value = af_device_id};
        DeviceIDs[] deviceids = { deviceid };

        RequestData req = new RequestData {
            timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
            device_os_version = "1.0.0",
            device_model = SystemInfo.operatingSystem.Replace(" ", "-").Replace("(", "").Replace(")", "").Substring(0, 24),
            app_version = SteamApps.GetAppBuildId().ToString(),
            device_ids = deviceids,
            request_id = GenerateGuid(),
            limit_ad_tracking = false
        };

        string json = JsonConvert.SerializeObject(req, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        Debug.Log(json);

        string auth = HmacSha256Digest(json, devkey);

        SteamAPICall_t api_handle;
        string url = "https://events.appsflyer.com/v1.0/c2s/" + (af_counter < 2 ? "first_open" : "session") + "/app/steam/" + appid;
        
        HTTPRequestHandle handle = SteamHTTP.CreateHTTPRequest(EHTTPMethod.k_EHTTPMethodPOST, url);
        AppsflyerRequestType REQ_TYPE = af_counter < 2 ? AppsflyerRequestType.FIRST_OPEN_REQUEST : AppsflyerRequestType.SESSION_REQUEST;
        SteamHTTP.SetHTTPRequestContextValue(handle, Convert.ToUInt64(REQ_TYPE));
        SteamHTTP.SetHTTPRequestHeaderValue(handle, "Authorization", auth);
        byte[] bytes = Encoding.ASCII.GetBytes(json);  
        uint size = (uint)bytes.Length;

        SteamHTTP.SetHTTPRequestRawPostBody(handle, "application/json", bytes, size);

        SteamHTTP.SendHTTPRequest(handle, out api_handle);
        m_SteamAPICallCompleted.Set(api_handle);
	}

    public void LogEvent(string event_name, string event_values) {
        DeviceIDs deviceid = new DeviceIDs{type = "custom", value = af_device_id};
        DeviceIDs[] deviceids = { deviceid };

        RequestData req = new RequestData {
            timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
            device_os_version = "1.0.0",
            device_model = SystemInfo.operatingSystem.Replace(" ", "-").Replace("(", "").Replace(")", "").Substring(0, 24),
            app_version = SteamApps.GetAppBuildId().ToString(),
            device_ids = deviceids,
            request_id = GenerateGuid(),
            limit_ad_tracking = false,
            event_name = event_name,
            event_values = event_values
        };

        string json = JsonConvert.SerializeObject(req, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        Debug.Log(json);

        string auth = HmacSha256Digest(json, devkey);

        SteamAPICall_t api_handle;
        string url = "https://events.appsflyer.com/v1.0/c2s/inapp/app/steam/" + appid;
        
        HTTPRequestHandle handle = SteamHTTP.CreateHTTPRequest(EHTTPMethod.k_EHTTPMethodPOST, url);
        AppsflyerRequestType REQ_TYPE = AppsflyerRequestType.INAPP_EVENT_REQUEST;
        SteamHTTP.SetHTTPRequestContextValue(handle, Convert.ToUInt64(REQ_TYPE));
        SteamHTTP.SetHTTPRequestHeaderValue(handle, "Authorization", auth);
        byte[] bytes = Encoding.ASCII.GetBytes(json);  
        uint size = (uint)bytes.Length;

        SteamHTTP.SetHTTPRequestRawPostBody(handle, "application/json", bytes, size);

        SteamHTTP.SendHTTPRequest(handle, out api_handle);
        m_SteamAPICallCompleted_inapp.Set(api_handle);
	}

    private string GenerateGuid() {
        Guid myuuid = Guid.NewGuid();
        return myuuid.ToString();
    }
    
    private void OnHTTPCallBack(HTTPRequestCompleted_t pCallback, bool bIOFailure) {
		if (!pCallback.m_bRequestSuccessful || bIOFailure) {
			Debug.Log("ERROR SENDING HTTP REQ");
		}
		else {
			Debug.Log("Success sending req of type: " + pCallback.m_ulContextValue);
			Debug.Log("status code: " + pCallback.m_eStatusCode);
            switch ((AppsflyerRequestType)pCallback.m_ulContextValue)
            {
                case AppsflyerRequestType.FIRST_OPEN_REQUEST: 
                case AppsflyerRequestType.SESSION_REQUEST:
                    af_counter++;
                    PlayerPrefs.SetInt("af_counter", af_counter);
                    break;
                case AppsflyerRequestType.INAPP_EVENT_REQUEST:
                    break;
            }
		}
	}

    string HmacSha256Digest(string message, string secret)
    {
        ASCIIEncoding encoding = new ASCIIEncoding();
        byte[] keyBytes = encoding.GetBytes(secret);
        byte[] messageBytes = encoding.GetBytes(message);
        System.Security.Cryptography.HMACSHA256 cryptographer = new System.Security.Cryptography.HMACSHA256(keyBytes);

        byte[] bytes = cryptographer.ComputeHash(messageBytes);

        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}

[Flags]  
enum AppsflyerRequestType: ulong
{
    FIRST_OPEN_REQUEST = 100,
    SESSION_REQUEST = 101,
    INAPP_EVENT_REQUEST = 102
}


[Serializable]
class RequestData {
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
class DeviceIDs {
    public string type;
	public string value;
}