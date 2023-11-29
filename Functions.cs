using Markdig;
using System.IO.Compression;
using System.Text;

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
        /// Creates a new draft in the draft folder and returns the path.
        /// If there is a title, add it to the post and the filename
        /// </summary>
        internal static string CreateNewDraft(string baseDir, bool useHtml, string title)
        {
            // drafts go in the /drafts directory of the working dir
            var draftsDirectory = Path.Combine(baseDir, "drafts");
            if (!Directory.Exists(draftsDirectory))
                Directory.CreateDirectory(draftsDirectory);

            // file contents
            if (String.IsNullOrEmpty(title))
                title = "Title on this line";

            var bodyBuilder = new StringBuilder();
            if (useHtml) 
            {
                bodyBuilder.AppendLine(title).AppendLine();
                bodyBuilder.AppendLine("<p>The rest of the text file is an <b>html</b> blog post. Use 'bashlessblog post' to publish it.</p>").AppendLine();
                bodyBuilder.AppendLine(String.Format("<p>{0} keep-this-tag-format, tags-are-optional, example</p>", Config.TemplateTagsLineHeader));
            }
            else // markdown
            {
                bodyBuilder.AppendLine(title).AppendLine();
                bodyBuilder.AppendLine("The rest of the text file is an **Markdown** blog post. Use 'bashlessblog post' to publish it.").AppendLine();
                bodyBuilder.AppendLine(String.Format("{0} keep-this-tag-format, tags-are-optional, beware-with-underscores-in-markdown, example", Config.TemplateTagsLineHeader));
            }

            // create unique filename
            var filename = CreateUniqueFilename(draftsDirectory, title, useHtml);
            var filePath = Path.Combine(draftsDirectory, filename);
            File.WriteAllText(filePath, bodyBuilder.ToString());

            return filePath;
        }

        /// <summary>
        /// Gets the contents of a post as HTML
        /// </summary>
        internal static string GetHtmlContent(string postPath, bool isDraft)
        {
            var html = String.Empty;
            var postContents = File.ReadAllText(postPath);

            if (isDraft)
            {
                if (Path.GetExtension(postPath) == ".md")
                    html = Markdown.ToHtml(postContents);
                else if (Path.GetExtension(postPath) == ".html")
                    html = postContents;
            }
            else
            {
                throw new NotImplementedException();
            }

            return html;
        }

        /// <summary>
        /// Given post content, create the HTML page in the working directory
        /// </summary>
        internal static void CreateHtmlPage(string postContents, string workingDir)
        {
            // the first line is expected to be the title
            var title = String.Empty;
            var content = new StringBuilder();
            using (var reader = new StringReader(postContents))
            {
                // get title for filename
                var currentLine = reader.ReadLine();    // read first line (title)
                content.AppendLine(currentLine);        // add to output
                if(!String.IsNullOrEmpty(currentLine))
                    title = currentLine.Replace("<p>", "").Replace("</p>", "");

                // title can't be empty
                if (String.IsNullOrEmpty(title))
                    throw new Exception("Cannot parse title from content");

                // read until we get to tags
                while (String.IsNullOrEmpty(currentLine) || !currentLine.StartsWith("<p>" + Config.TemplateTagsLineHeader))
                {
                    content.AppendLine(currentLine);        // add to output
                    currentLine = reader.ReadLine();        // get next line
                    continue;
                }

                // process tags into the correct output of tags with links
                var tags = new List<string>();
                if (currentLine.StartsWith("<p>" + Config.TemplateTagsLineHeader))
                {
                    // remove junk from tags line and split on comma
                    var cleanLine = currentLine.Replace("<p>", "").Replace("</p>", "").Replace(Config.TemplateTagsLineHeader, "");
                    tags = cleanLine.Split(',', (StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries)).ToList();

                    // create the lines with tag links
                    var tagsLine = new StringBuilder();
                    foreach (var tag in tags)
                        tagsLine.Append(String.Format("<a href='{0}{1}'>{1}</a>, ", Config.PrefixTags, tag));

                    content.AppendLine(tagsLine.ToString());
                }

                // if there is anything else in the source content, add it
                content.Append(reader.ReadToEnd());
            }

            // we have everything we need to make the html file
            //create_html_page "$content" "$filename" no "$title" "$2" "$global_author"
            // TODO: handle .header.html

        }


        /// <summary>
        /// Given a target path, title, and file format this function
        /// returns a unique filename with regards to the target directory
        /// </summary>
        private static string CreateUniqueFilename(string targetDir, string title, bool useHtml)
        {
            var filename = title.Replace(' ', '-') + (useHtml ? ".html" : ".md");
            int i = 1;
            while (File.Exists(Path.Combine(targetDir, filename)))
                filename = title.Replace(' ', '-') + i + (useHtml ? ".html" : ".md");

            return filename;
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
