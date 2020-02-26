#pragma once
#include <iostream>
//#using <mscorlib.dll>
//#include "../CudaLib/main.h"
#include "../CudaLib/kernel.h"

using namespace System;

namespace CLRLib {

	public ref class CudaWrapper
	{
	private:
	public:
		// % = reference
		int Execute(
			array<System::Byte>^ output,
			array<System::Double>^% outputCalc,

			array<System::Int32>^ in1,
			array<System::Int32>^ in2,
			array<System::Double>^ in3,
			array<System::Byte>^ in4,

			System::Int32 inputCount,
			System::Int32 width,
			System::Int32 height
		)
		{
			pin_ptr<unsigned char> h_output = &output[0];
			unsigned char* h_outputPtr = h_output;
			size_t output_size = output->Length;

			pin_ptr<double> h_outputCalc = &outputCalc[0];
			double* h_outputCalcPtr = h_outputCalc;
			size_t outputCalc_size = outputCalc->Length;

			pin_ptr<int> h_in1 = &in1[0];
			int* h_in1Ptr = h_in1;
			size_t in1_size = in1->Length;

			pin_ptr<int> h_in2 = &in2[0];
			int* h_in2Ptr = h_in2;
			size_t in2_size = in2->Length;

			pin_ptr<double> h_in3 = &in3[0];
			double* h_in3Ptr = h_in3;
			size_t in3_size = in3->Length;

			pin_ptr<unsigned char> h_in4 = &in4[0];
			unsigned char* h_in4Ptr = h_in4;
			size_t in4_size = in4->Length;

			int h_inputCount = inputCount;
			int h_width = width;
			int h_height = height;

			//System::Console::WriteLine(output_size);
			//System::Console::WriteLine(outputCalc_size);
			//System::Console::WriteLine(in1_size);
			//System::Console::WriteLine(in2_size);
			//System::Console::WriteLine(in3_size);
			//System::Console::WriteLine(in4_size);
			//
			//System::Console::WriteLine(h_inputCount);
			//System::Console::WriteLine(h_width);
			//System::Console::WriteLine(h_height);
			
			int result = CudaProccess(h_outputPtr, output_size,
				h_outputCalcPtr, outputCalc_size,
				h_in1Ptr, in1_size,
				h_in2Ptr, in2_size,
				h_in3Ptr, in3_size,
				h_in4Ptr, in4_size,
				h_inputCount, h_width, h_height);

			return result;
		};
	};
}