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
Select `Build Settings` from the `File` menu. Click `Build`, select the `_builds` folder to build your project into.

*Please do not build into the same directory as this project.*

`_builds/assets/` contains sample image and video files.
This project is currently hardcoded to pull its image/video content from this folder so you can create multiple builds when testing without duplicating these large assets for each build.

Do not remove or rename any of the subfolders in `_builds/assets/`, but you can add/remove/rename files within each subdirectory if need be. Unity will load all the files in each subfolder based on the .extentions.

All files in the `_builds` directory will be ignored by Git *except* `_builds/assets/`. So you can safely keep your working builds in `_builds` without clogging up the repo.
