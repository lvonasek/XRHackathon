# Introduction
This is the first XR experience powered by your imagination. You'll be able to conjure up any object and play with it. The project works on AR/VR Quest headsets. To keep this project completely cost-free, a Sketchfab login is required.

[![Youtube video](https://user-images.githubusercontent.com/6472545/142728320-d1e61d87-8ef1-4dfb-b5d4-20be6e2b9d3c.png)](https://youtu.be/XpW1Pzdtq3w)

[Download APK](https://github.com/lvonasek/XRHackathon/releases/download/v0.0.1/XR_Conjuring.apk)

[How to sideload apps on the Oculus Quest](https://www.androidcentral.com/how-sideload-apps-oculus-quest)

# How it works
![Screenshot_20211121_170704](https://user-images.githubusercontent.com/6472545/142769779-fbc7b204-f5e9-4a56-b533-5600aa79f0bd.png)


# Used technology
The project is built using Unity 2019.4.32f1. It uses third party SDKs: 
* [Oculus Unity Integration v34](https://developer.oculus.com/downloads/package/unity-integration/34.0/)
* Modified version of [Sketchfab Unity integration](https://github.com/sketchfab/unity-plugin/releases/tag/1.2.1)

In Sketchfab Unity integration following changes were done:
* Added [SimpleJSON](https://github.com/Bunny83/SimpleJSON/tree/ac13597cb66536d08f89b3441bff6624564608cf) to fix missing dependency
* Ported integration to Android
* Removed prefab creation code
* Removed Sketchfab exporter
* Removed Sketchfab precompiled library
* Removed UI which didn't make sense anymore
* Replaced GLTF importer with [GLTFUtility](https://github.com/Siccity/GLTFUtility/releases/tag/0.7)

# Created art
* Magic ball was inspired by [Terminator 2 time travel effect](https://www.youtube.com/watch?v=iD-64QzizV4&t=23s). My version of the effect don't have lightning outside of the ball because it would make it too dramatic. Using passthrough layer I received automatically mask of the model and assign the secondary effect only on that object without need of writing complex shaders.

* Background music was originally a remix of a song [Gala - Come into my life](https://music.youtube.com/watch?v=5I0EHVyE_aA) I did 15 years ago. I reopened the project and made from it ambient music loop which has now with the original song nothing in common (except coords). This simple loop helped the experience to give it a nice relaxing feeling and also it is helped in audiovisual feedback.

# Project scripts
The project scripts are located in `Assets/Conjuring/Scripts`.

**Core** - the main logic
* `MagicBall.cs` - Magic ball state handling, assigning and destroying objects
* `ModelConjuring.cs` - Logic of conjuring objects using VoiceSDK and modified Sketchfab integration
* `SketchfabIntegration.cs` - Sketchfab login, model searching and downloading integration

**Screen** - actions called from UI
* `ScreenLogin.cs` - Integrates login screen persistency and actions
* `ScreenWait.cs` - Monitoring authentication process and decision of next screen to be shown

**Utils** - Audio, UI and other utilities
* `AudioVolumeHandler.cs` - Handling of background music volume
* `UITransform.cs` - Updates UI transformation to be visible to the user
* `VRPointer.cs` - Using controller or hand, this class integrates usage of UI objects

# Special thanks to
* Facebook/Meta for organizing [XRHackathon](https://xr2021.facebookhackathons.com/)
* the great hackathon community for sharing useful tips
* Sketchfab who provided me with a temporary enterprise account after I told them about this project.
* my girlfriend for supporting me
