kernel void sum (global float4 * x, global float4 * y)
{
    int index = get_global_id(0);
    
    printf("%d", x[index]);
    x[index] = x[index] + y[index];
}