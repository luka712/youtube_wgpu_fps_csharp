struct VSInput 
{
    @location(0) position: vec3f,
    @location(1) color: vec4f,
    @location(2) texCoords: vec2f,
}

struct VSOutput 
{
    @builtin(position) position: vec4f,
    @location(1) color: vec4f,
    @location(2) texCoords: vec2f,
}

@group(0) @binding(0)
var<uniform> transform: mat4x4f;

@group(1) @binding(0)
var<uniform> perspectiveView: mat4x4f;


@vertex fn main_vs(
        in: VSInput,
        @builtin(vertex_index) vid : u32) -> VSOutput
{
     var out: VSOutput;

     out.position = perspectiveView * transform * vec4f(in.position, 1.0);
     out.color = in.color;
     out.texCoords = in.texCoords;

     return out; 
}

@group(2) @binding(0)
var mixTexture: texture_2d<f32>;
@group(2) @binding(1)
var mixSampler : sampler;

@group(2) @binding(2)
var redTexture: texture_2d<f32>;
@group(2) @binding(3)
var redSampler : sampler;

@group(2) @binding(4)
var greenTexture: texture_2d<f32>;
@group(2) @binding(5)
var greenSampler : sampler;

@group(2) @binding(6)
var blueTexture: texture_2d<f32>;
@group(2) @binding(7)
var blueSampler : sampler;

@group(2) @binding(8)
var<uniform> textureTilling: vec2f;

@fragment fn main_fs(in: VSOutput) -> @location(0) vec4f 
{
    var mix = textureSample(mixTexture, mixSampler, in.texCoords);
    
    // Texture tilling is used to move from [0,1] to [0,2] to [0,3] etc.
    var color = textureSample(redTexture, redSampler, in.texCoords * textureTilling) * mix.r +
                textureSample(greenTexture, greenSampler, in.texCoords * textureTilling) * mix.g +
                textureSample(blueTexture, blueSampler, in.texCoords * textureTilling) * mix.b;
                   
    return color * in.color;
} 