using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace background.Tools
{
   public class FileHelper
    {
        // private static ILog log = LogManager.GetLogger(LogHelper.repository.Name, typeof(FileHelper));

        private static Logger log = new Logger("FileHelper");
        public static bool SaveBinaryToFile(object obj, string savePath)
        {
            if (string.IsNullOrWhiteSpace(savePath)) return false;
            IFormatter serializer = new BinaryFormatter();
            FileStream saveFile = new FileStream(savePath, FileMode.Create, FileAccess.Write);
            try
            {
                serializer.Serialize(saveFile, obj);
            }
            catch (Exception e)
            {
                log.Error("缓存到文件保存错误：" + e.Message);
                return false;
            }
            finally
            {
                saveFile.Close();
            }
            return true;
        }
        public static object ReadFileToBinary(string savePath)
        {
            if (string.IsNullOrWhiteSpace(savePath)) return false;
            IFormatter serializer = new BinaryFormatter();
            FileStream loadFile = new FileStream(savePath, FileMode.Open, FileAccess.Read);
            try
            {
                return serializer.Deserialize(loadFile);
            }
            catch (Exception e)
            {
                log.Error("文件到缓存读取错误：" + e.Message);
            }
            finally
            {
                loadFile.Close();
            }
            return default(object);
        }

        public static Task<bool> SaveBinaryToFileAsync(object obj, string savePath)
        {
            return Task.Factory.StartNew(() =>
            {
                return SaveBinaryToFile(obj, savePath);
            });
        }
        public static Task<object> ReadFileToBinaryAsync(string savePath)
        {
            return Task.Factory.StartNew(() =>
            {
                return ReadFileToBinary(savePath);
            });
        }
    }
}
