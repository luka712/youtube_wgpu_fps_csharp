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
var<uniform> perspective: mat4x4f;

@group(1) @binding(1)
var<uniform> view: mat4x4f;


@vertex fn main_vs(
        in: VSInput,
        @builtin(vertex_index) vid : u32) -> VSOutput
{
     var out: VSOutput;

     var modelView = view * transform;

     // Get rid of x and y axis rotation
     modelView[0][0] = transform[0][0];
     modelView[0][1] = 0.0;
     modelView[0][2] = 0.0;
     modelView[2][0] = 0.0;
     modelView[2][1] = 0.0;
     modelView[2][2] = transform[2][2];

     out.position = perspective * modelView * vec4f(in.position, 1.0);
     out.color = in.color;
     out.texCoords = in.texCoords;

     return out; 
}

@group(2) @binding(0)
var texture: texture_2d<f32>;
@group(2) @binding(1)
var textureSampler : sampler;

@fragment fn main_fs(in: VSOutput) -> @location(0) vec4f 
{
    var color = textureSample(texture, textureSampler, in.texCoords);
    if(color.a < 0.1) {
		discard;
	}

    return color;
} 