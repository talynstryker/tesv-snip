namespace TESVSnip.Domain.Services
{
    using System;
    using System.IO;

    public static class SaveReadStream
  {

    public static void SaveStreamToDisk(MemoryStream ms)
    {
      string filename = DateTime.Now.ToString("yyyyMMddhhmmss");
      FileStream file = new FileStream(@"c:\temp\" + filename + ".bin", FileMode.Create, System.IO.FileAccess.Write);
      byte[] bytes = new byte[ms.Length];
      ms.Read(bytes, 0, (int)ms.Length);
      file.Write(bytes, 0, bytes.Length);
      file.Close();
      //ms.Close();

    }
  }
}
