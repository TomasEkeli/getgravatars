# getgravatars

Console application to download gravatar images and store them in a local folder.

Intende use us is to get the images of contributors in [gource](https://gource.io)
videos, but this can also be used for any other case where you'd like a local copy
of the gravatars of a set of e-mail addresses.

## Cloning

This repository has sub modules, clone it with:

```text
$ git clone --recursive <repository url>
```

If you've already cloned it, you can get the submodules by doing the following:

```text
$ git submodule update --init --recursive
```

## Usage

Once compiled you run the application like this:

```text
$ getgravatars -i {inputfile} -o {outputdirectory (optional)} -t {gravatarType (optional)} -s {size (optional)}
```

Known good gravatarTypes are (if none is provided `identicon` will be assumed)
- `mp`
- `identicon`
- `monsterid`
- `wavatar`
- `retro`
- `robohash`
- `blank`


This program downloads the gravatar images of e-mail addresses given in the inputfile
and stores them as the ""full-name.jpg"" in the output direcotory given (defaults
to "output")

The inputfile should contain emails and full names, one on each line

Example of an input-file:

> author.name@company.com\|Author Name<br/>
> author2.othername@company.com\|Author2 Othername

## Use With Gource

The goal is to get gource to generate a visualisation with the gravatar images.

Install [gource](https://gource.io) and [ffmpeg](https://ffmpeg.zeranoe.com/builds/),
and make sure they are on the path (or modify the commands below as fit).

Open a command window (bash, powershell, cmd, whatever..) and go to the root
directory of your project (where there is a `.git` -directory).

This assumes you have `getgravatars.exe`in your current working directory:

```text
$ git log  --pretty="%ae|%an" > authors.txt
$ ./getgravatars.exe -i authors.txt -o avatars -t monsterid -s 48
$ gource -title "You project name" -s 0.5 -a 1 ---user-image-dir avatars -hide filenames,dirnames --stop-at-end --user-scale 2 --highlight-all-users -1280x720 -o gource.ppm --date-format %Y-%m-%d
$ ffmpeg -y -r 60 -f image2pipe -vcodec ppm -i gource.ppm -vcodec libx264 -preset medium -pix_fmt yuv420p -crf 1 -threads 0 -bf 0 gource.mp4
```

These options for gource and ffmpeg work fine, and if you want others you should
go read up on those tools.

Lean back and watch the visualisation it will close automatically once it's done.
Encoding the `gource.ppm` -file takes a while, depending on how large it is and
your hardware. You will end up with a file called `gource.mp4` which is the video
of the gource visualisation.

You should probably delete the intermediate files once the final one is complete:

```text
$ rm authors.txt
$ rm gource.ppm
$ rm avatars/*.jpg
$ rm avatars
```

Have fun!