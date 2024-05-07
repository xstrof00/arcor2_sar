# ARCOR2 SAR

This repository contains user interface, based on projected augmented reality, which complements [AREditor](https://github.com/robofit/arcor2_areditor), the main user interface of the [ARCOR2](https://github.com/robofit/arcor2) system.
Video, showcasing this project is published here: https://youtu.be/wQrgCxiG_Vg?si=mBBA77-IEYMHNidJ

## Usage

### Calibration

First of all, you need to calibrate the system. To help you with that, you can use this [guide](https://github.com/xstrof00/arcor2_sar/blob/main/Calibration/README.md).

### Building in Unity

After calibration, it is necessary to build the project in Unity, so that the project can use the calibration results from previous step when creating the scene.
Open the folder in Unity Hub as a new project and open it.

Before building, you need to load the scene, containing Kinect, projector and Canvas objects. You can do that by going to `File -> Open Scene` and there, selecting the `SampleScene.unity`, which is located in the `Assets/Resources` folder.
Then, import TMP packages if Unity asks you to.

After this, you can build it by going to `File -> Build Settings...` and there, make sure the Scene is selected. There, set your OS as a target platform and build the game.

### Run the application

Last step is to run the application, using the built executable. It should connect to the ARServer automatically.
