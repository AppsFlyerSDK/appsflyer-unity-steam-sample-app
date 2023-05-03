---
title: Unity Steam
category: 6446526dddf659006c7ea807
order: 4
hidden: false
slug: unity-steam
---

> Link to repository  
> [GitHub](https://github.com/AppsFlyerSDK/appsflyer-unity-steam-sample-app)

# AppsFlyer Unity Steam SDK integration

AppsFlyer empowers gaming marketers to make better decisions by providing powerful tools to perform cross-platform attribution.

Game attribution requires the game to integrate the AppsFlyer SDK that records first opens, consecutive sessions, and in-app events. For example, purchase events.
We recommend you use this sample app as a reference for integrating the AppsFlyer SDK into your Unity Steam game.

<hr/>

**Prerequisites**:

- Unity Engine.
- [Steamworks SDK](https://steamworks.github.io/) integrated within your Unity project.
- Steam client installed with an active user. Note: It must be running for testing.

<hr/>

## AppsflyerSteamModule - Interface

`AppsflyerSteamModule.cs`, included in the scenes folder, contains the required code and logic to connect to AppsFlyer servers and report events.

### `AppsflyerSteamModule(string appid, string devkey)`

This method receives your API key and app ID and initializes the AppsFlyer Module.

**Usage**:

```
AppsflyerSteamModule afm = new AppsflyerSteamModule("STEAM_APP_ID", "DEV_KEY");
```

**Arguments**:

- `STEAM_APP_ID`: Found in the [SteamDB](https://steamdb.info/apps/).
- `DEV_KEY`: Get from the marketer or [AppsFlyer HQ](https://support.appsflyer.com/hc/en-us/articles/211719806-App-settings-#general-app-settings).

### `public void Start()`

This method sends first open and session requests to AppsFlyer.

**Usage**:

```
afm.Start();
```

### `public void LogEvent(string event_name, string event_values)`

This method receives an event name and JSON object and sends in-app events to AppsFlyer.

**Usage**:

```
//set event name
string event_name = "af_purchase";
//set json string
string event_values = "{\"af_currency\":\"USD\",\"af_price\":6.66,\"af_revenue\":24.12}";
afm.LogEvent(event_name, event_values);
```

## Running the sample app

1. Open Unity hub and open the project.
2. Add Steamworks to your Unity project. Follow the [Steamworks SDK instructions](https://steamworks.github.io/) and add it through your package manager.
3. Use the sample code in `SteamScript.cs` and update it with your `DEV_KEY` and `APP_ID`.
4. Add the `SteamManager` and` SteamScript` to an empty game object (or use the one in the scenes folder).  
   ![Request-OK](https://files.readme.io/7a002a6-small-SteamGameObject.PNG)
5. Launch the sample app via the Unity editor and check that your debug log shows the following message:  
   ![Request-OK](https://files.readme.io/1f7dcf0-small-202OK.PNG)
6. After 24 hours, the dashboard updates and shows organic and non-organic installs and in-app events.

## Deleting Steam cloud saves (resetting the attribution)

1. [Disable Steam cloud](https://help.steampowered.com/en/faqs/view/68D2-35AB-09A9-7678#enabling).
2. [Delete the local files](https://help.steampowered.com/en/faqs/view/68D2-35AB-09A9-7678#where).
