# Kroble Pinball

![](https://github.com/timjohansen/KroblePinball/blob/master/Assets/_Textures/_CanvasElements/TitleImage.png)

## [Click here to play in your browser](https://timjohansen.github.io/KroblePinball/)

## About
Kroble Pinball is my attempt at making a flexible pinball engine with limited dependence on Unity's built-in physics. 
Flexibility is achieved by adhering to object-oriented programming principles, and by employing event-based communication 
to keep playfield objects as independent as possible. 

### Tools
This project was made within the Unity game engine using the C# language. Art assets were created in Blender and Photoshop.
No LLMs or other AI/ML tools were used for this project.

Aside from the game code, the project also required some Python scripting for Blender. The pinball physics simulation
is entirely 2D, and Unity is not able to convert 3D geometry into 2D collision shapes. My solution was to export a
series of 3D edges from Blender to a text file, and then use a second script in Unity to automatically create collision shapes from that text file when
it detected changes. This sped up development dramatically since it meant only a single click was needed to test out a change to the playfield's layout.

### Future Plans
Most of the engine's features were made with this specific game in mind, and that led to more hard-coding than I'd like, so I want to eventually make things more
generic and able to accomodate more styles of pinball. I would also like to eventually make it portable by removing all of Unity's built-in physics.

As for the game itself, there are plenty of score balancing issues to work out, and I'd love to eventually expand it into a bigger game with multiple
tables and a more polished experience overall. But...that might be a while. 

