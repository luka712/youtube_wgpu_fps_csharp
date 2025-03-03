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
var mixTextureSampler : sampler;
@group(2) @binding(2)
var redChannelTexture: texture_2d<f32>;
@group(2) @binding(3)
var redChannelTextureSampler : sampler;
@group(2) @binding(4)
var greenChannelTexture: texture_2d<f32>;
@group(2) @binding(5)
var greenChannelTextureSampler : sampler;
@group(2) @binding(6)
var blueChannelTexture: texture_2d<f32>;
@group(2) @binding(7)
var blueChannelTextureSampler : sampler;
@group(2) @binding(8)
var<uniform> textureTilling: vec2f;

@fragment fn main_fs(in: VSOutput) -> @location(0) vec4f 
{
    var mixColor = textureSample(mixTexture, mixTextureSampler, in.texCoords);
    var color = textureSample(redChannelTexture, redChannelTextureSampler, in.texCoords * textureTilling) * mixColor.r;
    color += textureSample(greenChannelTexture, greenChannelTextureSampler, in.texCoords * textureTilling) * mixColor.g;
    color += textureSample(blueChannelTexture, blueChannelTextureSampler, in.texCoords * textureTilling) * mixColor.b;

    return color;
} 