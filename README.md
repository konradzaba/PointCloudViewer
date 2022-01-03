# PointCloudViewer
Sample application which uses Xamarin with Monogame framework to visualize point clouds

![Alt text](https://i.imgur.com/p3dg1xe.png)

A point cloud is a set of data in some coordinate system. One of the sources for point cloud data is LIDAR, that is a suerveying method that measures distance to a target by illuminating that target with laser light and measuring the reflected pulses with a sensor. This in turn allows for a very precise 3D representation of target (usually: terrain). These types of data are used in GIS.

[![Youtube video](https://i.imgur.com/Dwii6WE.jpg)](https://www.youtube.com/watch?v=v5nRHh4IKkc)

This is only a simple proof of concept application, that shows professional usage of Xamarin and Monogame. Apart from the hard codes as well as obvious features missing (listing downloaded files etc) the following functionalities would need to be developed:
- Advanced level of detail
- Dynamic point cloud loading (to support larger files)
- Support for LAS files,
- Automatic quality settings depending on devices
- Graceful exit on Android (right now you need to force closing the app)
- and more

You can check the compiled version for Android devices here: https://play.google.com/store/apps/details?id=com.konradzaba.PointCloudViewer

The UWP version for Windows must be compiled manually from the source code.
