# corteva_wall

## Unity 
This project was built using the latest Untiy 2017.4 LTS (2017.4.25f1 at time of original commit). Please only edit this project using a version of Unity from this LTS build.

*The LTS version updates include address crashes, regressions, and issues that affect the wider community but do not add any new features, API changes or improvements.*

The latest version can be downloaded here: [https://unity3d.com/unity/qa/lts-releases](https://unity3d.com/unity/qa/lts-releases)

## Git
This project uses [git-lfs](https://git-lfs.github.com/) for large file managemement. Please make sure you have it installed.

## Project

### Opening
After pulling down the repo, open Unity, select `Open` in the Projects dialogue, 
select the main folder of this project (`Corteva` at the moment), click open.

### Building
Select `Build Settings` from the `File` menu. Click `Build`. 

Make sure to not build your project into the same directory as your working files. 

The `assets` folder in this repo contains sample image and video files.

**dev** Make sure to move or copy the `assets` folder into the same directory that contains your build files. 
The project is currently hardcoded to pull its content from this folder. 
This way, you can have multiple builds for testing without duplicating these large assets for each build.

Do not remove or rename any of the folders in the directory, you can add/remove files within each subdirectory if need be. Unity pulls all the files in each subfolder based on the .extentions.
