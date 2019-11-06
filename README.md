# Science Birds
A cover version of the "Angry Birds" game used for research purposes.

<p float="left">
  <img src="/Docs/MainMenu.png" width="210" />
  <img src="/Docs/LevelSelect.png" width="210" />
  <img src="/Docs/Level1.png" width="210" />
  <img src="/Docs/NextLevel.png" width="210" />
</p>

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

![Alt text](/Docs/Level1.png?raw=true "Level 1")

```
<?xml version="1.0" encoding="utf-16"?>
<Level>
  <Camera x="0" y="-1" minWidth="15" maxWidth="17.5">
  <Birds>
  	<Bird type="BirdRed"/>
  	<Bird type="BirdRed"/>
  	<Bird type="BirdBlue"/>
  </Birds>
  <Slingshot x="-5" y="-2.5">
  <GameObjects>
    <Block type="RectMedium" material="wood" x="5.25" y="-3.23" rotation="0" />
    <Block type="RectFat" material="wood" x="5.22" y="-2.71" rotation="90.00001" />
    <Pig type="BasicMedium" x="5.21" y="-2.03" rotation="0" />
    <TNT type="" x="3.21" y="-4" rotation="0" />
  </GameObjects>
</Level>
```

## Citing this Work

If you use this clone in your research, please cite:

```
@inproceedings{ferreira_2014_a,
    author = {Lucas Ferreira and Claudio Toledo},
    title = {A Search-based Approach for Generating Angry Birds Levels},
    booktitle = {Proceedings of the 9th IEEE International Conference on Computational Intelligence in Games},
    series = {CIG'14},
    year = {2014},
    location = {Dortmund, Germany}
}
```
