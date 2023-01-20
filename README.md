

<b>Features: </b>
- It parses Tomb Raider 2 level data like models, animation, textures. Then it renders them back in unity3D.
- It uses data driven shader model instead of fixed function render pipeline. 
- Uses vertex buffer instead of separate geometry commands. 
- Original texture Id based UV mapping is replaced with single large texture atlas for performance gain. 
- In original file models were not defined in complete mesh. Model parts are stored separately. They were rearranged procedurally using attached position and rotation information. Now it represents model with skeletal transformation hierarchy in unity3D.
- Animation is represented with Unity3D animation curve.
- Includes Character Controller with animation state machine and physics. 

- Zones and Triggers are marked using Unity Box Collider for convinient collision detection.
- Auto detects grabbable platform of any level by analyzing room mesh, no floor data is needed.
- Properly scaled level, ready for Unity physics and GI lighting system.
- Included experimental day-night system.

<b>Upgrades: </b>

- Auto detection of water in level.
- Added under water caustic lighting effect.
- Added under water refraction effect.
- Extended Character Controller with diving-swimming-pulling up support.

<b> Demo</b>

- WebGL build http://tickleheadstudios.com/demo/unity/tr2webgl/
- Tomb Raider 2 Custom Level https://www.youtube.com/watch?v=P8GPi6-G9CM

<h3>Frequently asked qustions about TR2 Level Viewer</h3>


<b>Q. How can I browse a tr2 level file?</b>

Ans.  Now you can browse for tr2 level file in PC as well as in android. 
	Thanks aeroson(Jakub Neuman) for fixing browsing limit.
	For android phone or tab, files should be placed in internal memory card (sdcard). 

<b>Q. Browser shows files with tr2 extension. But I cannot select it.</b>

Ans.  Browser can recognize files with *.TR2 extension by default. Try changing *.tr2 extension into upper case *.TR2 extension.

<b>Q. What are the basic controls?</b>

Ans.  You can use keyboard + mouse to perform various actions. Basics are followings-
On PC:
- Run - Up arrow
- Walk - Num 5
- Jump/swim - Right mouse button
- Look around - Move mouse while pressing left mouse button.
- Long Jump - Press jump while you are running.
- Short Jump - Press up arrow + jump same time.
- Pull up -   Get close to a platform and press up arrow + left mouse button.

On Android Devices:
Controls are not completed yet for android devices. I have planned to develop them in future. Meanwhile, 
Following controls are available:
- Run – Double tap screen and hold.
- Look around – Touch screen and move your fingers.
- Pull up -   Get close to a platform double tap screen.
Note: Optionally custom key can also be setup with KeyMapper prefab in unity.


<b>Q. How can I configure global parameters of the project?</b>

Ans. You can configure global parameter in Settings.cs script. 

<b>Q. How can I attach custom behavior to movable tr2 object?</b>

Ans. You can attach custom behavior to movable tr2 object through AI prefab in unity. Place AI prefab in scene.
AI prefab has component AICallBackHandler. You can attach your custom behavior script there in OnAttachingBehaviourToObject call back.
Return value OnAttachingBehaviourToObject should be true if you want to attach your behavior script. Otherwise return false.
callback for attaching behavior:
public static bool OnAttachingBehaviourToObject (GameObject AI, int ObjectID, GameObject Player, Parser.Tr2Item tr2item)

Description of arguments:

AI  		: This is the unity game object where you can attach your custom behavior script.
ObjectID        : This is ID of the AI in TR2 to game engine. You can identify any TR2 AI type with this ID and script it.
Player		: Unity game object that represents Lara.



<b>Q. How can I control animation state with my custom behavior script?</b>


First set ShowObjectID to true in Settings.cs. This will print TR2 object ID in 3D text.  
Alternatively you can look up this ID in Unity3D Editor’s Hierarchy panel. Search for game object starting 
‘Object X‘ where X means ID.

Then, select TR2 object that you want to control based on this ID.
Then, you have to attach AnimationTester.cs script to that object. AnimationTester.cs is a utility script for manual 
test of animations of a tr2 object.
Game Object ‘AI’ in level scene has a component AICallBackHandler  which handles a request with ObjectID argument. 
Check this Object ID . If this ID matches your selected TR2 object ID, then you can process it.

<b>Q. What unity layers are used for objects?</b>
    Layers are defined in MaskedLayer.cs and GlobalLayer .cs.Following layers are used
   	- Switch = 8       Defines layers of switch objects
	- Player = 9       Defines unity layer of Lara
	- Default = 0
	
<b>Q. What features can I play around?</b>
  Possible areas where you can develop are:
- Sound System
- Day Night System
- Health Monitoring System
- GUIManager : It handles player statistics health, crossed distance, time of the day
- Shader
- Parser: It is core file IO class that parses TR2 level file into Unity.
- AnimationStatePlayer / Behaviour
- KeyMapper


<b>Q. Can I generate entire level in edit mode?</b>

Ans. Yes you can.  From Menu TR2 Editor -> Create Level. Be sure to create new scene in that case. Otherwise you 
will mix-up objects already exist in scene with newly generated objects : ) There is a unit test scene in this project 
demostrating the feature.

Required prefabs should be placed in scene for minimum level funtionality:

- ThirdPersonCam (Prefav camera)
- KeyMapper (Prefav KeyMapper)
- Mouse (Prefav Mouse)


<b>Q. Is it possible to load level file from http server?</b>

  Ans. Yes it is possible. You can play around Browser.cs .  It uses WWW class to fetch level file data bytes from specified server url. 


<b> Known Issues </b>

Project was developed in Unity 3.5.If project import fails because of meta missing in Unity 5 or higher just cancel import and try to reimport.


<b>Q.How to contact the authour?</b>

Here is my mail address suruzkento@gmail.com, you can contact me to asked some thing I missed here, to share your ideas, or to simply say “Hi” :) 





