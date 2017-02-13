# How to contribute

One of the easiest ways to contribute is to participate in discussions and discuss issues. 
You can also contribute by submitting pull requests with code changes.

Here is a list of (looked after) contributions:
* Submitting missing help patches : Please look into [Missing Help](https://github.com/mrvux/dx11-vvvv/blob/master/MISSINGHELP.md) page to see which nodes require help file.
* Bug fixes : You can look at [Issues](https://github.com/mrvux/dx11-vvvv/issues) and check for the up-for-grabs flags (in case of issues it either means it's very specific, or that I miss the hardware combination to fix it myself).
* Enhancements : Same as above, but for additional features/nodes.

Note: All new contributions should be provided as Pull Requests.

The only exception to it is Missing Help patches, which can be submitted as issue with a zip (ideally, if you only need to work on help, you can clone [Girlpower](https://github.com/mrvux/dx11-vvvv-girlpower/tree/master) instead of the whole dx11 repository

## Bugs and Issues

Please log a new issue in the GitHub repo.

The best way to get your bug fixed is to be as detailed as you can be about the problem.

Here is a list of informations you should provide while filing a bug (eg: to greatly increase chances to have it solved):
* Which vvvv + pack version (and 32/64 bits)
* Does the issue happens with other vvvv / pack version combinations?
* System information (Operating system, graphics card model), also you should ensure that all relevant c++ redistributables are installed on your machine.
* Do you have a version of visual studio installed (if yes, which).
* Logstartup information (if relevant), please include as a text file attachment, do not add in the issue itself.
* Example patch reproducing the issue : This should work on a barebone vvvv version (with addonpack). 
* Reproduction steps (eventual screen capture) : This is identical as above, I should be able to replicate the process, 
so I can't add nodes which are part of your library (plus it means that it could be an issue with your code). You can send a small patch as a starting point, but I will not debug your whole library.

Please note that one liners will be closed immediately, if you don't make at least any effort to provide az minimum of information as specified above, don't expect other people to make effort for you.


## Contributing Code

Here are (non exhaustive) guidelines:
* Code should of course be built (now this is semi automated via appveyor).
* Tests should pass (except missing help patches until they are all present)
* Since latest releases, warning are treated as errors. So hiding those in pragmas (unless very specific cases which you should explain why), will mean that pull request will not be accepted.
* The minimum vvvv supported version is 33.7 (with a plan to move to 34.2), so please make sure you contribution works with that version (this is especially valid when contributing modules).
* Examples, help patches should also work with a barebone vvvv version (that means WITHOUT addonpack).
* In case of new nodes, please provide a help patch, there is now an automated testing to check that nodes have their help patch (so you will fail tests)
* Large pull request may take a while to go trough, so try to split your requests in several parts (when relevant of course). 
* Submitted code becomes part of the repository, current license (BSD) applies to it as soon as pull request is accepted and code is merged.
* As the repository owner, by accepting your pull request I take responsibility of maintaining your changes/additions, so while that means I will not come at yopur doorstep to ask you to fix a bug, but it might take a while for your contribution to be accepted.

