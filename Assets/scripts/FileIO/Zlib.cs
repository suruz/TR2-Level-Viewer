//refference: https://stackoverflow.com/questions/737258/zlib-from-c-to-chow-to-convert-byte-to-stream-and-stream-to-byte

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zlib;
using System.IO;
using System.IO.Compression;

public class Zlib  {



    public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
    {
        byte[] buffer = new byte[4136175];
        int len;
        while ((len = input.Read(buffer, 0, 2000)) > 0)
        {
            output.Write(buffer, 0, len);
        }
        output.Flush();


    }

    public static byte[] CompressData(byte[] inData)
    {
        using (MemoryStream outMemoryStream = new MemoryStream())
        using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream, zlibConst.Z_DEFAULT_COMPRESSION))
        using (Stream inMemoryStream = new MemoryStream(inData))
        {
            CopyStream(inMemoryStream, outZStream);
            outZStream.finish();
            return outMemoryStream.ToArray();
        }
    }

    public static byte[] DecompressData(byte[] inData)
    {
        using (MemoryStream outMemoryStream = new MemoryStream())
        using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
        using (Stream inMemoryStream = new MemoryStream(inData))
        {
            CopyStream(inMemoryStream, outZStream);
            outZStream.finish();
            return outMemoryStream.ToArray();
        }
    }





    static void test()
    {

        string inputString = "The text to compress and decompress";
        byte[] inputArray = System.Text.Encoding.ASCII.GetBytes(inputString);

        MemoryStream stream = new MemoryStream();
        DeflateStream compressionStream = new DeflateStream(stream, CompressionMode.Compress, true);
        compressionStream.Write(inputArray, 0, inputArray.Length);
        compressionStream.Close();

        stream.Position = 0;
        DeflateStream decompressionStream =
        new DeflateStream(stream, CompressionMode.Decompress);
        byte[] outputArray = new byte[inputArray.Length];
        decompressionStream.Read(outputArray, 0, outputArray.Length);
        string outputString = System.Text.Encoding.ASCII.GetString(outputArray);

        Debug.Log(outputString);

    } 

    /*
     * The answer above is correct but isn't exactly clear on the "why". The first two bytes of a raw ZLib stream provide details about the type of compression used. Microsoft's DeflateStream class in System.Io.Compression doesn't understand these. The fix is as follows:
     */

    public static byte[] DecompressData(byte[] inData, uint output_length)
    {
        byte[] outputArray = new byte[output_length];
        using (var stream = new MemoryStream(inData, 2, inData.Length - 2))
        using (var inflater = new DeflateStream(stream, CompressionMode.Decompress))
        {
            inflater.Read(outputArray, 0, outputArray.Length);
        }
        return outputArray;
    }

}
