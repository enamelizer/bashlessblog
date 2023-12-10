﻿// See https://aka.ms/new-console-template for more information

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
        // TODO
    }
    else if (firstArg == "tags")
    {
        // TODO
    }
    else if (firstArg == "help")
    {
        // TODO
    }

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
        // rebuild all entries and tags
        BashlessBlog.Rebuild();
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
    // TODO preview?
    if (args.Length != 2)
        errorAndExit("Error: Invalid arguments");

    var draftPath = args[1];

    if (!File.Exists(draftPath))
        errorAndExit($"Error: File does not exist: {draftPath}");

    var filename = BashlessBlog.WriteEntry(draftPath);
    Console.WriteLine($"Post written to {filename}");
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
    Console.WriteLine($"Deleted {postPath} and created the draft {draftPath}");
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
                       new [-html] [title]     create a new draft in the drafts folder, using markdown
                                               '-html' overrides the default behavior and creates an HTML draft
                                               'title' will override the default title with the supplied title, the title must be in quotes
                   
                       edit [filename]         create a draft from a published post and depublish the post
                   
                       post [filename]         publishes a draft post to the blog
                                               if the title of a previously posted entry changes, the filename will change to match
                                               this operation rebuilds the blog
                   
                       delete [filename]       delete a published the post
                   
                       rebuild                 regenerates all the pages and posts, preserving the content of the entries
                   
                       list                    list all posts
                   
                       tags [-n]               list all tags in alphabetical order
                                               use '-n' to sort list by number of posts
                   
                   For more information please check the comments and config options in the source code
                   """;

    Console.WriteLine(headerString);
    Console.WriteLine(helpText);
}