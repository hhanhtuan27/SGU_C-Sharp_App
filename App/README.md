# VinhKhanhGuide — Smart Audio Guide for Vĩnh Khánh Food Street

WinForms C# application that automatically narrates food stalls on Vĩnh
Khánh street (District 4, HCMC) as the user walks between them. Built for
an IT final-year project. Dark "night-market" theme, VN/EN text-to-speech,
GMap.NET map, SQL Server backend, offline fallback.

---

## 1. Project layout

```
VinhKhanhGuide/
├── Program.cs
├── App.config                 ← SQL connection string
├── Database/
│   └── CreateDatabase.sql     ← run this once in SSMS
├── Models/
│   ├── PointOfInterest.cs
│   └── PoiCategory.cs         ← enum + colors + display names
├── Services/
│   ├── HaversineCalculator.cs ← the formula from the spec
│   ├── GeofenceService.cs     ← entry detection + cooldown
│   ├── NarrationService.cs    ← System.Speech wrapper
│   ├── GpsSimulator.cs        ← fake GPS for classroom demo
│   └── PoiRepository.cs       ← SQL load + offline seed
├── Controls/
│   └── PoiListBox.cs          ← owner-drawn list with category badges
└── Forms/
    └── MainForm.cs            ← everything wired together
```

## 2. Create the Visual Studio project

1. **File → New Project → Windows Forms App (.NET Framework 4.7.2)**.
   Name it `VinhKhanhGuide`.
2. Delete the auto-generated `Form1.cs`.
3. Drop all the files above into the project, keeping the folder layout.
   (Right-click project → Add → Existing Item... and tick "Add as link"
   off — we want real copies.)
4. Replace the generated `App.config` with the one in this folder.
5. In **Project → Properties → Application**, set the startup object to
   `VinhKhanhGuide.Program`.

## 3. NuGet packages

Open **Tools → NuGet Package Manager → Package Manager Console** and run:

```powershell
Install-Package GMap.NET.WindowsForms -Version 1.7.4
Install-Package GMap.NET.Core         -Version 1.7.4
```

Then **Add Reference...** and tick:

- `System.Speech` (for `System.Speech.Synthesis`)
- `System.Data` (usually already referenced — needed for `System.Data.SqlClient`)
- `System.Configuration` (for reading `App.config`)

> Note: `GMap.NET` ≥ 1.8 changed a few APIs. If you use the newest version
> and get build errors on `GMarkerGoogle` / `MarkerTooltipMode`, pin to
> `1.7.4` as above — it's the version this code targets.

## 4. Database

Open **SSMS**, connect to `(local)` (or whatever your instance name is),
open `Database/CreateDatabase.sql` and hit **Execute**. It will:

- create the `VinhKhanhGuide` database,
- create `PointsOfInterest` and `NarrationLog` tables,
- seed 20 POIs with VN + EN descriptions.

If your SQL Server instance isn't `(local)`, edit `App.config`:

```xml
<add name="VinhKhanhDb"
     connectionString="Server=YOUR_INSTANCE;Database=VinhKhanhGuide;Integrated Security=True;TrustServerCertificate=True;" />
```

**No DB? No problem.** `PoiRepository` falls back to an embedded seed list
so the app still runs for a demo without any database at all.

## 5. First run checklist

- [ ] **F5** — the main form should open with a dark sidebar on the left,
      a full OpenStreetMap on the right, and 20 red/blue markers around
      Vĩnh Khánh street.
- [ ] Click a POI in the list → the map pans to it and zooms to level 18.
- [ ] **Double-click** anywhere on the map → you "teleport" the fake GPS
      to that spot. If you double-click inside any POI's 30 m radius, the
      app narrates the description out loud.
- [ ] Click **▶ Play Narration** — the `GpsSimulator` walks along a
      pre-made route, crossing several POIs and narrating each in turn.
- [ ] Click the **🌐 VN / EN** button to switch language. The list labels,
      pill text, and TTS voice all swap together.

## 6. Demo script for your defense

1. Start the app. Show the sidebar and explain the category color system.
2. Click a POI → show map pan + zoom → press double-click → narration.
3. Press **Play Narration** → narrate that this is the `GpsSimulator`
   walking the fake user along a route; each time a POI enters the 30 m
   radius, `GeofenceService` raises `GeofenceEntered` and
   `NarrationService` speaks the description.
4. Switch to EN and rerun — same route, English voice.
5. Show the source: highlight `HaversineCalculator.DistanceMeters` — this
   is the exact formula from the requirements document.
6. Show `GeofenceService.UpdateLocation` — explain the `_currentlyInside`
   set (fires only on **entry**, not every tick) and the 5-minute
   cooldown that prevents spam at the edge of a zone.

## 7. Extra credit ideas (if you have time)

- Draw a `GMapPolygon` circle around each POI to visualize the 30 m
  radius on the map.
- Add a `TrackBar` on the sidebar that steps the GPS simulator along a
  user-defined path (the spec mentions this).
- Log every narration to `dbo.NarrationLog` and render a top-5 chart on a
  simple `Tools → Statistics` dialog for your "Analytics" module.
- Export the seed list to `VinhKhanh.sqlite` and switch the repo at
  runtime for the offline story in Module 4.

## 8. Troubleshooting

| Symptom | Fix |
| --- | --- |
| App runs but map is grey | Check internet — OpenStreetMap tiles need HTTPS. You can swap `GMapProviders.OpenStreetMap` for `GoogleMapProvider.Instance` if you have an API key. |
| No sound on narration | `System.Speech` uses installed Windows TTS voices. Open **Settings → Time & Language → Speech** and add a Vietnamese voice pack. EN works out of the box on any Win 10/11. |
| `The type or namespace GMap does not exist` | NuGet install failed — re-run step 3. |
| DB connection error | The app silently falls back to the seed list — the status bar at the bottom still shows 20 POIs. Fix the connection string in `App.config` when you want real DB. |
