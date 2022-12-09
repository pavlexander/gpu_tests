# GPU tests

Testing computations on `GPU`, using `OpenCl`, `CUDA`, in future maybe `Vulkan`. All of this preferably on `.Net 6`

## Prerequisites

* `Visual studio 2022 w/ C++/CLR component`
* `.NET 6`
* `CUDA 11.2`

## OpenCL

For `OpenCL` I used the `CLEditor` to write and test-compile the kernel. You can also use the `OpenCLTemplate`, I think.

## Building

Until I learn how to commit build configuration in `git`, you will have to set following config, in order to things start working out-of-the-box.

![Image of build configuration](buildConfig.png)

Also, CUDA will perform a lot faster, when optimizations are applied, so I also recommend setting the following:

![Image of build configuration](buildConfig2.png)

## Performance

### Results (25.02.2020)

![Image of results](results.png)

```diff
- Current issues (25.02.2020):
```

* Native Cuda code launcher is slower than most of the methods, except the CPU. At the same time - ManagedCuda performs very well, despite using the same base as native.. Something wrong with the compiler settings??

### Results (28.02.2020)

Found the issue. It was the wrong number in grid/block size. After correction everything performs very well.

![Image of results](resultsNew.png)

Worth mentioning

* writing a CLR wrapper is more complicated than writing `managedCuda` code.

* writing `managedCuda` code is a bit harder than writing `ILGPU` code.

* writing `OpenCL` will make your code available on `Radeon` video cards, by sacrificing a tiny little bit of performance..

The choice is hard. 

### Results (09.12.2022)

Following updates were done:

* solution was migrated `VS2019` -> `VS2022`
* `CUDA` 10.2->11.2
* libraries got updated to latest versions (ex. `ILGPU`)
* .net `standard 2.0` -> 2.1
* `.Net Core 3.1` -> `.NET6`

Plus some minor modifications to kernel file.

![Image of results](results09122022.png)

### Settings:

It is possible to increase number of computation performed by setting the `DataGenerator` properties. For example: `DataGenerator.Height`.
