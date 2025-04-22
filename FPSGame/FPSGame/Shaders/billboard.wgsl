struct VSInput 
{
    @location(0) position: vec3f,
    @location(1) color: vec4f,
    @location(2) texCoords: vec2f,
    @location(3) transformRow0: vec4<f32>,
    @location(4) transformRow1: vec4<f32>,
    @location(5) transformRow2: vec4<f32>,
    @location(6) transformRow3: vec4<f32>
}

struct VSOutput 
{
    @builtin(position) position: vec4f,
    @location(1) color: vec4f,
    @location(2) texCoords: vec2f,
}

@group(0) @binding(0)
var<uniform> perspective: mat4x4f;

@group(0) @binding(1)
var<uniform> view: mat4x4f;


@vertex fn main_vs(
        in: VSInput,
        @builtin(vertex_index) vid : u32) -> VSOutput
{
     var out: VSOutput;

     let transform = mat4x4<f32>(
            in.transformRow0,
            in.transformRow1,
            in.transformRow2,
            in.transformRow3
      );

     var viewTransform = view * transform;

     // Now get rid of x and z rotation.
     viewTransform[0][0] = transform[0][0]; // Scale X 
     viewTransform[0][1] = 0.0;
     viewTransform[0][2] = 0.0;

     viewTransform[2][0] = 0.0;
     viewTransform[2][1] = 0.0;
     viewTransform[2][2] = transform[2][2]; // Scale Z 

     out.position = perspective * viewTransform * vec4f(in.position, 1.0);
     out.color = in.color;
     out.texCoords = in.texCoords;

     return out; 
}

@group(1) @binding(0)
var texture: texture_2d<f32>;
@group(1) @binding(1)
var textureSampler : sampler;

@fragment fn main_fs(in: VSOutput) -> @location(0) vec4f 
{
    var color = textureSample(texture, textureSampler, in.texCoords);

    if(color.a < 0.2) {
		discard;
	}

    return color * in.color;
} 