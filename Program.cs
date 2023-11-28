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
        Console.WriteLine("Please set the working directory to a valid BashlessBlog directory");
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

    if (firstArg != "new" && firstArg != "post" && firstArg != "rebuild" && firstArg != "list" && firstArg != "tags" && firstArg != "delete" && firstArg != "help")
    {
        printHelp();
        return;
    }

    // backup the blog
    Functions.Backup(currentWorkingDir);

    // do the things
    if (firstArg == "new")              // new
    {
        // get the optional title for the new draft
        bool useHtml = false;
        var title = String.Empty;
        if (args.Length == 2)
        {
            if (args[1] == "-html")
                useHtml = true;
            else
                title = args[1];
        }
        else if (args.Length == 3)
        {
            if (args[1] == "-html")
                useHtml = true;
            
            title = args[2];
        }

        Functions.CreateNewDraft(currentWorkingDir, useHtml, title);
    }
    else if (firstArg == "post")        // post
    {

    }
    else if (firstArg == "rebuild")
    {

    }
    else if (firstArg == "list")
    {

    }
    else if (firstArg == "tags")
    {

    }
    else if (firstArg == "delete")
    {

    }
    else if (firstArg == "help")
    {

    }
    else
    {
        printHelp();
        return;
    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

void printHelp()
{
    var headerString = String.Format("{0} version {1}", Functions.Config.GlobalSoftwareName, Functions.Config.GlobalSoftwareVersion);
    var helpText = @"Usage: bashlessblog command [option] [title/filename]
     
Commands:
    new [-html] [title]     create a new draft in the drafts folder, using markdown
                            '-html' overrides the default behavior and creates an HTML draft
                            'title' will override the default title with the supplied title
    post [filename]         posts a draft or a previously posted entry
                            if the title of a previously posted entry changes, the filename will change to match
                            this operation rebuilds the blog
    delete [filename]       deletes the post and rebuilds the blog
    rebuild                 regenerates all the pages and posts, preserving the content of the entries
    list                    list all posts
    tags [-n]               list all tags in alphabetical order
                            use '-n' to sort list by number of posts
     
For more information please check the comments and config options in the source code";

    Console.WriteLine(headerString);
    Console.WriteLine(helpText);
}