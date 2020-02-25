#pragma once
#include "kernel.h"

void CppCudaWrap(
	// output
	unsigned char* output,
	const int output_size,
	double* outputCalc,
	const int outputCalc_size,

	// input
	const int* in1,
	const int in1_size,
	const int* in2,
	const int in2_size,
	const double* in3,
	const int in3_size,
	const unsigned char* in4,
	const int in4_size,

	const int inputCount,
	const int width,
	const int height
);
