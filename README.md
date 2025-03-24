# $\beta$-boost
### _Special Relativity Simulator in Unity_

## Description
This project aims to allow anyone to experience the effects of special relativity, well
below the extreme velocities required in reality. By constructing a special relativity
simulation within the context of a computer game, the player of the game will get a
better intuition of some notable relativistic effects such as length contraction, Lorentz
boosts, coordinate transformations and more.

## Content
1. `PlayerController.cs` - Controls the playable character. Based on the following player controller from Unity: [Starter Assets - ThirdPerson](https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526?srsltid=AfmBOorK0biqhhL-1w10QyQ8-k_B-XmCcJ50nZA-1Nb8Vv9RkoAjo4BW)
2. `LorentzTransform.cs` - Contructs the Lorentz transformation matrix from the player's velocity parameters.
3. `VertexManager.cs` - Applies a Lorentz boost to mesh vertices for relativistic effects.
4. `MinimapScript.cs` - Handles player control, physics, and camera.
5. `SpeedDisplay.cs` - Displays the player's speed, $\beta$-factor and Lorentz factor on the UI.
