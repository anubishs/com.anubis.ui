using System;
using System.Collections.Generic;
using UnityEngine;

public enum CalibrationAccessMode
{
    Always,
    DevelopmentBuildOnly,
    UnlockCode
}

[Serializable]
class UICalibrationStateCollection
{
    public List<UICalibrationState> items = new List<UICalibrationState>();
}

public class UICalibrationManager : MonoBehaviour
{
    public static UICalibrationManager instance;

    [Header("Controls")]
    public KeyCode toggleKey = KeyCode.F2;
    public bool startEnabled;

    [Header("Profiles")]
    public string defaultProfile = "default";

    [Header("Access")]
    public CalibrationAccessMode accessMode = CalibrationAccessMode.DevelopmentBuildOnly;
    public string unlockCode = "ANUBIS";

    [Header("Snap")]
    public bool snapToGrid = true;
    public float snapGridSize = 10f;

    readonly List<ICalibratableUI> elements = new List<ICalibratableUI>();

    bool calibrationEnabled;
    bool sessionUnlocked;

    public bool CalibrationEnabled => calibrationEnabled;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        SetCalibrationEnabled(startEnabled && HasAccess());
    }

    void Update()
    {
        if (!Input.GetKeyDown(toggleKey))
            return;

        if (!HasAccess())
        {
            Debug.LogWarning("Calibration mode is locked by access settings.");
            return;
        }

        SetCalibrationEnabled(!calibrationEnabled);
    }

    public bool UnlockSession(string code)
    {
        if (string.IsNullOrEmpty(unlockCode))
            return false;

        sessionUnlocked = string.Equals(code, unlockCode, StringComparison.Ordinal);
        return sessionUnlocked;
    }

    public bool HasAccess()
    {
        switch (accessMode)
        {
            case CalibrationAccessMode.Always:
                return true;
            case CalibrationAccessMode.DevelopmentBuildOnly:
                return Debug.isDebugBuild || Application.isEditor;
            case CalibrationAccessMode.UnlockCode:
                return sessionUnlocked || Application.isEditor;
            default:
                return false;
        }
    }

    public void Register(ICalibratableUI element)
    {
        if (element == null || elements.Contains(element))
            return;

        elements.Add(element);
        element.SetCalibrationMode(calibrationEnabled && HasAccess());
    }

    public void Unregister(ICalibratableUI element)
    {
        if (element == null)
            return;

        elements.Remove(element);
    }

    public void SetCalibrationEnabled(bool enabled)
    {
        calibrationEnabled = enabled && HasAccess();

        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].SetCalibrationMode(calibrationEnabled);
        }
    }

    public void SaveProfile(string profileName = null)
    {
        string profile = NormalizeProfile(profileName);

        for (int i = 0; i < elements.Count; i++)
        {
            UICalibrationState state = elements[i].CaptureState();
            SaveElementState(profile, state);
        }

        PlayerPrefs.Save();
    }

    public void LoadProfile(string profileName = null)
    {
        string profile = NormalizeProfile(profileName);

        for (int i = 0; i < elements.Count; i++)
        {
            string key = BuildKey(profile, elements[i].StableId);
            if (!PlayerPrefs.HasKey(key))
                continue;

            UICalibrationState state = JsonUtility.FromJson<UICalibrationState>(PlayerPrefs.GetString(key));
            if (state != null)
                elements[i].ApplyState(state);
        }
    }

    public void ResetProfile(string profileName = null)
    {
        string profile = NormalizeProfile(profileName);

        for (int i = 0; i < elements.Count; i++)
        {
            string key = BuildKey(profile, elements[i].StableId);
            if (PlayerPrefs.HasKey(key))
                PlayerPrefs.DeleteKey(key);

            elements[i].ResetCalibrationDefault();
        }

        PlayerPrefs.Save();
    }

    public string ExportProfileToJson(string profileName = null)
    {
        string profile = NormalizeProfile(profileName);
        UICalibrationStateCollection collection = new UICalibrationStateCollection();

        for (int i = 0; i < elements.Count; i++)
        {
            string key = BuildKey(profile, elements[i].StableId);
            if (!PlayerPrefs.HasKey(key))
                continue;

            UICalibrationState state = JsonUtility.FromJson<UICalibrationState>(PlayerPrefs.GetString(key));
            if (state != null)
                collection.items.Add(state);
        }

        return JsonUtility.ToJson(collection, true);
    }

    public bool ImportProfileFromJson(string json, string profileName = null, bool overwrite = true)
    {
        if (string.IsNullOrEmpty(json))
            return false;

        UICalibrationStateCollection collection = JsonUtility.FromJson<UICalibrationStateCollection>(json);
        if (collection == null || collection.items == null)
            return false;

        string profile = NormalizeProfile(profileName);

        for (int i = 0; i < collection.items.Count; i++)
        {
            UICalibrationState state = collection.items[i];
            string key = BuildKey(profile, state.id);

            if (!overwrite && PlayerPrefs.HasKey(key))
                continue;

            PlayerPrefs.SetString(key, JsonUtility.ToJson(state));
        }

        PlayerPrefs.Save();
        LoadProfile(profile);
        return true;
    }

    public Vector2 ApplySnap(Vector2 value)
    {
        if (!calibrationEnabled || !snapToGrid || snapGridSize <= 0f)
            return value;

        return new Vector2(
            Mathf.Round(value.x / snapGridSize) * snapGridSize,
            Mathf.Round(value.y / snapGridSize) * snapGridSize
        );
    }

    string NormalizeProfile(string profileName)
    {
        if (string.IsNullOrEmpty(profileName))
            return defaultProfile;

        return profileName.Trim();
    }

    void SaveElementState(string profile, UICalibrationState state)
    {
        if (state == null || string.IsNullOrEmpty(state.id))
            return;

        string key = BuildKey(profile, state.id);
        string json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString(key, json);
    }

    string BuildKey(string profileName, string stableId)
    {
        return $"ui_calibration.{profileName}.{stableId}";
    }
}
