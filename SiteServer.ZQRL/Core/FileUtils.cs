using System;
using System.IO;
using System.Collections;
using BaiRong.Core;
using BaiRong.Model;
using System.Text;

namespace SiteServer.ZQRL
{	
	public class FileUtils
	{
        public static string ReadText(string filePath, ECharset charset)
		{
			StreamReader sr = new StreamReader(filePath, ECharsetUtils.GetEncoding(charset));
			string text = sr.ReadToEnd();
			sr.Close();
			return text;
		}

        public static string ReadText(string filePath, Encoding encoding)
        {
            StreamReader sr = new StreamReader(filePath, encoding);
            string text = sr.ReadToEnd();
            sr.Close();
            return text;
        }

		public static void WriteText(string filePath, ECharset charset, string content)
		{
			DirectoryUtils.CreateDirectoryIfNotExists(DirectoryUtils.GetDirectoryPath(filePath));

			StreamWriter sw = new StreamWriter(filePath, false, ECharsetUtils.GetEncoding(charset));
			sw.Write(content);
			sw.Flush();
			sw.Close();
		}

        public static void RemoveReadOnlyAndHiddenIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileAttributes fileAttributes = File.GetAttributes(filePath);
                if (IsReadOnly(fileAttributes) || IsHidden(fileAttributes))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }
            }
        }

        public static bool IsReadOnly(FileAttributes fileAttributes)
        {
            return ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
        }

        public static bool IsHidden(FileAttributes fileAttributes)
        {
            return ((fileAttributes & FileAttributes.Hidden) == FileAttributes.Hidden);
        }

        public static FileStream GetFileStreamReadOnly(string filePath)
        {
            return new FileStream(filePath, FileMode.Open);
        }

		public static string ReadBase64StringFromFile(string filePath)
		{
			string text;
			FileStream fin = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			byte[] storage = new byte[fin.Length];
			fin.Read(storage, 0, storage.Length);
			fin.Close();
			text = Convert.ToBase64String(storage, 0, storage.Length);
			return text;  
		}

		public static bool IsFileExists(string filePath)
		{
			bool exists = File.Exists(filePath);
			return exists;
		}

		public static bool DeleteFileIfExists(string filePath)
		{
            bool retval = true;
            try
            {
                if (IsFileExists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                //try
                //{
                //    Scripting.FileSystemObject fso = new Scripting.FileSystemObjectClass();
                //    fso.DeleteFile(filePath, true);
                //}
                //catch
                //{
                //    retval = false;
                //}
                retval = false;
            }
            return retval;
		}

		public static void DeleteFilesIfExists(string directoryPath, ArrayList fileNameArrayList)
		{
			foreach (string fileName in fileNameArrayList)
			{
				string filePath = Path.Combine(directoryPath, fileName);
				DeleteFileIfExists(filePath);
			}
		}

        public static void DeleteFilesIfExists(string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                DeleteFileIfExists(filePath);
            }
        }

        public static bool CopyFile(string sourceFilePath, string destFilePath)
        {
            return CopyFile(sourceFilePath, destFilePath, true);
        }

		public static bool CopyFile(string sourceFilePath, string destFilePath, bool isOverride)
		{
            bool retval = true;
            try
            {
              DirectoryUtils.CreateDirectoryIfNotExists(destFilePath);
                
                File.Copy(sourceFilePath, destFilePath, isOverride);

            }
            catch
            {
                retval = false;
            }
            return retval;
		}

        //public static bool MoveFile(string sourceFilePath, string destFilePath)
        //{
        //    DirectoryUtils.CreateDirectoryIfNotExists(destFilePath);
        //    bool retval = true;
        //    try
        //    {
        //        File.Move(sourceFilePath, destFilePath);
        //    }
        //    catch
        //    {
        //        retval = false;
        //    }
        //    return retval;
        //}

        public static void MoveFile(string sourceFilePath, string destFilePath, bool isOverride)
        {
            //����ļ������ڣ�����и��ơ� 
            bool isExists = FileUtils.IsFileExists(destFilePath);
            if (isOverride)
            {
                if (isExists)
                {
                    FileUtils.DeleteFileIfExists(destFilePath);
                }
                FileUtils.CopyFile(sourceFilePath, destFilePath);
            }
            else if (!isExists)
            {
                FileUtils.CopyFile(sourceFilePath, destFilePath);
            }
        }

        public static ECharset GetFileCharset(string filePath)
        {
            Encoding encoding = EncodingType.GetType(filePath);
            return ECharsetUtils.GetEnumType(encoding.BodyName);
        }

        public class EncodingType
        {
            /// <summary>
            /// �����ļ���·������ȡ�ļ��Ķ��������ݣ��ж��ļ��ı�������
            /// </summary>
            /// <param name="FILE_NAME">�ļ�·��</param>
            /// <returns>�ļ��ı�������</returns>
            public static System.Text.Encoding GetType(string FILE_NAME)
            {
                FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
                Encoding r = GetType(fs);
                fs.Close();
                return r;
            }

            /// <summary>
            /// ͨ���������ļ������ж��ļ��ı�������
            /// </summary>
            /// <param name="fs">�ļ���</param>
            /// <returns>�ļ��ı�������</returns>
            public static System.Text.Encoding GetType(FileStream fs)
            {
                byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
                byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
                byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //��BOM
                Encoding reVal = Encoding.Default;

                BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
                //int i;
                //int.TryParse(fs.Length.ToString(), out i);
                int i = TranslateUtils.ToInt(fs.Length.ToString());
                byte[] ss = r.ReadBytes(i);
                if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
                {
                    reVal = Encoding.UTF8;
                }
                else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
                {
                    reVal = Encoding.BigEndianUnicode;
                }
                else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
                {
                    reVal = Encoding.Unicode;
                }
                r.Close();
                return reVal;

            }

            /// <summary>
            /// �ж��Ƿ��ǲ��� BOM �� UTF8 ��ʽ
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            private static bool IsUTF8Bytes(byte[] data)
            {
                int charByteCounter = 1;�� //���㵱ǰ���������ַ�Ӧ���е��ֽ���
                byte curByte; //��ǰ�������ֽ�.
                for (int i = 0; i < data.Length; i++)
                {
                    curByte = data[i];
                    if (charByteCounter == 1)
                    {
                        if (curByte >= 0x80)
                        {
                            //�жϵ�ǰ
                            while (((curByte <<= 1) & 0x80) != 0)
                            {
                                charByteCounter++;
                            }
                            //���λ��λ��Ϊ��0 ��������2��1��ʼ ��:110XXXXX...........1111110X��
                            if (charByteCounter == 1 || charByteCounter > 6)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        //����UTF-8 ��ʱ��һλ����Ϊ1
                        if ((curByte & 0xC0) != 0x80)
                        {
                            return false;
                        }
                        charByteCounter--;
                    }
                }
                if (charByteCounter > 1)
                {
                    throw new Exception("��Ԥ�ڵ�byte��ʽ");
                }
                return true;
            }

        }

        public static string GetFileSizeByFilePath(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                FileInfo theFile = new FileInfo(filePath);
                long fileSize = theFile.Length;
                if (fileSize < 1024)
                {
                    return fileSize.ToString() + "B";
                }
                else if (fileSize >= 1024 && fileSize < 1048576)
                {
                    return (fileSize / 1024).ToString() + "KB";
                }
                else
                {
                    return (fileSize / 1048576).ToString() + "MB";
                }
            }
            else
            {
                return string.Empty;
            }
        }
	}
}