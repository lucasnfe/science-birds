# ScienceBirds
A cover version of the "Angry Birds" game used for research purposes.

![Alt text](/Docs/Screenshot.png?raw=true "Optional Title")

## Game Objects

The original Angry Birds has several game objects: birds, pigs, blocks, TNTs and other miscellaneous
objects used mainly for decoration. There are several types of birds and they vary in colour and size.
Some of them have special abilities that are triggered by touching the screen after a shot. The game also
has different types of pigs, each of which having different size and "health" points. Blocks have
different materials, what impact in their physical properties and "health points". TNTs are used to cause
explosions that deal damage in area, affecting several blocks and pigs. All these objects are placed on a
terrain that can be either completely flat or complex. Science Birds currently supports only part of these
objects: 

- **Birds**
  - **Red**: Regular bird, no special abilities.
  - **Blue**: Splits into three birds when clicked, strong against ice blocks.
  - **Yellow**: Shoots forward at high speed when clicked, strong against wood blocks.
  - **Black**: Explodes when clicked or after impact, strong against stone blocks.
  - **White**: Drops explosive egg when clicked.
- **Pigs**: 
  - **Small**
  - **Medium**
  - **Large**.
- **Blocks**: 
  - **ice**
  - **stone**
  - **wood**.
- **TNT**
- **Terrain**

All these objects can be seen in the level shown in the figure above. It has three blocks of each material, three pigs, a TNT block and five birds (one of each type). Moreover, it has two rows of static square platforms floating in the air. 

## Level Representation

As mentioned before, levels are represented internally using a XML format. This format is basically
composed by a number of birds and a list of game objects, as shown in Figure 3. Each game object has
four attributes: 

- **Type**: unique string representing the id of the object. 
- **Material**: string defining the material of a block. Valid values are only "wood",
"stone" and "ice".
- **Position**: (x,y) float numbers representing the position of the game object. The origin (0,0) of the
coordinates system is the centre of the level.
- **Rotation**: float number that defines the rotation of the game object. 
