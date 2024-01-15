// See https://aka.ms/new-console-template for more information

using bashlessblog;

try
{
    // check args, print help if invalid
    // init         - create a new blog structure
    // new <title>  - create new draft, title is optional
    // post <title> - post an existing draft, title is required
    // rebuild      - rebuild blog
    // list         - displays a list of posts
    // tags         - displays a list of tags
    // delete       - deletes a published post or draft
    // help         - prints help
    if (args.Length < 1)
    {
        printHelp();
        return;
    }

    var commandList = new List<string>() { "init", "new", "post", "rebuild", "list", "edit", "delete", "tags" };

    var firstArg = args[0].ToLowerInvariant();
    if (!commandList.Contains(firstArg))
    {
        printHelp();
        return;
    }

    if(firstArg == "init")
    {
        doInit();
        return;
    }

    // Load configuration
    doLoadConfig();

    // do the easy things (no writes)
    if (firstArg == "list")
    {
        doList();
        return;
    }
    else if (firstArg == "tags")
    {
        doTags();
        return;
    }
    else if (firstArg == "help")
    {
        printHelp();
        return;
    }

    // do the hard things (blog changes)

    // backup the blog
    BashlessBlog.Backup();

    // create or copy css files
    BashlessBlog.CreateCss();

    // create or copy includes
    BashlessBlog.CreateIncludes();

    if (firstArg == "new")              // new
    {
        // creates a draft file using the optional title
        // in the format specified
        // the first half of write_entry up until the editor opens
        doNew();
    }
    else if (firstArg == "post")        // post
    {
        // publishes a draft to the blog
        // the second half of write_entry
        doPost();
    }
    else if (firstArg == "rebuild")
    {
        // rebuild all entries and tags, optionally css and includes
        doRebuild();
    }
    else if (firstArg == "delete")
    {
        // delete a bublished post and rebuild tags
        doDelete();
    }
    else if (firstArg == "edit")
    {
        // create a draft from a published post and depublish the post
        doEdit();
    }

    BashlessBlog.RebuildIndex();
    BashlessBlog.AllPosts();
    BashlessBlog.MakeRss();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

void doInit()
{
    if (File.Exists(".config"))
        errorAndExit("Error: The .config file already exists in this directory, aborting init");

    BashlessBlog.Init();
    Console.WriteLine("Blog has been initalized, use 'bashlessblog new' to create a new draft");
}

void doLoadConfig()
{
    var configFile = ".config";
    if (!File.Exists(configFile))
        errorAndExit("Error: .config does not exist in the current working directory. Please run bashlessblog from the blog directory or run 'bashlessblog init' to create a new blog");

    // load the config
    try
    {
        var warnings = BashlessBlog.LoadAndValidateConfig(configFile);
        if (!String.IsNullOrEmpty(warnings))
            Console.WriteLine(warnings);
    }
    catch (Exception ex)
    {
        // either load or validate config threw an exception
        Console.WriteLine(ex.Message);
        return;
    }
}

void doList()
{
    var posts = BashlessBlog.GetPostList();
    if (posts.Count == 0)
        Console.WriteLine("No posts yet. Use 'bashlessblog new' to create a new draft or 'bashlessblog post [filename]' to post a draft");

    int i = 1;
    foreach (var post in posts)
    {
        // print post num, name, date in a column format
        Console.WriteLine(string.Format("{0,-5}{1,-55}{2,20}", i, post.Value, post.Key.ToString(BashlessBlog.Config.DateFormat)));
        i++;
    }
}

void doTags()
{
    // get a list of all tags and the posts with that tag
    var allTags = BashlessBlog.PostsWithTags();
    if (args.Length > 1 && args[1].ToLowerInvariant() == "-n")
    {
        var tagsByNum = allTags.OrderByDescending(x => x.Value.Count).Select(x => new KeyValuePair<int, string>(x.Value.Count, x.Key)).ToList();

        foreach (var tag in tagsByNum)
            Console.WriteLine(string.Format("{0,-20}{1,40}", tag.Value, tag.Key));
    }
    else
    {
        foreach (var tag in allTags)
            Console.WriteLine(string.Format("{0,-20}{1,40}", tag.Key, tag.Value.Count.ToString()));
    }
}

void doNew()
{
    // get the optional title for the new draft
    bool useHtml = false;
    var title = String.Empty;
    if (args.Length == 2)       // could either be the -html option or the title
    {
        if (args[1].ToLowerInvariant() == "-html")
            useHtml = true;
        else
            title = args[1];
    }
    else if (args.Length == 3)  // if there are 3 args, expect -html and title
    {
        if (args[1].ToLowerInvariant() != "-html")
            errorAndExit("Error: Invalid arguments");

        useHtml = true;
        title = args[2];
    }
    else if (args.Length != 1)
    {
        errorAndExit("Error: Invalid arguments");
    }

    var draftPath = BashlessBlog.CreateNewDraft(useHtml, title);
    Console.WriteLine($"Draft written to {draftPath} - use 'bashlessblog post' to publish the post after editing");
}

void doPost()
{
    // post a draft to the blog
    // the second half of write_entry
    // TODO add preview functionality
    if (args.Length != 2)
        errorAndExit("Error: Invalid arguments");

    var draftPath = args[1];

    if (!File.Exists(draftPath))
        errorAndExit($"Error: File does not exist: {draftPath}");

    var filename = BashlessBlog.WriteEntry(draftPath);
    Console.WriteLine($"Post written to {filename}");
}

void doRebuild()
{
    if (args.Length > 1 && args[1].ToLowerInvariant() == "-all")
        BashlessBlog.Rebuild(true);

    BashlessBlog.Rebuild(false);
    Console.WriteLine("Blog rebuild complete");
}

void doDelete()
{
    if (args.Length != 2)
        errorAndExit("Error: Invalid arguments");

    var postPath = args[1];

    if (!File.Exists(postPath))
        errorAndExit($"Error: File does not exist: {postPath}");

    BashlessBlog.DeleteEntry(postPath);
    Console.WriteLine($"Deleted {postPath}");
}

void doEdit()
{
    if (args.Length != 2)
        errorAndExit("Error: Invalid arguments");

    var postPath = args[1];

    if (!File.Exists(postPath))
        errorAndExit($"Error: File does not exist: {postPath}");

    if (Directory.GetDirectoryRoot(postPath) == BashlessBlog.Config.DraftDir)
        errorAndExit($"Error: Cannot depublish a post in the draft directory: {postPath}");

    var draftPath = BashlessBlog.EditEntry(postPath);
    Console.WriteLine($"Deleted {Path.GetFileNameWithoutExtension(postPath)} and created the draft {draftPath}");
}

// Print the error message, then print the help text, then exit
void errorAndExit(string message)
{
    Console.WriteLine(message);
    printHelp();
    Environment.Exit(1);
}

void printHelp()
{
    var headerString = $"{Config.Internal.GlobalSoftwareName} version {Config.Internal.GlobalSoftwareVersion}";
    var helpText = """
                   Usage: bashlessblog command [option] [title/filename]
                   
                   Commands:
                       init                    initializes a new blog stucture in the current directory

                       new [-html] [title]     creates a new draft in the drafts folder, using commonmark flavored markdown
                                               '-html' overrides the default behavior and creates an HTML draft
                                               'title' will override the default title with the supplied title, the title must be in quotes
                   
                       edit [filename]         creates a draft from a published post and depublishes the post
                   
                       post [filename]         publishes a draft to the blog
                   
                       delete [filename]       deletes a published post
                   
                       rebuild [-all]          regenerates all the pages and posts, preserving the content of the entries
                                               '-all' will regenerate the CSS, title, header, and footer files
                                                      (caution: custom and edited files will be deleted!)
                   
                       list                    lists all posts
                   
                       tags [-n]               lists all tags in alphabetical order
                                               use '-n' to sort list by number of posts
                   
                   For more information please check the comments and config options in the source code
                   """;

    Console.WriteLine(headerString);
    Console.WriteLine(helpText);
}