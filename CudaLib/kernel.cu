#include <cuda.h> 
#include "cuda_runtime.h"
#include <device_launch_parameters.h> 
#include <texture_fetch_functions.h> 
#include <builtin_types.h> 
#include <vector_functions.h>

//#include <cassert>
//#include <cstdio>
//#include <cfloat>
//#include <cinttypes>
//#include <algorithm>
//#include <memory>
//#include <curand_kernel.h>

//#include "float.h"
#include <stdio.h>

#include "kernel.h"
#include <iostream>

//#define _SIZE_T_DEFINED

// https://stackoverflow.com/questions/6061565/setting-up-visual-studio-intellisense-for-cuda-kernel-calls
// nvcc does not seem to like variadic macros, so we have to define
// one for each kernel parameter list:
#ifdef __CUDACC__
#define CUDA_CALLABLE_MEMBER __host__ __device__
#define KERNEL_ARGS2(grid, block) <<< grid, block >>>
#define KERNEL_ARGS3(grid, block, sh_mem) <<< grid, block, sh_mem >>>
#define KERNEL_ARGS4(grid, block, sh_mem, stream) <<< grid, block, sh_mem, stream >>>
#else
#define CUDA_CALLABLE_MEMBER
#define KERNEL_ARGS2(grid, block)
#define KERNEL_ARGS3(grid, block, sh_mem)
#define KERNEL_ARGS4(grid, block, sh_mem, stream)
#endif

// Now launch your kernel using the appropriate macro:
//kernel KERNEL_ARGS2(dim3(nBlockCount), dim3(nThreadCount)) (param1);

/*
#ifndef __CUDACC__
#define __CUDACC__
#endif
*/

/*
#ifndef __cplusplus
#define __cplusplus
#endif
*/

// Texture reference
//texture<float2, 2> texref;

// restrict
// https://devblogs.nvidia.com/cuda-pro-tip-optimize-pointer-aliasing/
// http://www.orangeowlsolutions.com/archives/310

// shared
// https://stackoverflow.com/questions/16754885/is-it-worthwhile-to-pass-kernel-parameters-via-shared-memory
// https://stackoverflow.com/questions/7903566/how-is-2d-shared-memory-arranged-in-cuda?rq=1
// https://devblogs.nvidia.com/using-shared-memory-cuda-cc/

// shared and blocks
// https://stackoverflow.com/questions/43195914/gpu-shared-memory-practical-example

// blocks !!
// https://algoslaves.wordpress.com/2013/09/16/real-life-cuda-example-time-series-denoising-with-daubechies-4-discrete-wavelet-transform-with-managedcuda-and-c/


// has to be extern, so that managed cuda could see it
// todo: try without __restrict__ 
// todo: try using shared memory for parameters vs passing as parameters
// todo: test what |extern "C"| does exactly with generated code
// todo: check what blocks and grids are
// todo: use tenary operator
// todo: use 1D array instead of 2D
extern "C"
{
	//__device__ __constant__ int width;
	//__device__ __constant__ int inputCount;
	//__device__ __constant__ int height;

	//kernel code
	__global__ void proccess(
		unsigned char* __restrict__ output,
		double* __restrict__ outputCalc,

		const int* __restrict__ in1,
		const int* __restrict__ in2,
		const double* __restrict__ in3,
		const unsigned char* __restrict__  in4, // unsigned char

		const int inputCount,
		const int width,
		const int height
	)
	{
		//int index = threadIdx.x;
		int index = blockIdx.x * blockDim.x + threadIdx.x;

		if (index >= inputCount) {
			return;
		}
		
		bool isTrue = false;
		int varA = in1[index];
		int varB = in2[index];

		// __shared__ double calculatable = 0;;
		double calculatable = 0;
		//bool result = false;

		// __syncthreads();

		// https://stackoverflow.com/questions/8011376/when-is-cudas-shared-memory-useful
		// __shared__ float in3_shared[sizeof(in3)];
		// __shared__ float in4_shared[sizeof(in4)][];
		bool isLastFirstCondition = false;
		for (int row = 0; row < height; row++)
		{
			// in3_shared[index] = in3[index];
			// __syncthreads();

			if (isTrue)
			{
				int idx = width * row + varA;

				if (!in4[idx]) {
					continue;
				}

				calculatable = calculatable + in3[row];
				isTrue = false;

				isLastFirstCondition = true;
			}
			else
			{
				int idx = width * row + varB;

				if (!in4[idx]) {
					continue;
				}

				calculatable = calculatable - in3[row];
				isTrue = true;

				isLastFirstCondition = false;
			}
		}

		/*
		outputCalc[0] = in4[0];
		outputCalc[1] = in4[1];
		outputCalc[2] = in4[2];
		outputCalc[3] = in4[3];
		outputCalc[4] = 1111;*/

		output[index] = isLastFirstCondition;
		outputCalc[index] = calculatable;

		/*
		// testing
		output[index] = in4[0];
		outputCalc[index] = index;*/
	}
}

__global__ void proccess2(
	bool* __restrict__ output,
	double* __restrict__ outputCalc
)
{
	int index = blockIdx.x * blockDim.x + threadIdx.x;

	// testing
	output[index] = true;
	outputCalc[index] = 15.4;
}

int CudaProccess(
	unsigned char* output,
	const size_t output_size, // unsigned int
	double* outputCalc,
	const size_t outputCalc_size,

	const int* in1,
	const size_t in1_size,
	const int* in2,
	const size_t in2_size,
	const double* in3,
	const size_t in3_size,
	const unsigned char* in4,
	const size_t in4_size,

	const int inputCount,
	const int width,
	const int height

	//unsigned char* __restrict__ output,
	//const int* __restrict__ in1,
	//const int* __restrict__ in2,
	//const double* __restrict__ in3,
	//const unsigned char* __restrict__  in4
) {
	//
	// Create int arrays on the CPU.
	// ('h' stands for "host".)
	//
	// SKIP (see method's input params)

	//
	// Create corresponding int arrays on the GPU.
	// ('d' stands for "device".)
	//
	// init dev variables
	unsigned char* d_output;
	double* d_outputCalc;
	int* d_in1;
	int* d_in2;
	double* d_in3;
	unsigned char* d_in4;
	//int d_inputCount;
	//int d_height;
	//int d_width;

	// calculate native total sizes
	//int constSize = sizeof(int);
	unsigned int output_totalSize = output_size * sizeof(unsigned char);
	unsigned int outputCalc_totalSize = outputCalc_size * sizeof(double);
	unsigned int in1_totalSize = in1_size * sizeof(int);
	unsigned int in2_totalSize = in2_size * sizeof(int);
	unsigned int in3_totalSize = in3_size * sizeof(double); // TBD: issue?
	unsigned int in4_totalSize = in4_size * sizeof(unsigned char);

	// allocate memory for device variables
	cudaMalloc(&d_output, output_totalSize);
	cudaMalloc(&d_outputCalc, outputCalc_totalSize);
	cudaMalloc(&d_in1, in1_totalSize);
	cudaMalloc(&d_in2, in2_totalSize);
	cudaMalloc(&d_in3, in3_totalSize);
	cudaMalloc(&d_in4, in4_totalSize);
	
	// Allocate Unified Memory – accessible from CPU or GPU
	//cudaMallocManaged(&output, output_totalSize);
	//cudaMallocManaged(&outputCalc, outputCalc_totalSize);

	// write host -> device
	cudaMemcpy(d_in1, in1, in1_totalSize, cudaMemcpyHostToDevice);
	cudaMemcpy(d_in2, in2, in2_totalSize, cudaMemcpyHostToDevice);
	cudaMemcpy(d_in3, in3, in3_totalSize, cudaMemcpyHostToDevice);
	cudaMemcpy(d_in4, in4, in4_totalSize, cudaMemcpyHostToDevice);

	//cudaMalloc((void**)&d_inputCount, constSize);
	//cudaMalloc((void**)&d_height, constSize);
	//cudaMalloc((void**)&d_width, constSize);

	/*
	std::string s = " ";
	std::cout << "Before" << std::endl;
	std::cout << in4[0] << std::endl;
	for (size_t i = 0; i < 10; i++)
	{
		//std::cout << i << s << output[i] << s << outputCalc[i] << std::endl;
	}*/

	// execute
	//dim3 dimBlock(inputCount, 1);
	//dim3 dimGrid(1, 1);
	//proccess KERNEL_ARGS2(dimGrid, dimBlock) (d_output, d_outputCalc, d_in1, d_in2, d_in3, d_in4, inputCount, width, height);
	//proccess KERNEL_ARGS2(inputCount, 1) (d_output, d_outputCalc, d_in1, d_in2, d_in3, d_in4, inputCount, width, height);
	//proccess << <inputCount, 1 >> > (d_output, d_outputCalc, d_in1, d_in2, d_in3, d_in4, inputCount, width, height);

	//proccess2 KERNEL_ARGS2(40, 1) (d_output, d_outputCalc);
	// 
	// Attempt 2: (working)
	//int blockDimensions = 256;
	//int gridDimensions = (inputCount + blockDimensions - 1) / blockDimensions;
	//proccess << <gridDimensions, blockDimensions >>> (d_output, d_outputCalc, d_in1, d_in2, d_in3, d_in4, inputCount, width, height);

	
	// Attempt 3: (working)
	int blockSize;      // The launch configurator returned block size 
	int minGridSize;    // The minimum grid size needed to achieve the maximum occupancy for a full device launch 
	int gridSize;       // The actual grid size needed, based on input size 
	cudaOccupancyMaxPotentialBlockSize(&minGridSize, &blockSize, proccess, 0, inputCount);
	//int blockSize = 1024;
	//int minGridSize = 56;
	//int gridSize  = 1024;
	proccess KERNEL_ARGS2(gridSize, blockSize) (d_output, d_outputCalc, d_in1, d_in2, d_in3, d_in4, inputCount, width, height);

	// Wait for GPU to finish before accessing on host
	cudaDeviceSynchronize();

	//
	// Copy output array from GPU back to CPU.
	//
	cudaMemcpy(output, d_output, output_totalSize, cudaMemcpyDeviceToHost);
	cudaMemcpy(outputCalc, d_outputCalc, outputCalc_totalSize, cudaMemcpyDeviceToHost);

	/*
	std::cout << "After" << std::endl;
	for (size_t i = 0; i < 10; i++)
	{
		std::cout << i << s << output[i] << s << outputCalc[i] << std::endl;
	}*/

	//
	// Free up the arrays on the GPU.
	//
	cudaFree(d_output);
	cudaFree(d_outputCalc);
	cudaFree(d_in1);
	cudaFree(d_in2);
	cudaFree(d_in3);
	cudaFree(d_in4);

	// check for error
	// https://stackoverflow.com/questions/14038589/what-is-the-canonical-way-to-check-for-errors-using-the-cuda-runtime-api
	cudaError_t error = cudaGetLastError();
	if (error != cudaSuccess)
	{
		// print the CUDA error message and exit
		printf("CUDA error name: %s\n", cudaGetErrorName(error));
		printf("CUDA error description: %s\n", cudaGetErrorString(error));
		//exit(-1);
		return -1;
	}

	return 0;
}