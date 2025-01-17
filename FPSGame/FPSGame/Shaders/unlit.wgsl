struct VSInput 
{
    @location(0) position: vec3f,
    @location(1) color: vec3f,
}

struct VSOutput 
{
    @builtin(position) position: vec4f,
    @location(1) color: vec3f,
}

@group(0) @binding(0)
var<uniform> perspectiveView: mat4x4f;


@vertex fn main_vs(
        in: VSInput,
        @builtin(vertex_index) vid : u32) -> VSOutput
{
     var out: VSOutput;

     out.position = perspectiveView * vec4f(in.position, 1.0);
     out.color = in.color;

     return out; 
}


@fragment fn main_fs(in: VSOutput) -> @location(0) vec4f 
{
    return vec4f(in.color, 1.0);
} 