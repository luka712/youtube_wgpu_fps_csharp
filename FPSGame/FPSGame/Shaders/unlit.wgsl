struct VSInput 
{
    @location(0) position: vec3f,
    @location(1) color: vec4f,
}

struct VSOutput 
{
    @builtin(position) position: vec4f,
    @location(1) color: vec4f
}


@vertex fn main_vs(
        in: VSInput,
        
        @builtin(vertex_index) vid : u32) -> VSOutput
{
     var out: VSOutput;

     out.position = vec4f(in.position, 1.0);
     out.color = in.color;

     return out; 
}

@fragment fn main_fs(in: VSOutput) -> @location(0) vec4f 
{
    return in.color;
} 