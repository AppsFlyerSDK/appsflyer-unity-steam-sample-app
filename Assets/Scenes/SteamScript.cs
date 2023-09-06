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
            AppsflyerSteamModule afm = new AppsflyerSteamModule(DEV_KEY, STEAM_APP_ID, this, IS_SANDBOX, COLLECT_STEAM_UID);
            afm.Start();

            // set event name
            string event_name = "af_purchase";
            // set event values
            Dictionary<string, object> event_parameters = new Dictionary<string, object>();
            event_parameters.Add("af_currency", "USD");
            event_parameters.Add("af_price", 6.66);
            event_parameters.Add("af_revenue", 12.12);
            // send logEvent request
            afm.LogEvent(event_name, event_parameters);
            afm.Stop();
            afm.LogEvent(event_name, event_parameters);
        }
        else
        {
            Debug.LogError("Steam Client is not running");
        }
    }

    private void Update() { }
}
