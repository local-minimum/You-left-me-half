using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DeCrawl.Systems.Storage
{
    public class FileStorage : MonoBehaviour, IStorage
    {
        [SerializeField]
        bool _Enabled = true;

        public bool Enabled => _Enabled;

        [SerializeField]
        string FilePrefix = "save";

        [SerializeField]
        string FileSuffix = "json";

        [SerializeField]
        string Directory;

        string StorageRoot => System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        string GetPath(string key)
        {
            var filename = string.IsNullOrEmpty(FilePrefix) ? key : $"{FilePrefix}-{key}";
            if (!string.IsNullOrEmpty(FileSuffix))
            {
                filename = $"{filename}.{FileSuffix}";
            }

            return Path.Combine(StorageRoot, Directory, filename);
        }


        bool PlatformWithoutFileSystem => Application.platform == RuntimePlatform.WebGLPlayer;

        public bool Has(string key) => !PlatformWithoutFileSystem && File.Exists(GetPath(key));

        public bool Read(string key, out string data)
        {
            if (Has(key))
            {
                data = File.ReadAllText(GetPath(key));
                return true;
            }

            data = null;
            return false;
        }

        public bool Save(string key, string data)
        {
            if (PlatformWithoutFileSystem) return false;

            var path = GetPath(key);
            var directory = Path.GetDirectoryName(path);
            System.IO.Directory.CreateDirectory(directory);
            File.WriteAllText(path, data);
            return true;
        }
    }
}
