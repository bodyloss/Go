Go
==

Go lets you add identifier->commands pairs to a file and then execute that command by using the identifier, just like alias's in bash.
To use it just compile (specifying the file you want to read/write from) and plonk the executable in a directory which you have on your %PATH%.
Then in the Run Dialog (WindowsKey + R) type "go add identifier command", now you can enter "go identifier" to run that command, all nice and easily from the run dialog.

Usage:
go add identifier command
go list [order]
go clear
go identifer

It should be noted that while I realize that the way this is coded could be described as horrible or horrendous, I was looking for an intresting way of performing different actions based on the command line arguments and this seemed like an intresting way of doing it. Obviously it could be written much more elegantly than by using a hashtable matching regex's to inline Funcs but oh well, its fun :]

TODO
==
- Add a method of identifying if a command should be run as an executable or though a shell
- Add passing on any extra arguments when using "go identifier" as arguments to the command
- Use more inline funcs
