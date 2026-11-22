# Unity Field Notes: Skyline Rush (TheKillerWhales)

**Full Sail University · Project & Portfolio 2 · Team Project**
**Covers:** September 29 – November 16, 2026 (repo creation through the Beta Sprint Review)
**Engine:** Unity 6.6 (`6000.6.3f1`), Universal Render Pipeline (URP)
**Written:** November 18, 2026, from the git history (323 commits), every script in `Assets/Scripts`, the project settings, the saved project notes, and every Claude session we had on this project.

This is a personal reference: everything we built, broke, fixed and learned while making a 3D tower-defense game in Unity for the first time. It's written so you can come back months from now, in a different project, and still find the "why" behind each lesson.

> **How to use this file**
>
> - Part 2 is the Unity primer: concepts in the order you'll need them in a new project.
> - Part 3 explains each game system we built and how its code works.
> - Parts 4–5 cover git with Unity, and building/shipping.
> - Part 6 is the bug casebook: symptom → cause → fix → lesson. Search it when something feels familiar.
> - Parts 9–12 are checklists, a glossary and quick-reference tables.
> - Anything marked *(unverified)* was found by reading files, not confirmed in Play mode.

---

## Table of contents

- [Part 1 — The project at a glance](#part-1--the-project-at-a-glance)
- [Part 2 — Unity fundamentals we learned](#part-2--unity-fundamentals-we-learned)
- [Part 3 — The systems we built, and how they work](#part-3--the-systems-we-built-and-how-they-work)
- [Part 4 — Git and teamwork in a Unity project](#part-4--git-and-teamwork-in-a-unity-project)
- [Part 5 — Building, hosting and turning in](#part-5--building-hosting-and-turning-in)
- [Part 6 — Bug casebook](#part-6--bug-casebook)
- [Part 7 — Art, assets and the asset pipeline](#part-7--art-assets-and-the-asset-pipeline)
- [Part 8 — C# patterns and gotchas](#part-8--c-patterns-and-gotchas)
- [Part 9 — Open issues when the project ended](#part-9--open-issues-when-the-project-ended)
- [Part 10 — Checklist for the next Unity project](#part-10--checklist-for-the-next-unity-project)
- [Part 11 — Glossary](#part-11--glossary)
- [Part 12 — Quick reference](#part-12--quick-reference)
- [Appendix A — Week-by-week timeline](#appendix-a--week-by-week-timeline)

---

## Part 1 — The project at a glance

### 1.1 The game

**Skyline Rush** is a 3D tower-defense game for PC (Windows), with WebGL added as a second platform at the end.

- **Core loop:** enemies walk along curved paths (splines) toward your base (the "fence"). You build towers on fixed tower spots ("bases"), upgrade them, cast spells, and survive a set number of waves.
- **Aggressive play is rewarded:** calling the next wave early (Next button or Spacebar) pays coins for every second skipped. This came straight from the Game Design Document.
- **Difficulties are called temperaments:** Annoyance (easy), Anger (medium), Violence (hard), Bloodlust (super hard). Clearing a level on a temperament is worth that many stars (Annoyance = 1 … Bloodlust = 4, and the 4th star is pink).
- **Meta progression:** stars are spent in the Shop on permanent tower upgrades and on unlocking/upgrading spells (Fireball is free; Ice and Lightning are unlocked).
- **Level select:** a 3D map with a figure-8 path, key levels on side branches, and a boss gated behind 3 keys.
- **Towers (final lineup, 4 tiers each):** Turret, Mortar, Magic (Obelisk), Mine Launcher.
- **Spells:** Fireball (area damage), Ice (damage + freeze), Lightning (several strikes over time).
- **Levels shipped at Beta:** Coastal Chaos (Level 1), Level 2, Pentassault Peridition (Level 3, the 5-path map), Technolooper (Level 4), and Lightning Raceway (Key Level A, the whole game runs at 2× speed). Everything else on the map shows **COMING SOON**.
- **Hosted WebGL build:** <https://ndhardy395.itch.io/skyline-rush>

### 1.2 The team

| Person | Main areas |
| --- | --- |
| **Noah Hardy** (you) | Towers and guns (aiming, bullets, mortar, rockets, magic towers, mine towers), interfaces (`IDamageable`, `IPathProgress`, `ITower`), the Hero warplane, SFX system, level select + save system (built with Claude), WebGL port, builds, installer and turn-ins, lots of merge rescues |
| **Michael Prado** (Mikey) | Enemy pathing (waypoints, then splines), the JSON wave system, multi-path maps, health scaling, balancing, 50-wave level, `EasyLevelWirer` level package, extra levels (5 Path Map, Lightning Raceway, Technolooper) |
| **La'Rue Campbell III** (he/him) | Enemy art and animations, `EnemyHealthDisplay`, enemy selection/info UI, currency drops and pickups, win/lose, base health, the Shop, spells (ice, lightning, upgrades), tips bar, camera shake, credits polish, level 2 and 3 content |
| **Logon O'Brien** (he/him) | Menus and game flow (`ClickCode`), tower placement on bases (`BassClick`, `levelClick`, `towerClick`), pause, restart, Next button, timer, settings menu, spells-in-HUD, sell button, credits. In the Simulation & Visualization program, not Game Development |

Coordination happened in a **Discord group chat** and a private **Trello** board. A habit that paid off: whenever a fix touched someone else's file, we posted a short heads-up in Discord.

### 1.3 Tools and packages

| Tool | What we used it for |
| --- | --- |
| Unity Hub + Unity 6.6 (6000.6.3f1) | Engine. Modules: Windows Build Support (default), **Web Build Support** (added Nov 15) |
| URP 17.6 | Render pipeline (PC and Mobile render assets in `Assets/Settings`) |
| Input System 1.20 | The **new** input system only (`activeInputHandler: 1`). `Mouse.current`, `Keyboard.current` |
| uGUI 2.6 + TextMeshPro | All UI (Canvas, Button, Slider, TMP text) |
| Splines 2.9.1 | Enemy paths (`SplineContainer`) |
| AI Navigation, Post-processing, Timeline, Visual Scripting, 2D Sprite | Installed; Sprite was added for the shop's preview images |
| Visual Studio / VS Code | C# editing; VS Code also as git merge tool |
| Git + GitHub + Git Bash | Version control (`ndhardy395/Team_Project_Project_Portfolio_2`) |
| Inno Setup 6.7.3 | Windows installer for the turn-in |
| itch.io | Hosting the WebGL build |
| Audacity | Recording/editing the SFX |
| Discord, Trello | Team communication and task tracking |

### 1.4 Repository layout (end of project)

```text
TheKillerWhales/                     ← repo root (the Unity project folder)
├─ Assets/
│  ├─ Scripts/                       ← ALL C# lives here (team rule from Nov 4)
│  │  ├─ Audio/        Sfx.cs, SfxEvent.cs, VolumeSlider.cs
│  │  ├─ LevelSelect/  SaveManager, SaveData, LevelData, LevelNode, LevelSelectScreen, ...
│  │  │  └─ Editor/    LevelSelectBuilder, LevelSelectArtImporter, SaveDataMenu (editor-only)
│  │  ├─ MineTower/    MineTower.cs, Mine.cs
│  │  ├─ Platform/     HideOnWebGL.cs
│  │  ├─ Towers/       PlacedTowers.cs
│  │  └─ (root)        Gun, Bullet, WaveManager, ClickCode, EnemyMovement, SpellSpawner, ...
│  ├─ Prefabs/                       ← our own prefabs (towers, bullets, menus, effects, ClickCode)
│  ├─ Scenes/                        ← game scenes + LevelSelect art/data/prefab
│  ├─ Resources/                     ← wave JSON files, Audio/UI Click.asset (loaded by path)
│  ├─ Audio/        SFX/ (.wav) and Events/ (SfxEvent assets)
│  ├─ Assets Enemy/                  ← enemy packs and the "Game Enemys" prefabs
│  ├─ Enviroment/                    ← environment packs (Mini Village, Meadow Forest, ...)
│  ├─ TowerDefenceAssets/            ← low-poly tower/gun pack (Models, Prefabs, Materials, Textures)
│  ├─ Settings/                      ← URP assets, volume profile, Build Profiles
│  └─ TextMesh Pro/                  ← TMP fonts and materials
├─ Packages/manifest.json            ← package list (committed)
├─ ProjectSettings/                  ← all project settings (committed; changes affect everyone)
├─ Library/, Temp/, Logs/, UserSettings/, Build/   ← generated or local, all gitignored
├─ .gitignore, .gitattributes
```

### 1.5 Scenes and what they're for

| Scene | Role |
| --- | --- |
| **Main Game and UI** (was `UI.unity` until Nov 8) | The boot scene (first in the build list). Holds the persistent `ClickCode` menu object and the 3D level select |
| **Path Scene_Mikey** | Level 1, "Coastal Chaos" |
| **Path Scene 2_Mikey** | Level 2 |
| **5 Path Map** | Level 3, "Pentassault Peridition" (Path Scene 3_Mikey stopped being used on Nov 8) |
| **Level Package Test** | Level 4, "Technolooper" (built with EasyLevelWirer) |
| **Lightning Raceway** | Key Level A, 2× speed |
| **Shop** | The star shop (a separate scene) |
| Path Scene, Wave Test, SampleScene, level 1, test UI 1, Logon, Testing Server… | Test/sandbox scenes, not in the build |

**Rule we learned the hard way:** always press Play from **Main Game and UI**, not from a level. The `ClickCode` menu system (tower building, HUD, spells) only exists in the boot scene and carries over with `DontDestroyOnLoad`. Starting Play inside a level hides bugs and breaks others.

### 1.6 Milestones

| When | Milestone |
| --- | --- |
| Late Oct (~Oct 26) | Pre-production milestone: Coding Standards doc, Game Design Document, whiteboard UI sketches |
| Oct 28 – Nov 2 | **Alpha Sprint**: tower upgrades (Gun_1→Gun_2), mortar, rocket mortar, magic towers, level select. Alpha builds `Alpha1` and `AlphaMilestoneReview` |
| Nov 3 – Nov 9 | Alpha Sprint 2: new enemies and bosses, mine towers, 50-wave level, 4 tower tiers, more levels |
| Nov 7 | Decision: **first playable** = 3 levels, difficulty locked to Annoyance |
| Nov 10 – Nov 15 | Beta Sprint: spells, SFX, sell button, shop polish, WebGL |
| Nov 15 | **Beta Sprint Review** turn-in: installer + hosted WebGL link + video |

---

## Part 2 — Unity fundamentals we learned

Each section explains the concept, then shows where it bit us or helped us in Skyline Rush.

### 2.1 The editor itself

#### Windows you use constantly

| Window | What it is |
| --- | --- |
| **Hierarchy** | Every GameObject in the open scene(s), as a parent/child tree. Objects that survive scene loads show under a **DontDestroyOnLoad** heading in Play mode |
| **Scene view** | The editable 3D world. Gizmos (collider wireframes, range spheres) draw here |
| **Game view** | What the camera renders, i.e. what the player sees. Has its own **Gizmos** toggle in its toolbar |
| **Inspector** | Components and fields of the selected object or asset |
| **Project** | The `Assets` folder as Unity sees it. Do renames/moves **here**, not in File Explorer |
| **Console** | Logs, warnings and errors. **Clear** empties it; old errors stay until you clear |

#### Habits and shortcuts we picked up

- **F** in the Scene view frames the selected object. Great for finding a spawned enemy or a tower clone (`GunTower 2(Clone)`).
- Type in the **Hierarchy search box** to find an object (e.g. `CurrencyManager`).
- **Ctrl+D** duplicates; **F2** renames; **Ctrl+S** saves the scene or the open prefab.
- **Ctrl+R** refreshes the asset database. In this project Unity **didn't notice files changed outside it** (git pulls, VS Code edits, Claude's edits) until its window got focus or you pressed Ctrl+R. The loading spinner in the bottom-right corner means it's recompiling.
- **UI in the Scene view** is huge because Screen Space Overlay canvases are drawn in pixels. Press the **2D** button, select the canvas, press **F**.
- The **Inspector padlock** (top right) keeps the Inspector on one object while you drag things into it from elsewhere.
- The small **◎ object picker** next to a field opens a searchable list; it has an **Assets** and a **Scene** tab.
- The **eye icon** column on the left of the Hierarchy hides objects in the Scene view only (the game ignores it). We hid a particle system by accident once and thought it was gone.
- **Edit → Preferences → Colors → Playmode tint**: pick a loud color so you always know you're in Play mode.

#### Play mode rules

- **Anything you change on scene objects during Play is thrown away when you press Stop.** We lost Inspector changes this way more than once (the Tower 2 button wiring on Oct 28 was the classic case).
- **Changes to assets during Play are kept**: ScriptableObjects (Level Data, Sfx Events), materials, prefab files opened in Prefab Mode, and anything written to disk (save files, PlayerPrefs).
- To keep a value you found while playing: right-click the component header → **Copy Component**, press Stop, right-click → **Paste Component Values**.

#### Enter Play Mode settings (domain and scene reload)

`Edit → Project Settings → Editor → Enter Play Mode Settings`. The URP template we started from had **"Do not reload Domain or Scene"** for faster Play. That caused:

- `static` variables keeping their values from the last Play session (why `SaveManager` and `LevelSession` have a `ResetStatics()` method),
- a burst of `IndexOutOfRangeException` in `UnityEngine.UI.Selectable.OnDisable` (Unity's own UI code), because its internal list of buttons got out of sync.

On Nov 1 the setting was changed to **Reload Domain and Scene** (`m_EnterPlayModeOptions: 0`). Play starts a few seconds slower, but every run starts clean. This lives in `ProjectSettings/EditorSettings.asset`, so changing it changes Play for the whole team.

> Even after that, bursts of the same `Selectable.OnDisable` error sometimes appeared right after "Begin MonoManager ReloadAssembly" (on Play or recompile). They come from ugui's editor-only static reset and are harmless; restarting Unity clears them. Don't blame your last script edit just because it triggered a reload.

### 2.2 GameObjects, components and Transforms

- A **GameObject** is an empty container; behavior comes from **components** (Transform, MeshRenderer, Collider, your scripts).
- Every GameObject has a **Transform** (position, rotation, scale), and Transforms form the parent/child tree.
- **Local vs world:** `localPosition`/`localScale` are relative to the parent; `position`/`lossyScale` are in the world.
- **Scale is inherited.** This created a real bug: `EasyLevelWirer.prefab`'s root is scaled **0.18**, so Unity scaled everything dragged under it up **5.56×** to keep its on-screen size. `WaveSpawner` instantiates enemies **without a parent**, so a copy keeps the 5.56× local scale and comes out about 5.6× too big *(unverified in Play mode)*. Fix idea: `newEnemy.transform.localScale = prefab.transform.lossyScale;`.
- **Parenting effects to things that get destroyed:** `Destroy(gameObject)` also destroys every child. Rocket exhaust and bullet smoke vanished on impact until we detached them first (`SetParent(null)`). Explosions are spawned **unparented** for the same reason.
- `SetActive(false)` hides an object and everything under it, and stops its scripts' `Update`. An inactive parent hides an active child (`activeSelf` vs `activeInHierarchy`).

### 2.3 Prefabs

A **prefab** is a saved GameObject template (`.prefab` asset). Copies in scenes are **instances** linked to it.

1. **Editing an instance in a scene doesn't change the prefab file.** Unity stores each change as an **override** inside the *scene* file. Your ~112 hand edits to the level select map were overrides on its instance in `Main Game and UI`, not in `LevelSelect.prefab`. To push them into the prefab: select the instance → **Overrides** dropdown → **Apply All**. (The root's position/rotation always stay in the scene.)
2. **To edit the prefab itself, open Prefab Mode**: double-click the prefab in the Project window (or the arrow next to an instance). The title shows `*` when there are unsaved changes.
3. **Prefab Mode has exactly one root.** Anything you paste goes *inside* it. To make a separate prefab from a part (the mortar's cannonball), paste into an empty scene (`File → New Scene → Empty`), set it up, drag it into `Assets/Prefabs` (**Original Prefab**), then discard the scene.
4. **Instantiating a child loses the parent's scale.** A bullet that's really a child of a 0.33-scaled tower would spawn 3× too big. Bullets got their own prefab with their own scale.
5. **You can't cut or move parts of an imported model inside a prefab**, but you can copy them, and in a variant you can delete or untick them.
6. **Original Prefab vs Prefab Variant**: a variant inherits from a base prefab and stores only differences (GunMortar was a variant of the Gun_6 model).
7. **Objects that are NOT prefab instances don't update.** The spiders in the ENEMY test scene were placed straight from `Spider.fbx`, so fixing the spider *prefabs* never reached them.
8. **Prefabs can't reference scene objects.** `EasyLevelWirer` couldn't point at each level's camera, so `EnemySelection` needed a `Camera.main` fallback.
9. **Overrides beat prefab changes.** A prefab change only reaches instances that haven't overridden that field. Level 1's enemy stats lived as overrides on scene copies of the enemies, so levels that registered the prefab assets got different stats. When you fix a prefab, check the scene copies too.
10. **Model (.fbx) vs prefab:** models are read-only meshes (you only change import settings). Prefabs add components. Don't edit an asset pack's own prefabs (a re-import would overwrite them); make your own in `Assets/Prefabs`.
11. **A missing prefab asset** leaves its instances in the scene as red "Missing Prefab" placeholders. When `LevelSelect.prefab` was deleted by accident (Nov 14), the scene quietly kept ~3,000 placeholder objects. Restoring the file from git (same GUID) turned it blue again, and the scene file shrank from 2.3 MB to 156 KB on the next save.
12. **There can be two copies of "the same" UI.** The build menu existed as `Base Manu.prefab` (used by levels) *and* as a spare "Base Menu" inside `ClickCode`. Tower 4's button was added to the wrong one. Before editing, make sure you're editing the copy the game actually uses.

### 2.4 Assets, GUIDs and `.meta` files

- Every asset and folder in `Assets/` has a **`.meta` file** holding its **GUID** (permanent ID) and import settings.
- **Scenes and prefabs reference assets by GUID, not by name or path.** That's why renaming a prefab or an Sfx Event inside Unity doesn't break any links.
- **Always commit `.meta` files.** The Unity `.gitignore` only ignores a few junk `.meta` patterns (`*.pdb.meta`, etc.), never `*.meta`. Without them every clone gets new GUIDs and everything breaks (missing scripts, pink materials, empty slots). This was the very first question of the project (Oct 6).
- **Move/rename assets inside Unity's Project window** so the `.meta` follows. If you move files outside Unity, move the `.meta` too.
- **Restoring a deleted asset from git brings back the same GUID**, so every reference reconnects. Recreating it by hand gives a *new* GUID and nothing reconnects. (Deleted `Assets/Material` folder, Nov 11; deleted `LevelSelect.prefab`, Nov 15.)
- When you delete assets from the Project window, Unity warns it can't undo, but on Windows they go to the **Recycle Bin**. That's how the accidentally deleted `Assets/Prefabs` folder was recovered on Nov 1, `.meta` files and all. If you confirm a delete by accident: Recycle Bin first, then git.
- **Replacing a file's contents keeps its GUID.** To update a sound, copy the new `.wav` over the old one in File Explorer (same name). Deleting it in Unity and importing a new one creates a new GUID and empties every slot that pointed at it.
- The one place names *do* matter: anything loaded by path (`Resources.Load("Audio/UI Click")`) or found by name in code (`transform.Find("Cost 3")`).
- **Committing new assets without their `.meta`** (or committing a material without its texture) shows up for teammates as missing or pink.

### 2.5 Scenes and scene management

- Scenes must be in the **build scene list** (`File → Build Profiles → Scene List`, stored in `ProjectSettings/EditorBuildSettings.asset`) or `SceneManager.LoadScene("Name")` fails in a build. New levels added on Nov 15 (Lightning Raceway, Level Package Test) weren't in the list at first.
- The **first scene in the list** is what the built game opens with (ours: `Main Game and UI`).
- When two people add the same scene, `EditorBuildSettings.asset` can auto-merge into **duplicate entries** (Level 2 appeared twice on Nov 8).
- `DontDestroyOnLoad(gameObject)` keeps an object alive across scene loads. We used it for `ClickCode` (all menus, the HUD, the spell bar) and for the `Sfx` audio manager. Side effects we hit:
  - A persistent menu stays drawn **over** the next scene (the Shop "didn't open" because the main menu canvas covered it).
  - Per-level data on a persistent object must be cleared on `SceneManager.sceneLoaded` (tower cost dictionaries carried over between restarts).
  - A duplicate must destroy itself (the singleton pattern in `ClickCode.Start`).
- **Two people editing the same scene = merge trouble** (Part 4). Put work into prefabs and make scene edits in small, announced commits.
- **Editing a scene file on disk while Unity has it open** (a pull, a merge, or an outside edit) doesn't change what Unity has in memory. If you then save, Unity writes its old copy back. That's how the "game starts on Credits" fix got undone on Nov 15. When Unity asks, choose **Reload**; otherwise re-open the scene before saving.
- The **active state of objects is saved in the scene**. Leaving a panel switched on in the editor (Credits, the spare Base Menu, a disabled LevelSelect root) ships that state. A startup guard in code (`ClearAll(); MainManu.SetActive(true);`) protects against it.

### 2.6 The MonoBehaviour lifecycle

| Callback | When | What we learned |
| --- | --- | --- |
| `Awake` | When the object is created/loaded, **only if it's active** | `TowerUpgrad`'s panel starts hidden, so its `Awake` hadn't run; towers find it with `FindAnyObjectByType<TowerUpgrad>(FindObjectsInactive.Include)` instead of a static set in `Awake` |
| `OnEnable` / `OnDisable` | Each time the object is turned on/off (and before destroy) | `Mine` adds/removes itself from a static `All` list here; `VolumeSlider` refreshes its value in `OnEnable`, saves in `OnDisable` |
| `Start` | Before the first `Update`, after all `Awake`s | `WaveSpawner.Start` validates its references and refuses to run if any are missing |
| `Update` | Every frame | Game logic. Still runs when `Time.timeScale = 0` |
| `LateUpdate` | After all `Update`s | Range rings and level-select nodes, so they follow things that moved this frame |
| `OnDestroy` | When destroyed | Clear static `Instance` references so the next scene's object can take over |
| `OnMouseDown` | Click on this object's **collider** | Placed towers weren't clickable until they got a (trigger) `BoxCollider` |
| `OnDrawGizmosSelected` | Scene view, object selected | Range spheres, mine trigger/blast radii, path landing spots |

Also:

- `IEnumerator Start()` makes `Start` a coroutine (`CurrencyPickUp` waits, then adds coins).
- `[RuntimeInitializeOnLoadMethod]` runs a static method automatically: `BeforeSceneLoad` (create `Sfx`, apply saved volume) or `SubsystemRegistration` (reset statics when domain reload is off).
- **Execution order between different objects isn't guaranteed.** `Level Package Test` had two `WaveSpawner`s; whichever `Awake`d first won and the other destroyed itself, so the scene could work "by luck".
- `FindFirstObjectByType` is deprecated in Unity 6.6: use `FindAnyObjectByType` (or `FindObjectsByType`).

### 2.7 The Inspector and serialization

- `public` fields and `[SerializeField] private` fields show in the Inspector and are saved in the scene/prefab. The label is generated from the variable name (`currentCurrency` → "Current Currency"; our `exhaust` variable → an "Exhaust" slot, which looked like a built-in Unity feature but isn't).
- **The value saved in the scene beats the default in code.** `public int currentCurrency = 0;` showed 100 in level 1 because 100 was saved in the scene. Changing the code default does nothing for existing objects. Each scene has its own value.
- Attributes we used: `[Header]`, `[Tooltip]`, `[Range(0,1)]`, `[Min(1)]`, `[TextArea]`, `[ContextMenu("Test/Deal 50 damage")]` (right-click the component to run it), `[RequireComponent(typeof(Slider))]`, `[CreateAssetMenu]`, `[System.Serializable]` (so a struct/class shows inside a list), `[System.NonSerialized]` (keep a field out of saving).
- **A field typed as a component only accepts objects that have it.** `public Gun upgradePrefab;` refused `GunMortar2` (it was the shell, with `Bullet`, not `Gun`); `public Bullet bulletPrefab;` refused `MagicShot` until it had a `Bullet` on its root. Unity shows a 🚫 cursor.
- **Renaming a serialized field loses its saved values** (Unity matches by name). The same goes for JSON save files: fixing the `turrentUpgrades` typo would reset everyone's saved upgrades. For Inspector fields, `[FormerlySerializedAs("oldName")]` carries values over.
- **Two different "layer" settings:** the GameObject's own **Layer** (top of the Inspector) vs a script's `LayerMask` field (e.g. Gun's **Enemy Layer**). Putting the mortar *itself* on the Enemy layer made bullets hit the tower; leaving the LayerMask on **Nothing** made towers never fire.
- **Hooking buttons twice:** if code adds a listener by name (`onClick.AddListener(UpgradeButton)`), don't also add it in the Inspector's **On Click ()** list, or it fires twice.
- **Selecting several prefabs in the Project window** lets you set a field on all of them at once (we did this for death sounds). Editing a prefab file directly has no Apply button; just Ctrl+S.
- **Values typed into the wrong field** happen. `TurretSound` once had Spatial Blend 0.06 (meant for Min Interval). When a value "doesn't work", re-read the whole component.

### 2.8 "Unity null" and destroyed objects

- Unity overrides `==` so a **destroyed** object compares equal to `null`, even though the C# reference still exists. `if (target == null)` safely drops a dead enemy.
- **That only works on Unity types.** Through an interface (`ITower selected`), `selected == null` is a plain C# check and says "not null" for a destroyed tower. Fix used in `TowerUpgrad`:

  ```csharp
  static bool Exists(ITower tower) => tower is UnityEngine.Object obj && obj != null;
  ```

- `list.RemoveAll(m => m == null)` cleans destroyed objects out of a list (`MineTower`).

### 2.9 Time, pausing and coroutines

- `Time.deltaTime` = seconds since last frame. Move things by `speed * Time.deltaTime` so speed doesn't depend on frame rate.
- `Time.timeScale` = global game speed. **0 = paused**: `deltaTime` is 0, and physics, Animators and `WaitForSeconds` stop. **But `Update` still runs and UI still works** (clicks and hover color fades use real time).
- Our menus run at `timeScale = 0`. The level waits for the HUD **Start** button, which sets it to 1. Pause sets 0. That's why the level select animates with `Time.unscaledDeltaTime`/`unscaledTime`, and why the camera shake uses unscaled time so it finishes even during the game-over pause.
- `SPEED.cs` (Lightning Raceway) sets `Time.timeScale = 2` and scales `Time.fixedDeltaTime` by the same factor so physics doesn't jitter, then restores both in `OnDestroy`.
- **Coroutines** (`IEnumerator` + `StartCoroutine`): `yield return null` (next frame), `new WaitForSeconds(t)` (scaled time), `WaitUntil(() => cond)`, `yield break` (stop). Store the `Coroutine` handle to `StopCoroutine` it later (wave banner, camera shake). `StopAllCoroutines()` stops every coroutine on that script (used when the game ends).
- `try { … } finally { … }` inside a coroutine: the `finally` block runs even if the coroutine stops early. `WaveSpawner.SpawnGroupRoutine` uses this so its counters stay correct when a group fails.
- `InvokeRepeating(nameof(FindTarget), 0f, scanInterval)` runs a method on a timer (towers re-scan for targets 4× a second instead of every frame).

### 2.10 Physics: colliders, layers and queries

- **Colliders** define the shape physics and raycasts see. **Is Trigger** colliders don't block anything but still get hit by queries and `OnMouseDown`. Towers use trigger box colliders just so they can be clicked.
- **Rigidbody** makes an object physical. `MagicTower1` had a Rigidbody with gravity, not kinematic, and only a trigger collider, so it **fell through the ground** when placed. Fix: remove it or tick **Is Kinematic**.
- **Layers** + **LayerMask** filter queries. We created an **Enemy** layer (Oct 18) and towers only look on it.
- Queries we used:
  - `Physics.Raycast(ray, out hit, distance, mask)` and `RaycastNonAlloc` (bullets, clicking things).
  - `Physics.OverlapSphere` / `OverlapSphereNonAlloc` (tower targeting, splash damage, mine trigger and blast, spells).
  - `QueryTriggerInteraction.Ignore` to skip trigger colliders (spells hitting the ground).
- **Colliders return colliders, not enemies.** One enemy can have several colliders, so: `hit.GetComponentInParent<IDamageable>()` to get the health script, and a `HashSet` so each enemy is damaged once per blast.
- **The collider must match the model.**
  - Spiders had ~6 m sphere colliders offset from the body, so bullets stopped in mid-air. Replaced with a capsule along the body (radius 0.5, height 2.2, direction Z).
  - The minotaur's capsule floated ~4 units above its feet, so **mines never went off** under it (mines trigger within 1 unit of the path). Six ground enemies were fixed on Nov 15 so the collider reaches the feet.
  - Aim at `collider.bounds.center`, not the pivot (enemy pivots are at their feet). After tightening colliders, shots aimed at the pivot flew under the body.
  - A tiny splash radius (0.01) at the target's feet stopped finding the spider once its capsule was raised off the ground: the magic towers did zero damage. Fix: a landing shot always damages its target directly too.
- **Dead enemies keep their collider** during the death animation. Bullets stopped on corpses until `Bullet.FirstLiveHit` started skipping enemies whose `IsDead` is true.
- **Seeing colliders:** in Play mode, turn on **Gizmos** in the Game view toolbar and select the enemy. Its collider shows as a green wireframe.

### 2.11 The Input System (new)

- `Mouse.current.leftButton.wasPressedThisFrame`, `Mouse.current.position.ReadValue()`, `Keyboard.current.spaceKey.wasPressedThisFrame`.
- Clicking things in the world: `Camera.main.ScreenPointToRay(mousePos)` + `Physics.Raycast`.
- **Ignore clicks that land on UI:** `EventSystem.current.IsPointerOverGameObject()` before handling a world click.
- With the new Input System, the UI's **EventSystem** must use `InputSystemUIInputModule`.
- Code that has to compile either way: `#if ENABLE_INPUT_SYSTEM … #else … #endif`.

### 2.12 UI: uGUI, Canvas and TextMeshPro

#### Canvas and scaling

- **Render Mode:** Screen Space – Overlay (drawn on top in pixels; what we used), Screen Space – Camera, World Space.
- **Canvas Scaler:** use **Scale With Screen Size**, Reference **1920×1080**, Match **0.5**. The Shop used *Constant Pixel Size* and was laid out on a ~2,000 px wide Game view, so on a smaller Game view it looked "extremely zoomed in".
- Size UI with the **Rect Tool / Width & Height**, not the Scale tool. A scroll view scaled 9 × 0.145 looked stretched.
- **Sort order** decides which overlay canvas draws on top; ties are unpredictable.

#### Clicks

- Every UI element with **Raycast Target** on blocks clicks behind it, even if it's invisible. A blank wave-notification text in the middle of the screen made one tower unclickable. Turn Raycast Target off on display-only text and images.
- A scene needs an **EventSystem** or no button responds (levels 2/3 had none at first).
- `RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, cam)` tells whether a click was inside a panel; we used it to close menus when clicking outside them.

#### Buttons

- Hook up in the Inspector (**On Click ()** list: drag the object, pick the function) **or** in code with `button.onClick.AddListener(...)`. Not both.
- **Selectable states:** Normal, Highlighted, Pressed, **Selected**, Disabled. Selected outranks Highlighted, so a button that stays selected never shows its hover color. When a scene load destroyed the EventSystem that selected the Shop button, it stayed stuck. Fix: `EventSystem.current.SetSelectedGameObject(null)` before loading.
- `button.interactable = false` greys it out (Upgrade on the last tier, locked spells).
- A button's clickable area is its **Image/Graphic rect**, not the visible picture: the Fireball button was clickable outside its icon.

#### Finding UI by name

- `transform.Find("tower 3/Cost 3")` walks a path of child names and is **case-sensitive**. A child named `cost 3` threw a `NullReferenceException`. Name-based lookups need exact names, so document them.
- Logon's build menu reads the price **out of the label text** with a regex (`Regex.Replace(text, @"\D", "")`). Change the label, change the price.
- Hierarchy **name** vs **TMP text content** are different things: renaming the object doesn't change the words on the button.

#### Positioning UI over 3D

- `camera.WorldToScreenPoint(worldPos)` + `RectTransformUtility.ScreenPointToLocalPointInRectangle(...)` keeps level-select nodes glued to their spot on the 3D map at any window size.
- `transform.SetAsLastSibling()` draws a panel on top of its siblings (popups, result panels).
- `CanvasGroup.alpha` fades a whole group (locked temperaments).

#### TextMeshPro

- TMP text needs a **font asset**; its look comes from a **material**. The default `LiberationSans SDF` material is **shared**: a teammate restyled it (red face, outline), so all new TMP text came out red, and later an outline color change affected every outlined text. Make your own material copy for your own UI (the level select uses `LS_Text_*`).
- **Dynamic font assets** (`Color shop`, `ColorsForScreen`, `Menu Color`) write new glyphs into themselves when you press Play, and builds clear them. They show as modified constantly and conflict on almost every pull. Treat them as a cache: discard local changes, take either side in a conflict, never hand-merge.
- A broken font asset (atlas declared 1×1 but holding a 512×512 image) produced errors like *"expected 1, got 262144"*.
- Rich text tags work in TMP strings: `<b>`, `<color=#7F92A8>`.
- **File encoding:** a `·` typed in `Shop.cs` got saved in the wrong encoding and became `�`, which shows as a box in game. Save scripts as UTF-8.

### 2.13 Rendering and materials (URP)

- **Magenta/pink = no working material/shader.** Causes we hit: a Particle System with **no material** (Add Component doesn't assign one; `GameObject → Effects → Particle System` does), legacy/Built-in shaders under URP, an empty **Trail Material**, the Low Poly Mini Village *prefabs* (their materials don't work in URP; use the FBX models), and a deleted material.
- **Materials are shared.** The material panel at the bottom of the Inspector edits the **shared asset** for everything that uses it (35 prefabs used `GradientColor_1_Mat`). Duplicate the material (Ctrl+D, ~4 KB) for one-off looks; the texture is shared, not copied.
- **Palette textures + UVs** (the low-poly pack): one texture is a grid of color strips, and each model's UVs land on a small patch of it. That's where a model's color comes from; no Unity setting "chooses" it. View UVs: select the mesh inside the FBX → preview dropdown → **UV Layout**. To change UVs at the source you'd use Blender.
- **Base Map Tiling/Offset** shifts the UVs. Offset X of **0.5414** (9 strips × 64 px ÷ 1064 px) moved the bullet from blue to orange; on `GradientColor_1` one strip is **1/16 = 0.0625**, and Offset Y moves lighter/darker within a strip.
- **Base Color is a multiply.** Blue × orange = brown, so a tint can darken a color but can't change its hue. For a solid color, clear the texture first.
- **Metallic 1 / Smoothness 0** makes colors look darker and duller.
- **Emission** (on the material): tick Emission, set an HDR color with intensity > 1. With **Bloom** in a Volume (threshold 1), it glows. Don't confuse it with the Particle System's **Emission** module, which controls spawn rate.
- **Bloom/post-processing** comes from a **Volume** (Global Volume → Add Override → Bloom). The project default had Bloom at intensity 0; level 1 had threshold 1, intensity 0.25. Bloom affects the whole level, so agree on it as a team.
- `LineRenderer` draws lines/rings in the game (Gizmos only show in the Scene view). Range rings use `Shader.Find("Sprites/Default")` for a simple unlit, tintable material. Lift lines slightly off the ground so they don't flicker (z-fighting).
- **Quality levels per platform:** Web defaulted to **Mobile** (render scale 0.8, 1 shadow cascade) and looked blurry; set the Web column's default to **PC** in `Project Settings → Quality`.
- "Use External Materials (Legacy)" import setting on old FBXs gives the warning *MaterialLocation.External is obsolete*. Harmless. `minotaur1.fbx` used it, so each machine generated its own `Materials` folder with its own GUIDs, which caused clashing commits (and committed conflict markers).

### 2.14 Particle systems

| Module / setting | What it does / what we learned |
| --- | --- |
| **Main → Simulation Space** | **World** leaves a trail behind a moving emitter; **Local** makes particles ride along (no trail) |
| **Main → Scaling Mode** | **Local** so a tiny parent scale doesn't shrink the effect to nothing |
| **Main → Start Lifetime × speed** | Sets trail length. Bullets at 80 u/s with 0.25 s lifetime drew a 20-unit line back to the gun |
| **Main → Start Color** | Tints the particle; with additive materials **alpha acts as brightness** |
| **Main → Stop Action: Destroy** | The effect deletes itself when finished (explosions don't pile up) |
| **Main → Looping / Duration / Prewarm** | One-shot vs continuous; Prewarm makes it look already running on spawn |
| **Emission → Rate over Time / Rate over Distance / Bursts** | Rate over Distance gives an even trail at any speed; Bursts for explosions (Count 50 at Time 0) |
| **Shape** | Cone (exhaust), Sphere (explosion), Hemisphere (ground blasts; rotate X −90 so the dome points up) |
| **Limit Velocity over Lifetime** | Dampen makes a burst shoot out then slow down like an explosion |
| **Color / Size over Lifetime** | Fade alpha to 0; grow or shrink. A size curve starting at 0 made puffs invisible early on |
| **Noise** | Wobble for smoke |
| **Renderer → Material / Trail Material** | URP `ParticlesUnlit`, or our additive `MagicGlow`. Empty = magenta |
| **Emitter Velocity Mode** | Transform (not Rigidbody) for objects moved by script |

Workflow notes:

- A particle system only previews in the Scene view **while it's selected**, with a Play/Restart overlay. If the overlay is gone, the backtick key (`` ` ``) over the Scene view brings up the overlay menu.
- Our `Bullet.cs` has an optional **Exhaust** slot: on impact it detaches the effect (`SetParent(null)`), sets `stopAction = Destroy`, and stops emitting, so the smoke fades instead of vanishing. It also back-fills the last frame's trail with `ParticleSystem.Emit(EmitParams)`, because the particle system never gets its final update.
- **Additive** blending (URP Particles/Unlit, Transparent, Additive, `Default-Particle` texture) makes overlapping particles add brightness: good for magic glows. Black is invisible; bright backgrounds wash it out.
- One white additive material + a different **Start Color** per effect = many colors without new materials.
- If a yellow **"Fix Now"** box appears under Vertex Streams, click it.
- Avoid real-time Point Lights on many small objects (15 mines × several towers). Bloom plus an emissive material looks close enough and costs much less.
- Two identical particle systems stacked in one spot is an easy mistake to make and hard to see; check the Hierarchy.

### 2.15 Audio

- Sound needs an **AudioListener** (usually on the main camera) that is **enabled**. The one in `Main Game and UI` was unchecked, so the main menu and level select were silent, which made the button-click sound look broken. Only one listener should be active at a time.
- `AudioSource` plays clips. We used a **pool of 24** sources so many sounds can overlap (Part 3.11).
- **Spatial Blend 0** (2D) for our high overhead camera; fully 3D sounds came out too quiet.
- `AudioListener.volume` = master volume for everything (our Settings slider).
- **Import settings** for short SFX: Force To Mono on, Load Type *Decompress On Load*, Compression *Vorbis* (~70), and **Preload Audio Data on**. With Preload off, the first play of each sound was late.
- `Project Settings → Audio → DSP Buffer Size`: *Best Performance* adds ~40–80 ms of latency on Windows; *Good latency* is a better default for games. (Project-wide setting: tell the team.)
- Trim silence at the start of a sound in Audacity (zoom in, select back to the start with Shift+J, Delete, export the same file name). An impact sound should start with the impact, not a whoosh.
- **Browsers block audio until the first click** (autoplay policy). Silence before that in WebGL is normal.

### 2.16 Animation

- Enemies use an **Animator** with parameters: `SetBool("IsMoving", …)` for walk/idle and `SetTrigger("Die")` for the death animation. Check `animator.runtimeAnimatorController != null` before setting parameters.
- **Freezing an animation:** `animator.speed = 0`, remembering the old speed to restore it (Ice spell). Restore it in `OnDisable` too, or an enemy killed while frozen can't play its death animation (La'Rue's fix on Nov 10).
- `StateMachineBehaviour` scripts can sit on Animator states (`SpiderAnimator` exists but is still the empty template).
- **Procedural motion** for floaty enemies and props: `Mathf.Sin(Time.time * speed) * amount` for bobbing, swaying, banking (`SharkBoss`, `AirCraft`, `EyeBallMovement`, `PortalAnimation`, `Propelrturn`). Animate a **child** model while the parent follows the path.
- **Facing:** models aren't all authored facing +Z. `EnemyMovement.facingOffset` rotates the model on top of the path direction. The small Imp "moonwalked" with 180°; 360° (or 0°) fixed it. Spiders and the eyeball need 180°.
- **Model axes vs script axes:** `Bullet.cs` steers along the blue **Z** arrow, but the rocket model's nose pointed along green **Y**, so it flew sideways. Fix: put the mesh on a child named `Model` rotated 90° on X so Z points out the nose.

### 2.17 Splines (enemy paths)

- A `SplineContainer` holds a curve you edit in the Scene view (Michael replaced waypoints with splines on Oct 15: "faster and easier pathmaking, and CURVES").
- `CalculateLength()`, `EvaluatePosition(t)` and `EvaluateTangent(t)` with **t from 0 (start) to 1 (end)**.
- Constant speed along a spline: `progress += (speed / length) * Time.deltaTime`.
- Distance left (used to target the enemy closest to the base): `(1 - progress) * length`.
- `MineTower` samples the path every 0.5 units with `EvaluatePosition` to find landing spots inside its range.

### 2.18 ScriptableObjects

- A **ScriptableObject** is a data asset you create in the Project window and fill in through the Inspector, like a prefab without a GameObject.
- `[CreateAssetMenu(menuName = "Skyline Rush/Level Data")]` adds it to **Right-click → Create**.
- We used them for **Level Data** (name, scene, unlock requirements, preview image, Coming Soon) and **Sfx Events** (clips + volume/pitch settings).
- Separation that worked well: the Level Data asset *describes* a level and never changes during play; the save file records what the *player has done*.
- Changes to ScriptableObject assets in Play mode **persist** (they're assets, not scene objects).
- Gotcha: a ScriptableObject's non-serialized runtime field (like `SfxEvent.lastPlayed`) can keep its value between Play sessions in the editor; code defensively.

### 2.19 Saving data and JSON

- **`JsonUtility`**: `JsonUtility.FromJson<T>(text)` / `ToJson(obj, prettyPrint)`. Works on `[Serializable]` classes with public fields. Fields are matched **by name**; unknown JSON fields are **silently ignored** (the wave JSON has `delayAfterGroup`, which no class reads, so it does nothing); missing fields keep their defaults.
- **JSON doesn't allow comments.** La'Rue's commit on Nov 14 says it all: *"Ok good to know don't add a comment in a Json."* A comment makes the whole file fail to parse.
- **String IDs must match exactly**, spaces and capitals included. `EnragedSpider` in the JSON vs `Enrage Spider` in the registry stopped level 1 at wave 24.
- **`Application.persistentDataPath`**: on Windows `C:\Users\<you>\AppData\LocalLow\<Company>\<Product>\`, so ours was `…\LocalLow\TheKillerWhales\SkylineRush\save.json`. The folder comes from Player Settings' company/product names.
- The save file isn't part of the project or the build, but **a build run on your PC reads the same file** as the editor (test wins showed up in the build).
- **PlayerPrefs** for small settings (volume, "don't show tips", which tip is next) and for the whole save on WebGL, where files in `persistentDataPath` aren't reliably kept. Call `PlayerPrefs.Save()` at sensible times (e.g. when a panel closes), not every frame.
- Good save habits: a `version` field for future format changes; compute totals (stars, keys) from records instead of storing them; only keep improvements (a weaker win never lowers stars). Open issue: a crash mid-save can corrupt `save.json`; writing to a temp file and then replacing would make it safe.

### 2.20 Editor scripting

- Any folder named **`Editor`** is editor-only and never goes into a build. Editor scripts outside such a folder (or `using UnityEditor` in game code) **break the build**. When the level select scripts moved to `Assets/Scripts/LevelSelect`, the editor ones went to `LevelSelect/Editor`.
- `[MenuItem("Tools/Skyline Rush/Build Level Select")]` adds a menu command. A second method with `[MenuItem("same path", true)]` is the **validate** function that greys it out when it doesn't apply.
- `Selection.activeObject` is whatever's selected (right-click test tools on Level Data assets).
- `AssetDatabase.FindAssets("t:LevelData")` returns **GUIDs** → `GUIDToAssetPath` → `LoadAssetAtPath<T>`. Also `CreateAsset`, `CreateFolder`, `IsValidFolder`, `ImportAsset`, `SaveAssets`.
- `PrefabUtility.InstantiatePrefab` (keeps the prefab link) and `SaveAsPrefabAssetAndConnect` (create/overwrite a prefab from a scene object).
- `EditorSceneManager.OpenScene`, `MarkSceneDirty`, `SaveScene`, `SaveCurrentModifiedScenesIfUserWantsTo`.
- `EditorUtility.DisplayDialog` (confirm), `RevealInFinder` (open folder).
- An **`AssetPostprocessor`** with `OnPreprocessTexture` sets import settings automatically for files in a folder (the level select art importer makes every PNG a Sprite and gives 9-slice borders to `rounded.png`/`chamfer.png`).
- **Generators vs hand edits:** the level select is rebuilt from code by `LevelSelectBuilder`. That makes it repeatable and reviewable, but **re-running it throws away hand edits** (yours lived as scene overrides). It also hard-codes paths, so renaming `UI.unity` broke it until the path was updated. Pick one source of truth per thing.

### 2.21 Debugging

- `Debug.Log` / `LogWarning` / `LogError`. **Pass the object as the second argument** (`Debug.LogError(msg, this)`), and clicking the message in the Console highlights that object in the Hierarchy.
- **Write error messages that say exactly what's missing and where.** `WaveSpawner` used to say "Assign BaseHealth, GameResultManager, and a JSON file containing waves." Now it says *"WaveSpawner in scene 'Path Scene 2_Mikey' can't start the waves. Missing: Game Result Manager…"*.
- **Don't leave per-frame logs in.** A debug line in `Update` printed every frame while paused; `Debug.LogError` used for tracing made normal clicks show up as red errors.
- **The real editor log** for this project is `Logs/Editor.log` in the project folder (`Editor-prev.log` = previous session). The global `%LOCALAPPDATA%\Unity\Editor\Editor.log` was only a stub pointing there.
- **Harmless noise we learned to ignore:** Unity services sign-in errors (401, "Token Exchange failed", `curl error 6`) after the editor sat open overnight (restart or sign in again); ugui `Selectable.OnDisable` bursts on reload; the `MaterialLocation.External` warning.
- **Code that works in the editor but breaks builds:** `using UnityEditor…` in a runtime script (VS autocomplete added `using UnityEditor.Rendering.Universal;` to `Gun.cs`). Wrap editor-only calls in `#if UNITY_EDITOR` (ClickCode's Quit does this to stop Play mode).
- Platform checks: `#if UNITY_WEBGL && !UNITY_EDITOR` (runs only in the browser build, so in the editor the Quit button still shows; that's expected).
- **Test from the boot scene.** The double spell bar only appeared when starting from the main menu; playing level 1 directly hid it.
- `[ContextMenu]` test methods on components (right-click → "Test/Deal 50 damage").
- Editor-only cheat tools (fake a win, print progress, delete save) save a lot of play time.
- "No output" from a shell command that redirects to a file (`git log > file.txt`) means it worked.
- Compile-check without the editor: Unity's own compiler response files live in `Library/Bee/artifacts/…/Assembly-CSharp.rsp`; re-running them with Unity's bundled `dotnet` + `csc.dll` reproduces the exact compile errors (handy when the editor is closed or busy).

### 2.22 Project settings worth knowing

| Setting | Where | Note |
| --- | --- | --- |
| Company / Product name | Player | Decide the save folder path (`LocalLow/<Company>/<Product>`) and the exe name. Ours: TheKillerWhales / SkylineRush |
| Default resolution / fullscreen | Player | 1920×1080, fullscreen window; Web canvas 1280×720 |
| Enter Play Mode Settings | Editor | Reload Domain and Scene (see 2.1) |
| Tags & Layers | Tags and Layers | Added layer **Enemy** and tags `Enemy`, `tower bass` |
| Active Input Handling | Player | Input System Package (New) |
| Quality per platform | Quality | Web default Mobile → PC |
| DSP Buffer Size | Audio | Good latency |
| Build Profiles / Scene List | File → Build Profiles | Shared scene list; Web profiles carry their own Player Settings copy (Part 5) |

All of `ProjectSettings/` is committed, so **every change there affects the whole team**. Announce it.

### 2.23 Performance habits we used

- **NonAlloc queries + reused buffers** (`OverlapSphereNonAlloc(pos, r, hits, mask)` into a `static readonly Collider[] hits`) so towers, bullets and mines don't create garbage every scan. A shared static buffer is safe because Unity runs scripts one at a time on the main thread; just `Clear()` shared sets before each use.
- `sqrMagnitude` instead of `magnitude`/`Distance` when only comparing distances (no square root).
- Scan for targets on an interval (`InvokeRepeating`, 0.25 s), not every frame.
- Pool audio sources; throttle repeated sounds (`minInterval`).
- Cache component lookups in `Awake`/`Start` instead of `GetComponent` every frame.
- No per-frame logging; no real-time lights on dozens of small objects.
- Unity already **strips unreferenced assets from builds**, so deleting unused packs shrinks the project folder and import cache, not the build.

---

## Part 3 — The systems we built, and how they work

### 3.1 Enemy pathing: waypoints → splines → multiple paths

**Stage 1, waypoints (Oct 13).** Michael's first pathing (`pathFollowTest.cs`, `PathFollow.cs`) gave each enemy an array of `Transform` waypoints and moved it with `Vector3.MoveTowards` toward the current one, turning with `Quaternion.LookRotation(direction) * Quaternion.Euler(0, facingOffset, 0)` and stepping to the next waypoint when within 0.1 units. The Animator's `IsMoving` bool switched walk/idle.

**Your test rig while building the gun.** `TestEnemySpawner` spawned capsules with `GameObject.CreatePrimitive(PrimitiveType.Capsule)` (it comes with a `CapsuleCollider`), put them on the Enemy layer, and added `EnemyNoah`, which just walks forward and despawns. It also drew its lane with Gizmos (cyan start, magenta end). Building a throwaway test enemy let the gun be developed before real enemies existed.

**Stage 2, splines (Oct 15).** `EnemyMovement` follows a `SplineContainer`:

```csharp
progress += (speed / splineLength) * Time.deltaTime;          // 0..1 along the path
Vector3 worldPosition = splineContainer.EvaluatePosition(progress);
transform.position = worldPosition + Vector3.up * heightOffset; // flying enemies use heightOffset
Vector3 forward = splineContainer.EvaluateTangent(progress);
Quaternion targetRotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, facingOffset, 0f);
transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
```

- `DistanceRemaining => (1f - progress) * splineLength` implements `IPathProgress`, which towers use to shoot the enemy closest to the base.
- At the end of the path, `ReachEndPath()` damages the base (`baseHealth.TakeDamage(baseDamage)`), reports itself removed to the wave tracker, and destroys itself. **Damage first, then report**, so the win check sees the updated base health.
- `freezeFor(duration)` (Ice spell) pauses movement and sets `animator.speed = 0`, restoring it when the freeze ends or the enemy is disabled.

**Stage 3, many paths (Oct 17 → Nov 1).** Forks and multiple simultaneous paths, then the 5 Path Map. Each spawn group in the wave JSON can name a `pathID`; `0` means the level's default spline, any other number is looked up in the WaveSpawner's **Path Registry** list. Gotcha: a JSON written for one path (`SingleLineWaveData.json` uses pathID 0 everywhere) only ever uses one path on a multi-path map. Each map needs its own JSON if groups should use specific paths (Michael's note: "that's the path, visuals, AND the spawns that need to be loaded").

### 3.2 The wave system (`WaveManager.cs`, class `WaveSpawner`)

> The file is `WaveManager.cs` but the class inside is `WaveSpawner`; other scripts call `WaveSpawner.Instance`. Unity's rule is that a MonoBehaviour's class name should match its file name. Keep them identical in new projects; the mismatch confused everyone.

#### JSON format (`Assets/Resources/*.json`, classes in `WaveData.cs`)

```json
{
  "waves": [
    {
      "waveNumber": 4,
      "delayBeforeWave": 10,
      "spawns": [
        { "enemyId": "Spider", "count": 5, "spawnInterval": 0.7 },
        { "enemyId": "Imp",    "count": 2, "spawnInterval": 1.0, "delayBeforeGroup": 3, "pathID": 2 }
      ]
    }
  ]
}
```

```csharp
[System.Serializable] public class GameData  { public WaveData[] waves; }
[System.Serializable] public class WaveData  { public int waveNumber; public float delayBeforeWave; public SpawnGroup[] spawns; }
[System.Serializable] public class SpawnGroup{ public string enemyId; public int count; public float spawnInterval;
                                                public float delayBeforeGroup; public int pathID; }
```

Files: `SingleLineWaveData.json` (level 1, 50 waves), `MultiPathWaveData.json` (level 2), `5PATHS.json` (5 Path Map). Michael's Oct 23 commit: *"After fighting the compiler for 3 hours straight, I FINALLY got a wave system working! Just uses a JSON file to load the waves. Much easier to edit that way!"*

#### How a level's waves run (final version)

1. **Awake:** one spawner per scene (a second destroys itself). Copies the Inspector **Enemy Registry** (`enemyId` → prefab) into a `Dictionary<string, GameObject>`, parses the JSON with `JsonUtility`.
2. **Start:** validates Base Health, Game Result Manager and the JSON, and logs exactly which is missing. Nothing runs until they're all set.
3. **Update** (only while set up, base alive, not paused): counts down the next wave's `delayBeforeWave`, then calls `LaunchNextWave`. The countdown starts **once the previous wave has spawned its last enemy** (Nov 13), not when it's cleared, so waves can overlap.
4. **LaunchNextWave:** reserves the wave's whole enemy count up front (so the HUD shows the total), shows "Wave N incoming!", pays the **early-call reward** if the player skipped the countdown (`floor(secondsLeft) × 3` coins), and starts one coroutine per spawn group.
5. **SpawnGroupRoutine:** looks up the prefab and path (fails loudly if missing), waits `delayBeforeGroup`, then spawns `count` enemies `spawnInterval` apart. Each enemy gets `Initialize(waveNumber, healthMultiplier, speedMultiplier)`, a `WaveEnemyTracker` with a callback, and `EnemyMovement.Initialize(spline)`. A `finally` block fixes the counters if the group stops early.
6. **EnemyRemoved** (via the tracker, on death or reaching the base) updates counters and checks wave completion.
7. **TryWinGame**, after the last wave launched: wins only when no group is still spawning and no enemy is left.

The **Next** button (`ClickCode.OnHUDNextClick`) and **Spacebar** (`WaveSkipper`) call `ForceNextWave()`, which has a 0.75 s safety cooldown so one press can't launch two waves.

#### The tracker callback pattern

```csharp
public class WaveEnemyTracker : MonoBehaviour
{
    Action onRemoved; bool reported;
    public void Initialize(Action callback) { onRemoved = callback; reported = false; }
    public void ReportRemoved()
    {
        if (reported) return;      // dying AND reaching the base must not count twice
        reported = true;
        onRemoved?.Invoke();
        onRemoved = null;
    }
}
// in the spawner:  tracker.Initialize(() => EnemyRemoved(wave));
```

The enemy doesn't need to know about the spawner; it just calls "I'm gone". The lambda remembers which wave it belongs to.

#### Health scaling and difficulty

- **Nov 7:** exponential health scaling. **Nov 9:** replaced with linear, per enemy, because exponential "made the early game easy and the midgame impossible":

  ```text
  maxHealth = (baseMaxHealth + max(0, wave - firstWaveUnlocked) * hpPerWave) * healthMultiplier
  ```

  Example from Michael's commit: an enemy that first appears on wave 5 with 200 HP and +5 HP/wave has 220 HP on wave 10.
- Bosses can use `overrideHealth`.
- `healthMultiplier`/`speedMultiplier` exist on the spawner (tooltips suggest 0.9/1.0/1.1/1.3 and 0.95/1.0/1.05/1.1 for the four difficulties), but **temperament was never wired to them**, so every temperament plays the same.

#### How the wave design changed (and the bugs each change caused)

| Date | Change | Lesson |
| --- | --- | --- |
| Oct 23–24 | JSON waves; then several groups spawning at once | Data-driven waves are easy to edit without code |
| Oct 31 | Multi-path `pathID` + path registry | One JSON per map |
| Nov 4 | Next skips anytime; **counter bug fixed** | Each wave reset `activeEnemyCount = waveTotal`, so leftovers from a skipped wave drove it negative and the one-time `== 0` win check never passed. Never reset a shared counter; add to it, and check the win continuously |
| Nov 4 | Cooldown on forced waves + reward for time left | |
| Nov 8 | 50 waves, "plenty of enemy variety" | 17 entries used `EnragedSpider`/`EnragedBigImp` while the registry said `Enrage Spider`/`Enrage Big Imp` → `spawnFailed` stopped everything at wave 24. IDs must match exactly |
| Nov 9 | Rewrite: `Update`-driven, overlapping waves, per-wave progress objects | |
| Nov 11 | Countdown starts at wave launch (more pressure) | Waves arrived while the previous one was still spawning |
| Nov 13 | Countdown starts after the last spawn | Compromise: pressure, but no pile-ups. Wave 15 was labeled `5` in the JSON (wrong HUD and HP); some delays were 100–108 s, making early-call worth ~300 coins |
| Nov 14 | Harder starting HP, spawn safety catch | "The game is basically unbeatable, we should try using that algorithm I pitched instead of trying to eyeball it" (your Nov 8 commit). Balance by math, not by feel |

### 3.3 Enemies

**Health and death (`Enemy Health Display.cs`, class `EnemyHealthDisplay`)** is the main enemy script (La'Rue). It implements `IDamageable`:

- `Initialize(...)` computes max HP (formula above) and pushes speed to `EnemyMovement`.
- `TakeDamage` ignores hits when already dead, shows the health bar on first hit, updates the slider and `"HP / Max"` text.
- `Die()`: sets `isDead`, plays the death sound, disables extra scripts (`scriptsToStopOnDeath`), reports to the wave tracker, hides the health bar, spawns the death effect, fires the `Die` animator trigger, disables movement, drops coins, and destroys itself after `despawnDelay` (2.5 s) so the death animation can play.
- `[ContextMenu("Test/Deal 50 damage")]` for testing in Play mode.

`Enemy.cs` is an older, simpler health script (destroys instantly). `EnemyNoah.cs` is the test capsule enemy.

**Interfaces you added (Oct 18)** so towers work with every enemy type:

```csharp
public interface IDamageable  { void TakeDamage(float amount); bool IsDead { get; } }
public interface IPathProgress { float DistanceRemaining { get; } }
```

Towers, bullets, mines and splash all talk to `IDamageable`, never to a specific enemy class. That's why adding new enemy scripts never required touching tower code.

**Coins.** `EnemyDropCurrency.DropItem()` spawns a coin prefab and sets its value; `CurrencyPickUp` uses `IEnumerator Start()` to wait `pickupDelay` (silver 1 s, gold 1.5 s since Nov 13) and then adds coins automatically. Coin values per enemy were raised on Nov 15 (minotaur 5 → 50, Big Enrage Imp 5 → 35, Normal Spider 15 → 17).

**Clicking enemies.** `EnemySelection` raycasts from the camera on left click (ignoring clicks on UI) and passes the hit's `EnemyHealthDisplay` (via `GetComponentInParent`) to `EnemyInfoUI`, which shows name, description, HP text and slider, refreshed in `LateUpdate`. Level 2 had two half-wired `EnemySelection` components (one with no camera, one with no UI), so nothing worked; the fix falls back to `Camera.main` and the component on the same object.

**Spawn portal.** `PortalAnimation` grows/shrinks with `MoveTowards` toward a target size, pulses with `Sin`, and spins. `EnemyPortalControll` counts active spawners and closes the portal (coroutine + `WaitUntil`) when the last one finishes.

**Roster (from the levels' registries and prefabs):** Spider, Enrage Spider, Imp (small Imp), Enrage Imp, Big Imp, Enrage Big Imp, Little Devil (`devil@fly`), Scared / Pumpkin / Witch / Bull Ghosts, Minotaur (boss), Small Demon, Medium Demon, Demon King, Yeti, Yeti King, baby Yeti, Angry Log, floating eyeball, LOG boss. Several bosses got code-driven motion (`SharkBoss`, `AirCraft`, `EyeBallMovement`).

#### Enemy gotchas collected

- Collider must reach the feet (mines) and fit the body (bullets). See 2.10.
- A copy-pasted second health script on a child (`polySurface1` on Little Devil, 75 HP, default name "Big Imp") absorbed hits and reported a second death to the wave tracker.
- Level 1's tuned stats were **scene overrides** on enemy copies the registry pointed at; other levels using the prefab assets got prefab stats (Small Demon 300 HP vs the fireball's 75 damage → "fireball does nothing on level 2").
- Facing offset per model (moonwalking imp).

### 3.4 Towers

#### `Gun.cs`: the shooting tower

**Targeting** (every 0.25 s via `InvokeRepeating`):

```csharp
int count = Physics.OverlapSphereNonAlloc(transform.position, range, hits, enemyLayer);
// keep the live enemy with the smallest DistanceRemaining = furthest along the path ("first" targeting)
```

Each frame it drops the target if it died (`IsDead`) or walked out of range (`sqrMagnitude > range * range`).

**Aiming** (only if the tower has pivots): the stand turns left/right (yaw), the barrels tilt up/down (pitch).

```csharp
Vector3 flat = target.position - standPivot.position;
flat.y = 0f;                                               // ignore height so the stand never tips
standPivot.rotation = Quaternion.RotateTowards(standPivot.rotation,
                         Quaternion.LookRotation(flat), turnSpeed * Time.deltaTime);

Vector3 toTarget = target.position - barrelPivot.position;
float horizontal = new Vector2(toTarget.x, toTarget.z).magnitude;
float elevation  = Mathf.Atan2(toTarget.y, horizontal) * Mathf.Rad2Deg;   // + = up
elevation = Mathf.Clamp(elevation, minElevation, maxElevation);           // stop barrels clipping the body
// Unity is left-handed: a positive X rotation tips +Z DOWN, so negate to aim up.
barrelPivot.localRotation = Quaternion.RotateTowards(barrelPivot.localRotation,
                         Quaternion.Euler(-elevation, 0f, 0f), turnSpeed * Time.deltaTime);
```

For this to work, the model has to be re-parented under empty pivot objects (`standPivot` → `barrelPivot` → barrels → `FirePoint`s at each muzzle with the blue Z arrow pointing out). Towers with no turning parts (mortar, magic, rocket pad) leave the pivots empty and skip aiming; homing shots do the work.

**Firing:** `fireCooldown -= deltaTime`; at 0, `Shoot()` and reset to `1 / fireRate` (fireRate = shots per second). Barrels alternate round-robin: `nextBarrel = (nextBarrel + 1) % firePoints.Length`. The bullet is created with `Instantiate(bulletPrefab, ...)`; because `bulletPrefab` is typed `Bullet`, Instantiate returns the `Bullet` component directly, and `bullet.Init(damage, splashRadius, enemyLayer, target)` hands it everything.

**Loaded round:** an optional model sitting on the tower (the rocket on GunMortar2's pad). On firing it's hidden and the shot launches from exactly where it sat, so it looks like that rocket took off; it reappears after `min(reloadTime, 1/fireRate)`.

#### `Bullet.cs`: projectiles

**Straight shots** have no Rigidbody or collider. Each frame they raycast the distance they're about to move, so a fast bullet can't skip through a thin enemy between frames:

```csharp
float step = speed * Time.deltaTime;
if (homing && target alive) transform.forward = (targetBody.bounds.center - transform.position).normalized;
if (FirstLiveHit(step, out RaycastHit hit)) { Impact(hit.point, hit.collider.GetComponentInParent<IDamageable>()); sinkLeft = hit.distance + penetration; }
```

- Aims at the **middle of the target's collider**, not its pivot.
- `FirstLiveHit` uses `RaycastNonAlloc` and skips dead enemies (corpses no longer eat shots).
- **Penetration** (0.5): after the hit, the bullet keeps flying a little into the model before it's removed, so the smoke trail ends inside the enemy instead of at the collider's edge. Damage still lands on contact; no second hit is possible.
- A straight shot that misses is cleaned up after `lifetime`.

**Arcing shots** (`arcHeight > 0`, mortar/rockets/magic):

```csharp
float t = Mathf.Clamp01(timeFlown / flightTime);                      // 0 at launch, 1 on landing
Vector3 next = Vector3.Lerp(launchPoint, landingPoint, t) + Vector3.up * (4f * arcHeight * t * (1f - t));
transform.forward = (next - transform.position).normalized;           // nose follows the curve
```

`t·(1−t)` is 0 at both ends and 0.25 in the middle, so `4·arcHeight·t·(1−t)` peaks at exactly `arcHeight`. The landing point follows the target while it's alive, then stays where it was last seen. At launch the shot is pointed along the arc's starting direction (`forward = (landing − launch) + up · 4h`) so it doesn't flash sideways for a frame.

**Impact:** with `splashRadius > 0`, every live enemy in an `OverlapSphereNonAlloc` takes full damage once (a static `HashSet<IDamageable>` dedupes enemies with several colliders), and **the targeted enemy always takes the hit too**, even if the small sphere missed it. Then the explosion prefab is spawned unparented with `Quaternion.identity`, the impact SFX plays, and the exhaust is detached and allowed to fade.

#### Tower lineup and tiers

| Line | Tier progression (prefab names) | History |
| --- | --- | --- |
| Turret | `Turret` → `TurretEnhanced` → `TurretUpgrade` → `TurretMax` | Started as GunTower 1 (Gun_1 model, Logon's version "GunTower 1 logon") → GunTower 2 (Gun_2, 4 barrels, orange bullets) on Oct 28 |
| Mortar | `Mortar` → `MortarEnhanced` → `MortarUpgrade` → `MortarMax` | GunMortar (Gun_6, arcing cannonball, splash) Oct 28 → GunMortar2 (Gun_5, rocket from a pad, exhaust + explosion) Oct 31 |
| Magic | `ObeliskEnhanced` → `ObeliskUpgrade` → `ObeliskMax` | MagicTower1/2 (Nov 1): glowing additive MagicShot, slight arc |
| Mine | `MineLauncher` → `MineLauncherEnhanced` → `MineLauncherUpgraded` → `MineLauncherMax` | Your own design (Nov 8): MineTower1 (Tower_15) → MineTower2 (Tower_11) |

Michael wired four tiers for every line on Nov 8. Build prices (Nov 15, set on the menu labels): **75 / 195 / 215 / 290**.

**How upgrades work:** each tier is its own prefab with its own model and stats. The tier below has `upgradePrefab` + `upgradeCost`; `TowerUpgrad.UpgradeButton` charges the cost, instantiates the next prefab at the same position/rotation, destroys the old tower, and re-selects the new one so the player sees its new range. A new tier = a new prefab + two fields on the tier below. The last tier leaves `upgradePrefab` empty (the button shows "Max").

**Checklist for a working tower prefab** (built up over the Alpha sprint):

- [ ] `Gun` (or `MineTower`) on the **root**, with **Enemy Layer = Enemy** (not Nothing)
- [ ] The tower's own GameObject layer = **Default** (not Enemy)
- [ ] Fire Points (empty children at the muzzles, blue Z out) and a Bullet Prefab whose root has `Bullet`
- [ ] `towerClick` on the root + a **trigger** `BoxCollider` covering the model
- [ ] No free-falling Rigidbody (remove it or make it kinematic)
- [ ] Optional Stand/Barrel Pivots if it should turn
- [ ] `upgradePrefab`/`upgradeCost` on every tier but the last; `towerCost` matching the price paid (for selling)
- [ ] Shop ID if it should get the permanent shop bonus
- [ ] Its own prefab in `Assets/Prefabs`, not a scene-only object or the asset pack's prefab
- [ ] Sensible stats (fire rate is shots/second; defaults like 20 shots/s and range 50 are not sensible)

#### `ITower` and the shared range ring

When the mine tower arrived, the Tower Manu was hard-wired to `Gun`. You introduced an interface so the menu treats any tower the same:

```csharp
public interface ITower
{
    Transform transform { get; }        // every MonoBehaviour already has this
    GameObject UpgradePrefab { get; }   // next tier, or null on the last tier
    int UpgradeCost { get; }
    void ShowRange(bool show);
}
```

`Gun` and `MineTower` implement it with **explicit interface implementation** (`GameObject ITower.UpgradePrefab => ...`), which only shows up when the object is treated as an `ITower`. The ring-drawing code moved into a static helper, `RangeRing.Build/Draw`, used by both: a looped `LineRenderer` with 64 points, redrawn in `LateUpdate` so a range upgrade resizes it instantly, lifted 0.1 above the ground.

#### The mine tower (`MineTower.cs` + `Mine.cs`)

A tower that doesn't shoot: it throws mines onto the path inside its range; a mine waits, then explodes when an enemy steps near it.

- **Finding spots:** `FindSpots()` walks every `SplineContainer` in the level in 0.5-unit steps with `EvaluatePosition` and keeps points within range (flat XZ distance, the same circle the ring draws). Recomputed only when `range` changes.
- **Picking a spot fairly:** `PickSpot()` counts the free spots, picks a random number below that count, then walks to it, so every free spot has an equal chance without building a second list.
- **Spacing:** `IsFree` checks every mine of every tower (`Mine.All`) against `mineSpacing`.
- **Limits:** at most `maxMines` out at once; the drop timer only counts while there's room, and only resets when a mine actually went out.
- **Loaded mines:** like the rocket pad, ammo models on the tower hide on throw and reappear after `reloadTime`.
- **Upgrade takeover:** the next tier spawns exactly where the old one stood; `TakeOverMines()` adopts every mine whose `DroppedFrom` is (almost) this position, so old mines count toward the new limit and explode with the new stats.
- **A mine's life** (`Mine.cs`): `Launch()` copies the tower's stats → `Fly()` along the same `Lerp + 4h·t(1−t)` arc → lands → waits `armDelay` → each frame checks `OverlapSphereNonAlloc(triggerRadius)` for a live enemy → `Explode()` damages everything in `blastRadius` once (and always the enemy that triggered it) → spawns the explosion and destroys itself.
- **The static registry:** `static readonly List<Mine> all` with `public static IReadOnlyList<Mine> All => all;`. Mines add themselves in `OnEnable` and remove themselves in `OnDisable` (which also runs on `Destroy`), so the list only ever holds live mines.
- **Balance gotchas you found:** `maxMines` 15 vs spacing 2 → geometry limits it to ~5–8 on 16 units of path; spacing 0 → mines stack and a stack does double/triple damage; an upgrade with a *lower* `maxMines` is effectively weaker.

### 3.5 Building, upgrading and selling (the menu wiring)

```text
Click a base (BassClick.OnMouseDown)
  → closes Tower Manu, colors the base cube blue, ClickCode.SetTowerSpace(base), opens Base Menu
Click "Tower N" (levelClick.TowerNButton → BuyTower("tower N/Cost N", N+1))
  → reads the price from the "Cost N" label (regex: digits only)
  → if affordable: ClickCode.SetTowerPrefab(N+1); ClickCode.OnTowerHUDTowerClick()
       → Instantiate(TowerPrefabs[N], base position); remember cost + base; hide the base
     CurrencyManager.SubCurrency(price); close the menu
Click a placed tower (towerClick.OnMouseDown)
  → ClickCode.SetTowerSpace(tower)        (so Sell acts on the tower you clicked)
  → TowerUpgrad.Find().Select(GetComponent<ITower>())   (ring + Upgrade panel)
Upgrade → swap prefab, add upgrade cost to the spot's total
Sell    → refund 70% of everything spent on that spot, show the original base again, destroy the tower
Click anywhere else in the world → menus close
```

Details worth remembering:

- `ClickCode.TowerPrefabs` is picked **by index**: element 0 is the base prefab (`Bass`), 1 = Turret, 2 = Mortar, 3 = ObeliskEnhanced, 4 = MineLauncher. Adding upgrade tiers to this list shifted every button to the wrong tower (Nov 1). Upgrades come from `upgradePrefab`, never from this list.
- The price lives in the **label text**, so the label *is* the price. Button colors turn dark green/red by comparing that price to your coins.
- **Don't close a menu on the same click that opened it:** both `BassClick` and the pause menu remember `Time.frameCount` when they opened and ignore clicks on that frame.
- **Closing on click-outside:** check whether the mouse is inside any of the menu's buttons with `RectTransformUtility.RectangleContainsScreenPoint`; if not, close.
- **Selling (final version, Logon's + patches):** `ClickCode` keeps dictionaries keyed by **tower position**: total spent per spot and the hidden base under it. `UpTower` adds each tier's hand-typed `towerCost`. The label shows `Sell: <70%>`. Dictionaries clear on `sceneLoaded`. Your component-based version (`PlacedTower` with `baseSpot` + `spent`, carried over on upgrade) is commented out.
- **Lesson:** the sell refund comes from `towerCost` typed on each prefab, a *copy* of the menu price. Copies drift (towers 3 and 4 had them swapped). When a price or `upgradeCost` changes, update `towerCost` too, or better, keep one source of truth.

### 3.6 Economy, base health and game results

- **`CurrencyManager`**: a per-level singleton (`instance`, not DontDestroyOnLoad). `AddCurrency`/`SubCurrency` update the "Coins: N" text. Starting coins are set **in each scene's Inspector** (level 1: 100), not in code.
- **`BaseHealth`**: lives (default 20). `TakeDamage` updates "Lives: N", shakes the camera, and at 0 calls `GameResultManager.LoseGame()`. `IsAlive` gates the wave spawner.
- **`BaseHitShake`** (La'Rue, Nov 14): a coroutine moves the camera's `localPosition` by a random offset that fades out over 0.2 s, using **unscaled** time so it finishes even when the game pauses on defeat, and always restores the start position (also in `OnDisable`). Repeated hits restart the shake from the rest position instead of drifting.
- **`GameResultManager`**: `WinGame`/`LoseGame` → `EndGame(panel)`: hide gameplay UI (`ClickCode.HideGamePlayUI`), hide listed HUD objects (but never the panel's own parents), stop the spawner's coroutines, activate the panel's canvas and panel, `SetAsLastSibling`, log an error if a parent is still inactive, and set `timeScale = 0`. Then `LevelSession.RecordWin/RecordLoss(wave)` writes the save. Buttons: Restart (reload the active scene by build index, reset spell cooldowns, restore the Start button), Leave to Main Menu, Continue to Level Select.
- **`LevelTimer`**: `mm:ss` from `Mathf.FloorToInt(t / 60)` and `t % 60` with `string.Format("{0:00}:{1:00}", ...)`.

### 3.7 Spells (`SpellSpawner.cs` + spell scripts)

- **Selecting and aiming:** clicking a spell button picks it (if unlocked and off cooldown) and shows a **preview ring** (a `LineRenderer` circle sized to that spell's `hitRadius`). Each frame the ring follows a mouse raycast onto the **ground layers**; left click casts there, right click cancels. Clicks over UI and a paused game are ignored.
- **Fireball** (`FireballMovement` in `FireballDrop.cs`): spawned `spawnHeight` above the target, moves down with `MoveTowards`, then `OverlapSphere` damages every enemy in `hitRadius` once (HashSet) and spawns the impact effect.
- **Ice** (`IceBallDrop`): same fall; damages first, then **freezes the survivors** (`EnemyMovement.freezeFor`), so low-HP enemies die normally.
- **Lightning** (`LightningSpell`): no fall; a coroutine strikes 3 times, 0.5 s apart, **re-querying each strike** because enemies move. The bolt visual (`LightningBoltEffect`) is a `LineRenderer` whose middle points get random X/Z offsets, redrawn every 0.04 s for a flicker, destroyed after 0.3 s.
- **Cooldowns and labels:** per-spell timers; the label shows `"<name> - locked"`, a countdown, or "Left-click to cast / Right click to cancel".
- **Unlocks and upgrades** come from the save file: Fireball is free; Ice costs 2 stars, Lightning 4; each spell has 2 upgrades costing `(n+1)×2` stars (+25 fireball damage, +15 ice damage and +1 s freeze, +10 lightning damage per strike per upgrade).
- **Where the spell bar lives:** Logon moved it into the HUD as a child of `ClickCode` in the boot scene (Nov 11), so every level gets it automatically. A later commit re-added a separate SpellManager + SpellCanvas to level 1, which gave two stacked spell bars with separate cooldowns (every spell castable twice). Found in the first WebGL test; fixed by deleting the level's copy. **One shared system beats per-level copies** (level 2's own copy had been half-wired with the Ice prefab in the Fireball slot).

### 3.8 Progress, stars, temperaments and the Shop

- **`SaveManager`** is a **static class** (no scene object needed; works even if you press Play inside a level). It lazy-loads `save.json` on first use (`EnsureLoaded`) and keeps a `Dictionary<string, LevelProgress>` for quick lookups.
- **`SaveData`** holds `version`, a list of `LevelProgress { levelId, bestStars, bestWave, hasKey }`, `spentStars`, `unlockedSpells`, and upgrade counts for each tower line and spell.
- **Stars:** `(int)temperament` (the enum values *are* the star counts). `RecordWin` only keeps improvements and returns how many **new** stars were earned. Available stars = total earned − spent.
- **Unlocking:** a level opens when every level in its `requiredLevels` is beaten and you have `keysRequired` keys, **unless** its Level Data has **Coming Soon** ticked (added for the first playable). Beaten key levels give a key.
- **`LevelSession`** (static) carries the chosen level and temperament from the level select into the level, so the result screen knows what to save. It's null when a level was started directly in the editor (nothing gets saved then).
- **Permanent tower upgrades:** 2 per line, costing 2 then 4 stars, +20% damage each, looked up by each prefab's `shopTowerId`. `MineTower.Start` applies it; **`Gun.Start` currently reads the upgrade count but no longer multiplies the damage**, and most prefabs' IDs (`Gun2`, `AoE`, `mortar2`, `MagicTower1`, `MineTower1`…) don't match the four IDs the save knows (`turret`, `aoe`, `spellTower`, `landMine`). So the shop's tower upgrades probably have no effect in game right now. See Part 9.
- **The Shop scene** (La'Rue): pages for turrets and spells, button text that shows "Unlock · N stars", "Need N more stars", "Level 2/3" or "Max", and screenshots of each tower/spell as preview sprites.
- **Temperament lock (first playable):** `LevelDetailsPopup.highestTemperament` (default Annoyance). The default is set in the `.cs` file on purpose, because the level select builder recreates the popup and would reset a prefab value.
- **Editor test tools** (`SaveDataMenu`): Tools › Skyline Rush › Save Data › Print Progress / Open Save Folder / Delete Save File, and right-click a Level Data asset › Skyline Rush › Test: Win on Annoyance…Bloodlust / Lose on Wave 20. Remember a fake win only unlocks levels that *require* that level, and the map only re-reads the save when it opens.

### 3.9 The level select

Built on Nov 2 from the whiteboard sketch (figure-8 path, key branches, boss with 3 locks) and an HTML mockup:

- **A generated 3D map** made from Path Scene_Mikey's environment packs (Mini Village ground/water/houses, Meadow Forest trees and rocks, grass-stone pads), seen by its own camera, with the mockup's UI drawn on top.
- **Generator:** `Tools › Skyline Rush › Build Level Select` (`LevelSelectBuilder.cs`) creates the Level Data assets once, rebuilds `LevelSelect.prefab` from scratch, and keeps one instance in the boot scene. Layout positions are written in **mockup pixels** (1280×720) and projected onto the 3D ground, so the map lines up with the design at 16:9. Trails are chains of cubic Bézier curves sampled into dashes.
- **Runtime scripts:** `LevelSelectScreen` (shows itself whenever `ClickCode.IsLevelMenuOpen`, so Logon's menu flow kept working; refreshes from the save each time it opens; keeps nodes glued to the 3D anchors in `LateUpdate`), `LevelNode` (states Locked/Open/Next/Beaten, star rows with the pink Bloodlust star, key and boss variants, hover scale and pulsing glow on unscaled time), `LevelTrail` (lit gold/blue once its start level is beaten), `LevelDetailsPopup` (best wave, best clear, temperament picker, START), `LevelSelectStyle` (all colors as hex in one place).
- **START** → `LevelSession.Begin(level, temperament)` → `ClickCode.StartLevel(sceneName)` (loads the level, pauses until the HUD Start button).
- **Preview pictures** live on each Level Data asset (`previewImage`): import a screenshot as **Sprite (2D and UI)**, Max Size 1024, drag it into that level's slot.
- **Gotchas:** the root `LevelSelect` object must stay **active** (its script hides the map itself); hand edits in the scene are overrides a rebuild would wipe; the builder's hard-coded scene path broke when `UI.unity` was renamed; the prefab was once deleted in an unrelated commit.

### 3.10 Menus and game flow (`ClickCode`)

`ClickCode` (Logon) is a `DontDestroyOnLoad` singleton in the boot scene holding every panel: Main, Level, Settings (Video/Audio panels), Pause, HUD, Credits (Coders/Assets panels), Base Menu (build), Tower Menu (upgrade/sell), and the spell bar.

- **Panel switching:** `ClearAll()` hides every panel, then one is shown. A `last` field remembers where Back should return to.
- **Time gating:** `timeScale = 0` at startup, when a level loads (until the HUD **Start** button sets 1), and on pause. Pause closes when you click outside its content.
- **Scene changes:** `StartLevel(scene)`, Restart (reload the active scene by build index), Leave (load `Main Game and UI`), `ReturnToMainMenu`, `ReturnToLevelSelect`, `OpenShop` (clear the stuck selection, hide menus, load `Shop`).
- **Quit:** `Application.Quit()` (no effect in the editor or a browser) plus `#if UNITY_EDITOR EditorApplication.isPlaying = false`. Hidden in WebGL by `HideOnWebGL`.
- **Tips bar** (`TipBar`, La'Rue): rotates tips on unscaled time; "Don't show again" saved in PlayerPrefs; each level now opens on the next tip (stored in PlayerPrefs).
- **Volume slider** (`VolumeSlider`): sets `AudioListener.volume`, saves to PlayerPrefs on panel close, applies the saved value at startup via `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`, uses `SetValueWithoutNotify` when the panel opens, and plays a preview click on mouse-up (`IPointerUpHandler`).

### 3.11 The SFX system (`Sfx.cs` + `SfxEvent.cs`)

The project had no audio at all until Nov 13, when you recorded a full set of SFX. The system:

```csharp
[CreateAssetMenu(menuName = "Audio/Sfx Event")]
public class SfxEvent : ScriptableObject
{
    public AudioClip[] clips;                         // one picked at random each play
    [Range(0, 1)]   public float volume = 1f;
    [Range(0, 0.5f)] public float pitchVariation = 0.08f;  // so 50 kills don't sound copy-pasted
    [Range(0, 1)]   public float spatialBlend = 0f;   // 2D suits the overhead camera
    public float minInterval = 0.05f;                 // skip plays closer than this (10 towers firing ≠ noise)
    public void Play(Vector3 position) { ... Sfx.PlayClip(randomClip, position, volume, 1 ± variation, spatialBlend); }
}
```

- `Sfx` **creates itself** before the first scene loads (`[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` + `DontDestroyOnLoad`), so no scene needs it. It owns a **pool of 24 AudioSources**; if all are busy, the oldest sound is cut off.
- `Sfx.Play(sfxEvent, position)` is null-safe: an empty slot just stays silent, so hooks could be added to scripts before sounds existed.
- **Every UI Button clicks automatically:** on each `sceneLoaded`, `Sfx` finds all buttons (inactive ones too) and adds a click listener, removing it first so persistent menus aren't hooked twice. The click sound is loaded by path from `Assets/Resources/Audio/UI Click.asset`, the only asset whose name and folder matter.
- **Hooks:** one `SfxEvent` field + one `Sfx.Play` line each in `Gun` (Shoot Sound), `Bullet` (Impact Sound), `Mine` (Explode Sound), `MineTower` (Throw Sound), `EnemyHealthDisplay` (Death Sound).
- **Mapping:** TurretSound → all turret tiers; MortarLaunch1/2 → mortar tiers 1–2 / 3–4; MortarImpact1/2 → MortarShell / Mortar2Rocket; MagicTowerShot/Impact → obelisk tiers and MagicShot/MagicShot2; MineTower1/2 → mine launcher throws; GhostDies, GiantImpDies (also on Demon King, Yeti King, Minotaur), ImpDies, SpiderDies, LittleDevilDies → enemy prefabs. Files in `Assets/Audio/SFX`, events in `Assets/Audio/Events`.
- **Why an event asset and not the raw .wav:** the slot holds settings (volume, pitch variation, interval) you can tune without Audacity, and one event in many slots changes all of them at once.

### 3.12 Level packaging: EasyLevelWirer, and what a level needs

Getting levels 2 and 3 playable showed how much "invisible wiring" a level needs. The checklist (Nov 9):

1. **WaveSpawner**: Wave JSON, Default Map Spline, Path Registry, Enemy Registry (every ID the JSON uses), Base Health, Game Result Manager, the HUD text slots.
2. **GameResultManager** + Win and Game Over panels (on an active canvas outside the gameplay HUD).
3. **BaseHealth** linked to the result manager and the lives text.
4. **CurrencyManager** + coins text and a starting amount.
5. **Tower menus** (build and upgrade), and every base (`Bass`) pointing at them.
6. **An EventSystem**, or no UI button works.
7. **HUD**: coins, lives, wave counter, pause, Next, timer, enemy info panel.
8. The scene in the **build scene list** and a **Level Data** asset pointing at it.

Michael packaged all of this into one prefab, `EasyLevelWirer.prefab` (Nov 14, "Made an easy way to copy everything needed to wire a level. GO NUTS GUYS."), then used it for two new levels in a day (Technolooper and the 2× speed Lightning Raceway). Caveats: a prefab can't reference the level's own camera or splines (they must be set per scene or found at runtime), and its 0.18 root scale probably inflates spawned enemies.

### 3.13 WebGL support (code side)

- **Saves:** `SaveManager` routes all file access through four helpers; on WebGL they use PlayerPrefs instead of `File.*`:

  ```csharp
  #if UNITY_WEBGL && !UNITY_EDITOR
      const string PrefsKey = "SkylineRush_Save";
      static bool SaveExists() => PlayerPrefs.HasKey(PrefsKey);
      static string ReadSave() => PlayerPrefs.GetString(PrefsKey);
      static void WriteSave(string json) { PlayerPrefs.SetString(PrefsKey, json); PlayerPrefs.Save(); }
      static void EraseSave() { PlayerPrefs.DeleteKey(PrefsKey); PlayerPrefs.Save(); }
  #else
      static bool SaveExists() => File.Exists(SavePath);
      ...
  #endif
  ```

  (When first pasted, this block landed *inside* `Save()`, which made the helpers local to that one method so the other methods couldn't call them. Where you paste code matters.)
- **Quit:** `HideOnWebGL` deactivates its object in `Awake` in browser builds only.
- Everything else (URP, Input System, no threads/plugins/video) already worked in the browser. Settings and hosting are in Part 5.

---

## Part 4 — Git and teamwork in a Unity project

Four people pushing to one Unity repo produced most of this project's drama. Michael's Oct 15 commit message sums up the mood: *"I HATE GIT. But seriously, fixing merge conflicts that when I do EVERYTIME, the most Wile E. Coyote stuff happens and it blows the whole thing up."* Here's everything we learned so it goes smoother next time.

### 4.1 Repo setup

- **What to commit:** `Assets/` (with every `.meta`), `Packages/` (`manifest.json`, `packages-lock.json`), `ProjectSettings/`.
- **What to ignore** (Unity's GitHub template `.gitignore`): `Library/`, `Temp/`, `Obj/`, `Build/`, `Logs/`, `UserSettings/`, `.vs/`, `*.csproj`, `*.sln`, and a few generated `.meta` patterns. Things we found that should also be ignored:
  - `/Data/` at the project root (WebGL builds leave Burst output `lib_burst_generated.wasm/.cpp` there)
  - `UIElementsSchema/` (Unity-generated `.xsd` files, ~35,000 lines committed by accident on Oct 28)
  - merge-tool leftovers: `*_BACKUP_[0-9]*`, `*_BASE_[0-9]*`, `*_LOCAL_[0-9]*`, `*_REMOTE_[0-9]*`, `*.orig`
- **`.gitattributes`:** `* text=auto` plus **Git LFS** patterns for big binaries (`.psd`, `.mp3`, `.wav`, `.ogg`, `.mp4`, `.zip`, `.exe`, `.dll`, `.unitypackage`, …). Added Oct 13.
- **UnityYAMLMerge ("Smart Merge"):** Michael added `*.unity merge=unityyamlmerge` lines on Oct 28. They do **nothing** unless every teammate also configures the merge driver in their own git config, pointing at `UnityYAMLMerge.exe` in their Unity install. Since nobody had, git kept doing plain text merges on scenes. The lines were removed the same day. (If you ever set it up properly, it can merge scene YAML much better than plain git; everyone has to configure it.)
- **Branches:** `master` (default), `Dev` (where all work happened), plus `Logon/UI`, `Logon/pause`, `Logon/Bass`, `feature/multi-path-spawner`, `fix_logon`, `backup-dev-oct15`, and `versioncontrol` (the asset trim). The first five merges went through GitHub pull requests (#1–#5, Sep 29 – Oct 15); after that everyone pushed straight to `Dev`, and **`master` stopped at Oct 15**. For a portfolio, merge `Dev` into `master` at each milestone so the default branch shows the real game.
- **Two clones on one machine** caused confusion on Oct 26: `repos\Team_Project_Project_Portfolio_2` was 65 commits behind because `git pull` only updates the folder you run it in. Keep one working clone.

### 4.2 Daily workflow that worked

1. Save scenes in Unity (**File → Save**), and close Unity or at least don't click into it while git changes files.
2. `git status` to see what you actually changed. **Discard junk** before committing (see 4.4).
3. Commit your own work with a clear message, staging specific files instead of `git add .` when Unity has rewritten unrelated files.
4. `git pull`, resolve conflicts if any, then **check the project compiles** (open Unity, Console clean) **before** pushing.
5. `git push`, then post a short Discord heads-up if you touched someone else's area.

**If `git pull` refuses** ("Your local changes to the following files would be overwritten by merge"), it fetched but didn't merge, and your branch is now N commits behind. Options:

```bash
git restore "Assets/TextMesh Pro/Resources/Fonts & Materials/Color shop.asset"   # throw away junk
git stash        # park real uncommitted work
git pull
git stash pop    # bring it back on top
```

### 4.3 Merge conflicts in Unity files

Scenes (`.unity`), prefabs (`.prefab`), materials and assets are **YAML text**, so git merges them line by line and *can* produce conflicts. But a "clean" text merge can still be broken for Unity, and a conflict can't be fixed by just picking lines.

#### The incidents

| Date | What happened | How it was fixed / lesson |
| --- | --- | --- |
| Oct 15 | Constant scene conflicts while merging pathing, guns and enemies | Restored working versions from earlier commits; "Restore clean version of Path Scene.unity" |
| Oct 28 | Merge `b4a1006` committed **11 unresolved conflicts** (33 marker lines) into `Path Scene_Mikey.unity`. Unity couldn't load it; a follow-up emptied the file and committed a **blank scene** | Re-did the merge in a temp folder (Michael's deletions of old spawners + La'Rue's win/lose UI), verified 757 internal references and no duplicate IDs, committed `ab17840`. Lesson: **never commit a file with `<<<<<<<` in it**, and one person edits a scene at a time |
| Nov 2 | TMP font assets conflicted; a zip of the fonts folder and a broken duplicate `Menu Color` font came in | Took one side of the font assets whole; deleted the zip (Unity can't use files inside a zip) |
| Nov 4 | "deleted by them / modified by us" on `Menu Color.asset` | `git rm` the file (it was unused), `git commit --no-edit` |
| Nov 4 | Merge `f861f9c` committed conflict markers into 2 minotaur `.mat` files **and 3 `.meta` files** (Unity only complained about the `.mat`s) | Restored all 5 from one side (`82c2128`). Cause: `minotaur1.fbx` uses legacy external materials, so each machine generated its own `Materials` folder with different GUIDs |
| Nov 8 | Merge `bbac0d3` had **no conflict markers but didn't compile**: one side renamed members to the `ITower` interface, the other added code using the old `Gun` names. Also a duplicate Level 2 entry in Build Settings | Fixed in `e7fccea`. Lesson: "0 conflicts" ≠ "works". Compile after every merge |
| Nov 16 | `Color shop.asset` (dynamic TMP font) committed with conflict markers, plus **16 merge-tool leftovers** (`Color shop_BACKUP_380.asset` etc.) that Unity had imported and given `.meta` files | `git checkout --theirs` the font, `git rm` the leftovers, scan for markers, commit |

#### Commands that help

```bash
# What state am I in?
git status

# Back out of a merge completely (returns to before the pull)
git merge --abort

# Resolve a delete/modify conflict by accepting the deletion
git rm "path/to/file.asset"

# Take one side of a conflicted file whole (good for generated/cache files)
git checkout --theirs -- "path/to/file.asset"     # the incoming version
git checkout --ours   -- "path/to/file.asset"     # your version
git add "path/to/file.asset"

# Finish the merge with git's default message ("Merge branch 'Dev' of ...")
git commit --no-edit

# After ANY merge: scan for leftover conflict markers (should print nothing)
git grep -n -E "^(<<<<<<<|>>>>>>>) " -- Assets ProjectSettings

# Which files did both sides change? (check these in Unity)
git diff --name-only <base> <parent1> > a.txt
git diff --name-only <base> <parent2> > b.txt
comm -12 <(sort a.txt) <(sort b.txt)

# How was each conflict resolved in a merge commit?
git show --remerge-diff <merge-commit>
```

#### Using VS Code as the merge tool

```bash
git config --global merge.tool vscode
git config --global mergetool.vscode.cmd 'code --wait --merge "$REMOTE" "$LOCAL" "$BASE" "$MERGED"'
git config --global mergetool.keepBackup false   # no .orig files for Unity to import
git config --global mergetool.prompt false       # skip "Hit return to start"
```

- The first line picks the tool by name; the second says what that name runs. `$REMOTE` = incoming, `$LOCAL` = yours, `$BASE` = the common ancestor, `$MERGED` = the real file. `--wait` makes git wait (Git Bash looks frozen) until you close the tab.
- `git pull` never opens a tool by itself. Run `git mergetool`, press Enter, resolve in the **Result** pane, click **Complete Merge**, then `git commit`.
- Or skip the config: open the folder in VS Code → Source Control → **Merge Changes** → **Resolve in Merge Editor**.
- **Don't run the merge tool on Unity asset files** (fonts, scenes). It drops temp files into `Assets/`, Unity imports them, and they get committed. Take a side instead.

### 4.4 Files that change by themselves (don't commit these blindly)

| File | Why it changes | What to do |
| --- | --- | --- |
| `Assets/TextMesh Pro/.../Color shop.asset` (and other dynamic TMP fonts) | Play mode writes glyphs into it; builds clear it | Discard local changes; in conflicts take either side whole |
| `ProjectSettings/UnityConnectSettings.asset` | Rewritten by builds/services | Discard |
| `Assets/Settings/Mobile_RPAsset.asset` | URP updates shader prefilter flags during builds | Usually fine to commit once, or it keeps coming back |
| Texture `.meta` files after switching platform | Unity adds per-platform override entries | Commit once with the platform change |
| `TheKillerWhales.slnx` / `.csproj` | Regenerated by Unity | Ignored or harmless |
| `/Data/`, `UIElementsSchema/` | Generated by builds / UI Toolkit | Gitignore |
| Scenes you only *opened* | TMP re-saves style hashes, Unity re-serializes | Check the diff; discard if you didn't mean to change it |

### 4.5 Accidental deletions in unrelated commits

Twice, a commit about something else deleted assets other people depended on:

- **Nov 10, `8568f3a`** ("Add the remaining spell in game and it works") deleted the whole **`Assets/Material`** folder (bullet smoke, MagicGlow, missile, portal glow materials and textures).
- **Nov 14, `2aac1f4`** ("Shop spells are now using the correct shop purchase") deleted **`LevelSelect.prefab`**.

Both were restored from the commit *before* the deletion, which brings back the same GUIDs so every reference reconnects:

```bash
git checkout 8568f3a^ -- Assets/Material
git checkout 2aac1f4~1 -- "Assets/Scenes/LevelSelect/LevelSelect.prefab" "Assets/Scenes/LevelSelect/LevelSelect.prefab.meta"
```

(`^` and `~1` both mean "the parent commit". Run git from the project root, not from inside `Assets`.)

**Lesson:** before committing, read the list of files in `git status`. A long list of deletions you don't recognise means stop and ask. Unity sometimes shows a deleted prefab's instances as red "Missing Prefab" placeholders and keeps working in the editor, so the damage isn't obvious.

### 4.6 Other git skills we used

```bash
# Commit report for class (all branches, no merges, your commits since a date)
git log --all --no-merges --since='Nov 2 2026' --author=Hardy --output=NoahHardyCommits.txt
#  --author is a case-sensitive pattern; repeat it to OR several names/emails
#  In Windows PowerShell 5, '>' writes UTF-16; --output= always writes plain text
#  No terminal output means it worked (everything went into the file)

# Who deleted a file, and when?
git log origin/Dev --diff-filter=D --format="%h %an %ad %s" --date=short -- "path/to/file"

# Bring back one folder from an older commit
git checkout e224dfa -- "Assets/Assets Enemy/Warplane" "Assets/Assets Enemy/Warplane.meta"

# New branch + annotated tag, pushed
git switch -c versioncontrol
git tag -a Trimmed -m "Assets trimmed"
git push -u origin versioncontrol
git push origin Trimmed

# Get a teammate's branch
git fetch origin && git switch versioncontrol
```

### 4.7 Teamwork habits that helped

- **One scene, one editor at a time.** Announce in Discord before editing a shared scene; put new work into prefabs and drop them into the scene in one small commit.
- **Heads-ups when touching someone else's code** (we wrote paste-ready Discord messages for nearly every cross-area fix: wave fixes, merge breaks, collider changes, sell button patches).
- **Pull right before you start and right before you push.**
- **Shared settings are shared** (`ProjectSettings/`, Build Profiles, the default TMP material): changing them changes everyone's project.
- **Know whose area a file is** before "fixing" it; ask, or at least tell them.
- **Respect intentional design calls** (Michael's wave-launch timer was deliberate pressure; the fix kept the intent and only moved when the countdown starts).

---

## Part 5 — Building, hosting and turning in

### 5.1 Windows build checklist

1. **All scenes the game loads are in the build scene list**, with the boot scene first.
2. **No editor-only code in runtime scripts** (`using UnityEditor` outside `#if UNITY_EDITOR` or an `Editor` folder fails the build even though Play mode works).
3. Save everything (**File → Save**, **File → Save Project**). Builds use the files on disk.
4. **File → Build Profiles → Windows** (Switch Platform if needed).
5. **Development Build off** for anything you hand in (see 5.2).
6. **Build into a new, empty folder** (e.g. `Build/SkylineRush_Beta_Release`) so old files don't mix in. `Build/` is gitignored.
7. Test the built exe, starting from its main menu, through every level.

**What a Windows build folder contains** (the `.exe` alone won't run):

```text
SkylineRush/
├─ SkylineRush.exe
├─ SkylineRush_Data/          ← scenes, assets, compiled code
├─ MonoBleedingEdge/          ← C# runtime
├─ D3D12/
├─ UnityPlayer.dll
├─ UnityCrashHandler64.exe
├─ dstorage.dll, dstoragecore.dll
└─ SkylineRush_BackUpThisFolder_ButDontShipItWithYourGame/   ← debug data: never ship
```

### 5.2 Development Build vs release build

The Beta Windows build was accidentally a **Development Build** with **Autoconnect Profiler**:

- `boot.config` contained `player-connection-mode=Connect` and `profiler-enable=1`,
- it used the 91 MB development `UnityPlayer.dll` and shows a "Development Build" watermark,
- at launch it tries to connect to the editor's profiler for up to ~10 seconds, which looks like a stall on a grader's PC with no editor.

It was shipped anyway because of the deadline. **Next time, untick Development Build and Autoconnect Profiler from the start** for any build you hand in.

### 5.3 Build Profiles (Unity 6)

- `File → Build Profiles` replaces the old Build Settings window. You can add several profiles per platform (we have **Windows**, **Web – Desktop – Development**, **Web – Desktop – Release**, saved under `Assets/Settings/Build Profiles/`, so commit them).
- **A profile created from a preset can carry its own full copy of Player Settings.** Changes in the main Player Settings window then don't reach it; change them in the profile's own **Player Settings** foldout. (Our first Dev build seemed to use the global settings instead, so check the output to see which one wins.)
- All profiles share the **Scene List** unless you give one its own.
- The active platform is stored per machine; switching doesn't affect teammates, but the first switch reimports everything (slow).

### 5.4 WebGL

#### Setup

1. Unity Hub → **Installs → 6000.6.3f1 → ⚙ → Add modules → Web Build Support** (~5.7 GB). Restart Unity.
2. Build Profiles → add **Web** (Desktop – Development and Desktop – Release presets). Switch to it.
3. Code changes: saves via PlayerPrefs, hide Quit (Part 3.13).
4. Settings:
   - **Publishing Settings → Compression Format**: Brotli for release. **Decompression Fallback** on if the host doesn't send `Content-Encoding: br` (itch.io *did* serve our Brotli build fine without it).
   - **Resolution and Presentation → Default canvas**: 1280×720 (was 960×600). Web Template: Default (has a fullscreen button).
   - **Texture compression**: DXT for desktop browsers (ETC2/ASTC are for phones).
   - **Code Optimization**: faster builds for testing; Disk Size or Runtime Speed for release. Skip "with LTO" (very slow).
   - **Quality**: set the Web column's default to **PC** (Mobile renders at 0.8 scale and looks blurry).
   - Leave WebGPU off (experimental).
5. **Build And Run** starts a local web server and opens the browser. Double-clicking `index.html` doesn't work, because browsers block a Unity web build opened from disk.

#### Testing a web build

- Progress survives a page refresh (PlayerPrefs save).
- Audio starts only after the first click (browser autoplay policy).
- The Quit button is gone.
- Esc closes popups in windowed mode; in fullscreen the browser uses Esc to leave fullscreen.
- For errors, press **F12** in the browser and read the **Console** tab.
- The first WebGL build is slow ("IL2CPP", "Linking"); it hasn't frozen.

The first browser test immediately found a real bug that also affected Windows (two stacked spell bars), because it was the first time anyone tested the full flow from the main menu in a build.

### 5.5 Hosting on itch.io

1. Zip the **contents** of the WebGL build folder so `index.html` is at the top level of the zip (our `SkylineRush_WebGL_itchUpload.zip`, 40.7 MB).
2. itch.io → Create new project: Kind of project **HTML**, Pricing **No payments**, upload the zip, tick **"This file will be played in the browser"**.
3. Embed options: viewport size (match the canvas), **Fullscreen button** on, SharedArrayBuffer off.
4. Save as **Draft**, click **Run game**, make sure it reaches the menu.
5. Set Visibility to **Public**, then test the URL signed out (private window).
6. For later builds, upload to the **same page** and remove the old file, so the URL and any link files stay valid.

Our page: <https://ndhardy395.itch.io/skyline-rush>

### 5.6 The Windows installer (Inno Setup)

A zipped build folder doesn't count as an installer. We used **Inno Setup**, a free installer builder many indie games use:

```bash
winget install JRSoftware.InnoSetup
```

`Build/SkylineRush.iss`:

```ini
#define BuildDir "C:\Users\noahd\source\TheKillerWhales\Build\SkylineRush_Beta_Release"

[Setup]
AppId=TheKillerWhales.SkylineRush
AppName=Skyline Rush
AppVersion=Beta
AppPublisher=The Killer Whales
DefaultDirName={autopf}\Skyline Rush
DefaultGroupName=Skyline Rush
PrivilegesRequired=lowest                 ; per-user install, no admin prompt
OutputDir=C:\Users\noahd\source\TheKillerWhales\Build\TurnIn
OutputBaseFilename=SkylineRush_Setup
UninstallDisplayIcon={app}\SkylineRush.exe
Compression=lzma2/max
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"

[Files]
Source: "{#BuildDir}\*"; DestDir: "{app}"; Excludes: "*DontShip*,*.mp4"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\Skyline Rush"; Filename: "{app}\SkylineRush.exe"
Name: "{group}\Uninstall Skyline Rush"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Skyline Rush"; Filename: "{app}\SkylineRush.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\SkylineRush.exe"; Description: "Play Skyline Rush"; Flags: nowait postinstall skipifsilent
```

- Compile: open it in Inno Setup Compiler and press **Ctrl+F9**, or from the command line (to point at a different build folder):

  ```bash
  "$LOCALAPPDATA/Programs/Inno Setup 6/ISCC.exe" /DBuildDir="C:\...\Build\SkylineRush_Beta_Release" "C:\...\Build\SkylineRush.iss"
  ```

- Result: 370 files (290 MB) packed into a **77 MB** `SkylineRush_Setup.exe`. It installs to `%LOCALAPPDATA%\Programs\Skyline Rush`, adds Start menu entries and a desktop shortcut, registers an uninstaller in Windows Settings → Apps, and can launch the game at the end.
- **Test it:** install, launch from the Start menu shortcut, uninstall, confirm nothing's left.
- It isn't code-signed, so Windows SmartScreen may say *"Windows protected your PC"*: **More info → Run anyway**. Normal for student/indie projects; mention it in turn-in notes.
- The installer doesn't update itself. New build = recompile the `.iss`.

### 5.7 Sprint review turn-in requirements (Full Sail)

One zip named **`<TeamName>_<GameName>_<Milestone>.zip`** (ours: `TheKillerWhales_SkylineRush_BetaSprintReview.zip`, 100.8 MB). **Every team member submits the same zip.** It must contain:

| Item | Requirement |
| --- | --- |
| Installer | Installs a playable exe build with all its files and creates Start menu/desktop shortcuts. A zipped build folder doesn't count |
| Second platform | For WebGL: a `.url` link file or a `.txt` with the URL of the **hosted** build, inside the zip. Exported web files don't count |
| Video | An **.mp4** (not a link) of the **exported exe** (not the editor) showing the whole game and every sprint task. Michael recorded ours (2:08, 1080p60) |

A `.url` file is just text:

```ini
[InternetShortcut]
URL=https://ndhardy395.itch.io/skyline-rush
```

Packaging tips: put the four files in one folder (`Build/TurnIn`), select the **files** (not the folder) → Compress to ZIP, and don't zip a folder that already contains an older zip (it nests). Verify each file in the zip against its source. Discord accepted the 100 MB zip fine.

### 5.8 Our builds

| Folder in `Build/` | What |
| --- | --- |
| `Windows`, `WindowsDevBuild` | Early test builds (`Windows` holds the Oct 18 build and a gameplay video) |
| `TheKillerWhales_SkylineRush_Alpha1`, `…_AlphaMilestoneReview` | Alpha milestone builds (release builds) |
| `TheKillerWhales_SkylineRush_Core.zip` | Core build zip |
| `…_BetaSprintReview_Windows`, `…_BetaSprintReview_WebGL`, `WebGL` | Beta builds |
| `TurnIn/` | Installer, link files, video, final zip |
| `SkylineRush.iss`, `SkylineRush_WebGL_itchUpload.zip` | Installer script and itch upload |

### 5.9 Trimming the project size

On Nov 15 you asked to "trim the fat". A dependency walk followed every GUID reference out from the 5 shipped scenes, `Resources/`, ProjectSettings and the files the level select builder loads by path, and deleted everything unreachable: **1,135 files, ~498 MB; `Assets` went from 734 MB to 231 MB**. Kept on purpose: all scripts, TextMesh Pro, Settings, Resources, every scene (test scenes are small and are other people's work), and `EasyLevelWirer`. It went on its own branch `versioncontrol`, tagged **`Trimmed`**, not merged.

What it does and doesn't do:

- ✅ Smaller project folder and `Library/` import cache; faster imports.
- ❌ **The built game doesn't shrink**: Unity already leaves unreferenced assets out of builds (only `Resources/` and scenes in the build list are always included).
- ❌ **The git repo doesn't shrink**: history still holds everything (~380 MB pack). Rewriting history would disrupt the whole team.
- Most of what was removed were packs added for future levels; any of them can come back with `git checkout e224dfa -- "<folder>" "<folder>.meta"`.
- The project felt like "3 GB" mostly because of `Library/` (a local cache Unity rebuilds; gitignored).

---

## Part 6 — Bug casebook

Every real problem we hit, in roughly the order it happened. Search this when something feels familiar.

### 6.1 Towers, bullets and clicking

| Symptom | Cause | Fix | Lesson |
| --- | --- | --- | --- |
| Placed towers can't be clicked | No collider, so `OnMouseDown` never fires | Trigger `BoxCollider` on the tower root | `OnMouseDown` needs a collider |
| Clicking a tower opens nothing | `towerClick` referenced the Tower Manu **prefab file**, not the panel in the scene | Find the panel at runtime (`FindAnyObjectByType(FindObjectsInactive.Include)`) | Prefab fields pointing at prefab assets aren't the live object |
| One specific tower (center of screen) ignores clicks | An empty but enabled wave-banner text with **Raycast Target** on covered it | Turn off Raycast Target on display-only UI | Invisible UI still blocks clicks |
| Menus pile up; can't click off them | No close-on-click-outside; base menu and tower menu both open | Close on a world click that isn't a tower; close one menu before opening the other | Decide how every menu closes |
| New mortar never fires | `Enemy Layer` LayerMask set to **Nothing** | Set it to Enemy | Check LayerMasks on every new prefab |
| Bullets hit the mortar itself | The tower's GameObject layer was set to Enemy | Tower layer back to Default | GameObject layer ≠ LayerMask field |
| Gun.cs errors every frame on a tower with no turning parts | `Aim()` used empty pivots | Skip aiming when pivots are empty | Make components tolerate optional parts |
| Mortar shells land and do nothing | Landing only did splash, and Splash Radius was 0 | Landing always damages the target | Test the "0" case of every option |
| Magic towers do zero damage (spiders) | Splash radius 0.01 at the target's feet; the new spider capsule starts 0.6 above the ground | Always damage the direct target too | Tiny non-zero values change code paths; collider changes ripple |
| Tower 2 button does nothing | Inspector changes made in Play mode / on a scene copy / not saved | Edit the prefab in Prefab Mode, outside Play, and save | Check the file on disk changed |
| Tower 2 and 3 buttons place the wrong towers | Upgrade tiers were added to `TowerPrefabs`, shifting indexes | Only buyable base tiers go in the list | Index-based lists are fragile |
| `NullReferenceException` in `BuyTower` on Tower 3 | Child named `cost 3`, code looks for `Cost 3` | Rename exactly | `Find` is case-sensitive |
| Price labels never turn red | Compared coins with `towerCost`, which was never set | Parse each label's own price (La'Rue's fix) | Unused fields defaulting to 0 hide bugs |
| `GunMortar2` won't go into Upgrade Prefab | It was the shell (`Bullet`), not a tower (`Gun`) | Use the tower prefab; rename confusing assets | Typed fields only accept matching components |
| `MagicShot` won't go into Bullet Prefab | No `Bullet` component on its root | Add `Bullet` | Same rule |
| Rocket flies sideways and flips | Model's nose along +Y; `Bullet` steers by +Z | Mesh on a child rotated 90° X | Check model axes vs script axes |
| `MagicTower1` falls through the map | Non-kinematic Rigidbody with gravity, only a trigger collider | Remove Rigidbody / Is Kinematic | Don't keep components you don't need |
| Bullet smoke is a stiff line back to the gun, and stops short of the enemy | Lifetime × speed too long; bullet destroyed before the particles' last update; bullet removed at the collider surface | Shorter lifetime, Rate over Distance, back-fill the last frame, small penetration | Particles update after scripts |
| Smoke "never reaches" the bullet on long shots | New puffs too small and too sparse to overlap | Start bigger, emit more per unit | |
| Bullets stop on enemies that aren't the target | Dead enemies keep their colliders during the death animation | Raycast skips dead enemies; stop homing on a dying target | "Dead" ≠ "gone" |
| Bullets stop in mid-air before spiders | Spider colliders were ~6 m balls offset from the body | Fit a capsule to the body; aim at `bounds.center` | Gizmos on in Play mode to see colliders |
| Fixed spider prefab, but the ENEMY scene still has old colliders | Those spiders were placed from the FBX, not the prefab | (Test scene only) | Not everything in a scene is a prefab instance |
| Minotaur walks over mines | Capsule started ~4 units above its feet | Collider from feet to head (6 enemies fixed) | Measure the collider each level *actually spawns*, overrides included |
| Can't sell towers | Feature didn't exist | Sell button + 70% refund | Missing ≠ broken |
| Sell removes the wrong tower; "Sell label" text; crash; refunds 100% | Sell used the last base built on; label lookup threw; refund used full cost | Clicking a tower sets it as the sell target; `TryGetValue`; 70% | Make the selected object explicit |
| Sell button invisible | The duplicated `Sell` object was inactive and on top of Back | Activate it, move it | Duplicates copy position and active state |
| Refunds don't match prices paid | `towerCost` typed on prefabs drifted from menu labels (towers 3/4 swapped) | Fix values | One source of truth for prices |

### 6.2 Waves and levels

| Symptom | Cause | Fix | Lesson |
| --- | --- | --- | --- |
| Path Scene_Mikey is blank for everyone (Oct 28) | Merge committed with 11 conflicts, then the file was emptied | Re-merge, verify references, recommit | Never commit conflict markers |
| No win screen after skipping a wave | Shared counter reset each wave; went negative; one-time `== 0` check | Add to the counter; check the win continuously | Don't reset shared state |
| Next button throws `NullReferenceException` | `if (Instance == null) Instance.ForceNextWave();` (backwards check) | Call it outside the null check | Read null checks twice |
| Banner says "Wave: 3complete" | Missing space in the interpolated string | `$"Wave {n} complete"` | |
| Level 1 stuck forever at wave 24 | JSON `EnragedSpider` vs registry `Enrage Spider` → spawn failure flag | Find/replace in JSON | String IDs must match exactly |
| Level 2 waves never start | No `GameResultManager` in the scene; error message lumped 3 causes | Add the manager; error now names the missing slot and scene | Good error messages save hours |
| Waves pile up before you can clear one | Countdown started at wave launch | Countdown starts after the last spawn | Keep the designer's intent when fixing |
| HUD says "Wave 5 / 50" during wave 15 | `"waveNumber": 5` typo in JSON | Fix the data | Data needs review too |
| Early-call pays ~300 coins on some waves | 100–108 s delays in JSON | (Flagged to Michael) | Rewards scale with data mistakes |
| A JSON edit broke a level | A comment was added to the JSON | Remove it | JSON has no comments |
| Fireball "does nothing" on level 2 | Level 2 enemies used prefab stats (300 HP) vs 75 damage, and their HP wasn't visible | (Not a bug) Fix enemy clicking to see HP | Make state visible before diagnosing |
| Fire spell can't be cast in level 2 | Level 2's own SpellManager had the Ice prefab in the Fireball slot | Use the shared HUD spell bar | Per-scene copies drift |
| Every spell castable twice; two stacked spell bars | Level 1 re-added its own spell system while the HUD one also existed | Delete the level's copy | Test from the boot scene |
| Can't click enemies on level 2/3 | Two half-wired `EnemySelection` components; prefab can't reference the scene camera | Fallback to `Camera.main`; remove the duplicate | |
| Moonwalking imp | `facingOffset` 180 on one prefab | 360 | |
| Giant enemies from EasyLevelWirer *(unverified)* | Root scale 0.18 → children 5.56×; spawned without a parent | Spawn with `lossyScale`, or root scale 1 | Scaled parents are traps |
| Level 2/3 played at different difficulty than level 1 | Level 1's stats were scene overrides | Prefab variants with the tuned stats | Keep data in assets, not scene overrides |

### 6.3 Menus, UI, scenes and the level select

| Symptom | Cause | Fix | Lesson |
| --- | --- | --- | --- |
| Shop scene "extremely zoomed in" | Canvas Scaler on Constant Pixel Size, laid out for a larger screen | Scale With Screen Size 1920×1080, Match 0.5 | Always use Scale With Screen Size |
| Shop doesn't open; PLAY "opens" it; closing it lands on level select; purple hover gone | Persistent ClickCode menu drawn over the Shop scene; state kept; Shop button stuck **Selected** | Hide menus before loading; return through `ReturnToMainMenu`; `SetSelectedGameObject(null)` | DontDestroyOnLoad UI follows you |
| `IndexOutOfRangeException` in `Selectable.OnDisable/OnEnable` | Enter Play Mode without domain/scene reload; ugui's button list desynced | Reload Domain and Scene; restart Unity to clear | Not your code; harmless noise |
| New TMP text is red | Default TMP material was restyled | Own material copy | Shared defaults are shared |
| Font errors "expected 1, got 262144" | Broken duplicate font asset (atlas 1×1 vs 512×512) | Remove it | |
| Level select water looks black | `Water.FBX` is a tall slab; its top sat oddly for the map camera | Flat plane with the level's water material | |
| Village props magenta | The pack's *prefabs* use non-URP materials | Use the FBX models | |
| Map edits "not in the prefab" | They were scene overrides on the instance | Apply All (and don't re-run the builder) | |
| Level select builder fails | Hard-coded `UI.unity` path after the rename | Update the path | Generators need maintenance |
| Level map never opens | `LevelSelect` root object switched off | Turn it back on | Scripts on inactive objects don't run |
| Map works in editor but prefab is gone | `LevelSelect.prefab` deleted in an unrelated commit | Restore from git (same GUID) | Read your commit's file list |
| Game opens on Credits | Credits panel saved active in the boot scene, newly opaque | Turn it off; startup guard `ClearAll(); MainManu.SetActive(true);` | Code the startup state |
| Credits fix came back | Scene edited on disk while open; Unity saved its in-memory copy | Make the change in Unity, or Reload first | Unity's in-memory scene wins on save |
| Base Menu showing on the main menu | The spare copy inside ClickCode was left active (twice) | Keep it inactive | |
| Tower 4 button missing in game | Added to the spare menu inside ClickCode, not `Base Manu.prefab` | Edit the copy the game uses | |
| Fireball button clickable off the icon | Button's rect bigger than the picture | Size the Image/rect to the icon | Click area = rect |
| Only tip 1/5 ever shows | Every level opened on tip 1, rotating after 10 s | Remember the next tip in PlayerPrefs | |
| Fake "Win on Annoyance" doesn't unlock | Used on levels that unlock nothing / Coming Soon | Fake-win the *required* levels | Know your unlock graph |
| Shop button text shows a box | Non-UTF-8 saved character (`·` → `�`) | Save as UTF-8 | |

### 6.4 Audio

| Symptom | Cause | Fix |
| --- | --- | --- |
| No click sounds in main menu/level select | AudioListener on the boot scene's camera unchecked | Enable it |
| Sounds feel delayed | Preload Audio Data off; DSP buffer "Best Performance"; silence at file start | Preload on; Good latency; trim in Audacity |
| First magic tier silent | Its Shoot Sound slot wasn't saved | Assign + Ctrl+S |
| Turret sound odd | 0.06 typed into Spatial Blend instead of Min Interval | Fix fields |
| Volume slider "missing" a percent label | That step was optional | Add a TMP text and link it |

### 6.5 Project, git and build problems

| Symptom | Cause | Fix |
| --- | --- | --- |
| "Why are .meta files in the repo?" | They must be | Keep committing them |
| Red errors after a day away | Unity services sign-in expired | Restart/sign in; ignore |
| Build would fail with compile error | `using UnityEditor.Rendering.Universal;` in `Gun.cs` (added by autocomplete) | Delete the line |
| Deleted `Assets/Prefabs` folder | Accidental delete in Project window | Restore from Recycle Bin (with `.meta`) |
| Lots of pink materials after a pull | `Assets/Material` folder deleted in an unrelated commit | `git checkout <commit>^ -- Assets/Material` |
| Minotaur materials "seem to have merge conflicts" | Conflict markers committed in `.mat` and `.meta` | Restore from one side; scan for markers |
| 3 compile errors after a "clean" merge | Interface rename vs new code using old names | Fix names; compile before pushing |
| `git pull` "did nothing" | Blocked by local changes; fetched only | Restore junk / stash, pull, pop |
| Level 4 missing after pull | Same: pull didn't merge | Same |
| New levels fail to load in builds | Not in the build scene list | Add to Scene List |
| Built game stalls ~10 s at start | Development Build + Autoconnect Profiler | Release build |
| `Color shop.asset` conflicts every pull; leftover `_BACKUP_`/`_BASE_` files committed | Dynamic TMP font + interrupted merge tool | Take a side; `git rm` leftovers; gitignore patterns |
| Commit-report command "didn't print anything" | Output went into the file | That's success |
| `MaterialLocation.External is obsolete` warning | Legacy FBX import setting | Ignore, or switch to embedded materials |

---

## Part 7 — Art, assets and the asset pipeline

### 7.1 Asset packs we used

| Pack (folder) | Used for |
| --- | --- |
| `TowerDefenceAssets` (low-poly tower defence pack: 17 `Gun_*`, 15 `Tower_*` models) | All tower and gun models, bullets, the mine (Tower_15/Tower_11) |
| `Enviroment/Low_Poly_Mini_Village` | Ground, water, houses (use the FBX models in `Art/Meshes`) |
| `Enviroment/Meadow Forest Props` | Trees, rocks, bushes |
| `Enviroment/Fantasy Forest Environment Free Sample`, `HandPainted Ground Textures`, `CartoonMilitaryModelPack` | Level dressing / textures (some trimmed later) |
| `Assets Enemy/Spiders`, `Dark Big Ghosts LITE`, `Stylized3DMonster`, `minotaur1`, `Alstra Infinite/Fish - PolyPack`, … | Enemies and bosses |
| `Prefabs/LiquidFire Package 4 - BSH games` (Devtoid Gold Coins) | Coin drops |
| Warplane, AirCraft, Shark, Whale, Lava environment… | Hero/boss experiments and future levels (mostly trimmed on the `versioncontrol` branch) |

Logon added an **asset credits** panel to the Credits screen. Keep a list of every pack and its license from day one; it's much harder to reconstruct at the end.

### 7.2 Working with imported models

- **FBX models are read-only** in Unity: you change import settings (scale, materials, animation), not the mesh. To build a tower, drag the model into a scene, re-parent its parts under empty pivot objects, add components, and save as your own prefab in `Assets/Prefabs`.
- **Measure before you scale.** The Gun_6 model was ~9 units wide, about 4× a tower base; ~0.25–0.33 scale fit.
- **Colliders aren't included** with most models (or come as a MeshCollider on every part). Add your own simple colliders that fit the gameplay shape.
- **Import → Materials tab:** "Use Embedded Materials" is the modern setting; "Use External Materials (Legacy)" is what triggers the obsolete warning and generates per-machine material files.
- **Model forward axis** may not be +Z (rockets pointed +Y, some enemies faced backwards). Fix with a rotated child or an offset field, never by rotating the root that scripts move.

### 7.3 Recoloring low-poly models (palette textures)

1. Find which material the part uses (Mesh Renderer → Materials → Element 0; click it to highlight the asset).
2. **Duplicate the material** (Ctrl+D) so other models don't change. It's a ~4 KB file pointing at the same texture.
3. On the copy, change **Base Map Offset X** in whole-strip steps (1/16 = 0.0625 for `GradientColor_1`; 0.0602 per strip for `GradientColor_2`, where 0.5414 = 9 strips). Offset Y moves lighter/darker within a strip.
4. Assign the copy to the part and save the prefab.
5. To see what's going on: the texture preview (bottom of Inspector), the mesh's **UV Layout** preview, and the color picker's eyedropper.

For a solid color instead: clear the Base Map texture, then set Base Color (otherwise Base Color multiplies with the texture).

### 7.4 Effects

- **MagicGlow** (shared glowing particle material): URP Particles/Unlit, Transparent, **Additive**, `Default-Particle` texture, white; tint per effect with Start Color.
- **RocketExplosion / MagicExplosion / Mine2Explosion**: one-shot bursts with Stop Action Destroy, sized to the damage radius so players can see what got hit.
- **Bullet smoke**: a generated soft puff texture (`BulletSmokePuff.png`) + `M_BulletSmoke.mat`, World space, Rate over Distance, Noise, growing and fading puffs; grey for the blue bullet, orange for the orange one.
- **Lightning bolt**: a jittering `LineRenderer`, not a particle system.
- **Range rings / spell target rings**: `LineRenderer` circles (64 points).
- **Camera shake** on base damage instead of a red flash.

### 7.5 Images and UI art

- Screenshots/icons for UI: **Texture Type → Sprite (2D and UI)**, set Max Size to what's needed (a 340×190 preview doesn't need 4K), then Apply.
- 9-slice sprites (rounded panels): set **Sprite Border** so corners don't stretch (done automatically by the level select's `AssetPostprocessor`).
- The level select's icons were drawn from the mockup's SVGs and imported as sprites.
- Keep art organised: La'Rue's shop screenshots landed loose in `Assets/` with default Windows names (`Screenshot 2026-11-13 144701.png`). Dragging a sprite into the scene and then into the Project window creates a stray prefab (`turret photo_0.prefab`).

### 7.6 Audio files

- Record/edit in Audacity, export WAV, name files by what they are (`TurretShot.wav`, `SpiderDies.wav`).
- Drag them into `Assets/Audio/SFX` (Unity copies them; your originals stay put).
- Import settings: Force To Mono, Decompress On Load, Vorbis, Preload Audio Data.
- To replace a sound later, overwrite the file in File Explorer with the same name (keeps the GUID and every slot).

---

## Part 8 — C# patterns and gotchas

### 8.1 Patterns from our code

**Interfaces decouple systems.** Towers only know `IDamageable` and `IPathProgress`; the upgrade panel only knows `ITower`. New enemy or tower types plug in without touching the code that uses them.

**Explicit interface implementation** exposes a member only through the interface:

```csharp
GameObject ITower.UpgradePrefab => upgradePrefab != null ? upgradePrefab.gameObject : null;
int ITower.UpgradeCost => upgradeCost;
```

**Singleton MonoBehaviour** (one instance, reachable from anywhere):

```csharp
public static ClickCode Instance { get; private set; }
void Awake()   // ClickCode does this in Start; Awake is the safer place (others may look for it in their Start)
{
    if (Instance != null && Instance != this) { Destroy(gameObject); return; }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}
void OnDestroy() { if (Instance == this) Instance = null; }
```

`LevelSelectScreen` had to work around `ClickCode` setting `Instance` in `Start`, by calling `FindAnyObjectByType<ClickCode>()` on the first frame.

**Static class** when nothing needs to live in a scene: `SaveManager`, `LevelSession`, `RangeRing`, `LevelSelectStyle`. Works from any scene, including when you press Play directly in a level. Remember statics survive Play sessions if domain reload is off.

**Callbacks with `Action` and lambdas:**

```csharp
tracker.Initialize(() => EnemyRemoved(wave));   // the lambda remembers `wave`
onRemoved?.Invoke();                              // call it if anyone is listening
```

**Capturing loop variables in lambdas:** `LevelSelectScreen` copies the loop variable before using it in a listener:

```csharp
foreach (LevelNode node in nodes)
{
    LevelNode clicked = node;
    node.button.onClick.AddListener(() => popup.Open(clicked));
}
```

With `foreach`, C# gives a fresh variable per iteration, so the copy is just extra safety; with a `for (int i…)` loop the copy is **required**, or every listener sees the final `i`.

**Properties:**

```csharp
public bool IsDead => currentHealth <= 0f;                 // computed, read-only
public float maxHealth { get; private set; }               // anyone reads, only this class writes
public static IReadOnlyList<Mine> All => all;              // expose a list without letting others change it
```

**`const`, `readonly`, `static readonly`:** `const string FileName = "save.json";` (compile-time), `static readonly Collider[] hits = new Collider[64];` (one shared buffer, the reference can't be replaced but the contents can change).

**Collections:** `List<T>` (`Add`, `RemoveAll(pred)`), `Dictionary<K,V>` (`TryGetValue(key, out v)`, `ContainsKey`), `HashSet<T>` (`Add` returns `false` if already present: perfect for "damage each enemy once"), arrays for fixed sizes and NonAlloc buffers.

**`out` parameters:** `Physics.Raycast(ray, out RaycastHit hit)`, `dict.TryGetValue(key, out var value)`, `int.TryParse(text, out int cost)`. The method must fill them in.

**Null operators:** `FindRecord(level)?.bestStars ?? 0` (if the record is null, use 0). ⚠️ Don't use `?.` or `??` on **UnityEngine.Object** references that might be destroyed: they use the plain C# null check and skip Unity's "destroyed counts as null". Use an explicit `!= null` for Unity objects.

**Pattern matching:** `tower is UnityEngine.Object obj && obj != null`.

**Strings:** interpolation `$"Wave {n} / {total}"`, formatting `string.Format("{0:00}:{1:00}", min, sec)`, regex `Regex.Replace(text, @"\D", "")` (keep digits only), `string.Join(", ", list)`.

**Enums with meaning:** `enum Temperament { Annoyance = 1, Anger = 2, Violence = 3, Bloodlust = 4 }` so `(int)temperament` *is* the star count. Change the rule in one place.

**`switch`** on strings/enums for IDs (`"turret"`, `"aoe"`, `"spellTower"`, `"landMine"`).

**`%` for wrap-around:** next barrel `(i + 1) % count`, next tip `(current + 1) % tips.Length`.

**Error handling:** `try { … } catch (Exception e) { Debug.LogWarning(...) }` around loading the save (start fresh if it's corrupt). `try { … } finally { … }` in coroutines to always fix counters.

**`nameof(Method)`** instead of a string for `InvokeRepeating`, so renames don't silently break it.

**Preprocessor directives:** `#if UNITY_EDITOR`, `#if UNITY_WEBGL && !UNITY_EDITOR`, `#if ENABLE_INPUT_SYSTEM`. Code inside a false branch isn't compiled at all, so test both builds (we compiled the WebGL branch separately to check it).

### 8.2 Unity math we used

| Need | Code |
| --- | --- |
| Move toward a point at a fixed speed | `Vector3.MoveTowards(pos, target, speed * Time.deltaTime)` |
| Blend two points | `Vector3.Lerp(a, b, t)` (t 0..1) |
| Arc between two points, peak height h | `Lerp(a, b, t) + Vector3.up * (4f * h * t * (1f - t))` |
| Compare distances cheaply | `(a - b).sqrMagnitude < r * r` |
| Flat (ground) distance | zero the Y component first |
| Face a direction | `Quaternion.LookRotation(dir)` |
| Turn smoothly | `Quaternion.RotateTowards(current, target, degPerSec * Time.deltaTime)` |
| Angle from a vector | `Mathf.Atan2(y, x) * Mathf.Rad2Deg` |
| Rotation from angles | `Quaternion.Euler(x, y, z)`; "no rotation" = `Quaternion.identity` |
| Wobble/bob | `Mathf.Sin(Time.time * speed) * amount` |
| Clamp, round | `Mathf.Clamp`, `Clamp01`, `Max`, `Min`, `RoundToInt`, `FloorToInt`, `CeilToInt` |
| Random | `Random.Range(0, n)` (int: max **exclusive**), `Random.Range(a, b)` (float), `Random.insideUnitCircle` |
| Points on a circle | `new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius`, `a = i * 2π / count` |

**Unity is left-handed, +Y up, +Z forward.** A positive X rotation tips +Z **down**, which is why the barrel pitch is negated.

### 8.3 Code-quality lessons

- **Comments go stale.** The WaveManager comments written on Nov 4 described code that was rewritten by Nov 9, and some described the old "wait for clear" behavior after it was removed. Update comments in the same commit as the code.
- **Show the diff before applying sweeping changes** (we drafted comments in a scratch copy and compared "code with comments stripped" to prove nothing else changed).
- **Keep line endings** (CRLF) when editing team files, so diffs show only real changes.
- **Typos in serialized names are forever-ish:** `turrentUpgrades`, `notificationDuriation` ("Keep the original field name so existing Inspector settings are preserved"), `Manu`, `Bass`, `Adio`, `discription`. Renaming loses saved data unless you migrate it (`[FormerlySerializedAs]`, or a save `version` bump).
- **Coding standards** from the pre-production milestone: camelCase locals, PascalCase classes and methods, one-line getters, comment anything non-obvious.
- **Leftover debug output is a bug.** `BaseHealth.TakeDamage` logs a warning with a full stack trace on every hit; `EnemyMovement` warns every time an enemy reaches the base; `CurrencyManager` logs every coin change. Fine while debugging, noisy in a build.

### 8.4 Spot-the-bug exercises (found while writing these notes, *unverified in Play mode*)

Good practice for reading code. Each is small:

1. **`EyeBall Movement.cs`**: `Mathf.Sin(Time.time * hoverHeight) * hoverHeight` uses the *height* as the *speed*; `hoverSpeed` is never used.
2. **`FireBallImpact.cs`**: `if (elapsed > -duration) Destroy(gameObject);` is true on the first frame (elapsed ≥ 0 > −0.8), so the fade never plays. It should be `elapsed > duration`.
3. **`BulletEnemyTest.cs`**: `Update` always ends with `Destroy(gameObject)`, so the test bullet lives one frame.
4. **`PortalAnimation.cs`**: `IsOpen => currentSize <= 0.001f` is true when the portal is *closed*, and `IsClosed => currentSize <= 0.999f` is true almost immediately after it starts closing. The names don't match what they check.
5. **`Gun.cs` `Start()`**: reads `SaveManager.GetTowerUpgrades(shopTowerId)` but never multiplies `damage`, so the permanent shop upgrade does nothing for gun towers (MineTower does apply it).
6. **`SpellSpawner`** creates its preview ring with `new GameObject(...)`. New objects go into the *active scene*, so they're probably destroyed on every scene load even though the spawner survives under ClickCode, which would explain the `if (previewRing == null) RestGameObject();` re-creation in `Update`. Parenting the ring to the spawner would avoid it.

---

## Part 9 — Open issues when the project ended

Known as of Nov 16–18; none blocked the Beta turn-in.

| Issue | Details |
| --- | --- |
| **Temperament doesn't change difficulty** | It only changes the star reward. `WaveSpawner` has health/speed multipliers ready; nothing sets them from `LevelSession.Temperament`. Bloodlust plays like Annoyance for 4 stars |
| **Shop tower upgrades probably do nothing** *(from reading the files)* | `SaveManager` only recognises the IDs `turret`, `aoe`, `spellTower`, `landMine` (exact, case-sensitive). The prefabs use `turret` (Turret, TurretEnhanced), `Gun2` (TurretUpgrade, TurretMax), `AoE` (Mortar, MortarEnhanced; capital letters don't match `aoe`), `mortar2`, `MagicTower1/2` and `MineTower1/2`, so almost every lookup returns 0. On top of that, `Gun.Start` reads the count but no longer applies it; only `MineTower` applies the bonus, and its IDs don't match. Fix: one agreed ID per tower line on every tier, and put the `damage *= 1f + upgrades * 0.2f` line back in `Gun.Start` |
| **EasyLevelWirer scale** *(unverified)* | Root scale 0.18 likely makes spawned enemies ~5.6× too big; `Level Package Test` has two WaveSpawners |
| **Ghost/Little Devil/eyeball colliders vs mines** | Not measured; whether flying enemies should trigger mines wasn't decided |
| **Development build shipped** | Rebuild as release for the next milestone |
| **Save file safety** | A crash mid-save can corrupt `save.json` (write temp + replace) |
| **Startup menu guard** | `ClickCode.Start` doesn't force Main Menu; a panel left active in the editor ships active |
| **Duplicated price data** | Build price in labels vs `towerCost` on prefabs vs `upgradeCost`; easy to drift |
| **Level 1 enemy stats as scene overrides** | Other levels use prefab stats; consider prefab variants with the tuned values |
| **Long JSON delays (100–108 s)** | Possibly typos; inflate the early-call reward |
| **`master` branch** | Last updated Oct 15; merge `Dev` for the portfolio |
| **5 Path Map leftover SpellManager**, **level 1 SpellManager** | Harmless leftovers (no buttons) |
| **Level select builder vs hand edits** | The builder no longer matches the hand-edited map; re-running it would wipe ~112 overrides |
| **Asset trim** | On branch `versioncontrol` / tag `Trimmed`, not merged |
| **Unlock chain** | Levels 2–4 and Key A were set to open from the start (Required Levels emptied); a team decision whether to restore the 1 → 2 → 3 chain |

---

## Part 10 — Checklist for the next Unity project

### Day 1 setup

- [ ] Pick the Unity version as a team and install the same one (plus any build modules you'll need, e.g. Web).
- [ ] Use Unity's GitHub `.gitignore`; add `/Data/`, `UIElementsSchema/`, `*.orig`, merge-tool leftover patterns. Set up Git LFS for audio/video/zips.
- [ ] Decide **Enter Play Mode** settings up front (Reload Domain and Scene unless everyone understands static resets).
- [ ] Set Company/Product name, default resolution, Canvas Scaler conventions (Scale With Screen Size 1920×1080, Match 0.5).
- [ ] Folder rules: all scripts in `Assets/Scripts/<Feature>/`, editor scripts in an `Editor` subfolder, own prefabs in `Assets/Prefabs`, art in clearly named folders.
- [ ] Create layers you'll query (Enemy, Ground) and a `Default`-only rule for towers/props.
- [ ] Make a **boot scene** and test everything from it. Add a "start on main menu" guard in code.
- [ ] Keep a running **asset credits/licenses** list.
- [ ] Merge `Dev` into `master` at every milestone.

### Every feature

- [ ] Interfaces for things many systems talk to (damageable, selectable, upgradable).
- [ ] Data in assets (ScriptableObjects, prefabs, JSON), not in scene overrides.
- [ ] One source of truth for every number (prices, stats).
- [ ] Colliders that fit the gameplay shape; check them with Gizmos in Play mode.
- [ ] Error messages that name exactly what's missing and pass `this`.
- [ ] Shared systems (HUD, spells, audio) live once in the persistent scene, not copied per level.
- [ ] Editor cheat tools for testing progression.
- [ ] Remove debug logs before committing.

### Every commit / merge

- [ ] Save in Unity; read `git status`; discard generated junk (TMP dynamic fonts etc.).
- [ ] Don't commit unexplained deletions.
- [ ] After a merge: scan for conflict markers, open Unity, Console clean, Play from the boot scene.
- [ ] Tell the team when you touch their files or shared settings.

### Every build

- [ ] Scene list complete, boot scene first.
- [ ] Release build (no Development Build / Autoconnect Profiler) for anything handed in.
- [ ] New empty output folder; don't ship `*_DoNotShip`/`BackUpThisFolder`.
- [ ] Test the built exe from its main menu through every level.
- [ ] WebGL: platform-specific saves and quit, compression/fallback, canvas size, quality level, test in a hosted page.
- [ ] Installer tested: install → Start menu launch → uninstall.

---

## Part 11 — Glossary

| Term | Meaning |
| --- | --- |
| **Additive blending** | Colors add together instead of covering each other; overlapping particles glow brighter |
| **Animator / Animator Controller** | Component + state machine that plays animations; driven by parameters (`IsMoving`, `Die`) |
| **Asset** | Any file in `Assets/` Unity imports (model, texture, prefab, scene, script, material, ScriptableObject) |
| **AssetDatabase** | Editor-only API for finding, loading and creating assets from code |
| **AssetPostprocessor** | Editor script that runs during import to set import settings automatically |
| **Awake / Start / Update / LateUpdate** | MonoBehaviour lifecycle callbacks (see 2.6) |
| **Bloom** | Post-processing glow around pixels brighter than a threshold |
| **Build Profile** | Unity 6 per-platform build configuration (scene list, platform settings, optionally its own Player Settings) |
| **Brotli / Decompression Fallback** | WebGL compression format / a JS decompressor so the build loads on servers that don't send the right header |
| **Canvas / Canvas Scaler** | Root of uGUI / component that scales UI to the screen size |
| **Collider / Trigger** | Physics shape / a collider that detects but doesn't block |
| **Coroutine** | A method that can pause (`yield return`) and resume later; started with `StartCoroutine` |
| **Domain reload** | Unity reloading all scripts (and resetting statics) when entering Play mode or after a recompile |
| **DontDestroyOnLoad (DDOL)** | Keeps a GameObject alive across scene loads |
| **DSP buffer** | Audio mixing buffer size; bigger = more latency |
| **Emission (material)** | Self-lit color; with HDR intensity > 1 and Bloom it glows |
| **Emission (particles)** | How many particles spawn (rate over time/distance, bursts) |
| **EventSystem** | Scene object that routes mouse/keyboard input to UI; required for buttons to work |
| **FBX** | 3D model file format; read-only in Unity apart from import settings |
| **Gizmos** | Scene-view-only debug drawings (wire spheres, lines, collider outlines) |
| **GUID** | Permanent asset ID stored in the `.meta` file; what references actually use |
| **HDR color** | Color with intensity above 1 (for emission/bloom) |
| **IL2CPP** | Unity's C#→C++ compiler used for WebGL (and optionally other platforms) |
| **Inno Setup** | Free tool that builds Windows installers from an `.iss` script |
| **Instantiate / Destroy** | Create a copy of a prefab/object at runtime / remove an object (and its children) |
| **Kinematic Rigidbody** | A Rigidbody moved by code, not by gravity/forces |
| **Layer / LayerMask** | A GameObject's category / a filter of categories for physics queries |
| **LineRenderer** | Draws a line through points in the world (range rings, lightning) |
| **`.meta` file** | Sidecar file holding an asset's GUID and import settings; always commit it |
| **MonoBehaviour** | Base class for scripts you put on GameObjects |
| **NonAlloc** | Physics query versions that write into your array instead of allocating a new one |
| **Override (prefab)** | A change on a prefab instance, stored in the scene, not in the prefab file |
| **Persistent data path** | Per-user folder for save files (`AppData/LocalLow/<Company>/<Product>` on Windows) |
| **Pivot** | An object's origin; enemy pivots were at their feet |
| **Play mode** | Running the game inside the editor; scene changes made during it are lost |
| **PlayerPrefs** | Small key/value storage (registry on Windows, browser storage on WebGL) |
| **Prefab / Prefab Mode / Variant** | Reusable GameObject asset / editing it in isolation / a prefab that inherits from another |
| **Raycast / Raycast Target** | Shoot an invisible line to find colliders / the UI setting that makes an element catch clicks |
| **Render pipeline (URP)** | The rendering system; URP needs URP-compatible shaders (else magenta) |
| **Resources folder** | Assets here can be loaded by path at runtime (`Resources.Load`) and are always included in builds |
| **Rigidbody** | Component that makes an object physical |
| **ScriptableObject** | A data asset defined by a script, created in the Project window |
| **Selectable** | Base class of interactive UI (Button, Slider, Toggle) with Normal/Highlighted/Pressed/Selected/Disabled states |
| **Serialization** | Saving fields into scenes/prefabs/assets (and JSON); matched by field name |
| **Simulation Space** | Whether particles move with their emitter (Local) or stay in the world (World) |
| **Singleton** | Exactly one instance reachable through a static property |
| **Spline / SplineContainer** | Curved path / the component holding it; evaluated with t from 0 to 1 |
| **Sprite** | 2D image asset used by UI Images |
| **Temperament** | Skyline Rush's name for difficulty (Annoyance, Anger, Violence, Bloodlust) |
| **TextMeshPro (TMP) / font asset / dynamic font** | Text system / the generated font atlas / a font asset that adds glyphs at runtime (and changes on disk) |
| **timeScale** | Global game speed; 0 pauses scaled time |
| **Transform** | Position, rotation, scale, and the parent/child link |
| **UV** | 2D coordinates on a mesh saying where it samples its texture |
| **uGUI** | Unity's GameObject-based UI system (Canvas, Image, Button) |
| **WebGL** | Browser build target |

---

## Part 12 — Quick reference

### 12.1 Editor shortcuts

| Keys | Action |
| --- | --- |
| Ctrl+P | Play / stop |
| Ctrl+Shift+P | Pause |
| Ctrl+S | Save scene (or the open prefab) |
| Ctrl+D | Duplicate |
| F2 | Rename |
| F | Frame selected in Scene view |
| Ctrl+R | Refresh assets (pick up outside changes) |
| Ctrl+Z | Undo (doesn't undo asset deletion) |
| Q / W / E / R / T | Hand / Move / Rotate / Scale / Rect tool |
| `` ` `` (over Scene view) | Overlay menu |

### 12.2 Menu paths we used

| Path | For |
| --- | --- |
| File → Build Profiles | Platforms, scene list, Build / Build And Run, Switch Platform |
| Edit → Project Settings → Editor → Enter Play Mode Settings | Domain/scene reload |
| Edit → Project Settings → Player | Company/product name, resolution, Web publishing settings |
| Edit → Project Settings → Quality | Quality level per platform |
| Edit → Project Settings → Audio | DSP Buffer Size |
| Edit → Project Settings → Tags and Layers | Enemy layer |
| Edit → Preferences → Colors → Playmode tint | Play mode color |
| GameObject → Effects → Particle System | Particle system with a default URP material |
| GameObject → Volume → Global Volume | Post-processing (Bloom) |
| Right-click in Project → Create → Material / Skyline Rush → Level Data / Audio → Sfx Event | New assets |
| Tools → Skyline Rush → Build Level Select | Rebuild the level select (wipes hand edits!) |
| Tools → Skyline Rush → Save Data → Print Progress / Open Save Folder / Delete Save File | Save testing |
| Right-click a Level Data asset → Skyline Rush → Test: Win on … | Fake a win |
| Unity Hub → Installs → ⚙ → Add modules | Add Web Build Support etc. |

### 12.3 Important paths

| What | Where |
| --- | --- |
| Project | `C:\Users\noahd\source\TheKillerWhales` |
| Save file | `C:\Users\noahd\AppData\LocalLow\TheKillerWhales\SkylineRush\save.json` |
| Editor log | `<project>\Logs\Editor.log` (previous: `Editor-prev.log`) |
| Wave data | `Assets/Resources/SingleLineWaveData.json`, `MultiPathWaveData.json`, `5PATHS.json` |
| Button click sound | `Assets/Resources/Audio/UI Click.asset` |
| Level Data assets | `Assets/Scenes/LevelSelect/Levels/` |
| Build profiles | `Assets/Settings/Build Profiles/` |
| Builds, installer script, turn-in | `<project>\Build\` (gitignored), `Build\SkylineRush.iss`, `Build\TurnIn\` |
| Inno Setup compiler | `%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe` |
| WebGL page | <https://ndhardy395.itch.io/skyline-rush> |
| UI mockups | <https://claude.ai/artifact/ECFAhW2eBaycBUww5gsrS7> |
| Repo | github.com/ndhardy395/Team_Project_Project_Portfolio_2 |

### 12.4 Git cheat sheet (Unity flavored)

```bash
git status                                   # always first
git pull                                     # before starting, before pushing
git restore "<file>"                         # throw away local changes to one file
git stash / git stash pop                    # park work to let a pull through
git merge --abort                            # back out of a bad merge
git checkout --theirs -- "<file>"            # take the incoming side whole
git rm "<file>"                              # accept a deletion in a conflict
git commit --no-edit                         # finish a merge with the default message
git grep -n -E "^(<<<<<<<|>>>>>>>) " -- Assets ProjectSettings   # leftover markers?
git checkout <commit>^ -- "<path>"           # restore a deleted file/folder (same GUIDs)
git log --diff-filter=D -- "<path>"          # who deleted it?
git log --all --no-merges --since='<date>' --author=<name> --output=commits.txt
git show --remerge-diff <merge>              # how were conflicts resolved?
```

### 12.5 Numbers worth remembering from Skyline Rush

| Thing | Value |
| --- | --- |
| Tower build prices (Turret / Mortar / Magic / Mine) | 75 / 195 / 215 / 290 |
| Sell refund | 70% of everything spent on the spot |
| Early-call reward | floor(seconds skipped) × 3 coins |
| Forced-wave cooldown | 0.75 s |
| Shop: Ice / Lightning unlock | 2 / 4 stars |
| Shop: each upgrade (towers and spells) | (n+1) × 2 stars, max 2 upgrades |
| Tower shop bonus | +20% damage per upgrade |
| Mine trigger radius | 1 unit (collider must reach within 1 unit of the path) |
| Base lives | 20 |
| SFX voices | 24 |
| Canvas reference | 1920×1080, Match 0.5 |
| WebGL canvas | 1280×720 |
| Installer | 370 files / 290 MB → 77 MB |
| Asset trim | 734 MB → 231 MB (1,135 files) |
| Commits | 323 (La'Rue 104, Logon 81, Noah 90 across three author names, Michael 48) |

---

## Appendix A — Week-by-week timeline

### Week of Sep 29 — Repo

- Repo created, README with team members, first PR from `Dev` to `master`.

### Week of Oct 6 — Assets

- Tower/gun asset pack added. First lesson: `.meta` files belong in git.

### Week of Oct 13 — First playable pieces

- `.gitattributes` with Git LFS.
- Michael: waypoint pathing from demos, multiple spawns ("1 MILLION ENEMIES, HERE I COME!").
- You: turret aiming and bullets, "a working gun" (test capsule enemies, raycast bullets, yaw/pitch aiming).
- La'Rue: enemy assets, animations on waypoints. Logon: main menu.
- Oct 15: merge pain ("I HATE GIT"), `.gitignore` refactor, **splines replace waypoints**.
- Oct 16–18: enemy selection UI, health and death effects (La'Rue); pause menu (Logon); Hero warplane, Gun prefab and `Prefabs` folder (you); forks and multiple paths (Michael); tower spawning on bases (Logon); **Enemy layer, `IDamageable`, target-the-enemy-in-front with `IPathProgress`**, tower HUD canvas anchoring/scaling (you).

### Week of Oct 20 — Economy and waves

- Spawn portal (La'Rue). Towers disappear after a level, tower HUD linking, restart (Logon).
- Currency drops, auto pickup, currency display (La'Rue).
- **JSON wave system** (Michael, Oct 23), then multiple simultaneous groups.
- Towers cost money (Logon); enemies-left indicator (La'Rue); Next button (Logon).
- Oct 26: Pre-production docs (coding standards, GDD) and **UI mockups** from the whiteboard.
- Oct 27: win/lose condition (La'Rue).

### Week of Oct 27 — Alpha Sprint

- Oct 28: blank-scene merge disaster and restore; UnityYAMLMerge removed; base health display; **Gun_1 → Gun_2 upgrade + range ring**; **mortar** with arc and splash; save system and Level Data; Tower 2 button.
- Oct 30–31: working shop (La'Rue); Michael's preliminary HUD and multi-path wave manager; **GunMortar2 rocket** with reload, nose-first flight, exhaust and explosion.
- Nov 1: wave spawner on other maps and the **5 Path Map** (Michael); currency/lives display and button colors (La'Rue); **magic towers 1 and 2**; deleted-and-restored Prefabs folder; Play Mode settings fixed; first exe build checklist.
- Nov 2: **level select** generated from the whiteboard + mockup.

### Week of Nov 3 — Alpha Sprint 2

- New enemies and bosses with code animation (La'Rue).
- Nov 4: Next skips anytime + **no-win-screen counter bug** fixed; timer (Logon); wave-forcing cooldown and time-left reward (Michael); shop update; **bullet smoke trails**, spider colliders, Little Devil fix; minotaur material conflict markers fixed; scripts moved into `Assets/Scripts`; code review of the level select.
- Nov 5: affordable/unaffordable tower indicators (La'Rue).
- Nov 7: stars usable + fireball mechanic (La'Rue); exponential HP scaling (Michael); **first playable decision** (3 levels, Annoyance only, Coming Soon).
- Nov 8: 50-wave level, 4 tiers for every tower (Michael); level 2 and level 3 / 5 Path Map (La'Rue); **mine towers**, temperament lock, merge compile break and fix, wave-24 JSON crash fix.
- Nov 9: fireball fixes; comments on Mine/MineTower/WaveManager; **linear HP scaling**; final screen (Logon).

### Week of Nov 10 — Beta Sprint

- Nov 10: ice and lightning spells, freeze fix (La'Rue); accidental `Assets/Material` deletion.
- Nov 11: spells moved into the HUD (Logon); spells unlocked with stars (La'Rue); balancing and wave-launch timers (Michael); materials restored; shop main-menu bug fixed (you).
- Nov 13: consistent scene colors; shop preview photos and spell upgrades (La'Rue); **full SFX system and sounds** (you); countdown-after-last-spawn change.
- Nov 14: harder balance (Michael); click-off-to-close, camera shake, game-over screen fix, JSON comment lesson, level 2 enemies (La'Rue); **EasyLevelWirer** (Michael); `LevelSelect.prefab` accidentally deleted.
- Nov 15 (Beta crunch): enemy descriptions and **tips bar** (La'Rue); coin values, death sounds, menu SFX fix, **mine colliders**, sell button (you + Logon); credits and asset credits (Logon); **asset trim branch**; **volume slider**; "Ok this is it no more adding or trying to balance stuff" (La'Rue); credits-on-startup fix; **WebGL** prep, profiles and builds; two new levels incl. 2× speed (Michael); **Beta turn-in**: installer, itch.io WebGL, video, zip.
- Nov 16: final pulls and the `Color shop.asset` conflict cleanup.

---

*Built from the Skyline Rush repo, its history and our working sessions. When something in a future project feels familiar, search this file for the symptom first.*
