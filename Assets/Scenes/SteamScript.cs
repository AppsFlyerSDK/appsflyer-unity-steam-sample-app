using UnityEngine;
using Steamworks;
using System.Text;
using System;
using System.Collections.Generic;

public class SteamScript : MonoBehaviour
{
    public string DEV_KEY;
    public string STEAM_APP_ID;
    public bool IS_SANDBOX;
    public bool COLLECT_STEAM_UID = true;

    void Start()
    {
        if (SteamManager.Initialized)
        {
            // init the SDK
            AppsflyerSteamModule afm = new AppsflyerSteamModule(DEV_KEY, STEAM_APP_ID, this, IS_SANDBOX, COLLECT_STEAM_UID);
            
            // set CUID
            afm.SetCustomerUserId("testTEST12345");
            // start the SDK (send firstopen/session request)
            afm.Start();

            // LogEvent example
            // set event name
            string event_name = "af_purchase";
            // set event values
            Dictionary<string, object> event_parameters = new Dictionary<string, object>();
            event_parameters.Add("af_currency", "USD");
            event_parameters.Add("af_price", 6.66);
            event_parameters.Add("af_revenue", 12.12);
            // send logEvent request
            afm.LogEvent(event_name, event_parameters);
            // send logEvent request with custom params
            Dictionary<string, object> event_custom_parameters = new Dictionary<string, object>();
            event_custom_parameters.Add("goodsName", "新人邀约购物日");
            afm.LogEvent(event_name, event_parameters, event_custom_parameters);

            // the creation date in this example is "2023-03-23T08:30:00+00:00"
            bool newerDate = afm.IsInstallOlderThanDate("2023-06-13T10:00:00+00:00");
            bool olderDate = afm.IsInstallOlderThanDate("2023-02-11T10:00:00+00:00");

            // will return true
            Debug.Log("newerDate:" + (newerDate ? "true" : "false"));
            // will return false
            Debug.Log("olderDate:" + (olderDate ? "true" : "false"));

            // stop the SDK
            afm.Stop();
        }
        else
        {
            Debug.LogError("Steam Client is not running");
        }
    }

    private void Update() { }
}
