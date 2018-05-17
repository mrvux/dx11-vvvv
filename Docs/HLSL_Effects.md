# HLSL Effects : Semantics and annotations

This contains all semantics and annotations that can be attached to hlsl variables, and understood by the reflection processor.
In case something is specific to layer/texture/geometry fx, this will be specified.

As a standard rule, any pin that has a semantic will never be visible as an input pin.
If the semantic is not understood, then default value with be set (either as specified in shader or a null in case of resource).

=========

# Layer and Geometry FX : Camera bindings

# Layer and Geometry FX : World bindings

# Layer and Geometry FX : Resource bindings

# Input Pins annotations 

# Input Pins reference semantics

Those semantics are used in order to retrieve specific data from an input resource (for example : Size of an input texture). 
This allows to avoid an extra pin and an info node on top to retrieve data.

Those semantics require an annotation in order to tell which variable it needs to lookup for (which is "string ref")

For example :

```
Texture2D InputTexture;

cbuffer cbTextureData : register(b0)
{
	float2 textureSize : SIZEOF <string ref="InputTexture";> ;	
}
```

If it possible to several variables to point to the same or a different resource , for example, the following is valid :

```
Texture2D InputTexture;
Texture2D AlphaTexture;

cbuffer cbTextureData : register(b0)
{
	float2 textureSize : SIZEOF <string ref="InputTexture";> ;	
	float2 textureSize2 : SIZEOF <string ref="InputTexture";> ;
	float2 alphaTextureSize : SIZEOF <string ref="AlphaTexture";> ;
}
```

The following references are allowed :

## 2d Textures: 
* SIZEOF (float2) : Size of the texture
* INVSIZEOF (float2) : 1 / Size of the texture
* MIPLEVELSOF (int/float) : Mip Level Count of the texture
* INVMIPLEVELSOF (float) : 1 / Mip Level Count of the texture
* ASPECTOF (float4x4) : Builds an aspect ratio matrix that matches the texture size. You can use the annotation: string aspectmode="FitIn;FitOut;FitWidth;FitHeight"; to choose fitting mode.

## 3d Textures: 
* SIZEOF (float3) : Size of the texture
* INVSIZEOF (float3) : 1 / Size of the texture

## StructuredBuffer :
* SIZEOF (uint) : Element count

