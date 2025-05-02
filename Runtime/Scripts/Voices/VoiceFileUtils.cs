using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine.Networking;

using UnityEngine;
using System.IO;

namespace Bakery.Dialogs
{
    public static class VoiceFileUtils
    {
        public static string VoiceFolder = "Voices";
        public static string PersistentFolder => $"{Application.persistentDataPath}/{VoiceFolder}";
        public static string StreamingFolder => $"{Application.streamingAssetsPath}/{VoiceFolder}";

        public static string PersistentFilePath(string filename)
               => $"{PersistentFolder}/{filename}.mp3";

        public static string StreamingFilePath(string filename)
          => $"{StreamingFolder}/{filename}.mp3";

        public static string TextToFileName(string text, string actorName)
        {

            text = actorName.ToString() + text.ToLower();
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public static async Task<AudioClip> LoadAudioClipFromLocal(string filename)
        {
            //Debug.Log($"Loading from: {filename}");
            using var unityWebRequest =
                UnityWebRequestMultimedia.GetAudioClip($"file://{filename}"
                    , AudioType.MPEG);

            var operation = unityWebRequest.SendWebRequest();

            while (!operation.isDone) await Task.Yield();
            if (operation.webRequest.error != null)
            {
                Debug.LogWarning(operation.webRequest.error);
                return null;
            }

            return DownloadHandlerAudioClip.GetContent(unityWebRequest);
        }

        public static bool FileExistsLocally(string filename)
        {
            return File.Exists(StreamingFilePath(filename))
               || File.Exists(PersistentFilePath(filename));
        }
    }
}