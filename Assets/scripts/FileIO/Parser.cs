
//Based on E. Popov's TRViewer and TRosettaStone for TR4/TR5 formats

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


/*
     * Moveable structure.  This defines a list of contiguous meshes that
     * comprise one object, e.g. in WALL.TR2,
     * moveable[0]  = Lara (StartingMesh 0, NumMeshes 15),
     * moveable[13] = Tiger (StartingMesh 215, NumMeshes 27)
     * moveable[15] = Spikes (StartingMesh 249, NumMeshes 1)
     * moveable[16] = Boulder (StartingMesh 250, NumMeshes 1)
     * moveable[20] = Rolling Blade (StartingMesh 254, NumMeshes 1)
     */

//Trying to create a MonoBehaviour using the 'new' keyword.  This is not allowed.  
//MonoBehaviours can only be added using AddComponent().  Alternatively, your script can inherit from 
//ScriptableObject or no base class at all

public class Tr2Moveable// : MonoBehaviour
{   
	public uint ObjectID;          // Item Identifier
	public ushort NumMeshes;       // number of meshes in this object
	public ushort StartingMesh;    // first mesh
	public uint MeshTree;          // offset into MeshTree[]
	public uint FrameOffset;       // byte offset into Frames[] (divide by 2 for Frames[i])
	public ushort Animation;       // offset into Animations[]
	
	//Extended for unity
	public Transform[] TransformsTree;
	public GameObject UnityObject;
	public Animation UnityAnimation;
	public int AnimationStartOffset;
	public int AnimationFrameRate;
	public int NumClips;
	public List<TRAnimationClip> AnimClips;
	public LaraStatePlayer StatePlayer;
	public TextMesh Text3D;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
} 

public class Parser 
{
	static long m_FileVersion;
	static System.UInt16 m_LevelVersion;
	static Tr2Level m_Level = null;
	
	public enum TR2VersionType
	{
		TombRaider_UnknownVersion,
		TombRaider_1,
		TombRaider_2,
		TombRaider_3,
		TombRaider_4
	}

	//TODO:
	//Use enum instead of statics
	//enum TR2StructSize
	//{
		static int Tr2ColourSize = 3;
		static int Tr2Colour4Size = 4;
		static int Tr2VertexSize = 6;
		static int Tr2Face4Size = 10;
		static int Tr2Face3Size = 8;
		static int Tr2Textile8Size = 65536;
		static int Tr2Textile16Size = 131072; 
		static int Tr2RoomInfoSize = 16;
		static int Tr2RoomPortalSize = 32;
		static int Tr2RoomSectorSize = 8;
		static int Tr2RoomLightSize = 24;
		static int Tr2VertexRoomSize = 12;
		static int Tr2RoomSpriteSize = 4;
		static int Tr2RoomDataSize = 0;
		static int Tr2RoomSaticMeshSize = 20;
		static int Tr2RoomSize = 0;
		static int Tr2AnimationSize = 32;
		static int Tr2StateChangeSize = 6;
		static int Tr2AnimDispatchSize = 8;
		static int Tr2AnimCommandSize = 2;
		static int Tr2MeshTreeSize = 16;
		static int Tr2MoveableSize = 18;
		static int Tr2StaticMeshSize = 0;
		static int Tr2ObjectTextureVertSize = 4;
		static int TrExObjectTextureSize = 0;
		static int Tr2ObjectTextureSize = 20;
		static int Tr2SpriteTextureSize = 16;
		static int Tr2SpriteSequenceSize = 8;
		static int Tr2MeshSize = 0;
		static int Tr2FrameSize = 0;
		static int Tr2ItemSize = 24;
		static int Tr2SoundSourceSize = 16;
		static int Tr2BBoxSize = 8;
		static int Tr1BBoxSize = 20;
		static int Tr2AnimatedTextureSize = 0;
		static int Tr2CameraSize = 16;
		static int Tr2SoundDetailsSize = 8;
		static int Tr2CinematicFrameSize = 16;
		static int Tr2LevelSize = 0;
	//}
	
	public struct Tr2Colour // 3 bytes
	{  
		public byte Red;
		public byte Green;
		public byte Blue;
	} 
	
	public struct Tr2Colour4 // 4 bytes
	{  
		public byte Red;
		public byte Green;
		public byte Blue;
		public byte unused;
	} 
	
	public struct Tr2Vertex // 6 bytes
	{  
		public short x;
		public short y;
		public short z;
	} 

	/*
     * A rectangular (quad) face definition.  Four vertices (the values are
     * indices into the appropriate vertex list) and a texture (an index
     * into the texture list) or colour (index into 8-bit palette).
     * I've seen a few coloured polygons where Texture is greater than 255,
     * but ANDing the value with 0xFF seems to produce acceptable results.
     */

	public struct Tr2Face4					// 10 bytes
	{  
		public ushort Vertices0;			//4vertices
		public ushort Vertices1;
		public ushort Vertices2;	
		public ushort Vertices3;	
		public ushort Texture;
	} 
	
	/*
     * A triangular face definition.  Three vertices (the values are indices
     * into the appropriate vertex list) and a texture (an index into the
     * texture list) or colour (index into palette).
     * In the case of a colour, (Texture & 0xff) is the index into the 8-bit
     * palette, while (Texture >> 8) is the index into the 16-bit palette.
     */
	
	public struct Tr2Face3					// 8 bytes
	{  
		public ushort Vertices0;			//4vertices
		public ushort Vertices1;	
		public ushort Vertices2;	
		public ushort Texture;
	} 
	
	/*
     * An 8-bit texture tile (65536 bytes).  Each byte represents a pixel
     * whose colour is in the 8-bit palette.
     */

	public struct Tr2Textile8 // 65536 bytes
	{   
		public byte[] Tile;  //256*256
	} 
	
	/*
     * A 16-bit texture tile (131072 bytes).  Each word represents a pixel
     * whose colour is of the form ARGB, M-to-LSB:
     *    1-bit transparency (0: transparent, 1: opaque)
     *    5-bit red channel
     *    5-bit green channel
     *    5-bit blue channel
     */

	public struct Tr2Textile16 // 131072 bytes
	{  
		public ushort[] Tile; //256 * 256
	} 
	
	/*
     * The "header" of a room.  x/z indicate the base position of the room
     * mesh in world coordinates.  yLowest and yHighest indicate the lowest and
     * highest points in this room (even though their actual values appear to
     * be reversed, since a "high" point will have a smaller value than a "low"
     * point).  When positioning objects/items, as well as the room meshes
     * themselves, y is always 0-relative (not room-relative).
     */

	public struct Tr2RoomInfo 	  // 16 bytes
	{  
		public int x;             // X-offset of room (world coordinates)
		public int z;             // Z-offset of room (world coordinates)
		public int yBottom;       // Y-offset of lowest point in room (world coordinates) (actually highest value)
		public int yTop;          // Y-offset of highest point in room (world coordinates) (actually lowest value)
	} 

	
	/*
     * Portal structure.  This defines every viable exit from a given "room".
     * Note that "rooms" are really just areas;  they aren't necessarily enclosed.
     * The door structure below defines areas of egress, not the actual moveable
     * mesh, texture, and action (if any).
     */

	public struct Tr2RoomPortal 		// 32 bytes
	{   
		public ushort AdjoiningRoom;    // which room this "door" leads to 2 bytes
		public Tr2Vertex Normal;        // which way the "door" faces  6 bytes
		public Tr2Vertex[] Vertices;    // the 4 corners of the "door"  4 * 6 = 24 byte
	} 
	
	/*
     * Room sector structure.
     *
     * Sectors are 1024 * 1024 (world coordinates).  Floor and Ceiling are signed
     * number of 256 units of height (relative to 0), e.g. Floor 0x04 corresponds to
     * Y = 1024 in world coordinates.  Note: this implies that, while X and Z can
     * be quite large, Y is constrained to -32768..32512.  Floor/Ceiling value of
     * 0x81 indicates impenetrable wall.
     * Floor values are used by the game engine to determine what objects Lara can
     * traverse and how.  Relative steps of 1 (-256) can be walked up;  steps of
     * 2..7 (-512..-1792) can/must be climbed;  steps larger than 7 (-2048..-32768)
     * cannot be climbed (too tall).
     */

	public struct Tr2RoomSector 			// 8 bytes
	{   
		public ushort FDindex;
		public ushort BoxIndex;
		public byte  RoomBelow;             // 255 if none
		public sbyte   Floor;				//char ==2 byte in c# here; chaqnge it to byte
		public byte  RoomAbove;             // 255 if none
		public sbyte   Ceiling;
	} 
	
	/*
     * Room lighting structure.  X/Y/Z are in world coordinates.
     * Lighting values seem to range from 0..8192.
     */
	
	public struct Tr2RoomLight 		// 24 bytes
	{ 
		public int  x;
		public int  y;
		public int  z;
		public ushort Intensity1;
		public ushort Intensity2;
		public uint Fade1;
		public uint Fade2;
	} 
	
	/*
     * Room vertex structure.  This defines the vertices within a room.
     */

	public struct Tr2VertexRoom 		 // 12 bytes
	{   
		public Tr2Vertex Vertex; // following 3 entries * 2 byte = 6 bytes
		public short Lighting1;     // values range from 0 to 32767, 0=total darkness. (TR3)
		// I think the values ranged from 0 to 8192 in TR1/2, 0=total brightness                 		
		public ushort Attributes;   // 0x8000 something to do with water surface
		// 0x4000 under water lighting modulation and movement if viewed from above water surface
		// 0x2000 water/quicksand surface movement
		// 0x1fef nothing?
		// 0x0010 everything?
		public short Lighting2;     // seems to be the same as lighting1
	} 
	
	/*
     * Sprite structure
     */
	
	public struct Tr2RoomSprite 		 // 4 byte
	{   
		public short Vertex;             // offset into vertex list
		public short Texture;            // offset into texture list
	} 
	/*
     * Room mesh structure.  This is the geometry of the "room," including
     * walls, floors, rocks, water, etc.  It does _not_ include objects that
     * Lara can interact with (keyboxes, moveable blocks, moveable doors, etc.)
     */
	
	public struct Tr2RoomData 
	{
		public short           NumVertices;     // number of vertices in the following list
		public Tr2VertexRoom[] Vertices;       // list of vertices (relative coordinates)
		public short           NumRectangles;   // number of textured rectangles
		public Tr2Face4[]       Rectangles;     // list of textured rectangles
		public short           NumTriangles;    // number of textured triangles
		public  Tr2Face3[]       Triangles;      // list of textured triangles
		public short           Numsprites;      // number of sprites
		public Tr2RoomSprite[]  Sprites;        // list of sprites
	} 
	/*
     * Room static mesh data.  Positions and IDs of static meshes (e.g. skeletons,
     * spiderwebs, furniture)
     */
	
	public struct Tr2RoomStaticMesh   // 20 bytes
	{  
		public int  x;                // absolute position in world coordinates
		public int  y;
		public int  z;
		public ushort Rotation;       // high two bits (0xc000) indicate steps of 90 degrees
		public ushort Intensity1;
		public ushort Intensity2;
		public ushort ObjectID;       // which StaticMesh item to draw
	} 
	
	/*
     * Room structure.  Here's where all the room data come together.
     */


	public struct Tr2Room 
	{
		public  Tr2RoomInfo info;            				// where the room exists, in world coordinates
		public uint NumDataWords;            				// number of data words (ushort)
		//this should be read sequencialy to fill following members content 
		public byte[] Data;                  				// the raw data from which the rest of this is derive
		public Tr2RoomData RoomData;         				// the room mesh
		public ushort NumPortals;            				// number of visibility portals that leave this room
		public Tr2RoomPortal[] Portals;      				// list of visibility portals
		public ushort NumZsectors;           				// width of sector list
		public ushort NumXsectors;           				// height of sector list
		public Tr2RoomSector[] SectorList;   				// list of sectors in this room
		public short  Intensity1,Intensity2,LightMode;
		public ushort NumLights;              				// number of lights in this room
		public Tr2RoomLight[] Lights;        				// list of lights
		public ushort NumStaticMeshes;        				// number of static meshes
		public Tr2RoomStaticMesh[] StaticMeshes;            // static meshes
		public short  AlternateRoom;
		public short  Flags;                                // 0x0001 - room is filled with water
		// 0x0020 - Lara's ponytail gets blown by the wind
		public Tr2Colour RoomLightColour;                   // TR3 ONLY!
	} 
	
	/*
     * Animation structure.
     */


	public struct Tr2Animation               // 32 bytes
	{ 
		public 	uint 		FrameOffset;     // byte offset into Frames[] (divide by 2 for Frames[i])
		public  byte  		FrameRate;       // "ticks" per frame
		public 	byte  		FrameSize;       // number of words in Frames[] used by this animation
		public  short  		StateID;
		public  short    	Unknown1;
		public  short    	Unknown2;
		public  short    	Unknown3;
		public  short    	Unknown4;
		public 	ushort 		FrameStart;       // first frame in this animation
		public 	ushort 		FrameEnd;         // last frame in this animation (numframes = (End - Start) + 1)
		public  ushort 		NextAnimation;
		public  ushort  	NextFram;
		public  ushort  	NumStateChanges;
		public 	ushort 		StateChangeOffset; // offset into StateChanges[]
		public 	ushort 		NumAnimCommands;
		public 	ushort 		AnimCommand;       // offset into AnimCommands[]
	} 
	
	/*
     * State Change structure
     */
	
	public struct Tr2StateChange          // 6 bytes
	{  
		public ushort StateID;
		public ushort NumAnimDispatches;  // number of dispatches (seems to always be 1..5)
		public ushort AnimDispatch;       // Offset into AnimDispatches[]
	} 
	
	/*
     * Animation Dispatch structure
     */
	[System.Serializable]
	public class Tr2AnimDispatch         // 8 bytes
	{  
		public short Low;
		public short High;
		public short NextAnimation;
		public short NextFrame;
	} 
	
	
	/*
     * AnimCommand structure
     */

	public struct Tr2AnimCommand     // 2 bytes
	{   
		public short Value;
	}
	//  tr2_anim_command;
	
	/*
     * MeshTree structure
     *
     * MeshTree[] is actually groups of four ints.  The first one is a
     * "flags" word;  bit 1 (0x0002) indicates "make imediately previous mesh an anchor (e.g. PUSH)";
     *  bit 0 (0x0001) indicates "return to previous (saved) anchor (e.g. POP)".
     * The next three ints are X, Y, Z offsets from the last mesh position.
     */

	public struct Tr2MeshTree  // 16 bytes
	{   
		public int Flags;      // 0x0001 = POP, 0x0002 = PUSH
		public int 	x;
		public int 	y;
		public int 	z;
	} 
	

	/*
     * StaticMesh structure.  This defines meshes that don't move (e.g. skeletons
     * lying on the floor, spiderwebs, etc.)
     */
	
	
	/*
     * following structre can not be loaded as Explicit.Because [] does not starts in 8 byte allingment 
     */

	public struct Tr2StaticMesh                 // 32 bytes
	{ 
		public uint ObjectID;                   // Item Identifier
		public ushort StartingMesh;             // first mesh
		public Tr2Vertex[] BoundingBox;         //[2][2] * 6 bytes = 24 bytes
		public ushort Flags;

		//Extended
		public GameObject UnityObject;
	} 
	
	/*
     * Object texture vertex structure.  Maps coordinates
     * into texture tiles.
     */

	public struct Tr2ObjectTextureVertex     // 4 bytes
	{  
		public byte Xcoordinate;
		public byte Xpixel;
		public byte Ycoordinate;
		public byte Ypixel;
	} 
	/*
     * Object texture structure.
     */
	
	//Error Solved: Could not load type from assembly Version=0.0.0.0, Culture=neutral, PublicKeyToken=null Unity3d 
	//it's beacuse of public struct or class  with explicit layout with variable length field which is placed in wrong field offset

	public struct TrExObjectTexture                 // 38bytes
	{    
		public ushort TransparencyFlags;            // (0: Opaque; 1: Use transparency; 2: Use partial transparency [grayscale intensity :: transparency])
		public uint Tile;                           // index into textile list
		public Tr2ObjectTextureVertex[] Vertices;   // the four corners of the texture 4 * 4 = 16 bytes[can be expanded in to four filed]
	} 
	

	public struct Tr2ObjectTexture                  // 20 bytes
	{    
		public ushort TransparencyFlags;            // (0: Opaque; 1: Use transparency; 2: Use partial transparency [grayscale intensity :: transparency])
		public ushort Tile;                         // index into textile list
		public Tr2ObjectTextureVertex[] Vertices;   // the four corners of the texture 4 * 4 = 16 bytes[can be expanded in to four filed]
	}
	
	/*
     * Sprite texture structure.
     */
	

	public struct Tr2SpriteTexture         // 16 bytes
	{ 
		public ushort Tile;
		public byte  x;
		public byte  y;
		public ushort Width;               // actually, (width * 256) + 255
		public ushort Height;              // actually, (height * 256) + 255
		public short  LeftSide;
		public short  TopSide;
		public short  RightSide;
		public short  Bottomside;
	}
	
	/*
     * Sprite Sequence structure
     */
	

	public struct Tr2SpriteSequence          // 8 bytes
	{  
		public int ObjectID;                 // Item identifier (same numbering as in tr2_moveable)
		public short NegativeLength;         // negative of "how many sprites are in this sequence"
		public short Offset;                 // where (in sprite texture list) this sequence starts
	} 

	/*
     * Mesh structure.  The mesh list contains the mesh info for Lara (in all her
     * various incarnations), blocks, enemies (tigers, birds, bad guys), moveable
     * blocks, zip line handles, boulders, spinning blades, you name it.
     * If NumNormals is negative, Normals[] represent vertex lighting values (one
     * per vertex).
     */

	public struct Tr2Mesh {
		public Tr2Vertex Centre;                  // this seems to describe the approximate geometric centre
		// of the mesh (possibly the centre of gravity?)
		// (relative coordinates, just like the vertices)
		public int      CollisionSize;             // radius of collisional sphere
		public short      NumVertices;             // number of vertices in this mesh
		public Tr2Vertex[] Vertices;               // list of vertices (relative coordinates)
		public short      NumNormals;              // number of normals in this mesh (should always equal NumVertices)
		public Tr2Vertex[] Normals;                // list of normals (NULL if NumNormals < 0)
		public short[]      MeshLights;            // if NumNormals < 0
		public short      NumTexturedRectangles;   // number of textured rectangles in this mesh
		public Tr2Face4[] TexturedRectangles;      // list of textured rectangles
		public short      NumTexturedTriangles;    // number of textured triangles in this mesh
		public Tr2Face3[] TexturedTriangles;       // list of textured triangles
		public short      NumColouredRectangles;   // number of coloured rectangles in this mesh
		public Tr2Face4[] ColouredRectangles;      // list of coloured rectangles
		public short      NumColouredTriangles;    // number of coloured triangles in this mesh
		public Tr2Face3[] ColouredTriangles;       // list of coloured triangles
	} 
	
	/*
     * Frame structure.
     *
     * Frames indicates how composite meshes are positioned and rotated.  They work
     * in conjunction with Animations[] and Bone2[].  A given frame has the following
     * format:
     *    short BB1x, BB1y, BB1z           // bounding box (low)
     *    short BB2x, BB2y, BB2z           // bounding box (high)
     *    short OffsetX, OffsetY, OffsetZ  // starting offset for this moveable
     *    (TR1 ONLY: short NumValues       // number of angle sets to follow)
     *    (TR2/3: NumValues is implicitly NumMeshes (from moveable))
     *    What follows next is a list of angle sets.  In TR2/3, an angle set can
     *    specify either one or three axes of rotation.  If either of the high two
     *    bits (0xc000) of the first angle ushort are set, it's one axis:  only one
     *    ushort, low 10 bits (0x03ff), scale is 0x100 == 90 degrees;  the high two
     *    bits are interpreted as follows:  0x4000 == X only, 0x8000 == Y only,
     *    0xC000 == Z only.
     *    If neither of the high bits are set, it's a three-axis rotation.  The next
     *    10 bits (0x3ff0) are the X rotation, the next 10 (including the following
     *    ushort) (0x000f, 0xfc00) are the Y rotation, the next 10 (0x03ff) are the
     *    Z rotation, same scale as before (0x100 == 90 degrees).
     *    Rotations are performed in Y, X, Z order.
     *    TR1 ONLY: All angle sets are two words and interpreted like the two-word
     *    sets in TR2/3, EXCEPT that the word order is reversed.
     */

	public struct Tr2Frame 
	{
		public Tr2Vertex[] Vector;  //3
		public int        NumWords;
		public ushort[]	Words;
	} 

	/*
     * Item structure
     */

	public class Tr2Item                // 24 bytes
	{ 
		public short ObjectID;
		public short Room;
		public int x;
		public int y;
		public int z;
		public short Angle;
		public short Intensity1;
		public short Intensity2;
		public short Flags;   // 0x0100 indicates "inactive" or "invisible"

		//Unity3D extention
		public GameObject UnityObject = null;
		public GameObject ActivateObject = null;
		public Tr2Moveable ObjectBase;
		public TextMesh Text3D;
	}
	
	/*
     * SoundSource structure
     */

	public struct Tr2SoundSource { //16
		public int  		x;               // position of sound source
		public int  		y;
		public int   	z;
		public ushort 	SoundID;             // internal sound index
		public ushort 	Flags;               // 0x40, 0x80, or 0xc0
	} 

	
	/*
     * Boxes structure
     */

	public struct Tr2BBox                     //8 bytes
	{ 
		public byte 		Zmin;             // sectors (* 1024 units)
		public byte      Zmax;
		public byte      Xmin;
		public byte      Xmax;
		public short 	TrueFloor;             // Y value (no scaling)
		public short 	OverlapIndex;          // index into Overlaps[]
	} 
	

	public struct Tr1BBox   //20 byte
	{
		public int Zmin;
		public int Zmax; 
		public int Xmin; 
		public int Xmax;
		public short TrueFloor;
		public short OverlapIndex;
	} 
	
	/*
     * AnimatedTexture structure
     * - really should be simple short[], since it's variable length
     */

	public struct Tr2AnimatedTexture 
	{
		public short NumTextureIDs;    // Number of Texture IDs - 1
		public short[] TextureList;    // list of textures to cycle through
	} 
	
	/*
     * Camera structure
     */
	

	public struct Tr2Camera               //16 bytes
	{ 
		public int x;
		public int y;
		public int z;
		public short Room;
		public ushort Unknown1;           // correlates to Boxes[]?
	}
	
	/*
     * Sound sample structure
     */
	

	public struct Tr2SoundDetails         // 8 bytes
	{   
		public short Sample;
		public short Volume;
		public short SoundRange;
		public short Flags;              // bits 8-15: priority?, 2-7: number of sound samples
		// in this group, bits 0-1: channel number
	}
	
	/*
     * Cutscene Camera structure
     */

	public struct Tr2CinematicFrame         //16 bytes    its 24 bytes in tr4
	{ 
		public short 	 rotY;             // Rotation about Y axis, +/-32767 ::= +/- 180 degrees
		public short      rotZ;            // Rotation about Z axis, +/-32767 ::= +/- 180 degrees
		public short      rotZ2;           // Rotation about Z axis (why two?), +/-32767 ::= +/- 180 degrees
		public short      posZ;            // Z position of camera, relative to something
		public short      posY;            // Y position of camera, relative to something
		public short      posX;            // X position of camera, relative to something
		public short      Unknown1;
		public short      rotX;            // Rotation about X axis, +/-32767 ::= +/- 180 degrees
	} 
	
	/*
     * Structure to contain an entire level.
     */

	public class Tr2Level 
	{
		public byte[]  FileName;                          // filename (not in .TR2 file)
		public uint Version;                              // .TR2 file version
		public TR2VersionType EngineVersion;              // TombRaider_1, TombRaider_2, TombRaider_3
		public Tr2Colour[] Palette8;      		   	      // 256 sized 24-bit palette
		public uint[] Palette16;              			  // 256sized 32-bit palette
		public uint NumTextiles;                          // number of textiles
		
		public Tr2Textile8[] Textile8;                    // 8-bit (palettised) textiles
		public Tr2Textile16 [] Textile16;                 // 16-bit (ARGB) textiles
		
		public uint UnknownT;                             // 32-bit unknown (always 0 in real TR2 levels)
		public ushort NumRooms;                           // number of rooms in this level
		public Tr2Room[] Rooms;                           // list of rooms
		
		public uint NumFloorData;                         // number of words of floor data this level
		public ushort[] FloorData;                        // floor data
		
		public int    NumMeshes;                           // number of meshes this level
		public Tr2Mesh[] Meshes;                           // list of meshes
		
		public uint NumAnimations;                        // number of animations this level
		public Tr2Animation[]  Animations;                // list of animations
		
		public uint NumStateChanges;                      // number of structures(?) this level
		public Tr2StateChange[] StateChanges;             // list of structures
		
		public uint NumAnimDispatches;                    // number of ranges(?) this level
		public Tr2AnimDispatch[] AnimDispatches;          // list of ranges
		
		public uint NumAnimCommands;                      // number of Bone1s this level
		public Tr2AnimCommand[] AnimCommands;             // list of Bone1s
		
		public uint NumMeshTrees;                         // number of Bone2s this level
		public int[]  MeshTrees;       					  // list of Bone2s
		//public byte[] BytesMeshTrees;
		
		public uint NumFrames;                            // number of words of frame data this level
		public ushort[] Frames;                           // frame data
		
		public uint NumMoveables;                         // number of moveables this level
		public Tr2Moveable[] Moveables;                   // list of moveables
		
		public uint NumStaticMeshes;                      // number of static meshes this level
		public Tr2StaticMesh[] StaticMeshes;              // static meshes
		
		public uint NumObjectTextures;                    // number of object textures this level
		public Tr2ObjectTexture[] ObjectTextures;         // list of object textures
		public TrExObjectTexture[] ObjectTexturesTR4; 
		
		public uint NumspriteTextures;                    // number of sprite textures this level
		public Tr2SpriteTexture[] SpriteTextures;         // list of sprite textures
		
		public uint NumSpriteSequences;                   // number of sprite sequences this level
		public Tr2SpriteSequence[] SpriteSequences;       // sprite sequence data
		
		public int  NumCameras;                            // Number of Cameras
		public Tr2Camera[] Cameras;                        // cameras
		
		public  int  NumsoundSources;                      // Number of Sounds
		public Tr2SoundSource[] SoundSources;              // sounds
		
		public int  NumBoxes;                              // Number of Boxes
		public Tr2BBox[] Boxes;                            // boxes - looks like struct { ushort value[4]; } - value[0..2] might be a vector; value[3] seems to be index into Overlaps[]
		
		public int  NumOverlaps;                           // Number of Overlaps
		public short [] Overlaps;                            // overlaps - looks like ushort; 0x8000 is flag of some sort
		//            appears to be an offset into Boxes[] and/or Boxes2[]
		public short [] Zones;                               // boxes2
		
		public int  NumAnimatedTextures;                     // Number of AnimTextures
		public short[] AnimatedTextures;                     // animtextures
		
		public int  NumItems;                                // Number of Items
		public Tr2Item[] Items;                              // items
		
		public byte[] LightMap;                              // colour-light maps
		public ushort NumCinematicFrames;                    // number of cut-scene frames
		
		public Tr2CinematicFrame[] CinematicFrames;          // cut-scene frames
		public short  NumDemoData;                           // Number of Demo Data
		public byte[] DemoData;                              // demo data
		public short[] SoundMap;                             // sound map
		
		public int  NumsoundDetails;                         // Number of SampleModifiers
		public Tr2SoundDetails[] SoundDetails;               // sample modifiers
		
		public int  NumsampleIndices;                        // Number of Sample Indices
		public int[] SampleIndices;                          // sample indices
		
		public uint NumMeshPointers;
		public uint[]  MeshPointerList;


		//extented texture data
		public int m_TexWidth = 0;
		public int m_TexHeight = 0;
		
		public int m_MaxHeight = 4096;
		public int m_MaxWidth = 4096;
		public int m_MaxTiles = 0;

		//extented for unity
		public ThirdPersonCam Camera ;
		public TextMesh Text3DPrefav;
		//public Light LightPrefav
	} 
	
	//Raw structure data casting is problematice in C#. So break down a structure to it's fields and read them individually. This looks 
	//ugly but works : )
	static object Cast2Struct(byte[] buffer, System.Type type)
	{
		object retval = null;
		if(type ==  typeof(Tr2RoomInfo)) 
		{   
			Tr2RoomInfo t = new Tr2RoomInfo();
			t.x = System.BitConverter.ToInt32(buffer,0);
			t.z = System.BitConverter.ToInt32(buffer,4);
			t.yBottom = System.BitConverter.ToInt32(buffer,8);
			t.yTop = System.BitConverter.ToInt32(buffer,8);
			retval = t;
		}
		else if(type==  typeof(Tr2VertexRoom)) 
		{   
			Tr2VertexRoom t = new Tr2VertexRoom();
			t.Vertex.x = System.BitConverter.ToInt16(buffer,0);
			t.Vertex.y = System.BitConverter.ToInt16(buffer,2);
			t.Vertex.z = System.BitConverter.ToInt16(buffer,4);
			t.Lighting1 =  System.BitConverter.ToInt16(buffer,6);    // values range from 0 to 32767, 0=total darkness. (TR3)
			// I think the values ranged from 0 to 8192 in TR1/2, 0=total brightness
			t.Attributes =  System.BitConverter.ToUInt16(buffer,8);   // 0x8000 something to do with water surface
			// 0x4000 under water lighting modulation and movement if viewed from above water surface
			// 0x2000 water/quicksand surface movement
			// 0x1fef nothing?
			// 0x0010 everything?
			t.Lighting2 =  System.BitConverter.ToInt16(buffer,10);     // seems to be the same as lighting1
			retval = t;
		}
		else if(type ==  typeof(Tr2Vertex)) 
		{   
			Tr2Vertex  t = new Tr2Vertex();
			t.x = System.BitConverter.ToInt16(buffer,0);
			t.y = System.BitConverter.ToInt16(buffer,2);
			t.z = System.BitConverter.ToInt16(buffer,4);
			retval = t;
		}
		else if(type ==  typeof(Tr2RoomSector)) 
		{   
			Tr2RoomSector  t = new Tr2RoomSector();
			t.FDindex = System.BitConverter.ToUInt16(buffer,0);
			t.BoxIndex = System.BitConverter.ToUInt16(buffer,2);
			t.RoomBelow = buffer[4];       // 255 if none
			t.Floor =(sbyte) buffer[5];    //char ==2 byte in c# here; chaqnge it to byte
			t.RoomAbove = buffer[6];       // 255 if none
			t.Ceiling = (sbyte) buffer[7];
			retval = t;
		}
		else if(type ==  typeof(Tr2RoomLight)) 
		{   
			Tr2RoomLight  t = new Tr2RoomLight();
			t.x =  System.BitConverter.ToInt32(buffer,0);
			t.y =  System.BitConverter.ToInt32(buffer,4);
			t.z =  System.BitConverter.ToInt32(buffer,8);
			t.Intensity1 =  System.BitConverter.ToUInt16(buffer,12);
			t.Intensity2 =  System.BitConverter.ToUInt16(buffer,14);
			t.Fade1 = System.BitConverter.ToUInt32(buffer,16);
			t.Fade2  = System.BitConverter.ToUInt32(buffer,20);
			retval = t;
		}
		else if(type ==  typeof(Tr2RoomStaticMesh)) 
		{   
			Tr2RoomStaticMesh  t = new Tr2RoomStaticMesh();
			t.x =  System.BitConverter.ToInt32(buffer,0);              // absolute position in world coordinates
			t.y =  System.BitConverter.ToInt32(buffer,4);
			t.z =  System.BitConverter.ToInt32(buffer,8);
			t.Rotation =  System.BitConverter.ToUInt16(buffer,12);      // high two bits (0xc000) indicate steps of 90 degrees
			t.Intensity1 =  System.BitConverter.ToUInt16(buffer,14);
			t.Intensity2 =  System.BitConverter.ToUInt16(buffer,16);
			t.ObjectID  =  System.BitConverter.ToUInt16(buffer,18);     // which StaticMesh item to draw
			retval = t;
		}
		else if(type ==  typeof(Tr2RoomSprite)) 
		{   
			Tr2RoomSprite  t = new Tr2RoomSprite();
			t.Vertex = System.BitConverter.ToInt16(buffer,0);     // offset into vertex list
			t.Texture = System.BitConverter.ToInt16(buffer,2);    // offset into texture list
			retval = t;
		}
		else if(type ==  typeof(Tr2Face4))
		{   
			Tr2Face4  t = new Tr2Face4();
			t.Vertices0 =  System.BitConverter.ToUInt16(buffer,0);
			t.Vertices1 =  System.BitConverter.ToUInt16(buffer,2);
			t.Vertices2 =  System.BitConverter.ToUInt16(buffer,4);
			t.Vertices3 =  System.BitConverter.ToUInt16(buffer,6);
			t.Texture =  System.BitConverter.ToUInt16(buffer,8);
			retval = t;
		}
		else if(type==  typeof(Tr2Face3))
		{   
			Tr2Face3  t = new Tr2Face3();
			t.Vertices0 =  System.BitConverter.ToUInt16(buffer,0);
			t.Vertices1 =  System.BitConverter.ToUInt16(buffer,2);
			t.Vertices2 =  System.BitConverter.ToUInt16(buffer,4);
			t.Texture =  System.BitConverter.ToUInt16(buffer,6);
			retval = t;
		}
		else if(type ==  typeof(Tr2Animation)) 
		{   
			Tr2Animation  t = new Tr2Animation();
			t.FrameOffset =  System.BitConverter.ToUInt32(buffer,0);     // byte offset into Frames[] (divide by 2 for Frames[i])
			t.FrameRate =  buffer[4];                                    // "ticks" per frame
			t.FrameSize = buffer[5];                                      // number of words in Frames[] used by this animation
			t.StateID =  System.BitConverter.ToInt16(buffer,6);
			t.Unknown1 =  System.BitConverter.ToInt16(buffer,8);
			t.Unknown2 =  System.BitConverter.ToInt16(buffer,10);
			t.Unknown3 =  System.BitConverter.ToInt16(buffer,12);
			t.Unknown4 =  System.BitConverter.ToInt16(buffer,14);
			t.FrameStart =  System.BitConverter.ToUInt16(buffer,16);      // first frame in this animation
			t.FrameEnd =  System.BitConverter.ToUInt16(buffer,18);        // last frame in this animation (numframes = (End - Start) + 1)
			t.NextAnimation =  System.BitConverter.ToUInt16(buffer,20);
			t.NextFram =  System.BitConverter.ToUInt16(buffer,22);
			t.NumStateChanges =  System.BitConverter.ToUInt16(buffer,24);
			t.StateChangeOffset =  System.BitConverter.ToUInt16(buffer,26);  // offset into StateChanges[]
			t.NumAnimCommands =  System.BitConverter.ToUInt16(buffer,28);
			t.AnimCommand =  System.BitConverter.ToUInt16(buffer,30);        // offset into AnimCommands[]
			retval = t;
		}
		else if(type ==  typeof(Tr2StateChange)) 
		{   
			Tr2StateChange  t = new Tr2StateChange();
			t.StateID  =  System.BitConverter.ToUInt16(buffer,0);  ;
			t.NumAnimDispatches  =  System.BitConverter.ToUInt16(buffer,2);  ;  // number of dispatches (seems to always be 1..5)
			t.AnimDispatch  =  System.BitConverter.ToUInt16(buffer,4);  ;       // Offset into AnimDispatches[]
			retval = t;
		}
		else if(type ==  typeof(Tr2AnimDispatch)) 
		{   
			
			Tr2AnimDispatch  t = new Tr2AnimDispatch();
			t.Low =  System.BitConverter.ToInt16(buffer,0);
			t.High =  System.BitConverter.ToInt16(buffer,2);;
			t.NextAnimation =  System.BitConverter.ToInt16(buffer,4);;
			t.NextFrame =  System.BitConverter.ToInt16(buffer,6);;
			retval = t;
		}
		else if(type ==  typeof(Tr2AnimCommand)) 
		{   
			Tr2AnimCommand  t = new Tr2AnimCommand();
			t.Value = System.BitConverter.ToInt16(buffer,0);;
			retval = t;
		}
		else if(type ==  typeof(Tr2Moveable)) 
		{   
			Tr2Moveable  t = new Tr2Moveable();
			t.ObjectID =  System.BitConverter.ToUInt32(buffer,0);        // Item Identifier
			t.NumMeshes =  System.BitConverter.ToUInt16(buffer,4);       // number of meshes in this object
			t.StartingMesh =  System.BitConverter.ToUInt16(buffer,6);    // first mesh
			t.MeshTree =  System.BitConverter.ToUInt32(buffer,8);        // offset into MeshTree[]
			t.FrameOffset =  System.BitConverter.ToUInt32(buffer,12);    // byte offset into Frames[] (divide by 2 for Frames[i])
			t.Animation =  System.BitConverter.ToUInt16(buffer,16);      // offset into Animations[]
			retval = t;
			
		}
		else if(type ==  typeof(Tr2ObjectTextureVertex)) 
		{   
			Tr2ObjectTextureVertex t = new Tr2ObjectTextureVertex();
			t.Xcoordinate = buffer[0];
			t.Xpixel = buffer[1];
			t.Ycoordinate = buffer[2];
			t.Ypixel = buffer[3];
			retval = t;
		}
		else if(type ==  typeof(Tr2SpriteTexture)) 
		{   
			Tr2SpriteTexture t = new Tr2SpriteTexture();
			t.Tile =  System.BitConverter.ToUInt16(buffer,0);
			t.x = buffer[2];
			t.y = buffer[3];
			t.Width =  System.BitConverter.ToUInt16(buffer,4);     // actually, (width * 256) + 255
			t.Height =  System.BitConverter.ToUInt16(buffer,6);    // actually, (height * 256) + 255
			t.LeftSide =  System.BitConverter.ToInt16(buffer,8);
			t.TopSide =  System.BitConverter.ToInt16(buffer,10);
			t.RightSide =  System.BitConverter.ToInt16(buffer,12);
			t.Bottomside =  System.BitConverter.ToInt16(buffer,14);
			retval = t;
		}
		else if(type ==  typeof(Tr2SpriteSequence)) 
		{   
			Tr2SpriteSequence t = new Tr2SpriteSequence();
			t.ObjectID =  System.BitConverter.ToInt32(buffer,0);         // Item identifier (same numbering as in tr2_moveable)
			t.NegativeLength =  System.BitConverter.ToInt16(buffer,4);   // negative of "how many sprites are in this sequence"
			t.Offset =  System.BitConverter.ToInt16(buffer,6);           // where (in sprite texture list) this sequence starts
			retval = t;
		}
		else if(type ==  typeof(Tr2Camera)) 
		{ 
			Tr2Camera t = new Tr2Camera();
			t.x  =  System.BitConverter.ToInt32(buffer,0);
			t.y  =  System.BitConverter.ToInt32(buffer,4);
			t.z  =  System.BitConverter.ToInt32(buffer,8);
			t.Room  =  System.BitConverter.ToInt16(buffer,12); 
			t.Unknown1  =  System.BitConverter.ToUInt16(buffer,14);        // correlates to Boxes[]?
			retval = t;
		}
		else if(type ==  typeof(Tr2SoundSource)) 
		{   
			Tr2SoundSource t =new Tr2SoundSource();
			t.x =  System.BitConverter.ToInt32(buffer,0);;                 // position of sound source
			t.y =  System.BitConverter.ToInt32(buffer,4);;
			t.z =  System.BitConverter.ToInt32(buffer,8);;
			t.SoundID =  System.BitConverter.ToUInt16(buffer,12);;         // internal sound index
			t.Flags =  System.BitConverter.ToUInt16(buffer,14);;  ;        // 0x40, 0x80, or 0xc0
			retval = t;
		}
		else if(type ==  typeof(Tr2BBox)) 
		{   
			Tr2BBox t = new Tr2BBox();
			t.Zmin = buffer[0];             // sectors (* 1024 units)
			t.Zmax = buffer[1];
			t.Xmin = buffer[2];
			t.Xmax = buffer[3];
			t.TrueFloor =  System.BitConverter.ToInt16(buffer,4);        // Y value (no scaling)
			t.OverlapIndex =  System.BitConverter.ToInt16(buffer,6);     // index into Overlaps[]
			retval = t;
		}
		else if(type ==  typeof(Tr2Item)) 
		{   
			Tr2Item t = new Tr2Item();
			t.ObjectID  =  System.BitConverter.ToInt16(buffer,0); ;
			t.Room  =  System.BitConverter.ToInt16(buffer,2); ;
			t.x  =  System.BitConverter.ToInt32(buffer,4); ;
			t.y  =  System.BitConverter.ToInt32(buffer,8); ;
			t.z  =  System.BitConverter.ToInt32(buffer,12); ;
			t.Angle  =  System.BitConverter.ToInt16(buffer,16); ;
			t.Intensity1  =  System.BitConverter.ToInt16(buffer,18); ;
			t.Intensity2  =  System.BitConverter.ToInt16(buffer,20); ;
			t.Flags  =  System.BitConverter.ToInt16(buffer,22); ;         // 0x0100 indicates "inactive" or "invisible"
			retval = t;
		}
		else if(type ==  typeof(Tr2CinematicFrame))
		{
			Tr2CinematicFrame t = new Tr2CinematicFrame();
			t.rotY = System.BitConverter.ToInt16(buffer,2);           
			t.rotZ = System.BitConverter.ToInt16(buffer,2);              
			t.rotZ2 = System.BitConverter.ToInt16(buffer,2);               
			t.posZ = System.BitConverter.ToInt16(buffer,2);               
			t.posY = System.BitConverter.ToInt16(buffer,2);               
			t.posX = System.BitConverter.ToInt16(buffer,2);                
			t.Unknown1 = System.BitConverter.ToInt16(buffer,2);    
			t.rotX= System.BitConverter.ToInt16(buffer,2);   /*forgot to return it, causing error in some level :)*/     retval = t;   
		}
	
		return retval;
	}

	static void ExtractMeshes(Tr2Level Level, byte[] MeshData, uint NumMeshPointers, uint[] MeshPointers)
	{
		bool  NegativeSize;
		
		//Tr2Level Level = Level;
		Level.NumMeshes = (int) NumMeshPointers;
		Level.Meshes = new Tr2Mesh[Level.NumMeshes];
		
		//create a stream reader
		MemoryStream meshDataStream = new MemoryStream(MeshData);
		BinaryReader meshReader = new BinaryReader(meshDataStream);
		
		// get mesh start 
		//print("NumMeshPointers"+ NumMeshPointers);
		
		for (uint i = 0; i < NumMeshPointers; i++) 
		{
			//get mesh start 
			//print("MeshPointers["+i+"]"+ (int)MeshPointers[i]);
			
			int meshDataStartIndex = (int)MeshPointers[i];			//seek to meshDataStartIndex -> MeshPointer
			//byte meshDataStartByte = MeshData[meshDataStartIndex];
			meshDataStream.Seek(meshDataStartIndex,SeekOrigin.Begin);
			
			//read mesh center
			//int sizetr2Vertex =  6;
			//int sizetr2Vertex =  Tr2VertexSize;
			byte[] bcenter = meshReader.ReadBytes(Tr2VertexSize);
			Level.Meshes[i].Centre =(Tr2Vertex) Cast2Struct(bcenter,typeof (Tr2Vertex));
			
			//dummy read for bunkown
			meshReader.ReadBytes(4);
			
			//print("center:" + Level.Meshes[i].Centre.x+ " "+ Level.Meshes[i].Centre.y +" "+ Level.Meshes[i].Centre.z);
			
			//get number of vertices 
			Level.Meshes[i].NumVertices = meshReader.ReadInt16();
			//print("Level.Meshes["+i+"].NumVertices" + Level.Meshes[i].NumVertices);
			Level.Meshes[i].NumVertices = (short) Mathf.Abs(Level.Meshes[i].NumVertices);
			
			//get vertex list
			int sizeVertices = Tr2VertexSize * Level.Meshes[i].NumVertices;
			Level.Meshes[i].Vertices = new  Tr2Vertex [sizeVertices];
			
			for(int vtxCnt = 0; vtxCnt < Level.Meshes[i].NumVertices ; vtxCnt++)
			{
				byte[] vtxData =  meshReader.ReadBytes(Tr2VertexSize);
				Level.Meshes[i].Vertices[vtxCnt] = (Tr2Vertex) Cast2Struct(vtxData,typeof(Tr2Vertex));
				
			}
			
			//get number of normals
			Level.Meshes[i].NumNormals = meshReader.ReadInt16();
			NegativeSize = (Level.Meshes[i].NumNormals < 0);
			Level.Meshes[i].NumNormals = (short)Mathf.Abs(Level.Meshes[i].NumNormals);
			
			//get normal list 
			if (NegativeSize) 
			{
				//now Level.Meshes[i].NumNormals mean number of MeshLights
				int numMeshLights = Level.Meshes[i].NumNormals;
				Level.Meshes[i].MeshLights = new short[numMeshLights]; //could be turned in to byte array
				for(int meshLightCount = 0; meshLightCount < numMeshLights ;  meshLightCount++)
				{
					Level.Meshes[i].MeshLights[meshLightCount] = meshReader.ReadInt16();
				}
				
				NegativeSize = false;
			}
			else 
			{
				int numMeshNormals =  Level.Meshes[i].NumNormals;
				Level.Meshes[i].Normals = new Tr2Vertex[numMeshNormals];
				for(int nrmCount = 0 ; nrmCount < numMeshNormals; nrmCount++)
				{
					byte[] normData = meshReader.ReadBytes(Tr2VertexSize);
					Level.Meshes[i].Normals[nrmCount] = (Tr2Vertex) Cast2Struct(normData,typeof(Tr2Vertex));
				}
			}
			
			// get list of textured rectangles 
			Level.Meshes[i].NumTexturedRectangles = meshReader.ReadInt16();
			Level.Meshes[i].NumTexturedRectangles = (short) Mathf.Abs(Level.Meshes[i].NumTexturedRectangles);
			//print("Level.Meshes["+i+"].NumTexturedRectangles " + Level.Meshes[i].NumTexturedRectangles);
			
			if (Level.Meshes[i].NumTexturedRectangles > 0) 
			{
				Level.Meshes[i].TexturedRectangles = new Tr2Face4[Level.Meshes[i].NumTexturedRectangles];
				
				if (Level.EngineVersion == TR2VersionType.TombRaider_4) 
				{
					for (int j = 0; j < Level.Meshes[i].NumTexturedRectangles; j++) 
					{
						byte[] brectData = meshReader.ReadBytes(Tr2Face4Size);
						Level.Meshes[i].TexturedRectangles[j] = (Tr2Face4)Cast2Struct(brectData,typeof(Tr2Face4));
						
						meshReader.ReadBytes(2);  //read 2 dummy bytes
					}
				}
				else 
				{
					for (int j = 0; j < Level.Meshes[i].NumTexturedRectangles; j++) 
					{
						byte[] brectData = meshReader.ReadBytes(Tr2Face4Size);
						Level.Meshes[i].TexturedRectangles[j] = (Tr2Face4)Cast2Struct(brectData,typeof(Tr2Face4));
						//no dummy bytes
					}
				}
			}

			// get list of textured NumTexturedTriangles 
			Level.Meshes[i].NumTexturedTriangles = meshReader.ReadInt16();
			Level.Meshes[i].NumTexturedTriangles = (short) Mathf.Abs(Level.Meshes[i].NumTexturedTriangles);
			//print("Level.Meshes["+i+"].NumTexturedTriangles  " + Level.Meshes[i].NumTexturedTriangles );
			
			if (Level.Meshes[i].NumTexturedTriangles  > 0) 
			{
				Level.Meshes[i].TexturedTriangles  = new Tr2Face3[Level.Meshes[i].NumTexturedTriangles ];
				
				if (Level.EngineVersion == TR2VersionType.TombRaider_4) 
				{
					
					for (int j = 0; j < Level.Meshes[i].NumTexturedTriangles ; j++) 
					{
						byte[] btriData = meshReader.ReadBytes(Tr2Face3Size);
						Level.Meshes[i].TexturedTriangles[j] = (Tr2Face3)Cast2Struct(btriData,typeof(Tr2Face3));
						
						meshReader.ReadBytes(2);  //read 2 dummy bytes
					}
				}
				else 
				{
					for (int j = 0; j < Level.Meshes[i].NumTexturedTriangles; j++) 
					{
						byte[] btriData = meshReader.ReadBytes(Tr2Face3Size);
						Level.Meshes[i].TexturedTriangles[j] = (Tr2Face3)Cast2Struct(btriData,typeof(Tr2Face3));
						//no dummy bytes
					}
				}
			}
			
			// get list of colored rectangles 
			Level.Meshes[i].NumColouredRectangles = meshReader.ReadInt16();
			Level.Meshes[i].NumColouredRectangles = (short) Mathf.Abs(Level.Meshes[i].NumColouredRectangles);
			//print("Level.Meshes["+i+"].NumColouredRectangles " + Level.Meshes[i].NumColouredRectangles);
			if(Level.Meshes[i].NumColouredRectangles > 0)
			{
				Level.Meshes[i].ColouredRectangles = new Tr2Face4[Level.Meshes[i].NumColouredRectangles];
				
				for(int j = 0; j < Level.Meshes[i].NumColouredRectangles; j++)
				{
					byte[] brectColorData = meshReader.ReadBytes(Tr2Face4Size);
					Level.Meshes[i].ColouredRectangles[j] = (Tr2Face4)Cast2Struct(brectColorData,typeof(Tr2Face4));
				}
			}
			
			// get list of colored Triangles 
			Level.Meshes[i].NumColouredTriangles = meshReader.ReadInt16();
			Level.Meshes[i].NumColouredTriangles = (short) Mathf.Abs(Level.Meshes[i].NumColouredTriangles);
			//print("Level.Meshes["+i+"].NumColouredTriangles " + Level.Meshes[i].NumColouredTriangles);
			if(Level.Meshes[i].NumColouredTriangles > 0)
			{
				Level.Meshes[i].ColouredTriangles = new Tr2Face3[Level.Meshes[i].NumColouredTriangles];
				
				for(int j = 0; j < Level.Meshes[i].NumColouredTriangles; j++)
				{
					byte[] btriColorData = meshReader.ReadBytes(Tr2Face3Size);
					Level.Meshes[i].ColouredTriangles[j] = (Tr2Face3)Cast2Struct(btriColorData,typeof(Tr2Face3));
				}
			}
			
		}
		//done
	}
	
	static TR2VersionType getTREngineVersionFromVersion(uint version)
	{
		TR2VersionType retval = TR2VersionType.TombRaider_UnknownVersion;
		switch(version)
		{
		case 0x00000020: retval = TR2VersionType.TombRaider_1; break;
		case 0x0000002d: retval = TR2VersionType.TombRaider_2; break;
		case 0xff080038: retval = TR2VersionType.TombRaider_3; break;
		case 0xff180038: retval = TR2VersionType.TombRaider_3; break;
		case 0xfffffff0: retval = TR2VersionType.TombRaider_4; break; // bogus
		case 0x00345254: retval = TR2VersionType.TombRaider_4; break; // "TR4\0"
		case 0x00000000: retval = TR2VersionType.TombRaider_UnknownVersion; break;
		}
		return retval;
	}

	public static Tr2Level Parse(byte[] unzippeddata)
	{
		m_Level = new Tr2Level();
		MemoryStream  ms = new MemoryStream(unzippeddata);
		BinaryReader br = new BinaryReader(ms);

		if(System.BitConverter.IsLittleEndian)
		{
			//readm_LevelVersion
			m_Level.Version  =br.ReadUInt32();
			//print("m_Level.Version " + m_Level.Version );
			
			m_Level.EngineVersion = getTREngineVersionFromVersion(m_Level.Version);
			//print("m_Level.EngineVersion " +m_Level.EngineVersion );
			if (m_Level.EngineVersion != TR2VersionType.TombRaider_2) 
			{
				return null;
			}
			
			
			m_Level.Palette8  = new Tr2Colour[256];
			m_Level.Palette16 = new uint[256];
			for(int colorIdx = 0 ; colorIdx < 256; colorIdx++)
			{
				m_Level.Palette8 [colorIdx].Red = br.ReadByte();
				m_Level.Palette8 [colorIdx].Green = br.ReadByte();
				m_Level.Palette8 [colorIdx].Blue = br.ReadByte();
			}
			
			
			for(int colorIdx = 0 ; colorIdx < 256; colorIdx++)
			{
					
				m_Level.Palette16[colorIdx] = br.ReadUInt32();
			}

			//read NumTexTiles
			m_Level.NumTextiles = br.ReadUInt32();//System.BitConverter.ToUInt32(m_TrLevelData.bytes, sizeof(uint));
			//print("m_Level.NumTextiles " + m_Level.NumTextiles );
				
			//Tr2Textile8 
			m_Level.Textile8 = new Tr2Textile8[m_Level.NumTextiles];
			for(int texTilecount = 0 ;  texTilecount < m_Level.NumTextiles; texTilecount++)
			{
				m_Level.Textile8[texTilecount].Tile = br.ReadBytes(256 * 256);
			}
			
			//Tr2Textile16 
			m_Level.Textile16 = new Tr2Textile16[m_Level.NumTextiles];
			for(int texTilecount = 0 ;  texTilecount < m_Level.NumTextiles; texTilecount++)
			{
						
				m_Level.Textile16[texTilecount].Tile = new ushort[256 * 256];
				for(uint shortcnt = 0; shortcnt < (256 * 256); shortcnt++)
				{
					m_Level.Textile16[texTilecount].Tile[shortcnt] = br.ReadUInt16();
				}
			}
				
			m_Level.m_TexWidth = m_Level.m_MaxWidth;
			m_Level.m_TexHeight = (int)m_Level.NumTextiles * m_Level.m_TexWidth;
			m_Level.m_MaxTiles = (int)m_Level.NumTextiles;
			//padTiles = 16 - m_MaxTiles;
				
			m_Level.UnknownT = br.ReadUInt32(); //unused
			m_Level.NumRooms = br.ReadUInt16();
			//print("m_Level.NumRooms:"+m_Level.NumRooms);
			//extract room details 
			m_Level.Rooms = new Tr2Room[m_Level.NumRooms];
			for (int i = 0; i < m_Level.NumRooms; ++i) 
			{
				//print("process  room:"+i);
				//print("-------------:");
				//print("-------------:");
				
				//get room global data of length 
				//get room info
				//m_Level.Rooms[i].info = new Tr2RoomInfo();
				
				byte[] tmpArr =  br.ReadBytes(Tr2RoomInfoSize);
				m_Level.Rooms[i].info =(Tr2RoomInfo) Cast2Struct(tmpArr,typeof(Tr2RoomInfo)); 
				//print("m_Level.Rooms["+i+"].info length" + sizeData);
				//print("m_Level.Rooms["+i+"].info x z: " + m_Level.Rooms[i].info.x + " " + m_Level.Rooms[i].info.z);
				
				m_Level.Rooms[i].NumDataWords = br.ReadUInt32(); 
				int sizeRoomData = sizeof(ushort) * ((int)m_Level.Rooms[i].NumDataWords);
				m_Level.Rooms[i].Data = br.ReadBytes(sizeRoomData);
				//print("m_Level.Rooms["+i+"].Data" +System.Buffer.ByteLength(m_Level.Rooms[i].Data));
				
				
				byte[] dataArr = m_Level.Rooms[i].Data;  //variable length data for this room
				
				//create a stream reader
				MemoryStream roomDataStream = new MemoryStream(dataArr);
				BinaryReader roomReader = new BinaryReader(roomDataStream);
				
				//process ushort NumVertices;
				m_Level.Rooms[i].RoomData.NumVertices = roomReader.ReadInt16();
				
				if(m_Level.Rooms[i].RoomData.NumVertices > 0)
				{
					//print("m_Level.Rooms["+i+"].RoomData.NumVertices" +m_Level.Rooms[i].RoomData.NumVertices);
					m_Level.Rooms[i].RoomData.Vertices = new Tr2VertexRoom[m_Level.Rooms[i].RoomData.NumVertices];
					
					for(int vertAttribCount = 0; vertAttribCount < m_Level.Rooms[i].RoomData.NumVertices; vertAttribCount++)
					{
						byte[] bVertData = roomReader.ReadBytes(Tr2VertexRoomSize);
						m_Level.Rooms[i].RoomData.Vertices[vertAttribCount] = (Tr2VertexRoom) Cast2Struct(bVertData, typeof(Tr2VertexRoom));
						
						//print("m_Level.Rooms["+i+"].RoomData.Vertices"+"["+ vertAttribCount +"]"+".Lighting1 value" + 
						//m_Level.Rooms[i].RoomData.Vertices[vertAttribCount].Lighting1);
						//DataOffset = DataOffset + (uint) sizeVertex;
					}
				}

				//process NumRectangles	
				m_Level.Rooms[i].RoomData.NumRectangles = roomReader.ReadInt16();
				
				//print("m_Level.Rooms["+i+"].RoomData.NumRectangles" +m_Level.Rooms[i].RoomData.NumRectangles);
				
				if(m_Level.Rooms[i].RoomData.NumRectangles > 0)
				{
					m_Level.Rooms[i].RoomData.Rectangles = new Tr2Face4[m_Level.Rooms[i].RoomData.NumRectangles];
					for(int rectCount = 0; rectCount < m_Level.Rooms[i].RoomData.NumRectangles; rectCount++)
					{
						
						byte[] bVertData = roomReader.ReadBytes(Tr2Face4Size);
						m_Level.Rooms[i].RoomData.Rectangles[rectCount] = (Tr2Face4)Cast2Struct(bVertData,typeof(Tr2Face4));
					}
				}
				
				//process NumTriangles
				m_Level.Rooms[i].RoomData.NumTriangles = roomReader.ReadInt16();
				//print("m_Level.Rooms["+i+"].RoomData.NumTriangles" +m_Level.Rooms[i].RoomData.NumTriangles);
				
				if(m_Level.Rooms[i].RoomData.NumTriangles  > 0)
				{
					m_Level.Rooms[i].RoomData.Triangles = new Tr2Face3[m_Level.Rooms[i].RoomData.NumTriangles];
					
					for(int triCount = 0; triCount < m_Level.Rooms[i].RoomData.NumTriangles; triCount++)
					{

						byte[] bVertData = roomReader.ReadBytes(Tr2Face3Size);
						m_Level.Rooms[i].RoomData.Triangles[triCount] = (Tr2Face3)Cast2Struct(bVertData,typeof(Tr2Face3));
					}
				}
				
				
				//process Numsprites
				m_Level.Rooms[i].RoomData.Numsprites = roomReader.ReadInt16();
				//print("m_Level.Rooms["+i+"].RoomData.Numsprites" +m_Level.Rooms[i].RoomData.Numsprites);
				
				if(m_Level.Rooms[i].RoomData.Numsprites > 0)
				{
					m_Level.Rooms[i].RoomData.Sprites = new Tr2RoomSprite[m_Level.Rooms[i].RoomData.Numsprites];
					
					for(int spriteCount = 0; spriteCount < m_Level.Rooms[i].RoomData.Numsprites; spriteCount++)
					{
						byte[] bVertData = roomReader.ReadBytes(Tr2RoomSpriteSize);
						m_Level.Rooms[i].RoomData.Sprites[spriteCount] = (Tr2RoomSprite) Cast2Struct(bVertData,typeof(Tr2RoomSprite));
					}
				}
				
				//Done:
				//struct Tr2RoomData
				//now can free room data
				
				m_Level.Rooms[i].NumPortals = br.ReadUInt16();
				
				//print("m_Level.Rooms["+i+"].NumPortals"+ m_Level.Rooms[i].NumPortals);
				
				if(m_Level.Rooms[i].NumPortals > 0)
				{
					m_Level.Rooms[i].Portals = new Tr2RoomPortal [m_Level.Rooms[i].NumPortals];
					
					for(int portalCount = 0; portalCount < m_Level.Rooms[i].NumPortals; portalCount++)
					{
						//public ushort AdjoiningRoom;   // which room this "door" leads to 2 bytes
						//public Tr2Vertex Normal;       // which way the "door" faces  6 bytes
						//public Tr2Vertex[] Vertices;  // the 4 corners of the "door"  4 * 6 = 24 byte
						
						m_Level.Rooms[i].Portals[portalCount].AdjoiningRoom = br.ReadUInt16();
						byte[] dataPortalNormal = br.ReadBytes(Tr2VertexSize);
						m_Level.Rooms[i].Portals[portalCount].Normal = (Tr2Vertex) Cast2Struct(dataPortalNormal,typeof(Tr2Vertex));
						
						m_Level.Rooms[i].Portals[portalCount].Vertices = new Tr2Vertex[4];
						for(int vtxcount = 0;  vtxcount < 4 ;  vtxcount++)
						{
							byte[] dataPortalVertices = br.ReadBytes(Tr2VertexSize);
							m_Level.Rooms[i].Portals[portalCount].Vertices[vtxcount] = 
								(Tr2Vertex) Cast2Struct(dataPortalVertices,typeof(Tr2Vertex));
						}
						
					}
				}
				
				// read sector info 
				m_Level.Rooms[i].NumZsectors = br.ReadUInt16();
				m_Level.Rooms[i].NumXsectors = br.ReadUInt16();
				int numsector = m_Level.Rooms[i].NumZsectors * m_Level.Rooms[i].NumXsectors;
				
				//print("m_Level.Rooms["+i+"].NumZsectors:"+ m_Level.Rooms[i].NumZsectors );
				//print("m_Level.Rooms["+i+"].NumXsectors:"+ m_Level.Rooms[i].NumXsectors );

				if(numsector > 0)
				{
					m_Level.Rooms[i].SectorList = new Tr2RoomSector[numsector];
					for(int sectorCount = 0 ; sectorCount < numsector; sectorCount++)
					{
						byte[] sectorData = br.ReadBytes(Tr2RoomSectorSize);
						m_Level.Rooms[i].SectorList[sectorCount] =  (Tr2RoomSector) Cast2Struct(sectorData, typeof(Tr2RoomSector));
					}
				}
				
				//read room lighting & mode 
				//TR2fread(&Level->Rooms[i].Intensity1, 6, 1, m_FP);
				m_Level.Rooms[i].Intensity1 = br.ReadInt16();
				m_Level.Rooms[i].Intensity2 = br.ReadInt16();
				m_Level.Rooms[i].LightMode =  br.ReadInt16();
					
				//print("Light [TR2|[TR4][Intensity1] [Intensity2] [mode]:" + 
				//m_Level.Rooms[i].Intensity1 + " " + 
				//m_Level.Rooms[i].Intensity2 + " " + 
				//m_Level.Rooms[i].LightMode);
				
				// read room lighting info 
				m_Level.Rooms[i].NumLights = br.ReadUInt16();
				//print("m_Level.Rooms["+i+"].NumLights:" + (int)m_Level.Rooms[i].NumLights);
				
				if (m_Level.Rooms[i].NumLights > 0) 
				{
					m_Level.Rooms[i].Lights = new  Tr2RoomLight[m_Level.Rooms[i].NumLights];
					for(int lightCnt = 0; lightCnt < m_Level.Rooms[i].NumLights;  lightCnt++)
					{
						byte[] lightData = br.ReadBytes(Tr2RoomLightSize);
						m_Level.Rooms[i].Lights[lightCnt] = (Tr2RoomLight) Cast2Struct(lightData,typeof(Tr2RoomLight));
					}
					
				}
				
				//read Static Mesh Data
				m_Level.Rooms[i].NumStaticMeshes = br.ReadUInt16();
				//print("m_Level.Rooms["+i+"].NumStaticMeshes" +	m_Level.Rooms[i].NumStaticMeshes);
				
				if (m_Level.Rooms[i].NumStaticMeshes > 0) 
				{
					m_Level.Rooms[i].StaticMeshes =new Tr2RoomStaticMesh [m_Level.Rooms[i].NumStaticMeshes];
					for (int meshCnt = 0; meshCnt < m_Level.Rooms[i].NumStaticMeshes; meshCnt++) 
					{
						byte[] meshData = br.ReadBytes(Tr2RoomSaticMeshSize);
						m_Level.Rooms[i].StaticMeshes[meshCnt] = (Tr2RoomStaticMesh) Cast2Struct(meshData,typeof(Tr2RoomStaticMesh));
					}
				}
	
				//AlternateRoom settings
				m_Level.Rooms[i].AlternateRoom = br.ReadInt16();
				m_Level.Rooms[i].Flags = br.ReadInt16();
								
				//Done:
			}
		}

		//floor data
		m_Level.NumFloorData = br.ReadUInt32();
		//print("m_Level.NumFloorData"+ m_Level.NumFloorData);
		if (m_Level.NumFloorData > 0) 
		{
			m_Level.FloorData = new ushort[m_Level.NumFloorData];
			for(int floorDataCnt = 0; floorDataCnt < m_Level.NumFloorData;  floorDataCnt++)
			{
				m_Level.FloorData[floorDataCnt] = br.ReadUInt16();
			}
		}
		
		uint NumMeshDataWords; 
		byte[] RawMeshData = null;;	
		int dataSize = 0;
		
		NumMeshDataWords = br.ReadUInt32();	
		if(NumMeshDataWords > 0)
		{
			dataSize = (int)(2 * NumMeshDataWords);
			RawMeshData = br.ReadBytes(dataSize);
		}
		
		m_Level.NumMeshPointers =br.ReadUInt32();
		if(m_Level.NumMeshPointers > 0)
		{	
			m_Level.MeshPointerList = new uint[m_Level.NumMeshPointers];
			for(int ptrCount = 0; ptrCount < m_Level.NumMeshPointers; ptrCount++)
			{
				m_Level.MeshPointerList[ptrCount] = br.ReadUInt32();
			}
		}
		
		ExtractMeshes(m_Level,RawMeshData, m_Level.NumMeshPointers, m_Level.MeshPointerList);		
		//read animation data
		m_Level.NumAnimations = br.ReadUInt32();
		//print("m_Level.NumAnimations:" + m_Level.NumAnimations);	
		
		if (m_Level.NumAnimations > 0) 
		{
			m_Level.Animations = new Tr2Animation[m_Level.NumAnimations];
			for(int animCnt = 0; animCnt < m_Level.NumAnimations; animCnt++)
			{
				byte[] animData = br.ReadBytes(Tr2AnimationSize);
				m_Level.Animations[animCnt] = (Tr2Animation) Cast2Struct(animData, typeof(Tr2Animation));
					
				//print("m_Level.Animations["+animCnt+"].FrameOffset" +m_Level.Animations[animCnt].FrameOffset);     
				//print("m_Level.Animations["+animCnt+"].FrameRate" + m_Level.Animations[animCnt].FrameRate);      
				//print("m_Level.Animations["+animCnt+"].FrameSize" + m_Level.Animations[animCnt].FrameSize);       
			}
		}
		
		//read state changes
		m_Level.NumStateChanges = br.ReadUInt32();
		if (m_Level.NumStateChanges > 0) 
		{
			m_Level.StateChanges = new Tr2StateChange[m_Level.NumStateChanges];
			for(int stateChangeCount = 0; stateChangeCount <m_Level.NumStateChanges;stateChangeCount++)
			{
				byte[] state_change_data = br.ReadBytes(Tr2StateChangeSize);
				m_Level.StateChanges[stateChangeCount] = (Tr2StateChange) Cast2Struct(state_change_data,typeof(Tr2StateChange));
				//print("m_Level.StateChanges["+stateChangeCount+"].StateID"+ m_Level.StateChanges[stateChangeCount] .StateID);
			}
		}
		
		//read AnimDispatches 
		m_Level.NumAnimDispatches = br.ReadUInt32();
		//print("m_Level.NumAnimDispatches:"+ m_Level.NumAnimDispatches);
		if ( m_Level.NumAnimDispatches > 0) 
		{
			m_Level.AnimDispatches = new Tr2AnimDispatch[m_Level.NumAnimDispatches];
			for(int animDispatchCount = 0; animDispatchCount < m_Level.NumAnimDispatches;animDispatchCount++)
			{
				byte[] anim_dispatch_data  = br.ReadBytes(Tr2AnimDispatchSize);
				m_Level.AnimDispatches[animDispatchCount] = (Tr2AnimDispatch) Cast2Struct(anim_dispatch_data,typeof(Tr2AnimDispatch));
			}
		}
		
		//read anim commands
		m_Level.NumAnimCommands = br.ReadUInt32();
		//print("m_Level.NumAnimCommands" + m_Level.NumAnimCommands);
		if (m_Level.NumAnimCommands > 0) 
		{
			m_Level.AnimCommands = new Tr2AnimCommand[m_Level.NumAnimCommands];
			for(int animCommandCount = 0;  animCommandCount < m_Level.NumAnimCommands;   animCommandCount++)
			{
				byte[] anim_command_data =  br.ReadBytes(Tr2AnimCommandSize);
				m_Level.AnimCommands[animCommandCount] = (Tr2AnimCommand) Cast2Struct(anim_command_data,typeof(Tr2AnimCommand));
			} 
		}  
		
		
		// read MeshTrees
		m_Level.NumMeshTrees = br.ReadUInt32();  //total number of ints, not Tr2MeshTree
		//print("m_Level.NumMeshTrees" + m_Level.NumMeshTrees);
	
		//int numMeshTreeStruct =  (int)(m_Level.NumMeshTrees / sizeTr2MeshTree); 
		//print("numMeshTreeStruct" + numMeshTreeStruct);
		
		if ( m_Level.NumMeshTrees > 0) 
		{
			m_Level.MeshTrees = new int[m_Level.NumMeshTrees];  //read as int
			for(int meshtreecnt =0 ; meshtreecnt < m_Level.NumMeshTrees; meshtreecnt++)
			{
				m_Level.MeshTrees [meshtreecnt] = br.ReadInt32();
			}
		}
		
		// read frames 
		m_Level.NumFrames = br.ReadUInt32(); 
		//print( "m_Level.NumFrames" +  m_Level.NumFrames);
		
		if (m_Level.NumFrames > 0) 
		{
			m_Level.Frames = new ushort[m_Level.NumFrames];
			for(int frmCount = 0; frmCount < m_Level.NumFrames;frmCount++)
			{
				m_Level.Frames[frmCount] = br.ReadUInt16();
			}
		}
		
		// read moveables 
		m_Level.NumMoveables = br.ReadUInt32(); 
		//print("m_Level.NumMoveables" + m_Level.NumMoveables);
		if (m_Level.NumMoveables > 0) 
		{	
			m_Level.Moveables = new Tr2Moveable[m_Level.NumMoveables];
			for(int movableCount = 0;  movableCount  < m_Level.NumMoveables; movableCount++)
			{
				byte[] moveable_item_data = br.ReadBytes(Tr2MoveableSize);
				m_Level.Moveables[movableCount] = (Tr2Moveable) Cast2Struct(moveable_item_data,typeof(Tr2Moveable));
			}
		}
	
		//read static mesh
		m_Level.NumStaticMeshes = br.ReadUInt32(); 
		if (m_Level.NumStaticMeshes > 0) 
		{ 
			m_Level.StaticMeshes = new Tr2StaticMesh[m_Level.NumStaticMeshes];
			for(int staticMeshCount = 0; staticMeshCount < m_Level.NumStaticMeshes;staticMeshCount++)
			{
				m_Level.StaticMeshes[staticMeshCount].BoundingBox = new Tr2Vertex[4];
				m_Level.StaticMeshes[staticMeshCount].ObjectID = br.ReadUInt32(); 
				m_Level.StaticMeshes[staticMeshCount].StartingMesh = br.ReadUInt16(); 
				
				for(int boundingBoxCount = 0;boundingBoxCount < 4;boundingBoxCount++)
				{
					byte[] staticBoxData = br.ReadBytes(Tr2VertexSize);
					m_Level.StaticMeshes[staticMeshCount].BoundingBox[boundingBoxCount] =
						(Tr2Vertex) Cast2Struct(staticBoxData,typeof(Tr2Vertex));
				}
				
				m_Level.StaticMeshes[staticMeshCount].Flags = br.ReadUInt16(); 
				//print("m_Level.StaticMeshes["+staticMeshCount+"].ObjectID " + m_Level.StaticMeshes[staticMeshCount].ObjectID);  
			}
		}
		
		// read object textures 
		m_Level.NumObjectTextures = br.ReadUInt32(); 
		//print("m_Level.NumObjectTextures:"+ m_Level.NumObjectTextures);
		
		if (m_Level.NumObjectTextures > 0) 
		{
			m_Level.ObjectTextures = new Tr2ObjectTexture[m_Level.NumObjectTextures];
			for(int objTexCount = 0; objTexCount < m_Level.NumObjectTextures; objTexCount++)
			{
				m_Level.ObjectTextures[objTexCount].Vertices = new Tr2ObjectTextureVertex[4];
				m_Level.ObjectTextures[objTexCount].TransparencyFlags = br.ReadUInt16();
				m_Level.ObjectTextures[objTexCount].Tile = br.ReadUInt16();
					
				for(int vtxcount = 0 ; vtxcount < 4; vtxcount++)
				{
					byte[] object_texture_vert_data = br.ReadBytes(Tr2ObjectTextureVertSize);
					m_Level.ObjectTextures[objTexCount].Vertices[vtxcount] =
							(Tr2ObjectTextureVertex) Cast2Struct(object_texture_vert_data, typeof(Tr2ObjectTextureVertex));
				}

			}	
		}
		
		//read sprite textures 
		m_Level.NumspriteTextures = br.ReadUInt32(); 
		//print("m_Level.NumspriteTextures"+ m_Level.NumspriteTextures);
		if (m_Level.NumspriteTextures > 0) 
		{
			m_Level.SpriteTextures = new Tr2SpriteTexture[m_Level.NumspriteTextures];
			
			for(int spriteTexCount = 0;  spriteTexCount < m_Level.NumspriteTextures; spriteTexCount++)
			{
				byte[] spriteTexData = br.ReadBytes(Tr2SpriteTextureSize);
				m_Level.SpriteTextures[spriteTexCount] = 
					(Tr2SpriteTexture) Cast2Struct(spriteTexData, typeof(Tr2SpriteTexture));
			}
			
		}
		
		//read sprite texture data
		m_Level.NumSpriteSequences = br.ReadUInt32(); 
		//print("m_Level.NumSpriteSequences"+ m_Level.NumSpriteSequences);
		if (m_Level.NumSpriteSequences > 0) 
		{
			m_Level.SpriteSequences = new Tr2SpriteSequence[m_Level.NumSpriteSequences];
			for(int spriteSeqCount = 0; spriteSeqCount < m_Level.NumSpriteSequences; spriteSeqCount++)
			{
				byte[] spriteSeqData = br.ReadBytes(Tr2SpriteSequenceSize);
				m_Level.SpriteSequences[spriteSeqCount] = (Tr2SpriteSequence) Cast2Struct(spriteSeqData,typeof(Tr2SpriteSequence));
			}
		}   
		
		//read cameras
		m_Level.NumCameras = br.ReadInt32(); 
		//print("m_Level.NumCameras"+ m_Level.NumCameras);
		
		if (m_Level.NumCameras > 0) 
		{
			m_Level.Cameras = new Tr2Camera[m_Level.NumCameras];
			for(int camCount = 0 ; camCount <m_Level.NumCameras; camCount++)
			{
				byte[] camData = br.ReadBytes(Tr2CameraSize);
				m_Level.Cameras[camCount] = (Tr2Camera) Cast2Struct(camData,typeof(Tr2Camera));
			}
		}
		
		// read sound effects 
		m_Level.NumsoundSources = br.ReadInt32(); 
		//print("m_Level.NumsoundSources" + m_Level.NumsoundSources);
		if (m_Level.NumsoundSources > 0) 
		{
			m_Level.SoundSources = new Tr2SoundSource[m_Level.NumsoundSources];
			for(int soundSrcCount = 0; soundSrcCount < m_Level.NumsoundSources; soundSrcCount++)
			{
				byte[] sndSrcData = br.ReadBytes(Tr2SoundSourceSize);
				m_Level.SoundSources[soundSrcCount] = 
					(Tr2SoundSource) Cast2Struct(sndSrcData,typeof(Tr2SoundSource));
				
				//print("m_Level.SoundSources[soundSrcCount] x y z" + m_Level.SoundSources[soundSrcCount].x);
			}
		}
		
		//read boxes 
		m_Level.NumBoxes = br.ReadInt32(); 
		//print("m_Level.NumBoxes:"+ m_Level.NumBoxes);
		if (m_Level.NumBoxes > 0) 
		{
			m_Level.Boxes = new Tr2BBox[m_Level.NumBoxes];   
			//load tr2box data
			for( int tr2BoxCount = 0; tr2BoxCount <m_Level.NumBoxes; tr2BoxCount++)
			{
				byte[] tr2_boxData = br.ReadBytes(Tr2BBoxSize);
				m_Level.Boxes[tr2BoxCount] = (Tr2BBox) Cast2Struct(tr2_boxData,typeof(Tr2BBox));
			}
		}
		
		//read overlaps 
		m_Level.NumOverlaps =br.ReadInt32();
		//print("m_Level.NumOverlaps" + m_Level.NumOverlaps);
		
		if (m_Level.NumOverlaps > 0) 
		{
			m_Level.Overlaps =  new short[m_Level.NumOverlaps ];
			for(int overlapCnt = 0; overlapCnt < m_Level.NumOverlaps;overlapCnt++)
			{
				m_Level.Overlaps[overlapCnt] = br.ReadInt16();
			}
		}
		
		//read Zones 
		if (m_Level.NumBoxes > 0) 
		{
			m_Level.Zones =  new short[10 * m_Level.NumBoxes]; //10 shorts == 20 byte
			int sizeZones = 10 * m_Level.NumBoxes;
			for( int zoneCount = 0; zoneCount < sizeZones; zoneCount++)
			{
				m_Level.Zones[zoneCount] = br.ReadInt16();
			}
			
		}
		
		//read animation textures
		m_Level.NumAnimatedTextures = br.ReadInt32();
		if (m_Level.NumAnimatedTextures > 0) 
		{
			m_Level.AnimatedTextures = new short[m_Level.NumAnimatedTextures];
			for( int animTexCount = 0; animTexCount < m_Level.NumAnimatedTextures ; animTexCount++)
			{
				m_Level.AnimatedTextures[animTexCount] = br.ReadInt16();
			}
		}
		
		//08-09-2011
		//read items  
		m_Level.NumItems = br.ReadInt32();
		if(m_Level.NumItems > 0)
		{
			m_Level.Items = new Tr2Item[m_Level.NumItems];
			for (int trItemCount = 0; trItemCount < m_Level.NumItems ; trItemCount++) 
			{
				byte[] tr_itemData = br.ReadBytes(Tr2ItemSize);
				m_Level.Items[trItemCount] = (Tr2Item) Cast2Struct(tr_itemData,typeof(Tr2Item));	
			}
		}
		
		//read LightMaps 
		int numLightMapData = 32 * 256;
		m_Level.LightMap =  new byte[numLightMapData];
		for(int byteCnt = 0; byteCnt < numLightMapData; byteCnt++)
		{
			m_Level.LightMap[byteCnt] = br.ReadByte();
		}
		
		//read cinematic frames 
		m_Level.NumCinematicFrames  = br.ReadUInt16();
		if (m_Level.NumCinematicFrames > 0) 
		{
			m_Level.CinematicFrames = new  Tr2CinematicFrame[m_Level.NumCinematicFrames];
			for(int frameCount = 0; frameCount < m_Level.NumCinematicFrames; frameCount++)
			{
				byte[] tr_cenematic_data = br.ReadBytes(Tr2CinematicFrameSize);
				m_Level.CinematicFrames[frameCount] = (Tr2CinematicFrame) Cast2Struct(tr_cenematic_data, typeof(Tr2CinematicFrame));
			}
		}

		//read demodata
		m_Level.NumDemoData = br.ReadInt16();
		if (m_Level.NumDemoData > 0) 
		{
			m_Level.DemoData = new byte[m_Level.NumDemoData];
			m_Level.DemoData = br.ReadBytes(m_Level.NumDemoData);
		}
		
		return m_Level;
	}
}

