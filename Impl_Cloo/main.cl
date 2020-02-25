kernel void Proccess (
    global bool* output,
    global double* calculatables,
    global const int* in1, 
    global const int* in2, 
    global const double* in3, 
    global const bool* in4, 
    const int width, 
    const int height)
{
    int index = get_global_id(0);

    bool isTrue = false;
    int varA = in1[index];
    int varB = in2[index];

    double calculatable = 0;
    bool isLastFirstCondition = false;
    for (int row = 0; row < height; row++)
    {
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

    output[index] = isLastFirstCondition;
    calculatables[index] = calculatable;
}