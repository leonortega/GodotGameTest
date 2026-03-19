# Reference Implementations and Engine Sources

This document records external material used to complete the Godot-oriented specs. These sources inform architecture and workflow decisions only. Product identity, art, names, levels, and assets remain original.

## Official Godot Documentation

- Godot `CharacterBody2D` class reference:
  https://docs.godotengine.org/en/stable/classes/class_characterbody2d.html
  Used to ground player and enemy controller assumptions around velocity-based 2D movement.

- Godot `InputMap` class reference:
  https://docs.godotengine.org/en/stable/classes/class_inputmap.html
  Used for action-based control definitions rather than hard-coded key handling.

- Godot `Camera2D` class reference:
  https://docs.godotengine.org/en/stable/classes/class_camera2d.html
  Used for camera follow, limits, drag margins, and smoothing decisions.

- Godot tutorial for using TileMaps:
  https://docs.godotengine.org/en/stable/tutorials/2d/using_tilemaps.html
  Used for TileMap-layered level authoring and scene-tile placement decisions.

- Godot tutorial for saving games:
  https://docs.godotengine.org/en/stable/tutorials/io/saving_games.html
  Used for save-slot and lightweight progression persistence planning.

## Repository References

- Godot official demo projects, including the 2D platformer sample:
  https://github.com/godotengine/godot-demo-projects
  Used as a reference for practical Godot scene organization and platformer gameplay composition.

- GDQuest `godot-platformer-2d`:
  https://github.com/GDQuest/godot-platformer-2d
  Used as a reference for controller decomposition, reusable scenes, and platformer-oriented project layout.

- SlayHorizon `godot-platformer-template`:
  https://github.com/SlayHorizon/godot-platformer-template
  Used as a reference for a Godot 4 platformer template with shared systems such as cameras, input handling, and decoupled communication patterns.

## Use Policy

- External repositories are pattern references, not implementation sources.
- No Nintendo, Mario, or other third-party IP is to be copied into the game requirements.
- All player, enemy, world, story, visual, and audio content shall remain original to `Super Pixel Quest`.
