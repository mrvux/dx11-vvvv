dx11-vvvv
=========

DirectX11 Rendering within vvvv

[![Build status](https://ci.appveyor.com/api/projects/status/79m7u3obr15yopct?svg=true)](https://ci.appveyor.com/project/mrvux/dx11-vvvv)

# Licensing

vvvv dx11 nodes are released under a 3 clause [BSD license](https://raw.githubusercontent.com/mrvux/dx11-vvvv/master/License.md)

This library is free, but you are welcomed to support its development.

## Monthly donations via Patreon

[![Patreon](https://cloud.githubusercontent.com/assets/8225057/5990484/70413560-a9ab-11e4-8942-1a63607c0b00.png)](http://www.patreon.com/mrvux)

## Yearly invoice

If you are a company and Patreon is not ideal for you (exchange rates and inability to get proper invoices), I am also able to provide you a custom invoice (I am a registered company in France).

You will get the same rewards as per Patreon yearly equivalent.

## Custom develompent

If you have specific requirements, you can also sponsor custom development (either tailored to your requirements and private, or contributed back in the public repository).

# How to build

First of all, get the code of master branch

    git clone -b master git@github.com:mrvux/dx11-vvvv.git

Then update submodules.

    cd dx11-vvvv/
    git submodule init
    git submodule update

Open the `vvvv-dx11.sln` solution with Visual Studio: I installed Microsoft Visual Studio Express 2012 for Windows Desktop.

Set up targets, see screenshots below.

![open-configuration-manager!](https://raw.github.com/mrvux/dx11-vvvv/master/images/OpenConfigurationManager.png)

![set-targets!](https://raw.github.com/mrvux/dx11-vvvv/master/images/SetTargets.png)

Now, if you hit the Rebuild Solution button and you will gain a brand new lib and packs folders under Deploy/Debug folder.

![rebuild-solution!](https://raw.github.com/mrvux/dx11-vvvv/master/images/RebuildSolution.png)

Copy the content of vvvv core build under Deploy/Debug (recommended by mrvux if you want to build multiple times) or viceversa, copy lib and packs folders inside your vvvv folder.


