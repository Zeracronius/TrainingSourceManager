# Training Source Manager

A light tool for managing and tagging source files, for example to use for training AI.
With full support for drag'n drop and double click.

Dragging one or multiple files into the window will create a new source entry with the files attached, while dragging them into the file list of an existing source will attach them to that.
Similarly dragging files into windows will export those files to the specified folder.

Double clicking files will export them to %tmp% and open them.

![SourceManager](https://user-images.githubusercontent.com/64704191/141687874-956ce147-392e-4dfa-9788-f2e696082d6e.png)

## Filtering
The filtering format uses + and - to define tags to include or exclude.<br>
Everything after a tag filter is started is read as a single tag unless another tag filter is started. Trailing spaces are ignored.

Format:<br>
[Name contains] +[Include tag] -[Exclude tag]

The filter is not case sensitive.

For example, "<b>Source +Tag One -Tag2 +Tag3</b>" will filter to anything that includes "source" in the name and has tags "tag one" and "tag3" but not "tag2".