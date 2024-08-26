struct VSInput 
{
    @location(0) position: vec3f,
    @location(1) color: vec4f,
    @location(2) texCoords: vec2f
}

struct VSOutput 
{
    @builtin(position) position: vec4f,
    @location(1) color: vec4f,
    @location(2) texCoords: vec2f
}

@group(0) @binding(0)
var<uniform> transform: mat4x4f;

@vertex fn main_vs(
        in: VSInput,
        
        @builtin(vertex_index) vid : u32) -> VSOutput
{
     var out: VSOutput;

     out.position = transform * vec4f(in.position, 1.0);
     out.color = in.color;
     out.texCoords = in.texCoords;

     return out; 
}

@group(1) @binding(0)
var texture: texture_2d<f32>;
@group(1) @binding(1)
var textureSampler: sampler;

@fragment fn main_fs(in: VSOutput) -> @location(0) vec4f 
{
    return textureSample(texture, textureSampler, in.texCoords);
} 