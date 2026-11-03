# Warplane

A drop-in warplane, exported unpainted from Noah's Dogfighting project. It loops a route of waypoints at a fixed height, with a spinning prop, banking turns, moving ailerons, elevator, rudder and flaps, guns that overheat (the barrels glow red), bombs that reload, wake wisps, and it breaks apart and crashes when enemies shoot it down.

Everything lives in this folder and in the `Warplane` namespace, so it can't clash with the project's other scripts (`Bullet`, `Gun`...).

## Set up (once)
1. **Tools ▸ Warplane ▸ Build Plane** builds the plane at its set size:
   - `Generated/Prefabs/Warplane.prefab`, plus its bullet, bomb and explosion prefabs.
   - `Data/WarplaneSettings.asset`, which holds every number.
2. Open your scene and run **Tools ▸ Warplane ▸ Add Plane And Route To Scene**.
   - It adds a plane and a "Warplane Route" with four waypoints, centered where the Scene view is looking, 6 units above the ground.
   - Drag the waypoints to reshape the loop; add or remove children to change its shape. Only X and Z matter.
   - Save the scene.
3. Press **Play**. The plane loops the route at the height it was placed at.
4. To try the guns, bombs and damage before your targeting scripts exist, add **Warplane Test Controls** to the plane:
   - **F** (hold) fires at the ground under the mouse.
   - **B** drops a bomb.
   - **K** deals 25 damage. The 2nd hit breaks a wingtip, the 3rd the other one, and the 4th shoots it down.
   - Remove it once your scripts take over.

## Drawing routes (in Play)
- Needs a **Warplane Route Drawer** in the scene. **Add Plane And Route To Scene** adds one; for an existing scene, use **Tools ▸ Warplane ▸ Add Route Drawer To Scene**.
- **To draw:** press the left mouse button on a plane (or close to it), drag a line over the map and let go. The plane switches to that line, joining it at the nearest point.
- **Loop or back-and-forth:** end the line near where it started and it becomes a loop. Otherwise the plane flies it back and forth, swinging wide at each end to turn around.
- **Red sections** are bends tighter than the plane can turn; it cuts those corners. How tight it can turn comes from Speed, Turn Rate and Turn Slowdown in the settings.
- **Height:** the line is read at the plane's flying height, so the plane flies exactly over what you drew on screen.
- **Fireball clicks:** so pressing on a plane doesn't also drop a fireball, the fireball spawner checks first:
  `if (Warplane.RouteDrawer.IsOverPlane(mousePosition)) return;`
- **Tuning:** pick radius, point spacing, shortest line and loop-closing distance are under **Drawing routes**. Line width and colors are under **Routes in the game**.

## Driving it from your scripts
```csharp
using Warplane;

WarplaneGuns guns = plane.GetComponent<WarplaneGuns>();
guns.FireAt(enemy.transform);          // call every frame you want it shooting, like holding a trigger
bool ready = guns.CanFire;             // false while overheated (or shot down)

WarplaneBombs bombs = plane.GetComponent<WarplaneBombs>();
if (bombs.BombsReady > 0 && bombs.PredictLanding(out Vector3 landing)
    && Vector3.Distance(landing, enemy.position) < 1.5f)
    bombs.Drop();                      // a new bomb appears on that hardpoint a few seconds later

WarplaneHealth health = plane.GetComponent<WarplaneHealth>();
health.TakeDamage(10f);                // enemy attacks; SendMessage("TakeDamage", 10f) works too
health.onShotDown.AddListener(...);    // also onDamaged, onExploded, onRespawned (or hook them up in the Inspector)

WarplaneFlight flight = plane.GetComponent<WarplaneFlight>();
Vector3 v = flight.Velocity;           // e.g. for leading targets
flight.ForceFlaps = true;              // hold the flaps down (e.g. on a bombing run)
```

### Guns
- `FireAt` shoots the six wing guns at a point. The bullets fly straight at it, with no lead, so lead moving enemies yourself if needed.
- Heat:
  - About 5 s of continuous fire overheats the guns. They lock until half cool (about 1.5 s).
  - The barrels glow red to orange as they heat, and smoke while overheated.
  - An arc beside the plane shows the heat, and "OVERHEAT" flashes above it.

### Damage to enemies
Bullets and bombs call `TakeDamage(float)` on whatever they hit, through `SendMessageUpwards`. `Enemy` and `EnemyNoah` already have that method, so they just work. To limit what can be hit, set **Bullet Hit Mask** and **Bomb Hit Mask** in the settings to your enemy layer.

### Getting shot down
- Health falls to 2/3: a wingtip breaks off. The plane flies on, leaning and wobbling a little. At 1/3 the other wingtip goes.
- Health hits 0: a wing or the tail tears off. The plane spirals down, hits the ground and explodes.
- With **Respawn** on, it comes back at its start after a few seconds.

## Settings (`Data/WarplaneSettings.asset`)
- Speed, turning, bank, moving-part angles, health, gun heat, bullets, bombs, the overheat warning and the wake wisps all live here.
- **Wisp Size** is the one knob for how big the wisps are; **Wisp Length** and **Wisp Opacity** sit beside it.
- Changes work live in Play mode. The exception is **Wingspan**: after changing it, run **Build Plane** again.

## Notes
- The plane is moved by its own script (a kinematic Rigidbody), not by physics. Its colliders are there so enemies can target it.
- Each plane flies its **Route** if one is set. Otherwise it flies the nearest route in the scene, and with none at all it circles where it is.
- A route placed in the editor loops by default; untick **Closed** on it to have the plane fly it back and forth instead.
- From code, `flight.SetRoute(route)` switches routes. `route.SetPoints(points, closed)` makes a route from a list of points.
- The model came from the Dogfighting project's generator, with the stripes and insignia removed. If the model changes there, replace `Data/warplane_mesh.json` and run **Build Plane** again.
