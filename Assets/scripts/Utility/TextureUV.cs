using UnityEngine;
using System.Collections;
using System.IO;

public class TextureUV  {

	static Rect[] uvRects = null;
	static float cmult = 0.032258064516129f;

	public static float AdjustTextureCoordinateX(byte pixel, sbyte offset, ushort texTileIdx)
	{   		
		if(uvRects == null) return 0;


		Rect uvRect = uvRects[texTileIdx];
		float uvspanx = Mathf.Abs(uvRect.xMax - uvRect.xMin);
		if (offset >= 0)
		{
			++pixel;
		}
		else
		{
			--pixel;
		}
		
		
		if(pixel >= 255)
		{
			pixel = 255;
		}
		
		if(pixel <= 0)
		{
			pixel = 0;
		}
		
		
		float u = (float)((float)pixel / 255.0f);
		float adju = u * uvspanx;
		
		return (float)(uvRect.xMin + adju);
	}

	public static float AdjustTextureCoordinateY(byte pixel, sbyte offset, ushort texTileIdx)
	{
		if(uvRects == null) return 0;

		Rect uvRect = uvRects[texTileIdx];
		float uvspany = Mathf.Abs(uvRect.yMin - uvRect.yMax);
		
		if (offset >= 0)
		{
			++pixel;
		}
		else
		{
			--pixel;
		}
		
		
		if(pixel >= 255)
		{
			pixel = 255;
		}
		
		if(pixel <= 0)
		{
			pixel = 0;
		}
		
		
		float v = (float)((float)pixel / 255.0f);
		float adjv = v * uvspany;
		return (float)(uvRect.yMin + adjv);
	}


	public static Texture2D GenerateTextureTile(Parser.Tr2Level leveldata)
	{
		int c16_index = 0;
        int c16_pixel_count = 256 * 256; // pixel count optimization
		Color[][] ColorTable = new Color[leveldata.m_MaxTiles][];
		for(int tileCount = 0; tileCount < leveldata.m_MaxTiles; tileCount++)
		{ 
			
			ColorTable[tileCount] = new Color[c16_pixel_count];
			for(int c = 0 ; c < c16_pixel_count; c++)
			{
				ColorTable[tileCount][c] = Color.white;
			}
			//if(tileCount == 2) break;
			
			ushort[] tmparr = leveldata.Textile16[tileCount].Tile;
			Color[] cols = ColorTable[tileCount];
			for(c16_index = 0 ; c16_index < c16_pixel_count; c16_index++)
			{
				//argb
				ushort ucolor = tmparr[c16_index];
				
				if((ucolor & 0x8000) == 0x8000) ////optimized bit shift and & operation ((ucolor >> 15) & 0x1) == 1 with direct & (ucolor & 0x8000) == 1 operation
				{
					cols[c16_index].a = 1.0f;
				}
				else
				{
					cols[c16_index].a = 0.0f;
				}
				//fix color conversion with range 0 - 31
				cols[c16_index].b = (ucolor & 0x1f)  * cmult;
				cols[c16_index].g = ((ucolor >> 5) & 0x1f) * cmult;
				cols[c16_index].r = ((ucolor >> 10) & 0x1f) * cmult;
                //Vector3 alpha_vec = new Vector3(cols[c16_index].r, cols[c16_index].g, cols[c16_index].b);
                //if(alpha_vec.magnitude <= 0.097)
                //{
                    //cols[c16_index].a = 0.0f;
                //}
            }
			//render transparancy information into tile
			int ntexObj = leveldata.ObjectTextures.Length;
			for(int i = 0; i < ntexObj; i++)
			{
				Parser.Tr2ObjectTexture texobj = leveldata.ObjectTextures[i];
				
				
				// texobj.TransparencyFlags means geometry level transparancy, not pixel level transparency
				// this is used for geometry material batching

				if(texobj.Tile == tileCount && texobj.TransparencyFlags == 0) 	
				{
					//interpolate pixels
					//generate pixel bound
					Parser.Tr2ObjectTextureVertex[] vertices =  texobj.Vertices;
					
					int minxi = vertices[0].Xpixel;
					int maxxi = vertices[0].Xpixel;
					int minyi = vertices[0].Ypixel;
					int maxyi = vertices[0].Ypixel;
					
					for(int v = 0; v < vertices.Length - 1; v++)
					{
						if(vertices[v].Xcoordinate < minxi)
						{
							minxi = vertices[v].Xpixel;
						}
						
						if(vertices[v].Xcoordinate > maxxi)
						{
							maxxi = vertices[v].Xpixel;
						}
						
						if(vertices[v].Ycoordinate < minyi)
						{
							minyi = vertices[v].Ypixel;
						}
						
						if(vertices[v].Ycoordinate > maxyi)
						{
							maxyi = vertices[v].Ypixel;
						}

					}
					
					//render transparancy in generated bound
					
					if(vertices.Length < 4)
					{
					
						Vector3 p0 = new Vector3(vertices[0].Xpixel,  0, vertices[0].Ypixel );
						Vector3 p1 = new Vector3(vertices[1].Xpixel,  0, vertices[1].Ypixel );
						Vector3 p2 = new Vector3(vertices[2].Xpixel,  0, vertices[2].Ypixel );

					
						for(int y = minyi; y < maxyi; y++)
						{
							for(int x = minxi; x < maxxi; x++)
							{
				
								if(IsUVInSide( p2, p1,p0, new Vector3(x,0, y) ))
								{
									int idx = y * 256 + x;  
		
									cols[idx].a = 1;
								}
					
							}
				
						}
						
					}
					else
					{
						for(int y = minyi; y < maxyi; y++)
						{
							for(int x = minxi; x < maxxi; x++)
							{
				
								//if(IsUVInSide( p2, p1,p0, new Vector3(x,0, y) ))
								//{
									int idx = y * 256 + x;  
		
									cols[idx].a = 1;
								//}
					
							}
				
						}
						
						
					}
						

				
				} //end transparancy flag check
			}
			
        }

		//pack tiles
		float uvlength = 1.0f/ (float)(leveldata.m_MaxTiles); //bug fixed: used leveldata.m_MaxTiles instead of hardcoded value 16
        uvRects = new Rect[leveldata.m_MaxTiles];
		Texture2D tex = new Texture2D (256, 256 * leveldata.m_MaxTiles, TextureFormat.ARGB32,false,true);
		tex.filterMode  = FilterMode.Bilinear;
		tex.wrapMode = TextureWrapMode.Clamp;
		tex.anisoLevel = 9;

    
        for (int t = 0; t < leveldata.m_MaxTiles; t++)
        {
           Color[] cols = ColorTable[t];
           uvRects[t] = new Rect(0, uvlength * t, 1, uvlength);  //distorted uv error : reason careless uv stting
           tex.SetPixels(0, 256 * t, 256, 256, cols, 0);
        }
     
		
		tex.Apply(true);
		tex.name = "texAtlas";
		tex.hideFlags = 0;
			
		return tex;
	}
	
	public static bool IsUVInSide(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 test_point)
	{
		Vector3 vec1 = (p1 - p0);
		Vector3 vec2 = (p2 - p1);
		Vector3 vec3 = (p0 - p2);
		
		Vector3 in1 = Vector3.Cross(Vector3.up, vec1.normalized).normalized;
		Vector3 in2 = Vector3.Cross(Vector3.up, vec2.normalized).normalized;
		Vector3 in3 = Vector3.Cross(Vector3.up, vec3.normalized).normalized;
		
		if(Vector3.Dot(in1,(test_point - p0).normalized ) < 0) return false;
		if(Vector3.Dot(in2,(test_point - p1).normalized ) < 0) return false;
		if(Vector3.Dot(in3,(test_point - p2).normalized ) < 0) return false;
		
		return true;
	}
	

   	void generateBumpMap(Texture2D baseMap,Texture2D bumpMap)
   	{
   		Color[] colors = baseMap.GetPixels(0);
   		Color[] bumpColor = bumpMap.GetPixels(0);
   		Color csum = Vector4.zero;;
   		int colorAdr = 0;
   		
   		
   		int w = baseMap.width;
   		int h = baseMap.width;
   		
   		
   		int emboss_w = 3 ;
   		int emboss_h = 3;
		
		//int emboss_filter[emboss_w][emboss_h]={{2,0,0},{0,-1,0},{0,0,-1}}; 
		int emboss_sum=1;
		int[] emboss_filter = new int [emboss_w * emboss_h] ;
		
		emboss_filter[0] = 2;
		emboss_filter[1] = 0;
		emboss_filter[3] = 0;
		
		emboss_filter[4] = 0;
		emboss_filter[5] = -1;
		emboss_filter[6] = 0;
		
		emboss_filter[4] = 0;
		emboss_filter[5] = 0;
		emboss_filter[6] = -1;
		
		for(int i=1;i< w-1;i++)  //convert ImagesToGray scale
		{ 
			for(int j=1;j< h-1;j++)
			{
				colorAdr = w * j + i;
				
				Color color = colors[colorAdr];
				float gray = (color.r + color.g + color.b) / 3;
			
				if(gray < 0)
				{
					gray = 0;
				}
				if(gray > 1)
				{
					gray = 1;
				}
			
				color.r = gray;
				color.g = gray;
				color.b = gray;
				
				colors[colorAdr] = color;
			}
		}
		
		colorAdr = 0;
		for(int i=1;i< w-1;i++)  //embos
		{ 
			for(int j=1;j< h-1;j++)
			{
				csum = Vector4.zero;;
				colorAdr = w * j + i;
				
				for(int k=0;k<emboss_w;k++)
				{
					for(int l=0;l<emboss_h;l++)
					{
						int m = i-((emboss_w-1)>>1) + k;
						int n = j-((emboss_h-1)>>1) + l;
						int picColorAdr = w * n + m;
						int filter_idx =  emboss_w * l +  k;
						csum+= colors[picColorAdr] * emboss_filter[filter_idx];
					}
				}
				
				csum /= emboss_sum;
				
				if(csum.r < 0)
				{
					csum = Vector4.zero;
				}
				
				if(csum.r >1)
				{
					csum = Vector4.one;
				}
				
				//long invr = ~(long)(csum.r* 255);
				//long invg = ~(long)(csum.g* 255);
				//long invb = ~(long)(csum.b* 255);
			
				//bumpColor[colorAdr].b = invb * cmult;
				//bumpColor[colorAdr].r = invr * cmult;
				//bumpColor[colorAdr].g = invg * cmult;
				
				bumpColor[colorAdr] = csum;
				bumpColor[colorAdr].r = bumpColor[colorAdr].r * 0.5f;
				bumpColor[colorAdr].g = bumpColor[colorAdr].g  * 0.5f;
			}
		}
		
		 bumpMap.SetPixels(0,0,w,h,bumpColor);
    	 bumpMap.Apply();
   	}
	
}
