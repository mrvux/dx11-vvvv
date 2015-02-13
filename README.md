dx11-vvvv
=========

DirectX11 Rendering within vvvv

# How to build

First of all, get the code of master branch

    git clone -b master git@github.com:mrvux/dx11-vvvv.git

Then update submodules.

    cd dx11-vvvv/
    git submodule init
    git submodule update

Open the `vvvv-dx11.sln` solution with Visual Studio: I installed Microsoft Visual Studio Express 2012 for Windows Desktop.

Set up targets, see screenshots below.

![open-configuration-manager!](https://raw.github.com/mrvux/dx11-vvvv/alphatest/images/OpenConfigurationManager.png)

![set-targets!](https://raw.github.com/mrvux/dx11-vvvv/alphatest/images/SetTargets.png)

Now, if you hit the Rebuild Solution button and you will gain a brand new lib and packs folders under Deploy/Debug folder.

![rebuild-solution!](https://raw.github.com/mrvux/dx11-vvvv/alphatest/images/RebuildSolution.png)

Copy the content of vvvv core build under Deploy/Debug (recommended by mrvux if you want to build multiple times) or viceversa, copy lib and packs folders inside your vvvv folder.


