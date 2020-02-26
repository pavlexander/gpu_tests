# GPU tests

Testing computations on GPU, using OpenCl, CUDA, in future maybe Vulkan. All of this preferably on .Net Core

## Prerequisites

* Visual studio 2019 w/ C++/CLR component
* .Net Core 3.1
* Cuda 10.2

## Building

Until I learn how to commit build configuration in git, you will have to set following config, in order to things start working out-of-the-box.

![Image of build configuration](buildConfig.png)

Also, CUDA will perform a lot faster, when optimizations are applied, so I also recommend setting the following:

![Image of build configuration](buildConfig2.png)

## Performance

Current results (25.02.2020):

![Image of results](results.png)

```diff
- Current issues (25.02.2020):
```

* Native Cuda code launcher is slower than most of the methods, except the CPU. At the same time - ManagedCuda performs very well, despite using the same base as native.. Something wrong with the compiler settings??

### Settings:

It is possible to increase number of computation performed by setting the `DataGenerator` properties. For example: `DataGenerator.Height`.
