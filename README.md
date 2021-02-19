# Table of Contents:
![Build status](https://img.shields.io/appveyor/build/FIFOFridge/AOEMatchDataProvider/main?label=build%3A%20main)
![Tests](https://img.shields.io/appveyor/tests/FIFOFridge/AOEMatchDataProvider?label=tests%3A%20main)
![Build branch: Stable](https://img.shields.io/appveyor/build/FIFOFridge/aoematchdataprovider/stable?label=build%3A%20stable)
![License shield](https://img.shields.io/github/license/FIFOFridge/AOEMatchDataProvider)

## For users
- [App overview](#app-overview)
- [Installation](#installation)
- [Configuration](#configuration)
    - [Getting started with Steam version of game client](#getting-started-steam)
    - [~~Getting started with Windows Store version of game client~~](#getting-started-windows-store)
    - [Hotkeys](#hotkeys)
    - [App license](#license)
        - [3rd party licenses](#license)
- [App issues/bugs](#issues)
## For developers
- [Tools](#tools)
- [Testing](#testing)
- [Contribution](#contribution)
    - [Code of conduct](#code-of-conduct)
- [Deployment](#deployment)

#### Please take a note
##### Application is not releated nor owned by any of: "Microsoft", "Steam", "Forgotten Empires", "Tantalus Media", "Wicked Witch" or any other company
##### Application supports only "Age Of Empires 2 Definitive Edion"

# App overview:
## Description 

Application was made to simply track CURRENT MATCH data, like:
- [x] Players names
- [x] Complete player ELO(s) (app will find 1v1 rating for each player during Team Game)
- [x] Players colours
- [ ] Game server location
- [ ] Other player stats like streak(s) and/or country

Application is designed to behave like overlay so you can pop it up and hide at any moment, without colliding with your actual game, for more details check: [Hotkeys](#hotkeys)

##### Image: In game teams view
![in game teamsView image](https://raw.githubusercontent.com/FIFOFridge/AOEMatchDataProvider/main/.assets/.github/teamspanel_crop.png)

##### Image: In game notification window
![in game notification window image](https://raw.githubusercontent.com/FIFOFridge/AOEMatchDataProvider/main/.assets/.github/notification_crop.png)

#### [↑↑ Back to "Table of Contents"](#table-of-contents)

# Installation
[Here](https://github.com/FIFOFridge/AOEMatchDataProvider/releases) [(https://github.com/FIFOFridge/AOEMatchDataProvider/releases)](https://github.com/FIFOFridge/AOEMatchDataProvider/releases) you can find newest app realeases, download windows installer, run it and follow through installation process.

## Picking app installer:
- Release.zip - (No installation required) File packed up with Zip archive, simply unpack them anywhere and run AOEMatchDataProvider.exe
- Setup.exe - One click installer which will create shortcuts to desktop and start menu
- ~~BundledInstaller.exe - Single file installer, which will create shorcut on desktop, start menu and delete software shortcut~~

#### [↑↑ Back to "Table of Contents"](#table-of-contents)

# Configuration
## Getting started Steam

### 1. Navigate to Steam App -> Your Profile
![Navigation to profile image](https://raw.githubusercontent.com/FIFOFridge/AOEMatchDataProvider/main/.assets/.github/setup_0.png)

### 2. Copy your profile URL
![Steam profile URL copy image](https://raw.githubusercontent.com/FIFOFridge/AOEMatchDataProvider/main/.assets/.github/setup_1.png)

### 3. Navigate to: [steamid.io](https://steamid.io/) paste yor (copied) URL into form, and submit

### 4. Copy "SteamId64"
![Copy steam id 64 image](https://raw.githubusercontent.com/FIFOFridge/AOEMatchDataProvider/main/.assets/.github/setup_2.png)

### 4. Paste "Steamid64" into app configuration
![Paste steam id 64 into app image](https://raw.githubusercontent.com/FIFOFridge/AOEMatchDataProvider/main/.assets/.github/setup_3.png)

#### [↑↑ Back to "Table of Contents"](#table-of-contents)

## ~~Getting started Windows Store~~

Not ready yet

#### [↑↑ Back to "Table of Contents"](#table-of-contents)

# Hotkeys
## General
Toggling app visibility works anywhere except configuration screens, (downloading app resources and setting up user id)
To toggle app visibility use: `HOME` or `END`

## Notification
When notification screen pops up use: 
* `HOME` to display match data
* `END` to hide match data (you can always bring it back later using above hotkeys)

#### [↑↑ Back to "Table of Contents"](#table-of-contents)
## Special thanks to ❤️ aoe2.net ❤️
App data source comes from an unofficial AoE2 API: [aoe2.net](https://aoe2.net/)

#### [↑↑ Back to "Table of Contents"](#table-of-contents)
# License
Application code (with a few exceptions marked in code comments) is licensed Under Apache 2.0, license content can be found [here](https://github.com/FIFOFridge/AOEMatchDataProvider/blob/main/LICENSE)

## 3rd party licenses
## While using app you are accepting 3rd party licenses:
Application is using 3rd party resources like software libraries, custom fonts, icons, and other.
Their licenses can be found in application licenses window "i" icon on title bar (top part of application).

#### [↑↑ Back to "Table of Contents"](#table-of-contents)

# Issues
If you find any bugs, or app is not working as expected then submit ["Issue" HERE](https://github.com/FIFOFridge/AOEMatchDataProvider/issues)

In case of bugs please include log file that can be found there: `C:\Users\USER\AppData\Roaming\AOEMatchDataProvider\AppVersion`
#### [↑↑ Back to "Table of Contents"](#table-of-contents)
------------------------------------------------------------------
# For developers
# Tools
Tools required:
- Visual Studio 2017 or Visual Studio 2019 with WPF (Windows Presentation Foundation) support
- Nuget Package Manager (to install dependencies)
- (Optional) AutoIt 3
#### [↑↑ Back to "Table of Contents"](#table-of-contents)
# Testing
Running tests:
- Running tests will require MSTestAdapter and MSTestFramework (versioned by test project)
- Tests can be launched by visual studio "test explorer"
- Tests can be launched from powershell command line using included in repo scripts, please read Test/AoeMatchDataProviderTests/aboutTestExecution.txt to run them from powershell command line
#### [↑↑ Back to "Table of Contents"](#table-of-contents)
# Contribution 
To contribute project:

1. Clone repo
2. Create new branch
3. Implement changes/features/bug fixes
4. Submit pull request

# Code of conduct
Application is based on [MVVM structure](https://prismlibrary.com/docs/wpf/legacy/Implementing-MVVM.html):

![MVVMImage](https://prismlibrary.com/docs/wpf/legacy/images/Ch5MvvmFig1.png)

##### Image: prismlibrary.com

Extended by PRISM framework solutions:
- [Event Aggregator](https://prismlibrary.com/docs/event-aggregator.html)
- [Composite Commands](https://prismlibrary.com/docs/composite-commands.html) (IApplicationCommands)
- [View Model Locator](https://prismlibrary.com/docs/viewmodel-locator.html)
- [Region Navigation](https://prismlibrary.com/docs/wpf/region-navigation/basic-region-navigation.html) (see Views\Shell.xaml "ContentPresenter"s)

Core logic is provided by Services stored in DI container (Unity)

Simply follow these rules while contributing to project

# Deployment
## Bundling with Squirrel
For bundling with Squirrel check:
[https://github.com/Squirrel/Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows)

- Prepare nuget package: formatted as `"AOEMatchDataProvider-$VERSION.nupkg"` where `$VERSION` is same as deployment version
- Verify package, create matching git tag (and push it to origin/main) using `.tag-version.ps1`
- Use Squirrel to create installer (`Squirrel --relasify $VERSION`)

## Bundling with AutoIt

### Avoid bundling using autoit without valid code signing certificate!
#### Windows defender often marks autoit binaries as virus

Use 'Publish' from Visual Studio project view to create installer.
To bundle it into single file executable you will need [AutoIt 3](https://www.autoitscript.com/site/autoit/downloads/) Installed

After publishing execute: 
```
.bundler-generate-file-list.au3
```
The above command will generate file list for bundler to include:
```
.bundle-file-list.au3
```
Last step is to compile bundler into executable:
```
.bundle-deploy.au3
```