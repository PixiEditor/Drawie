struct VertexInput {
    @location(0) position: vec2f,
    @location(1) texCoord: vec2f,
};

/**
 * A structure with fields labeled with builtins and locations can also be used
 * as *output* of the vertex shader, which is also the input of the fragment
 * shader.
 */
struct VertexOutput {
    @builtin(position) position: vec4f,
    // The location here does not refer to a vertex attribute, it just means
    // that this field must be handled by the rasterizer.
    // (It can also refer to another field of another struct that would be used
    // as input to the fragment shader.)
    @location(0) texCoord: vec2f, 
};

@vertex
fn vertexMain(in: VertexInput) -> VertexOutput {
    var out: VertexOutput;
    out.position = vec4<f32>(in.position, 0, 1);
    out.texCoord = in.texCoord;
    return out;
}

@group(0) @binding(0) var tex_sampler: sampler;
@group(0) @binding(1) var tex: texture_2d<f32>;

@fragment
fn fragmentMain(in: VertexOutput) -> @location(0) vec4f {
    return textureSample(tex, tex_sampler, in.texCoord); 
}