// See https://aka.ms/new-console-template for more information

using bashlessblog;

try
{
    // Load configuration
    var configFile = ".config";
    if (!File.Exists(configFile))
    {
        Console.WriteLine("Error: .config does not exist in the current working directory. Please run bashlessblog from the blog directory");
        return;
    }

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

    // check args, print help if invalid
    // new <title>  - create new draft, title is optional
    // post <title> - post an existing draft, title is required
    // rebuild      - rebuild blog
    // list         - displays a list of posts
    // tags         - displays a list of tags
    // delete       - deletes a published post or draft
    // help         - prints help
    // TODO reset?
    if (args.Length < 1)
    {
        printHelp();
        return;
    }

    var firstArg = args[0].ToLowerInvariant();

    if (firstArg != "new" && firstArg != "post" && firstArg != "rebuild" && firstArg != "list" && firstArg != "edit" && firstArg != "delete" && firstArg != "tags")
    {
        printHelp();
        return;
    }

    // do the easy things (no writes)
    if (firstArg == "list")
    {

    }
    else if (firstArg == "tags")
    {

    }
    else if (firstArg == "help")
    {

    }
    else if (firstArg == "edit")
    {
        // only check if the file exists here
        // do the rest later
    }

    // backup the blog
    BashlessBlog.Backup();

    // create or copy css files if needed
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

    }
    else if (firstArg == "delete")
    {

    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

void doNew()
{
    // get the optional title for the new draft
    bool useHtml = false;
    var title = String.Empty;
    if (args.Length == 2)       // could either be the -html option or the title
    {
        if (args[1] == "-html")
            useHtml = true;
        else
            title = args[1];
    }
    else if (args.Length == 3)  // if there are 3 args, expect -html and title
    {
        if (args[1] != "-html")
        {
            printHelp();
            return;
        }

        useHtml = true;
        title = args[2];
    }
    else if (args.Length != 1)
    {
        Console.WriteLine("Error: Invalid arguments");
        printHelp();
        return;
    }

    var draftPath = BashlessBlog.CreateNewDraft(useHtml, title);
    Console.WriteLine($"Draft written to {draftPath} - use 'bashlessblog post' to publish the post after editing");
}

void doPost()
{
    // post a draft or edited post to the blog
    // the second half of write_entry
    // TODO preview?
    var postPath = String.Empty;
    if (args.Length != 2)
    {
        Console.WriteLine("Error: Invalid arguments");
        printHelp();
        return;
    }

    postPath = args[1];

    if (!File.Exists(postPath))
    {
        Console.WriteLine($"Error: File does not exist: {postPath}");
        printHelp();
        return;
    }

    var content = BashlessBlog.GetDraftContentAsHtml(postPath);
    var filename = BashlessBlog.WriteEntry(content);
    Console.WriteLine($"Post written to {filename}");
}

void printHelp()
{
    var headerString = $"{Config.Internal.GlobalSoftwareName} version {Config.Internal.GlobalSoftwareVersion}";
    var helpText = """
                   Usage: bashlessblog command [option] [title/filename]
                   
                   Commands:
                       new [-html] [title]     create a new draft in the drafts folder, using markdown
                                               '-html' overrides the default behavior and creates an HTML draft
                                               'title' will override the default title with the supplied title, the title must be in quotes
                   
                       edit [filename]         create a draft from a previously posted post
                                               this will remove the post from the blog and rebuild the blog
                   
                       post [filename]         publishes a draft from the drafts folder
                                               if the title of a previously posted entry changes, the filename will change to match
                                               this operation rebuilds the blog
                   
                       delete [filename]       deletes the post and rebuilds the blog
                   
                       rebuild                 regenerates all the pages and posts, preserving the content of the entries
                   
                       list                    list all posts
                   
                       tags [-n]               list all tags in alphabetical order
                                               use '-n' to sort list by number of posts
                   
                   For more information please check the comments and config options in the source code
                   """;

    Console.WriteLine(headerString);
    Console.WriteLine(helpText);
}