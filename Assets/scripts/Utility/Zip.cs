using UnityEngine;
using System.Collections;
using System.IO;
//using ICSharpCode.SharpZipLib.Zip;

public class Zip  {

	static byte[] UnzipData(byte[] compressedData)
	{
		/*MemoryStream zms =  new MemoryStream(compressedData);
		ZipInputStream zis = new ZipInputStream(zms);
		ZipEntry entry = zis.GetNextEntry();
		byte[] bytes = new byte[entry.Size];
		zis.Read(bytes, 0, bytes.Length);
		//Debug.Log(bytes.Length);	
		return bytes;*/

		return null;
	}
}
