---
title: Unity Steam
category: 6446526dddf659006c7ea807
order: 4
hidden: false
slug: unity-steam
---

> Link to repository  
> [GitHub](https://github.com/AppsFlyerSDK/appsflyer-unity-steam-sample-app)

## AppsFlyer Unity Steam SDK integration

AppsFlyer empowers gaming marketers to make better decisions by providing powerful tools to perform cross-platform attribution.

Game attribution requires the game to integrate the AppsFlyer SDK that records first opens, consecutive sessions, and in-app events. For example, purchase events.  
We recommend you use this sample app as a reference for integrating the AppsFlyer SDK into your Unity Steam game.

<hr/>

### Prerequisites

- Unity Engine.
- [Steamworks SDK](https://steamworks.github.io/) integrated within your Unity project.
- Steam client installed with an active user. Note: It must be running for testing.

<hr/>

## AppsflyerSteamModule - Interface

`AppsflyerSteamModule.cs`, included in the scenes folder, contains the required code and logic to connect to AppsFlyer servers and report events.

### AppsflyerSteamModule

This method receives your API key, Steam app ID, the parent MonoBehaviour and a sandbox mode flag (optional, false by default) and initializes the AppsFlyer Module.

**Method signature**

```c#
AppsflyerSteamModule(string devkey, string appid, MonoBehaviour mono, bool isSandbox = false)
```

**Usage**:

```c#
// for regular init
AppsflyerSteamModule afm = new AppsflyerSteamModule(DEV_KEY, STEAM_APP_ID, this);

// for init in sandbox mode (reports the events to the sandbox endpoint)
AppsflyerSteamModule afm = new AppsflyerSteamModule(DEV_KEY, STEAM_APP_ID, this, true);
```

**Arguments**

- `STEAM_APP_ID`: Found in the [SteamDB](https://steamdb.info/apps/).
- `DEV_KEY`: Get from the marketer or [AppsFlyer HQ](https://support.appsflyer.com/hc/en-us/articles/211719806-App-settings-#general-app-settings).

### Start

This method sends first open and session requests to AppsFlyer.

**Method signature**

```c#
void Start(bool skipFirst = false)
```

**Usage**:

```c#
// without the flag
afm.Start();

// with the flag
bool skipFirst = [SOME_CONDITION];
afm.Start(skipFirst);
```

### LogEvent

This method receives an event name and JSON object and sends in-app events to AppsFlyer.

**Method signature**

```c#
void LogEvent(string event_name, Dictionary<string, object> event_parameters)
```

**Usage**:

```c#
// set event name
string event_name = "af_purchase";
// set event values
Dictionary<string, object> event_parameters = new Dictionary<string, object>();
event_parameters.Add("af_currency", "USD");
event_parameters.Add("af_price", 6.66);
event_parameters.Add("af_revenue", 12.12);
// send logEvent request
afm.LogEvent(event_name, event_parameters);

```

### GetAppsFlyerUID

Get AppsFlyer's unique device ID. The SDK generates an AppsFlyer unique device ID upon app installation. When the SDK is started, this ID is recorded as the ID of the first app install.

**Method signature**

```c#
string GetAppsFlyerUID()
```

**Usage**:

```c#
AppsflyerSteamModule afm = new AppsflyerSteamModule(DEV_KEY, STEAM_APP_ID, this);
afm.Start();
string af_uid = afm.GetAppsFlyerUID();
```

### IsInstallOlderThanDate

This method receives a date string and returns true if the game folder creation date is older than the date string. The date string format is: "2023-03-13T10:00:00+00:00"

**Method signature**

```c#
bool IsInstallOlderThanDate(string datestring)
```

**Usage**:

```c#
// the creation date in this example is ""2023-03-13T10:00:00+00:00""

bool newerDate = afm.IsInstallOlderThanDate("2023-06-13T10:00:00+00:00");
bool olderDate = afm.IsInstallOlderThanDate("2023-02-11T10:00:00+00:00");

// will return true
Debug.Log("newerDate:" + (newerDate ? "true" : "false"));
// will return false
Debug.Log("olderDate:" + (olderDate ? "true" : "false"));

// example usage with skipFirst:
bool isInstallOlderThanDate = afm.IsInstallOlderThanDate("2023-02-11T10:00:00+00:00");
afm.Start(isInstallOlderThanDate);
```

## Running the sample app

1. Open Unity hub and open the project.
2. Add Steamworks to your Unity project. Follow the [Steamworks SDK instructions](https://steamworks.github.io/) and add it through your package manager.
3. Use the sample code in `SteamScript.cs` and update it with your `DEV_KEY` and `APP_ID`.
4. Add the `SteamManager` and `SteamScript` to an empty game object (or use the one in the scenes folder).  
   ![Request-OK](https://files.readme.io/7a002a6-small-SteamGameObject.PNG)
5. Launch the sample app via the Unity editor and check that your debug log shows the following message:  
   ![Request-OK](https://files.readme.io/1f7dcf0-small-202OK.PNG)
6. After 24 hours, the dashboard updates and shows organic and non-organic installs and in-app events.

## Implementing AppsFlyer in your Steam game

1. Add Steamworks to your Unity project. Follow the [Steamworks SDK instructions](https://steamworks.github.io/) and add it through your package manager.
2. Add `SteamManager.cs` to a game object.
3. Add the script from `Assets/Scenes/AppsflyerSteamModule.cs` to your app.
4. Use the sample code in `Assets/Scenes/SteamScript.cs` and update it with your `DEV_KEY` and `APP_ID`.
5. Initialize the SDK.

```c#
AppsflyerSteamModule afm = new AppsflyerSteamModule(DEV_KEY, STEAM_APP_ID, this);
```

6. [Start](#start) the AppsFlyer integration.
7. Report [in-app events](#logevent).

## Deleting Steam cloud saves (resetting the attribution)

1. [Disable Steam cloud](https://help.steampowered.com/en/faqs/view/68D2-35AB-09A9-7678#enabling).
2. [Delete the local files](https://help.steampowered.com/en/faqs/view/68D2-35AB-09A9-7678#where).
3. [Delete the PlayerPrefs data the registry/preferences folder](https://docs.unity3d.com/ScriptReference/PlayerPrefs.html), or use [PlayerPrefs.DeleteAll()](https://docs.unity3d.com/2020.1/Documentation/ScriptReference/PlayerPrefs.DeleteAll.html) when testing the attribution in the UnityEditor.
   ![AF guid & counter in the Windows Registry](https://files.readme.io/51b1681-image.png)
