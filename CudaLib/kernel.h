#pragma once

// When you pass a C-style array to a function it will decay to a pointer to the first element of the array, basically losing the size information.
int CudaProccess(
	// output
	unsigned char* output,
	const size_t output_size,
	double* outputCalc,
	const size_t outputCalc_size,

	// input
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
);
