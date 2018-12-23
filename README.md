# How to use SchematicToVox

`./SchematicToVox.exe --i [INPUT] --o [OUTPUT]`

[INPUT] refer to a .schematic file or .png file (mandatory)
[OUTPUT] refer to the destination path

## Options

```
-h, --help                     show this message and exit
-v, --verbose                  enable the verbose mode
-w, --way=VALUE                the way of schematic (0 or 1), default value is 0
-iminy, --ignore-min-y=VALUE   ignore blocks below the specified layer
-imaxy, --ignore-max-y=VALUE   ignore blocks above the specified layer
-e, --excavate                 delete all blocks which doesn't have at lease one face connected with air
-s, --scale=VALUE              increase the scale of each block
-t --texture                   export schematic with texture
-hm --heightmap                create voxels terrain from heightmap
  ```
