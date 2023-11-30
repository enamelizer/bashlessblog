// See https://aka.ms/new-console-template for more information

using bashlessblog;

try
{
    // Load default configuration, then override settings with the config file
    var currentWorkingDir = Directory.GetCurrentDirectory();
    var configFile = Path.Combine(currentWorkingDir, ".config");
    if (!File.Exists(configFile))
    {
        Console.WriteLine(".config does not exist at " + currentWorkingDir);
        Console.WriteLine("Please set the working directory to a valid bashlessblog directory");
        return;
    }

    // load the config
    Functions.LoadConfig(configFile);

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
    Functions.Backup(currentWorkingDir);

    // create or copy css files if needed
    Functions.CreateCss(currentWorkingDir);

    // create or copy includes
    Functions.CreateIncludes(currentWorkingDir);

    if (firstArg == "new")              // new
    {
        // creates a draft file using the optional title
        // in the format specified
        // the first half of write_entry up until the editor opens

        doNew(currentWorkingDir);
    }
    else if (firstArg == "post")        // post
    {
        // post a draft or edited post to the blog
        // the second half of write_entry
        // TODO preview?

        var postPath = String.Empty;
        if (args.Length == 2)
        {
            postPath = Path.GetFullPath(args[1]);
        }
        else
        {
            printHelp();
            return;
        }

        if (!File.Exists(postPath))
        {
            printHelp();
            return;
        }

        var content = Functions.GetHtmlContent(postPath, true);

        Functions.WriteEntry(content, currentWorkingDir);

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

void doNew(string workingDir)
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
    else
    {
        printHelp();
        return;
    }

    var draftPath = Functions.CreateNewDraft(workingDir, useHtml, title);
    var relPath = Path.GetRelativePath(workingDir, draftPath);

    Console.WriteLine($"Draft written to {relPath} - use 'bashlessblog post' to publish the post after editing");
}

void printHelp()
{
    var headerString = $"{Functions.Config.GlobalSoftwareName} version {Functions.Config.GlobalSoftwareVersion}";
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