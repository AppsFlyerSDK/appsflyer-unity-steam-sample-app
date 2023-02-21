# AppsFlyer Unity Steam Integration


## **Getting started with AppsFlyer Unity Steam Integration**

AppsFlyer empowers marketers to make better decisions by providing game marketers with powerful tools to perform cross-platform attribution.
Game attribution requires the game to integrate an AppsFlyer SDK, which handles first open, consecutive sessions, and in-app events. For example, purchase events. 

We recommend you use this sample app as a reference for integrating AppsFlyer SDK into your Unity-Steam game.

<hr/>

### Prerequisite
1. Unity Engine
2. [Steamworks SDK](https://steamworks.github.io/) integrated within your Unity project
3. Steam client installed with an active user (*MUST BE RUNNING FOR TESTING*)

<hr/>

## **AppsflyerSteamModule - Interface**

`AppsflyerSteamModule.cs`, included in the scenes folder, contains the required code and logic to connect to our servers and report events.


### `AppsflyerSteamModule(string appid, string devkey)`

This method receives your api key and app id, and initializes the AppsFlyer Module.

*Usage*:

```
AppsflyerSteamModule afm = new AppsflyerSteamModule("DEV_KEY", "STEAM_APP_ID");
```

*Arguments*:

* DEV_KEY - retrieve the Dev key from the marketer or the [AppsFlyer HQ](https://support.appsflyer.com/hc/en-us/articles/211719806-App-settings-#general-app-settings).
* STEAM_APP_ID - you may find your app id on the [SteamDB](https://steamdb.info/apps/).


### `public void Start()`
sends "first open/session" request to AppsFlyer.

*Usage*:

```
afm.Start();
```

### `public void LogEvent(string event_name, string event_values)`

This method receives an event name and json object and sends an in-app event to AppsFlyer.

*Usage*:

```
//set event name
string event_name = "af_purchase";
//set json string
string event_values = "{\"af_currency\":\"USD\",\"af_price\":6.66,\"af_revenue\":24.12}";
afm.LogEvent(event_name, event_values);
```

## Running the Sample App 

1. Open Unity hub and open the project.
2. Add [Steamworks SDK](https://steamworks.github.io/) via your package manager.
3. make use the sample code in SteamScript.cs with your DEV_KEY and APP_ID.
4. Launch the sample app from the Unity editor.
5. After 24 hours, the dashboard will update and show organic/non-organic install and in-app events.

