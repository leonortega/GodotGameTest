# Third-Party Assets

This project is set up to use free external assets, but every imported pack still needs a recorded source and license.

## Selected Sources

| Area | Recommended Source | License Target | Project Target |
| --- | --- | --- | --- |
| Terrain and props | Kenney `New Platformer Pack` | `CC0` | `res://assets/art/kenney_new_platformer_pack/` |
| UI panels and buttons | Kenney `UI Pack: Pixel Adventure` | `CC0` | `res://assets/ui/kenney_ui-pack-pixel-adventure/` |
| Stage music loops | HoliznaCC0 pack already copied into project | `CC0` | `res://audio/music/` |
| Retro sound effects | Mixed imported SFX library plus Kenney pack sounds | Verify local licenses before shipping | `res://audio/sfx/` and `res://assets/art/kenney_new_platformer_pack/Sounds/` |

## Source Links

- Kenney home and license: `https://kenney.nl/` and `https://kenney.nl/support`
- Kenney `New Platformer Pack`: `https://kenney.nl/assets/new-platformer-pack`
- Kenney `UI Pack: Pixel Adventure`: `https://kenney.nl/assets/ui-pack-pixel-adventure`
- OpenGameArt `Happy Chiptunes Collection`: `https://opengameart.org/content/happy-chiptunes`
- OpenGameArt `512 Sound Effects (8 bit style)`: `https://opengameart.org/content/512-sound-effects-8-bit-style`

## Current Runtime Audio Mapping

The runtime now points at files that already exist in the repo.

- Grassland music: `res://audio/music/03 HoliznaCC0 - Adventure Begins Loop.ogg`
- Cave music: `res://audio/music/06 HoliznaCC0 - Where It's Safe.ogg`
- Treetop music: `res://audio/music/06 HoliznaCC0 - Sunny Afternoon.ogg`
- Fortress music: `res://audio/music/12 HoliznaCC0 - NPC Theme.ogg`
- Core action SFX: `res://assets/art/kenney_new_platformer_pack/Sounds/`
- Variant SFX: `res://audio/sfx/Movement/`, `res://audio/sfx/General Sounds/`, `res://audio/sfx/Weapons/`, `res://audio/sfx/Death Screams/`

## Import Rules

- Prefer `CC0` when more than one pack can solve the same need.
- If a pack is `CC-BY`, add artist name and attribution text before shipping.
- Keep the original downloaded archive outside the Godot import tree if possible.
- Do not rename the upstream pack folder until the license and source are captured here.
- When extracting a few files from a larger pack, note the exact filenames used.

## Attribution Template

Use this per imported pack:

| Status | Area | Asset Pack | Author | License | Source URL | Local Path | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Imported | Music | HoliznaCC0 loop set | Holizna | Verify local pack metadata | Local files already copied | `res://audio/music/` | Assigned per stage in `AudioDirector.cs` |
