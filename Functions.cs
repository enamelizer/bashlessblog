using System.IO.Compression;

namespace bashlessblog
{
    internal static class Functions
    {
        internal static Config Config { get; private set; } = new Config();

        /// <summary>
        /// Loads a custom configuration by looking up properties via reflection
        /// Legacy support is provided by converting property names to the new style
        /// 
        /// The config file is structured as a key value pair with one property per line
        /// [property]=[value]
        /// </summary>
        internal static void LoadConfig(string configFile)
        {
            Config.LoadConfig(configFile);
        }

        /// <summary>
        /// Create a backup of all files, skipping the backup directory
        /// </summary>
        internal static void Backup(string baseDir)
        {
            // create backup dir if it doesn't exist
            var backupDir = Path.Combine(baseDir, "backup");
            if(!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);

            // create backup
            var backupName = "backup-" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".zip";
            var backupPath = Path.Combine(backupDir, "backup-" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".zip");


            using (var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create))
            {
                foreach (var path in Directory.GetFiles(baseDir, "*.*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(baseDir, path);
                    if (relativePath.StartsWith("backup"))
                        continue;

                    archive.CreateEntryFromFile(path, relativePath);
                }
            }

            // delete all other backups in this folder
            foreach (var path in Directory.GetFiles(backupDir, "backup-*.zip", SearchOption.TopDirectoryOnly))
            {
                if (path != backupPath)
                    File.Delete(path);
            }
        }

        /// <summary>
        /// Creates a new draft in the draft folder
        /// If there is a title, add it to the post and the filename
        /// </summary>
        internal static void CreateNewDraft(string baseDir, bool useHtml, string title)
        {
            // drafts go in the /drafts directory of the working dir
            var draftsDirectory = Path.Combine(baseDir, "drafts");
            if (!Directory.Exists(draftsDirectory))
                Directory.CreateDirectory(draftsDirectory);


        }


        // TODO NONE OF THIS IS NEEDED FUCK

        /// <summary>
        /// Creates a markdown file with a random component in the path name
        /// writes the file to the path and 
        /// </summary>
        /// <param name="outputPath"></param>
        /// <returns></returns>
        //public static string Markdown(string outputPath)
        //{

        //}

        //private static Random rand = new Random();
        //private static UInt16 Random()
        //{
        //    return (UInt16)rand.Next(0, 32767);
        //}


    }
}
