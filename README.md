# bashlessblog

[bashblog](https://github.com/cfenollosa/bashblog) but less

Bashlessblog is a simple static site generator that outputs a blog.

It is a .net port of the excellent bash script [bashblog](https://github.com/cfenollosa/bashblog) which was originally written by [Carlos Fenollosa](https://github.com/cfenollosa).

This project aims to take everything that makes bashblog great and apply it to a different workflow. Namely decoupling the editing and publishing process from a remote server and instead having a local first workflow and publishing the output more like a traditional static site generation workflow. Basically I wanted more freedom in text editors, more compatibility with syncing using git, and more cross platform support.

![Image of blog post](https://github.com/enamelizer/bashlessblog/blob/main/blog.png?raw=true)

## Getting Started

1. Install the [.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) on your target platform.
2. Create a directory where the blog artifacts will live (config, inputs, outputs, etc.)
3. Copy the bashlessblog utility to this directory
4. Run the command `bashlessblog init` to initialize the default blog structure
5. Edit the `.config` file, the `GlobalUrl` setting is especially important as this defines the base URL for the blog. I also suggest changing the license to something more permissive like a Creative Commons license.
6. Run the command `bashlessblog new` to create a new draft in the drafts directory
7. Edit your new post using your favorite text editor
8. Publish the post using `bashlessblog post drafts\title-on-this-line.md` - don't worry the filename will be updated with the title you supplied when you edited the post
9. Check out your new post in the `output` directory
10. Publish the contents of the `output` directory to your favorite web host

## Usage

`bashlessblog init`  
Initializes a new blog structure in the current directory

`bashlessblog new [-html] [title]`  
Creates a new draft in the drafts folder, using commonmark flavored markdown
'-html' overrides the default behavior and creates an HTML draft
'title' will override the default title with the supplied title, the title must be in quotes

`bashlessblog edit [filename]`  
Creates a draft from a published post and depublishes the post

`bashlessblog post [filename]`  
Publishes a draft post to the blog

`bashlessblog delete [filename]`  
Deletes a published post

`bashlessblog rebuild [-all]`  
Regenerates all the pages and posts, preserving the content of the entries
'-all' will regenerate the CSS, title, header, and footer files (caution: custom and edited files will be deleted!)

`bashlessblog list`  
Lists all posts

`bashlessblog tags [-n]`  
Lists all tags in alphabetical order
Use '-n' to sort list by number of posts

For more information and advanced features, please check the comments and configuration options in Config.cs

## Compatibility with bashblog

This project largely maintains compatibility with bashblog and can be used with an existing bashblog blog. The differences are:

* Writing a post is separated into two distinct steps, creating a draft and publishing that draft
* The editing of a draft is an external step and is done outside of the program flow
* Post dates are generated from the embedded timestamps in the post content, this preserves post dates from being mangled when used with git
* The directory structure is organized into inputs and outputs instead of everything being in a single directory
* Social media and third party tracking support is removed
* Removed `reset` command, see: `man rm`

While I did port the functionality for static pages and personalized headers and body, these features are untested.

### Using bashlessblog with an existing bashblog

To use bashlessblog with an existing bashblog, add the following items to `.config` so bashlessblog will use the single directory structure

    BackupDir=""
    IncludeDir=""
    OutputDir=""
    CssInclude=('main.css' 'blog.css')
    HeaderFile=".header.html"
    FooterFile=".footer.html"

### Upgrading an existing bashblog blog to bashlessblog defaults

1. Create three new directories in the bashblog directory
    1. `backup`
    2. `includes`
    3. `output`
2. Move any backup archives to the `backup` directory
3. Move `.footer.html`, `.header.html`, `.title.html`, `blog.css` and `main.css` to the `includes` directory
4. Combine the contents of `blog.css` and `main.css` into a single `blog.css`, deleting `main.css`
5. Move everything EXCEPT `bashlessblog(.exe)` and `.config` into the `output` directory

## Acknowledgments

This software is a port of [bashblog](https://github.com/cfenollosa/bashblog) and uses [Markdig](https://github.com/xoofx/markdig) for it's markdown support.

Thanks to:

[Carlos Fenollosa](https://github.com/cfenollosa) for creating [bashblog](https://github.com/cfenollosa/bashblog)

[Alexandre MUTEL](https://github.com/xoofx) aka [xoofx](https://xoofx.com/) author of [Markdig](https://github.com/xoofx/markdig) 
