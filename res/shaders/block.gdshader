shader_type spatial;

uniform sampler2DArray textures;

void fragment() {
	ALBEDO = texture(textures, vec3(UV.x, UV.y, COLOR.r)).rgb;
}
