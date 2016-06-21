Shader "Custom/Solid Color" {

    Properties {
        _Color ("Main Color", Color) = (0.675, 0.753, 0.757, 0.95)
    }

    SubShader {
        Pass { Color [_Color] }
    }
}
