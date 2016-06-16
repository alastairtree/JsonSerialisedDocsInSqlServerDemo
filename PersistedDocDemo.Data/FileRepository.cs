using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PersistedDocDemo.Data
{
    public class FileRepository<T> : RepositoryBase<T>
    {
        private readonly string fileSearchPattern = "*.dat";
        private string repositoryFolderPath;

        public FileRepository(IEntitySerialiser serialiser, IRepositoryConfig config)
        {
            Serialiser = serialiser;
            Config = config;
        }

        public FileRepository() : this(new JsonSerialiser(), new DefaultRepositoryConfig())
        {
        }

        public FileRepository(string repositoryFolderPath)
            : this()
        {
            this.repositoryFolderPath = repositoryFolderPath;
        }

        public IRepositoryConfig Config { get; }

        public string RepositoryFolderPath
        {
            get
            {
                if (repositoryFolderPath == null)
                {
                    repositoryFolderPath = GetTemporaryDirectory();
                }
                return repositoryFolderPath;
            }
            set { repositoryFolderPath = value; }
        }

        public string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public string GetFilenameFromId(object id)
        {
            var name = id.ToString().PadLeft(9, '0');
            var folderOne = name.Substring(0, 3);
            var folderTwo = name.Substring(3, 3);
            var fileName = name.Substring(6, name.Length - 6) + ".dat";

            return Path.Combine(RepositoryFolderPath, folderOne, folderTwo, fileName);
        }

        public override int Count()
        {
            return Directory.GetFiles(RepositoryFolderPath, fileSearchPattern, SearchOption.AllDirectories).Count();
        }

        public override T Get(object id)
        {
            var fileName = GetFilenameFromId(id);

            var value = default(T);

            if (File.Exists(fileName))
            {
                var data = File.ReadAllText(fileName);

                if (data != null)
                {
                    value = Serialiser.DeserializeObject<T>(data);
                    SetIdentity(value, id);
                }
            }

            return value;
        }

        public override ICollection<T> GetAll()
        {
            var files = Directory.EnumerateFiles(RepositoryFolderPath, fileSearchPattern, SearchOption.AllDirectories);

            var results = new List<T>();

            foreach (var fileName in files)
            {
                var data = File.ReadAllText(fileName);

                var item = Serialiser.DeserializeObject<T>(data);
                results.Add(item);
            }

            return results;
        }

        public override void Save(T item)
        {
            if (item == null) throw new ArgumentNullException("item");
            var id = GetIdentityValue(item);

            if (IsUndefinedKey(id))
            {
                throw new NotSupportedException("undefined key - you must use a key with a file repository");
            }

            var serialisedData = Serialiser.SerializeObject(item);

            var fileName = GetFilenameFromId(id);
            var folder = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(fileName, serialisedData.ToString());
        }

        public override bool Delete(object id)
        {
            var fileName = GetFilenameFromId(id);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                return true;
            }
            return false;
        }

        public override bool Delete(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            var id = GetIdentityValue(item);
            return Delete(id);
        }

        public override bool DeleteAll()
        {
            var files = Directory.EnumerateFiles(RepositoryFolderPath, fileSearchPattern, SearchOption.AllDirectories);

            var results = 0;

            foreach (var fileName in files)
            {
                File.Delete(fileName);
                results++;
            }

            RemoveEmptyDirectories(RepositoryFolderPath);

            return results > 0;
        }

        private static void RemoveEmptyDirectories(string path)
        {
            Parallel.ForEach(Directory.GetDirectories(path), directory =>
            {
                RemoveEmptyDirectories(directory);
                if (!Directory.EnumerateFileSystemEntries(directory).Any()) Directory.Delete(directory, false);
            });
        }
    }
}