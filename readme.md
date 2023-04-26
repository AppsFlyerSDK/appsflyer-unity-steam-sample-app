---
title: Unity Steam
category: 6446526dddf659006c7ea807
order: 4
hidden: true
slug: unity-steam
---

# AppsFlyer Unity Steam Integration

## **Getting started with AppsFlyer Unity Steam Integration**

AppsFlyer empowers marketers to make better decisions by providing game marketers with powerful tools to perform cross-platform attribution.
Game attribution requires the game to integrate an AppsFlyer SDK, which handles first open, consecutive sessions, and in-app events. For example, purchase events.

We recommend you use this sample app as a reference for integrating AppsFlyer SDK into your Unity-Steam game.

<hr/>

### Prerequisite

1. Unity Engine
2. [Steamworks SDK](https://steamworks.github.io/) integrated within your Unity project
3. Steam client installed with an active user (_MUST BE RUNNING FOR TESTING_)

<hr/>

## **AppsflyerSteamModule - Interface**

`AppsflyerSteamModule.cs`, included in the scenes folder, contains the required code and logic to connect to our servers and report events.

### `AppsflyerSteamModule(string appid, string devkey)`

This method receives your api key and app id, and initializes the AppsFlyer Module.

_Usage_:

```
AppsflyerSteamModule afm = new AppsflyerSteamModule("STEAM_APP_ID", "DEV_KEY");
```

_Arguments_:

- STEAM_APP_ID - you may find your app id on the [SteamDB](https://steamdb.info/apps/).
- DEV_KEY - retrieve the Dev key from the marketer or the [AppsFlyer HQ](https://support.appsflyer.com/hc/en-us/articles/211719806-App-settings-#general-app-settings).

### `public void Start()`

sends "first open/session" request to AppsFlyer.

_Usage_:

```
afm.Start();
```

### `public void LogEvent(string event_name, string event_values)`

This method receives an event name and json object and sends an in-app event to AppsFlyer.

_Usage_:

```
//set event name
string event_name = "af_purchase";
//set json string
string event_values = "{\"af_currency\":\"USD\",\"af_price\":6.66,\"af_revenue\":24.12}";
afm.LogEvent(event_name, event_values);
```

## Running the Sample App

1. Open Unity hub and open the project.
2. Add Steamworks to you unity project - follow the steps here [Steamworks SDK](https://steamworks.github.io/) and add it through your package manager.
3. Use the sample code in SteamScript.cs and update it with your DEV_KEY and APP_ID.
4. Add the SteamManager and SteamScript to an empty game object (or use the one in the scenes folder):
   ![Request-OK](https://files.readme.io/7a002a6-small-SteamGameObject.PNG)
5. Launch the sample app via the Unity editor and check that your debug log shows the following message:
   ![Request-OK](https://files.readme.io/1f7dcf0-small-202OK.PNG)
6. After 24 hours, the dashboard will update and show organic/non-organic install and in-app events.

## Deleting Steam Cloud Saves (resetting the attribition)

1. [Disable steam cloud](https://help.steampowered.com/en/faqs/view/68D2-35AB-09A9-7678#enabling)
2. [Delete the local files](https://help.steampowered.com/en/faqs/view/68D2-35AB-09A9-7678#where)
