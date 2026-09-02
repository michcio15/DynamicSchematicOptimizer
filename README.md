# What is it?

This is a ProjectMER optimizer for schematics that are meant to be animated / moved.

## Why shouldn't I just use MERO?

Because it's not a replacement for MERO, more like an extension. DSO also has a MERO compatibility that can be enabled
in the Config.yml file. Enabling it will make the plugin optimize schematics that are excluded from MERO.

# How to use it?

You need to have permissions from the Config.yml file and in RA or server console run
`optimizer create [schematic name]` and it will create a config file in the `LabAPI\configs\DynamicSchematicOptimizer`
path. Then you need to run `optimizer r` so the config will be applied to the server. Also the plugin has culling so
that the schematics won't be shown 24/7.

## What if I want to animate an optimized schematic?

You will need to animate **ONLY EMPTY OBJECTS** since they stay on the server side.

# Available commands

- `optimizer create [schematic name]` — Creates a config file for the schematic.
- `optimizer reload` — Reloads the configs.
- `optimizer culling` — Works the same as `.cullinginfo`
- `optimizer culling s` — Shows culling bounds of the schematic.
- `optimizer info` — Prints the number of client-sided and server-sided schematics for optimized schematics.
- `optimizer info [schematic name]` — Prints % of client-sided and server-sided schematics for the schematic.