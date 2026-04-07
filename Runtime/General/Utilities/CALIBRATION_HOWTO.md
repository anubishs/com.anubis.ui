# UI Calibration Suite - How To Use

This guide explains how to use the runtime calibration system introduced by:

- `UICalibrationManager`
- `ResizeByScale`
- `ResizeUiBySize`

---

## 1) Scene Setup

1. Create an empty GameObject in your scene.
2. Add `UICalibrationManager` to it.
3. Configure manager fields in the Inspector:
   - `toggleKey` (default `F2`)
   - `startEnabled`
   - `defaultProfile`
   - `accessMode` (`Always`, `DevelopmentBuildOnly`, `UnlockCode`)
   - `unlockCode` (if using `UnlockCode`)
   - `snapToGrid` and `snapGridSize`

---

## 2) Make UI Elements Calibratable

Attach one of these components to each UI element you want to calibrate:

- `ResizeByScale` (position + scale)
- `ResizeUiBySize` (position + size)

Each component auto-registers with `UICalibrationManager` when enabled.

### Important

- `objectName` is used as element ID (`StableId`).
- If `objectName` is empty, the component falls back to `gameObject.name` for local save/load keys.
- For best profile consistency, set unique `objectName` values.

---

## 3) Enter/Exit Calibration Mode (M1)

### Keyboard toggle

- Press `toggleKey` (default: `F2`) to toggle calibration mode.

### Programmatic toggle

```csharp
UICalibrationManager.instance.SetCalibrationEnabled(true);  // enable
UICalibrationManager.instance.SetCalibrationEnabled(false); // disable
```

When enabled:
- Elements can be repositioned/resized.
- Visual edit cues are shown by each component.
- Optional snapping is applied by manager settings.

---

## 4) Save / Load / Reset Profiles (M2)

Profiles are named layout buckets.

### Save

```csharp
UICalibrationManager.instance.SaveProfile("kiosk_1080p");
```

### Load

```csharp
UICalibrationManager.instance.LoadProfile("kiosk_1080p");
```

### Reset

```csharp
UICalibrationManager.instance.ResetProfile("kiosk_1080p");
```

If you omit the profile name, `defaultProfile` is used.

---

## 5) Export / Import JSON (M3)

### Export

```csharp
string json = UICalibrationManager.instance.ExportProfileToJson("kiosk_1080p");
```

### Import

```csharp
bool ok = UICalibrationManager.instance.ImportProfileFromJson(
    json,
    "kiosk_1080p",
    overwrite: true
);
```

`ImportProfileFromJson` loads the profile after import.

---

## 6) Access Control (M4)

Set manager `accessMode`:

- `Always`: calibration can always be toggled.
- `DevelopmentBuildOnly`: only in Editor / debug builds.
- `UnlockCode`: locked until unlocked in session.

### Unlock session

```csharp
bool unlocked = UICalibrationManager.instance.UnlockSession("ANUBIS");
```

If unlocked, toggling calibration is allowed for that session.

---

## 7) Snapping

Enable snapping in manager:

- `snapToGrid = true`
- `snapGridSize = 10` (or your preferred value)

Snapping is applied during drag reposition operations while calibration mode is active.

---

## 8) Common Workflow

1. Enter calibration mode (`F2`).
2. Move/resize UI elements.
3. Save a profile (`SaveProfile("my_layout")`).
4. Disable calibration mode.
5. On app start or scene load, call `LoadProfile("my_layout")`.

---

## 9) Troubleshooting

### Calibration does not toggle
- Check `accessMode` and unlock state.
- Ensure a `UICalibrationManager` instance exists in scene.

### Element does not respond
- Confirm it has `ResizeByScale` or `ResizeUiBySize` attached.
- Ensure calibration mode is currently enabled.

### Profile load has no visible effect
- Verify profile name matches the one used during save.
- Verify element IDs (`objectName`) stayed consistent.

