#version 330 core
layout (location = 0) in vec3 vPos;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec3 fPos;

void main()
{
    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);

    //We want to know the fragment's position in World space, so we multiply ONLY by uModel and not uView or uProjection
    fPos = vec3(uModel * vec4(vPos, 1.0));
}