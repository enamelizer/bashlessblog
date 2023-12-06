using System.Globalization;
using System.Text;

namespace bashlessblog
{
    internal class Config
    {
        // blog title - ex "My fancy blog"
        public string GlobalTitle  { get; private set; } = String.Empty;

        // blog description - ex "A blog about turtles and carrots"
        public string GlobalDescription { get; private set; } = String.Empty;

        // public base url - ex "https://example.com/blog"
        public string GlobalUrl { get; private set; } = String.Empty;

        // your name - ex "John Smith"
        public string GlobalAuthor { get; private set; } = String.Empty;

        // author URL - ex "https://mytotallygnarlywebsite.com"
        public string GlobalAuthorUrl { get; private set; } = String.Empty;

        // author email - ex "john@smith.com"
        public string GlobalEmail { get; private set; } = String.Empty;

        // the license used for your published content
        // I highly reccomend changing this to a Creative Commons license
        // to allow re-blogging/re-publishing but I am not the boss of you
        // https://creativecommons.org/share-your-work/
        public string GlobalLicense { get; private set; } = "All Rights Reserved";

        // generated files
        // index page of blog
        public string IndexFile { get; private set; } = "index.html";
        public int NumberOfIndexArticles { get; private set; } = 8;

        // post archive
        public string ArchiveIndex { get; private set; } = "all_posts.html";
        public string TagIndex { get; private set; } = "all_tags.html";

        // Non blogpost files. Bashlessblog will ignore these. Useful for static pages and custom content
        // Add them as a bash array, e.g. non_blogpost_files=("news.html" "test.html")
        public List<string> NonBlogpostFiles { get; private set; } = new List<string>() { };

        // feed file
        public string BlogFeed { get; private set; } = "feed.rss";
        public int NumberOfFeedArticles { get; private set; } = 10;

        // "cut" blog entry when putting it to index page. Leave blank for full articles in front page
        // i.e. include only up to first '<hr>', or '----' in markdown
        public bool CutDo { get; private set; } = true;

        // When cutting, cut also tags? If "no", tags will appear in index page for cut articles
        public bool CutTags { get; private set; } = true;

        // The HTML line where to do the cut
        public string CutLine { get; private set; } = "<hr />";

        // save markdown file when posting with "bashlessblog post -m". Set 'false' to discard it.
        public bool SaveMarkdown { get; private set; } = true;

        // prefix for tags/categories files
        // please make sure that no other html file starts with this prefix
        public string PrefixTags { get; private set; } = "tag_";

        // personalized header and footer (only if you know what you're doing)
        // DO NOT name them .header.html, .footer.html or they will be overwritten on rebuild
        // leave blank to generate them, recommended
        public string HeaderFile { get; private set; } = String.Empty;
        public string FooterFile { get; private set; } = String.Empty;

        // extra content to add just after we open the <body> tag
        // and before the actual blog content
        public string BodyBeginFile { get; private set; } = String.Empty;

        // extra content to add just before we close </body>
        public string BodyEndFile { get; private set; } = String.Empty;

        // extra content to ONLY on the index page AFTER `body_begin_file` contents
        // and before the actual content
        public string BodyBeginFileIndex { get; private set; } = String.Empty;

        // CSS files to include on every page, f.ex. css_include=('main.css' 'blog.css')
        // leave empty to use generated
        public List<string> CssInclude { get; private set; } = new List<string> { };

        // HTML files to exclude from index, f.ex. post_exclude=('imprint.html 'aboutme.html')
        public List<string> HtmlExclude { get; private set; } = new List<string> { };

        // Localization / i18n
        // "Read more..." (link under cut article on index page)
        public string TemplateReadMore { get; private set; } = "Read more...";

        // "View more posts" (used on bottom of index page as link to archive)
        public string TemplateArchive { get; private set; } = "View more posts";

        // "All posts" (title of archive page)
        public string TemplateArchiveTitle { get; private set; } = "All posts";

        // "All tags"
        public string TemplateTagsTitle { get; private set; } = "All Tags";

        // "posts" (on "All tags" page, text at the end of each tag line, like "2. Music - 15 posts")
        public string TemplateTagsPosts { get; private set; } = "posts";
        public string TemplateTagsPosts2_4 { get; private set; } = "posts";         // Some slavic languages use a different plural form for 2-4 items
        public string TemplateTagsPostsSingular { get; private set; } = "post";

        // "Posts tagged" (text on a title of a page with index of one tag, like "My Blog - Posts tagged "Music"")
        public string TemplateTagTitle { get; private set; } = "Posts tagged";

        // "Tags:" (beginning of line in HTML file with list of all tags for this article)
        public string TemplateTagsLineHeader { get; private set; } = "Tags:";

        // "Back to the index page" (used on archive page, it is link to blog index)
        public string TemplateArchiveIndexPage { get; private set; } = "Back to the index page";

        // "Subscribe" (used on bottom of index page, it is link to RSS feed)
        public string TemplateSubscribe { get; private set; } = "Subscribe";

        // "Subscribe to this page..." (used as text for browser feed button that is embedded to html)
        public string TemplateSubscribeBrowserButton { get; private set; } = "Subscribe to this page...";

        // TODO convert all this to .net date formatting
        // TODO handle date_inpost, use this instead of the file dates and extend to have last modified
        // date formatting
        // The format to use for the dates displayed on screen using c# conventions, see:
        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings
        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
        public string DateFormat { get; private set; } = "MMMM d, yyyy";

        // By default the locale used is the one for the current machine, but it can be overridden here
        // The culture format follows RFC 4646, more info can be found here:
        // https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo
        //https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
        public string DateLocale { get; private set; } = String.Empty;

        // Don't change these dates
        //public string DateFormatFull { get; private set; } = "%a, %d %b %Y %H:%M:%S %z";      // this is what is fed into the 'date' util and is
                                                                                                // fucking up dates on rebuilds
                                                                                                // TODO: it is also used in RSS so figure that out
        public string DateFormatTimestamp { get; private set; } = "yyyyMMddTHHmmss";
        public string DateAllpostsHeader { get; private set; } = "MMMM yyyy";


        // bashblog defaults, used for converting old posts to the new format
        public string DateInpostLegacy { get; private set; } = "bashblog_timestamp";
        public string DateFormatTimestampLegacy { get; private set; } = "yyyyMMddHHmm.ss"; //"%Y%m%d%H%M.%S";

        // URL where you can view the post while it's being edited
        // same as global_url by default
        // You can change it to path on your computer, if you write posts locally
        // before copying them to the server
        public string PreviewUrl { get; private set; } = String.Empty;

        // organization, espcially separating the input files from the ouput and allowing
        // for only the output to be published instead of having all items accessable under '.\blog'
        // for default bashblog behavior, set these to empty
        public string BackupDir { get; private set; } = "backup";
        public string DraftDir { get; private set; } = "drafts";
        public string IncludeDir { get; private set; } = "includes";
        public string OutputDir { get; private set; } = "output";


        // stuff we don't want the user to set via .config
        internal static class Internal
        {
            public static readonly string GlobalSoftwareName = "bashlessblog";
            public static readonly string GlobalSoftwareVersion = "0.1";

            // this is the internal/parsed version of DateLocal
            public static CultureInfo DateCulture { get; set; } = CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Loads a custom configuration by looking up properties via reflection
        /// Legacy support is provided by converting property names to the new style
        /// 
        /// The config file is structured as a key value pair with one property per line
        /// [property]=[value]
        /// </summary>
        internal void LoadConfig(string configFile)
        {
            foreach (var line in File.ReadAllLines(configFile))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // get key value pair
                var lineParts = line.Split('=');

                if (lineParts.Length != 2)
                    throw new Exception("config line invalid: " + line);

                var key = lineParts[0];
                var value = lineParts[1].Replace("\"", ""); // strip quotes

                if (string.IsNullOrWhiteSpace(key))
                    throw new Exception("config line invalid: " + line);

                // if this is an old string, convert it to the new name
                if (key.Contains('_'))
                    key = ConvertConfigKey(key);

                // set non-string properties
                if (key == "NumberOfIndexArticles")
                {
                    NumberOfIndexArticles = Convert.ToInt32(value);
                }
                else if (key == "NonBlogpostFiles")
                {
                    var files = value.Replace("\'", "").Replace("\"", "").Split(".html", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var file in files)
                        NonBlogpostFiles.Add(file + ".html");
                }
                else if (key == "NumberOfFeedArticles")
                {
                    NumberOfFeedArticles = Convert.ToInt32(value);
                }
                else if (key == "CutDo")
                {
                    if (string.IsNullOrWhiteSpace(value))
                        CutDo = false;
                    else
                        CutDo = true;
                }
                else if (key == "CutTags")
                {
                    if (value.ToLowerInvariant() == "yes")
                        CutTags = true;
                    else
                        CutTags = false;
                }
                else if (key == "SaveMarkdown")
                {
                    if (value.ToLowerInvariant() == "yes")
                        SaveMarkdown = true;
                    else
                        SaveMarkdown = false;
                }
                else if(key == "CssInclude")
                {
                    var files = value.Replace("\'", "").Replace("\"", "").Split(".css", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var file in files)
                        CssInclude.Add(file + ".css");
                }
                else if (key == "HtmlExclude")
                {
                    var files = value.Replace("\'", "").Replace("\"", "").Split(".html", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var file in files)
                        HtmlExclude.Add(file + ".html");
                }
                else if (key == "DateLocale")
                {
                    try { Internal.DateCulture = CultureInfo.GetCultureInfo(DateLocale, true); } catch { }
                }
                else
                {
                    // set the string property using reflection
                    var propertyInfo = this.GetType().GetProperty(key);
                    if (propertyInfo != null)
                        propertyInfo.SetValue(this, value);
                }

                // set defaults for empty paths
                if (String.IsNullOrEmpty(BackupDir))
                    BackupDir = Directory.GetCurrentDirectory();

                if (String.IsNullOrEmpty(DraftDir))
                    DraftDir = Directory.GetCurrentDirectory();

                if (String.IsNullOrEmpty(IncludeDir))
                    IncludeDir = Directory.GetCurrentDirectory();

                if (String.IsNullOrEmpty(OutputDir))
                    OutputDir = Directory.GetCurrentDirectory();
            }
        }

        /// <summary>
        /// Convert the string from the old config style to the new one
        /// old: config_key_style
        /// new: ConfigKeyStyle
        /// </summary>
        private string ConvertConfigKey(string key)
        {
            var keyParts = key.Split('_');
            var returnStringBuilder = new StringBuilder();

            foreach (var part in keyParts)
            {
                char[] letters = part.ToCharArray();
                letters[0] = char.ToUpper(letters[0]);
                returnStringBuilder.Append(letters);
            }

            return returnStringBuilder.ToString();
        }

        /// <summary>
        /// Validates the configuration and returns warnings.
        /// If there are errors, the warnings and errors are thrown in an exception
        /// </summary>
        public string ValidateConfig()
        {
            var warnings = new StringBuilder();
            var errors = new StringBuilder();

            // warnings
            if (String.IsNullOrEmpty(GlobalTitle))
                warnings.AppendLine("Config Warning: GlobalTitle is empty");

            if (String.IsNullOrEmpty(GlobalDescription))
                warnings.AppendLine("Config Warning: GlobalDescription is empty");

            if (String.IsNullOrEmpty(GlobalAuthor))
                warnings.AppendLine("Config Warning: GlobalAuthor is empty");

            if (String.IsNullOrEmpty(GlobalAuthorUrl))
                warnings.AppendLine("Config Warning: GlobalAuthorUrl is empty");

            if (String.IsNullOrEmpty(GlobalEmail))
                warnings.AppendLine("Config Warning: GlobalEmail is empty");

            if (HeaderFile == ".header.html")
                warnings.AppendLine("Config Warning: '.header.html' is the default 'HeaderFile' name and will be overwritten on rebuild");

            if (FooterFile == ".footer.html")
                warnings.AppendLine("Config Warning: '.footer.html' is the default 'FooterFile' name and will be overwritten on rebuild");

            // errors
            if (String.IsNullOrEmpty(GlobalUrl))
                errors.AppendLine("Config Error: GlobalUrl cannot be empty");

            if (errors.Length > 0)
                throw new Exception(warnings.ToString() + errors.ToString());

            return warnings.ToString();
        }
    }
}
