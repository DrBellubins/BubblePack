shader_type canvas_item;

uniform sampler2D input_texture : source_color, filter_linear, repeat_disable;
uniform vec2 texel_size = vec2(0.001, 0.001);

void fragment()
{
    vec2 uv = UV;

    vec4 color = vec4(0.0);

    color += texture(input_texture, uv + vec2(-4.0 * texel_size.x, 0.0)) * 0.051;
    color += texture(input_texture, uv + vec2(-3.0 * texel_size.x, 0.0)) * 0.0918;
    color += texture(input_texture, uv + vec2(-2.0 * texel_size.x, 0.0)) * 0.12245;
    color += texture(input_texture, uv + vec2(-1.0 * texel_size.x, 0.0)) * 0.1531;
    color += texture(input_texture, uv)                                     * 0.1633;
    color += texture(input_texture, uv + vec2( 1.0 * texel_size.x, 0.0)) * 0.1531;
    color += texture(input_texture, uv + vec2( 2.0 * texel_size.x, 0.0)) * 0.12245;
    color += texture(input_texture, uv + vec2( 3.0 * texel_size.x, 0.0)) * 0.0918;
    color += texture(input_texture, uv + vec2( 4.0 * texel_size.x, 0.0)) * 0.051;

    COLOR = color;
}