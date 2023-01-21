using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TR4
{
    public class ParserExtenstion
    {
        class Tr4ObjectTexture // 38 bytes
        {
            ushort Attribute;
            ushort TileAndFlag;
            ushort NewFlags;

            Parser.Tr2ObjectTextureVertex[] Vertices; // The four corners of the texture

            uint OriginalU;
            uint OriginalV;
            uint Width;     // Actually width-1
            uint Height;    // Actually height-1
        }

        public class Tr4Animation
        {
            public uint SpeedLateral; // New field
            public uint AccelLateral; // New field
            public uint Speed;            //Freal=Pwhole+(Pfrac÷65536)
            public uint Accel;            //Freal=Pwhole+(Pfrac÷65536)
        }

        public class Tr2Textile32 //262144 bytes
        {
            public uint[] Tile; //256 * 256
        }


        struct tr4_room_light   // 46 bytes
        {
            public int x, y, z;       // Position of light, in world coordinates
            Parser.Tr2Colour Colour;        // Colour of the light

            byte LightType;
            byte Unknown;       // Always 0xFF?
            byte Intensity;

            float In;            // Also called hotspot in TRLE manual
            float Out;           // Also called falloff in TRLE manual
            float Length;
            float CutOff;

            float dx, dy, dz;    // Direction - used only by sun and spot lights
        };

        public class tr4Level
        {
            public int NumRoomTextiles;
            public int NumObjTextiles;
            public int NumBumpTextiles;
            public uint Textile32_UncompSize;
            public uint Textile32_CompSize;
            public byte[] Textile32_Compressed;


            public uint Textile16_UncompSize;
            public uint Textile16_CompSize;
            public byte[] Textile16_Compressed;

            public uint Textile32Misc_UncompSize;
            public uint Textile32Misc_CompSize;
            public byte[] Textile32Misc_Compressed;

            public uint LevelData_UncompSize;
            public uint LevelData_CompSize;
            public byte[] LevelData_Compressed;
            public byte[] LevelData_Decompressed;

            public Tr2Textile32[] RoomTextiles;                 // 32-bit (ARGB) textiles
            public Tr2Textile32[] ObjTextiles;                 // 32-bit (ARGB) textiles
            public Tr2Textile32[] BumpTextiles;                 // 32-bit (ARGB) textiles

            public Parser.Tr2Textile16[] RoomTextiles16;                 // 16-bit (ARGB) textiles
            public Parser.Tr2Textile16[] ObjTextiles16;                 // 16-bit (ARGB) textiles
            public Parser.Tr2Textile16[] BumpTextiles16;                 // 16-bit (ARGB) textiles

            public Tr2Textile32[] Textile32Misc;                 // 32-bit (ARGB) textiles

        }

        /*public static object Cast2StructTR4(byte[] buffer, System.Type type)
        {
            object obj = (object)null;

            if (type == typeof(Tr2Animation))
                obj = (object)new Tr2Animation()
                {
                    FrameOffset = System.BitConverter.ToUInt32(buffer, 0),
                    FrameRate = buffer[4],
                    FrameSize = buffer[5],
                    StateID = System.BitConverter.ToInt16(buffer, 6),
                    Speed = System.BitConverter.ToUInt32(buffer, 8),
                    Accel = System.BitConverter.ToUInt32(buffer, 12),
                    SpeedLateral = System.BitConverter.ToUInt32(buffer, 16),
                    AccelLateral = System.BitConverter.ToUInt32(buffer, 20),

                    FrameStart = System.BitConverter.ToUInt16(buffer, 16 + 8),
                    FrameEnd = System.BitConverter.ToUInt16(buffer, 18 + 8),
                    NextAnimation = System.BitConverter.ToUInt16(buffer, 20 + 8),
                    NextFram = System.BitConverter.ToUInt16(buffer, 22 + 8),
                    NumStateChanges = System.BitConverter.ToUInt16(buffer, 24 + 8),
                    StateChangeOffset = System.BitConverter.ToUInt16(buffer, 26 + 8),
                    NumAnimCommands = System.BitConverter.ToUInt16(buffer, 28 + 8),
                    AnimCommand = System.BitConverter.ToUInt16(buffer, 30 + 8)
                };
            return obj;
        }*/


    }
}
