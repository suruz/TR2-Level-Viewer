# TR2-Level-Viewer

This cross platform level viewer can load any Tomb Raider II format file. Even it loads custom level from TRLE. This viewer is playable with any unity 3D supported devices, including iPhone, iPad, Android Devices, PC, MAC.
Features: 
• It parses Tomb Raider 2 level data like models, animation, textures. Then it renders them back in unity3D.
• It uses data driven shader model instead of fixed function render pipeline. 
• Uses vertex buffer instead of separate geometry commands. 
• Original texture Id based UV mapping is replaced with single large texture atlas for performance gain. 
• In original file models were not defined in complete mesh. Model parts are stored separately. They were rearranged procedurally using attached position and rotation information. Now it represents model with skeletal transformation hierarchy in unity3D.
• Animation is represented with Unity3D animation curve.
• Includes Character Controller with animation state machine and physics. 
