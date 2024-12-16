struct VSInput 
{
    @location(0) position: vec3f, 
}

struct VSOutput 
{
    @builtin(position) position: vec4f,
    @location(1) texCoords: vec3f,
}

@group(0) @binding(0)
var<uniform> u_projectionView: mat4x4f;

@vertex
fn main_vs(in: VSInput, @builtin(vertex_index) vid: u32) -> VSOutput
{
    var out : VSOutput;

    var pos = u_projectionView * vec4f(in.position, 1.0);

    out.position = pos.xyww;

    out.texCoords = in.position.xyz;

    return out; 
}

@group(1) @binding(0)
var u_texture: texture_cube<f32>;
@group(1) @binding(1)
var u_sampler: sampler;

@fragment
fn main_fs(in: VSOutput) -> @location(0) vec4f
{
	return textureSample(u_texture, u_sampler, in.texCoords);
}