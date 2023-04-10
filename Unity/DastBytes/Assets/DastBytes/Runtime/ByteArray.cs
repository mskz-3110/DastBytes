using System;

namespace DastBytes {
  public class ByteArray {
    static public void Copy(Array dstArray, int dstIndex, Array srcArray, int srcIndex, int byteCount){
      System.Buffer.BlockCopy(srcArray, srcIndex, dstArray, dstIndex, byteCount);
    }

    unsafe static public void Copy(byte[] dstBytes, int dstIndex, void* srcPointer, int byteCount){
      byte* p = (byte*)srcPointer;
      for (int i = 0; i < byteCount; ++i){
        dstBytes[dstIndex + i] = *(p + i);
      }
    }

    unsafe static public void Copy(void* dstPointer, byte[] srcBytes, int srcIndex, int byteCount){
      byte* p = (byte*)dstPointer;
      for (int i = 0; i < byteCount; ++i){
        *(p + i) = srcBytes[srcIndex + i];
      }
    }

    static public void Resize<T>(ref T[] array, int length){
      Array.Resize<T>(ref array, length);
    }

    static public void ResizeIfOversized<T>(ref T[] array, int length){
      if (array.Length < length) Array.Resize<T>(ref array, length);
    }

    static public void ResizeIfNotSamesized<T>(ref T[] array, int length){
      if (array.Length != length) Array.Resize<T>(ref array, length);
    }
  }
}