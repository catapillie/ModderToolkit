# Modder's Toolkit
Useful tools & utilities for Celeste modding.



## Tools

Tools are the main features of this mod. They only work in-game - that means, when you are inside a level. 



### Screenshot

**Default keybind: `F8`** (open & exit menu)

**Controls: `ENTER`** (save screenshot)

This tool allows you to capture the entire screen, or a subregion of it.
Screenshots are saved in a newly created `Screenshots/` directory, next to the Celeste executable.
Screenshotting with this tool constrains the image to be pixel-perfect, meaning you can't select a region whose bounds lie between two in-game pixels.
By default, the exported images are unscaled, but you can configure them to be rescaled, up to 16x larger.
This tool isn't really super useful, as it won't even copy your captures into your clipboard, but if you really want the pixel-perfectness, and a known size, this can probably help.


### Player Recording

**Default keybind: `F12`** (start & stop rec.)

The Player Recording tool will allow you to record what's called "player playback data", which are mostly used for player tutorials.
Playbacks will be saved as `.bin` files, in a newly created `Playbacks/` directory, next to the Celeste executable.
By default, a three-second countdown starts when you trigger a new recording: this can be disabled in your mod settings.
Note that the recording might cancel automatically in somes cases, for instance, when the player dies.



## Bugs & issues
If you happen to find a bug in this mod, please [open a new issue](https://github.com/catapillie/ModderToolkit/issues/new).
If you're unable to do so, I invite you to message me at `@catapillie#1927` on Discord, or find me on the [Mt. Celeste Climbing Association](https://discord.gg/celeste) server!
