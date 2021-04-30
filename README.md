# Cubism

This is me showcasing a few basic mechanics mechanics using SpatialOS and Unity.  
The [blank SpatialOS started project](https://github.com/spatialos/gdk-for-unity-blank-project) was used as a boilerplate (so the Unity project itself is in _workers/unity_).  

![](docs/demo.gif)

## Controls

_Space_ - jump  
_Space while in the air_ - smash  

## Technical aspects

### Network-synchronized animator state

The player character uses an `Animator`-based state machine (for the delayed jump animation) that's synched via a SpatialOS component.
Implemented in _Assets/Scripts/Entities/PlayerStateMachine.cs_ and _Assets/Scripts/Input/PlayerMovementControl.cs_

### Slowly-interpolated follow movement

The player character gradually rotates towards the requested movement direction, meanwhile moving foward in the direction it's facing now, akin to a car.  
Implemented in _Assets/Scripts/Input/PlayerMovementControl.cs_

### Server-side dynamic spawn system

Shapeshifting objects are spawned at random locations with random delays after the server's start. They drift around within the bounds specified on spawn.  
Implemented in _Assets/Scripts/Systems/ShapeshifterSpawnSystem.cs_

## Messy implementations

### Player creation flow changes

A networked game should be able to authentice the connecting player and load their data from some external store, but by default the `PlayerLifecycle` creates a player object without any external input - meaning it doens't support an `async` form of the `CreatePlayerEntityTemplate` delegate. So i've made a modified `AsyncHandleCreatePlayerRequestSystem` (_Assets/Scripts/Systems/AsyncHandleCreatePlayerRequestSystem.cs_) and unpacked the part linking it (_Assets/Scripts/Workers/UnityGameLogicConnector.cs_). It sends a dummy `SessionKey` parameter which in read in the `CreatePlayerEntityTemplate` server-side method.  
_Note: for stability, the whole `PlayerLifecycle` should be unpacked, in case some update will change parts outside the modified System._  

### Customized Universal Rendering Pipeline shader

URP doesn't seem to support partial modifications to it's vertex/fragment shader parts, so i've re-stitched parts of the original `Lit` shader and modified the vertex part to smoothly morph an object between it's original shape and the sphere (_Assets/Shaders/Entities/MorphingLit.shader_).  
_Note: in case multiple different modifications of the standard URP shader will be required, it might make sense to refactor the unpacked code into a more combinable set of blocks._  
