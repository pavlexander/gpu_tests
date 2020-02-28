#include <iostream>
#include <random>
#include <ctime>

#include "../CudaLib/kernel.h"

const double DoubleRandomRange = 3;
const int InputCount = 800000;
const int Height = 3000;
const int Width = 200;
const int HeightxWidth = Height * Width;

std::random_device rd;     // only used once to initialise (seed) engine
std::mt19937 rng(rd());    // random-number engine used (Mersenne-Twister in this case)

int intInIntervalInc(int min, int max) {
    std::uniform_int_distribution<int> uni(min, max); // guaranteed unbiased
    auto random_integer = uni(rng);
    return random_integer;
}

// https://stackoverflow.com/questions/2704521/generate-random-double-numbers-in-c
double doubleInIntervalInc(double lower_bound, double upper_bound)
{
    std::uniform_real_distribution<double> unif(lower_bound, upper_bound);
    //std::default_random_engine re;
    double a_random_double = unif(rng);

    return a_random_double;
}

double sum_of_array(double x[], int size)
{
    double result = 0;

    for (int i = 0; i < size; i++)
    {
        //std::cout << x[i] << "\n";
        result += x[i];
    }

    return result;
}

int sum_of_arrayInt(int x[], int size)
{
    int result = 0;

    for (int i = 0; i < size; i++)
    {
        //std::cout << x[i] << "\n";
        result += x[i];
    }

    return result;
}

double diffclock(clock_t start, clock_t end) {

    double diffticks = end - start;
    double diffms = diffticks / (CLOCKS_PER_SEC / 1000);

    return diffms;
}

int main()
{
    std::cout << "Start!\n";
    
    int* In1 = new int[InputCount]();
    int* In2 = new int[InputCount]();

    for (int i = 0; i < InputCount; i++)
    {
        In1[i] = intInIntervalInc(0, Width - 1);
        In2[i] = intInIntervalInc(0, Width - 1);
    }

    double* In3 = new double[Height]();
    for (int i = 0; i < Height; i++)
    {
        In3[i] = doubleInIntervalInc(0, DoubleRandomRange);
    }

    //std::cout << sum_of_arrayInt(In1, InputCount) << "\n";
    //std::cout << sum_of_arrayInt(In2, InputCount) << "\n";
    //std::cout << sum_of_array(In3, Height) << "\n";

    //bool In4_2[Height][Width]; // multidimensional
    bool** In4_2 = new bool* [Height];
    for (int i = 0; i < Height; ++i)
        In4_2[i] = new bool[Width];

    //unsigned char In4_2_bytes[Height][Width];
    unsigned char** In4_2_bytes = new unsigned char* [Height];
    for (int i = 0; i < Height; ++i)
        In4_2_bytes[i] = new unsigned char[Width];

    // populate bools
    for (int i = 0; i < Height; i++)
    {
        for (int y = 0; y < Width; y++)
        {
            int randomOneOrZero = intInIntervalInc(0, 1);
            In4_2[i][y] = (randomOneOrZero == 1); // 0 or 1
            In4_2_bytes[i][y] = (In4_2[i][y] == true) ? (unsigned char)1 : (unsigned char)0;
        }
    }

    bool* In4_3 = new bool[HeightxWidth]();
    for (int row = 0; row < Height; row++) // 2D to 1D
    {
        for (int col = 0; col < Width; col++)
        {
            In4_3[row * Width + col] = In4_2[row][col];
        }
    }

    unsigned char* In4_3_bytes = new unsigned char[HeightxWidth]();
    for (int i = 0; i < HeightxWidth; i++)
    {
        if (In4_3[i])
        {
            In4_3_bytes[i] = 1;
        }
    }

    int trueCount = 0;
    for (size_t i = 0; i < HeightxWidth; i++)
    {
        if (In4_3_bytes[i]) {
            trueCount++;
        }
    }
    //std::cout << trueCount << "\n";

    bool* results = new bool[InputCount]();
    unsigned char* resultsBytes = new unsigned char[InputCount]();
    double* calculatables = new double[InputCount]();
    double calculatable = 0;

    clock_t start = clock();
    int cudaExecutioResult = CudaProccess(
        // output
        resultsBytes,
        InputCount,
        calculatables,
        InputCount,

        // input
        In1,
        InputCount,
        In2,
        InputCount,
        In3,
        Height,
        In4_3_bytes,
        HeightxWidth,

        InputCount,
        Width,
        Height
    );

    clock_t end = clock();
    std::cout << "Total execution time is: " << diffclock(start, end) << " ms" << std::endl;

    delete[] In1;
    delete[] In2;
    delete[] In3;
    delete[] In4_2;
    delete[] In4_2_bytes;
    delete[] In4_3;
    delete[] In4_3_bytes;
    delete[] results;
    delete[] resultsBytes;

    calculatable = sum_of_array(calculatables, InputCount);
    std::cout << "Calculatable value: " << calculatable << "\n";
    std::cout << "End\n";

    delete[] calculatables;
    return cudaExecutioResult;
    return 1;
}