using Markdig;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace bashlessblog
{
    internal static class BashlessBlog
    {
        internal static Config Config { get; private set; } = new Config();

        /// <summary>
        /// Loads a custom configuration by looking up properties via reflection
        /// Legacy support is provided by converting property names to the new style
        /// 
        /// The config file is structured as a key value pair with one property per line
        /// [property]=[value]
        /// </summary>
        /// <returns>String of warnings</returns>
        /// <exception>Throws Exception with a list of errors</exception>
        internal static string LoadAndValidateConfig(string configFile)
        {
            Config.LoadConfig(configFile);
            return Config.ValidateConfig();
        }

        /// <summary>
        /// Create a backup of all files, skipping the backup directory
        /// </summary>
        internal static void Backup()
        {
            // create backup dir if it doesn't exist
            var backupDir = Config.BackupDir;
            if(!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);

            // get list of files to backup
            var fileSet = new HashSet<string>() { ".config" };

            foreach (var file in Directory.GetFiles(Config.DraftDir, "*.*", SearchOption.AllDirectories))
                fileSet.Add(file);

            foreach (var file in Directory.GetFiles(Config.IncludeDir, "*.*", SearchOption.AllDirectories))
                fileSet.Add(file);

            foreach (var file in Directory.GetFiles(Config.OutputDir, "*.*", SearchOption.AllDirectories))
                fileSet.Add(file);

            // create backup
            var backupPath = Path.Combine(backupDir, "backup-" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".zip");

            using (var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create))
            {
                foreach (var path in fileSet)
                {
                    var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), path);
                    if (relativePath.StartsWith(Config.BackupDir) || relativePath.StartsWith("backup")) // don't backup the backup
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
        /// <remarks>This is the first part of write_entry from bb.sh up until the opening of the editor</remarks>
        internal static string CreateNewDraft(bool useHtml, string title)
        {
            // drafts go in the /drafts directory of the working dir
            var draftsDirectory = Config.DraftDir;
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
                bodyBuilder.AppendLine($"<p>{Config.TemplateTagsLineHeader} keep-this-tag-format, tags-are-optional, example</p>");
            }
            else // markdown
            {
                bodyBuilder.AppendLine(title).AppendLine();
                bodyBuilder.AppendLine("The rest of the text file is an **Markdown** blog post. Use 'bashlessblog post' to publish it.").AppendLine();
                bodyBuilder.AppendLine($"{Config.TemplateTagsLineHeader} keep-this-tag-format, tags-are-optional, beware-with-underscores-in-markdown, example");
            }

            // create unique filename
            var filename = CreateUniqueFilepath(draftsDirectory, title, useHtml);
            var filePath = Path.Combine(draftsDirectory, filename);
            File.WriteAllText(filePath, bodyBuilder.ToString());

            return filePath;
        }

        /// <summary>
        /// Given post content, create the HTML page in the working directory
        /// </summary>
        /// <remarks>This is the second half of write_entry from bb.sh without the editor loop</remarks>
        internal static string WriteEntry(string draftPath)
        {
            // get the draft contents
            var draftContent = BashlessBlog.GetDraftContentAsHtml(draftPath);

            // this first section of code is parse_file from bb.sh

            // the first line is expected to be the title
            var title = String.Empty;
            var outputContent = new StringBuilder();
            var tags = new List<string>();
            using (var reader = new StringReader(draftContent))
            {
                // get title
                var currentLine = reader.ReadLine();    // read first line (title) but don't add to output
                if (!String.IsNullOrEmpty(currentLine))
                    title = currentLine.Replace("<p>", "").Replace("</p>", "");

                currentLine = reader.ReadLine();        // skip to next line

                // title can't be empty
                if (String.IsNullOrEmpty(title))
                    throw new Exception("WriteEntry Error: Cannot parse title from content");

                // read until we get to tags
                while (String.IsNullOrEmpty(currentLine) || !currentLine.StartsWith("<p>" + Config.TemplateTagsLineHeader))
                {
                    outputContent.AppendLine(currentLine);        // add to output
                    currentLine = reader.ReadLine();        // get next line
                    continue;
                }

                // process tags into the correct output of tags with links
                if (currentLine.StartsWith("<p>" + Config.TemplateTagsLineHeader))
                {
                    // special tag extraction for drafts
                    var cleanLine = currentLine.Replace("<p>", "").Replace("</p>", "").Replace(Config.TemplateTagsLineHeader, "");
                    tags = cleanLine.Split(',', (StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToList();

                    // create the lines with tag links
                    List<string> tagLinks = new List<string>();
                    foreach (var tag in tags)
                        tagLinks.Add($"<a href='{Config.PrefixTags}{tag}.html'>{tag}</a>");
                    var tagsLine = $"\n\n<p>{Config.TemplateTagsLineHeader} {String.Join(", ",tagLinks)}</p>";

                    outputContent.Append(tagsLine.ToString());
                }
            }

            var filename = CreateUniqueFilepath(Config.OutputDir, title, true);

            // we have everything we need to make the html file
            //create_html_page "$content" "$filename" no "$title" "$2" "$global_author"
            CreateHtmlPage(outputContent.ToString(), filename, false, title);     // TODO handle timestamp on edit

            // save markdown option
            // TODO uncomment this when done testing
            //if (Config.SaveMarkdown && Path.GetExtension(draftPath) == ".md")
            //    File.Move(draftPath, Path.ChangeExtension(filename, ".md"));
            //else
            //    File.Delete(draftPath);

            // rebuild tags
            var relevantPosts = PostsWithTags(tags);

            return filename;
        }

        /// <summary>
        /// Creates an HTML page
        /// </summary>
        /// <param name="content">The HTML content for the body of the page</param>
        /// <param name="filepath">The filename to write to</param>
        /// <param name="generateIndex">true to generate the index page, false to write a normal post</param>
        /// <param name="title">The title of the post, without HTML decoration</param>
        /// <param name="timestamp">Optional timestamp to use instead of now</param>
        internal static void CreateHtmlPage(string content, string filepath, bool generateIndex, string title, string timestamp = "")
        {
            var htmlBuilder = new StringBuilder();

            // header
            htmlBuilder.Append(File.ReadAllText(Path.Combine(Config.IncludeDir, ".header.html")));
            htmlBuilder.AppendLine($"<title>{title}</title>");
            htmlBuilder.AppendLine("</head><body>");

            // body begin file
            if(!String.IsNullOrEmpty(Config.BodyBeginFile))
                htmlBuilder.AppendLine(File.ReadAllText(Config.BodyBeginFile));

            // body begin file index
            if (generateIndex && !String.IsNullOrEmpty(Config.BodyBeginFileIndex))
                htmlBuilder.AppendLine(File.ReadAllText(Config.BodyBeginFileIndex));

            htmlBuilder.AppendLine("<div id=\"divbodyholder\">");
            htmlBuilder.AppendLine("<div class=\"headerholder\"><div class=\"header\">");
            htmlBuilder.AppendLine("<div id=\"title\">");
            htmlBuilder.Append(File.ReadAllText(Path.Combine(Config.IncludeDir, ".title.html")));
            htmlBuilder.AppendLine("</div></div></div>");
            htmlBuilder.AppendLine("<div id=\"divbody\"><div class=\"content\">");

            // TODO does this need to be handled? bb.sh line 459
            // file_url=${filename#./}
            // file_url =${ file_url %.rebuilt} # Get the correct URL when rebuilding

            // blog post
            if (!generateIndex)
            {
                htmlBuilder.AppendLine("<!-- entry begin -->");
                htmlBuilder.AppendLine($"<h3><a class=\"ablack\" href=\"{Path.GetFileName(filepath)}\">");
                htmlBuilder.AppendLine(title);
                htmlBuilder.AppendLine("</a></h3>");

                var creationDt = DateTime.Now;

                // if there is a timestamp passed in, use that for creationtime
                if (!String.IsNullOrEmpty(timestamp))
                {
                    var parsed = false;
                    if (!parsed)
                        parsed = DateTime.TryParseExact(timestamp, Config.DateFormatTimestamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out creationDt);
                    else if (!parsed)
                        parsed = DateTime.TryParse(timestamp, out creationDt);      // fallback
                    else
                        creationDt = DateTime.Now;                                  // fallback #2
                }

                // timestamp
                htmlBuilder.AppendLine($"<!-- creationtime: {creationDt.ToString(Config.DateFormatTimestamp, CultureInfo.InvariantCulture)} -->");

                // date and author
                htmlBuilder.Append($"<div class=\"subtitle\">{creationDt.ToString(Config.DateFormat, Config.Internal.DateCulture)}");
                if (!String.IsNullOrEmpty(Config.GlobalAuthor))
                    htmlBuilder.Append($" &mdash; \n{Config.GlobalAuthor}\n");
                htmlBuilder.Append("</div>\n");

                // content
                htmlBuilder.AppendLine("<!-- text begin -->");
                htmlBuilder.Append(content);
                if(!generateIndex)
                {
                    htmlBuilder.AppendLine("\n<!-- text end -->");
                    htmlBuilder.AppendLine("<!-- entry end -->");
                }

                htmlBuilder.AppendLine("</div>");

                // footer
                htmlBuilder.Append(File.ReadAllText(Path.Combine(Config.IncludeDir, ".footer.html")));
                htmlBuilder.AppendLine("</div></div>");

                // body end file
                if (!String.IsNullOrEmpty(Config.BodyEndFile))
                    htmlBuilder.Append(File.ReadAllText(Config.BodyEndFile));

                htmlBuilder.AppendLine("</body></html>");

                File.WriteAllText(filepath, htmlBuilder.ToString());
            }
        }

        /// <summary>
        /// Create default .css files if overrides are not defined in the config
        /// or if they do not already exist
        /// </summary>
        internal static void CreateCss()
        {
            // if CSS files are defined in the config, skip creation of the defaults
            if (Config.CssInclude.Count > 0)
                return;

            var blogCssPath = Path.Combine(Config.IncludeDir, "blog.css");
            var mainCssPath = Path.Combine(Config.IncludeDir, "main.css");

            Config.CssInclude.Add(mainCssPath);
            Config.CssInclude.Add(blogCssPath);

            if (!Directory.Exists(Config.IncludeDir))
                Directory.CreateDirectory(Config.IncludeDir);

            // if the defaults already exist do not recreate them
            // they may be modified by the user
            if (!File.Exists(blogCssPath))
            {
                var blogCssContent = """
                    #title{font-size: x-large;}
                    a.ablack{color:black !important;}
                    li{margin-bottom:8px;}
                    ul,ol{margin-left:24px;margin-right:24px;}
                    #all_posts{margin-top:24px;text-align:center;}
                    .subtitle{font-size:small;margin:12px 0px;}
                    .content p{margin-left:24px;margin-right:24px;}
                    h1{margin-bottom:12px !important;}
                    #description{font-size:large;margin-bottom:12px;}
                    h3{margin-top:42px;margin-bottom:8px;}
                    h4{margin-left:24px;margin-right:24px;}
                    img{max-width:100%;}
                    #twitter{line-height:20px;vertical-align:top;text-align:right;font-style:italic;color:#333;margin-top:24px;font-size:14px;}
                    """;

                File.WriteAllText(blogCssPath, blogCssContent);
            }

            if (!File.Exists(mainCssPath))
            {
                var mainCssContent = """
                    body{font-family:Georgia,"Times New Roman",Times,serif;margin:0;padding:0;background-color:#F3F3F3;}
                    #divbodyholder{padding:5px;background-color:#DDD;width:100%;max-width:874px;margin:24px auto;}
                    #divbody{border:solid 1px #ccc;background-color:#fff;padding:0px 48px 24px 48px;top:0;}
                    .headerholder{background-color:#f9f9f9;border-top:solid 1px #ccc;border-left:solid 1px #ccc;border-right:solid 1px #ccc;}
                    .header{width:100%;max-width:800px;margin:0px auto;padding-top:24px;padding-bottom:8px;}
                    .content{margin-bottom:5%;}
                    .nomargin{margin:0;}
                    .description{margin-top:10px;border-top:solid 1px #666;padding:10px 0;}
                    h3{font-size:20pt;width:100%;font-weight:bold;margin-top:32px;margin-bottom:0;}
                    .clear{clear:both;}
                    #footer{padding-top:10px;border-top:solid 1px #666;color:#333333;text-align:center;font-size:small;font-family:"Courier New","Courier",monospace;}
                    a{text-decoration:none;color:#003366 !important;}
                    a:visited{text-decoration:none;color:#336699 !important;}
                    blockquote{background-color:#f9f9f9;border-left:solid 4px #e9e9e9;margin-left:12px;padding:12px 12px 12px 24px;}
                    blockquote img{margin:12px 0px;}
                    blockquote iframe{margin:12px 0px;}
                    """;

                File.WriteAllText(mainCssPath, mainCssContent);
            }
        }

        // Create include files, or copy them if they are declared in config
        internal static void CreateIncludes()
        {
            if (!Directory.Exists(Config.IncludeDir))
                Directory.CreateDirectory(Config.IncludeDir);

            // .title.html
            var titlePath = Path.Combine(Config.IncludeDir, ".title.html");
            if (!File.Exists(titlePath))
            {
                var titleContentBuilder = new StringBuilder();
                titleContentBuilder.AppendLine($"<h1 class=\"nomargin\"><a class=\"ablack\" href=\"{Config.GlobalUrl}/{Config.IndexFile}\">{Config.GlobalTitle}</a></h1>");
                titleContentBuilder.AppendLine($"<div id=\"description\">{Config.GlobalDescription}</div>");
                File.WriteAllText(titlePath, titleContentBuilder.ToString());
            }

            // .header.html
            var headerPath = Path.Combine(Config.IncludeDir, ".header.html");
            if (!String.IsNullOrEmpty(Config.HeaderFile))
            {
                File.Copy(Config.HeaderFile, headerPath, true);
            }
            else if (!File.Exists (headerPath))
            {
                var headerContentBuilder = new StringBuilder("""
                    <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
                    <html xmlns="http://www.w3.org/1999/xhtml"><head>
                    <meta http-equiv="Content-type" content="text/html;charset=UTF-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1.0" />

                    """);

                foreach (var cssInclude in Config.CssInclude)
                    headerContentBuilder.AppendLine($"<link rel=\"stylesheet\" href=\"{Path.GetFileName(cssInclude)}\" type=\"text/css\" />");

                headerContentBuilder.AppendLine($"<link rel=\"alternate\" type=\"application/rss+xml\" title=\"{Config.TemplateSubscribeBrowserButton}\" href=\"{Config.BlogFeed}\" />");

                File.WriteAllText(headerPath, headerContentBuilder.ToString());
            }

            // .footer.html
            var footerPath = Path.Combine(Config.IncludeDir, ".footer.html");
            if (!String.IsNullOrEmpty(Config.FooterFile))
            {
                File.Copy(Config.FooterFile, footerPath, true);
            }
            else if (!File.Exists(footerPath))
            {
                var protectedMail = Config.GlobalEmail.Replace("@", "&#64").Replace(".", "&#46");
                var footerContent = $"""
                    <div id="footer">{Config.GlobalLicense} <a href="{Config.GlobalAuthorUrl}">{Config.GlobalAuthor}</a> &mdash; <a href="mailto:{protectedMail}">{protectedMail}</a><br/>
                    Generated with <a href="https://github.com/enamelizer/bashlessblog">bashlessblog</a>, a small .net program to easily create blogs like this one</div>

                    """;

                File.WriteAllText(footerPath, footerContent.ToString());
            }
        }

        /// <summary>
        /// Delete default include files
        /// </summary>
        internal static void DeleteIncludes()
        {
            File.Delete(Path.Combine(Config.IncludeDir, ".title.html"));
            File.Delete(Path.Combine(Config.IncludeDir, ".header.html"));
            File.Delete(Path.Combine(Config.IncludeDir, ".footer.html"));
        }

        /// <summary>
        /// Delete default css files
        /// </summary>
        internal static void DeleteCss()
        {
            File.Delete(Path.Combine(Config.IncludeDir, "blog.css"));
            File.Delete(Path.Combine(Config.IncludeDir, "main.css"));
        }

        /// <summary>
        /// Gets the contents of a draft as HTML
        /// skipping the first line as this is the title not content
        /// </summary>
        internal static string GetDraftContentAsHtml(string postPath)
        {
            var html = String.Empty;
            var postContents = File.ReadAllText(postPath);

            if (Path.GetExtension(postPath) == ".md")
                html = Markdown.ToHtml(postContents);
            else if (Path.GetExtension(postPath) == ".html")
                html = postContents;

            return html;
        }

        /// <summary>
        /// Given a target path, title, and file format this function
        /// returns a unique filepath with regards to the target directory.
        /// This will also attempt to make a clean ascii url-safe name.
        /// </summary>
        private static string CreateUniqueFilepath(string targetDir, string title, bool useHtml)
        {
            // this does not do a proper transliteration so I imagine it will fail on lots of non-english
            // languages, espcially non-latin ones. See this lib for a proper transliteration solution:
            // https://github.com/anyascii/anyascii#net
            var asciiTitle = Encoding.ASCII.GetString(Encoding.UTF8.GetBytes(title)).Replace(' ', '-').ToLowerInvariant();
            var asciiTitleStripped = Array.FindAll<char>(asciiTitle.ToArray(), (c => (char.IsLetterOrDigit(c) || c == '-')));
            var filepath = Path.Combine(targetDir, new string(asciiTitleStripped));

            // find a filename not in use and add the proper extension
            filepath = filepath + (useHtml ? ".html" : ".md");
            int i = 1;
            while (File.Exists(filepath))
                filepath = filepath + $"-{i}" + (useHtml ? ".html" : ".md");

            return filepath;
        }

        /// <summary>
        /// Given a list of tags, return a dictionary that maps each tag
        /// to the files that contain the tags
        /// </summary>
        private static Dictionary<string, List<string>> PostsWithTags(List<string> tags)
        {
            // initalize mapping
            var tagFileMapping = new Dictionary<string, List<string>>();
            foreach (var tag in tags)
                tagFileMapping.Add(tag, new List<string>());

            // get all html files from the output directory that don't start with the tag prefix
            var fileList = Directory.GetFiles(Config.OutputDir, "*.html");
            foreach (var file in fileList)
            {
                // skip the tag_ files
                if (Path.GetFileName(file).StartsWith(Config.PrefixTags))
                    continue;

                // get the tags in the post and if we have a match
                // against the tags we are looking for, add it to the mapping
                var tagsInPost = TagsInPost(file);
                foreach(var tag in tags)
                    if (tagsInPost.Contains(tag))
                        tagFileMapping[tag].Add(file);
            }

            return tagFileMapping;
        }

        /// <summary>
        /// Searches thru a file for the tagline
        ///  and returns a list of tags
        /// </summary>
        private static List<string> TagsInPost(string filename)
        {
            var tagLine = File.ReadAllLines(filename).FirstOrDefault(x => x.StartsWith("<p>" + Config.TemplateTagsLineHeader));

            if (String.IsNullOrEmpty(tagLine))
                return new List<string>();

            return ExtractTagsFromHtml(tagLine);
        }

        /// <summary>
        /// Given the line with tags in it, return a list of tags
        /// </summary>
        private static List<string> ExtractTagsFromHtml(string tagLine)
        {
            var tags = new List<string>();

            if (tagLine.StartsWith("<p>" + Config.TemplateTagsLineHeader))
            {
                // read thru all chars and extract the non-html bits
                // this is a goofy way to do it but avoids using another dependancy
                var tagChars = new List<char>();
                bool read = false;
                foreach(var ch in tagLine)
                {
                    if (ch == '<')
                        read = false;

                    if (read)
                        tagChars.Add(ch);

                    if (ch == '>')
                        read = true;
                }

                var cleanLine = new String(tagChars.ToArray()).Replace(Config.TemplateTagsLineHeader, "");
                tags = cleanLine.Split(',', (StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToList();
            }

            return tags;
        }
    }
}
