# EasyAPI

EasyAPI is an in-game script library for Space Engineers that aims to make the programmable blocks easier to use.

However, it is turning in to much more than that.

# What is coming soon

I'm working on a new website, http://spaceengineers.io, which, among other things, will be a place to manage Programming Block "modules".  If you have ever used a dependency manager like Composer for PHP it will be kind of like that.  

I'm hoping to combat the need for multiple programmable blocks on your ship, and to keep people from needing to reinvent the wheel all the time.

The idea is that instead of creating full scripts, people could create reusable libraries and/or classes that serve a specific purpose.  For example, some modules I have already created include EasyMenu, a menu generating class, and EasyLCD, a module to handle drawing to LCDs. Someone could select those modules, and the site will automatically bring in the libraries needed for those modules to work and generate a full script that's ready to be pasted into the programmable block.

If possible, the site will use steam authentication and will support linking a module to a steam workshop item.

# Random feature ideas

Automatically minify scripts to save space since there is a limit.

Generate a DoxyGen documentation page for each library that is submitted to the site.

Newb script generator.  Generate scripts without programming by specifying events and actions to perform.
