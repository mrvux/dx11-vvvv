# HLSL Effects : Semantics and annotations

This contains all semantics and annotations that can be attached to hlsl variables, and understood by the reflection processor.
In case something is specific to layer/texture/geometry fx, this will be specified.

As a standard rule, any pin that has a semantic will never be visible as an input pin.
If the semantic is not understood, then default value with be set (either as specified in shader or a null in case of resource).

=========

# Layer and Geometry FX : Camera bindings

Those semantics allow to retrieve View / Projection / Layer World Matrix

Unless specified they only apply to float4x4 types

* LAYER : Layer Matrix as per World Layer
* LAYERINVERSE : Inverse version of the above

* LAYERVIEW : Layer Matrix as per World Layer multiplied with view matrix of the renderer
* LAYERVIEWPROJECTION : Layer Matrix as per World Layer multiplied with view * projection matrix of the renderer

* PROJECTION : Renderer projection matrix
* VIEW : Renderer view matrix
* VIEWPROJECTION : Renderer view matrix * Renderer projection matrix

* PROJECTIONINVERSE : inverse of Renderer projection matrix
* VIEWINVERSE : inverse of Renderer view matrix
* VIEWPROJECTIONINVERSE : inverse of Renderer view matrix * Renderer projection matrix

* PROJECTIONTRANSPOSE : transpose of Renderer projection matrix
* VIEWTRANSPOSE : transpose of Renderer view matrix
* VIEWPROJECTIONTRANSPOSE : transpose of Renderer view matrix * Renderer projection matrix

* PROJECTIONINVERSETRANSPOSE : inverse of transpose of Renderer projection matrix
* VIEWINVERSETRANSPOSE : inverse of transpose of Renderer view matrix
* VIEWPROJECTIONINVERSETRANSPOSE : inverse of transpose of Renderer view matrix * Renderer projection matrix

* CAMERAPOSITION (float3) : camera position (extracted from view matrix)


# Layer and Geometry FX : World/Object bindings

These semantics use Transform In input pin, as well as Geometry input pin 

Transforms (all are float4x4)

* WORLD : Transform input matrix
* WORLDTRANSPOSE : transpose of Transform input matrix
* WORLDINVERSE : inverse of Transform input matrix
* WORLDINVERSETRANSPOSE : transpose of inverse of Transform input matrix

* WORLDLAYER : Transform input matrix *  World layer matrix
* WORLDLAYERINVERSETRANSPOSE : transpose of inverse of Transform input matrix *  World layer matrix (use for normals transformation)

* WORLDVIEW : Transform input matrix *  Renderer view
* WORLDLAYERVIEW :  Transform input matrix * World layer matrix * Renderer view
* WORLDVIEWPROJECTION : Transform input matrix *  Renderer view * Renderer projection
* WORLDLAYERVIEWPROJECTION :  Transform input matrix * World layer matrix * Renderer view * Renderer projection

 
 Objects :
 * DRAWINDEX (int/float) : Current draw index for this layer
 * BOUNDINGMIN (float3) : Minimum bounds of geometry bounding box (object space)
 * BOUNDINGMAX (float3) : Maxmimum bounds of geometry bounding box (object space)
 * BOUNDINGSCALE (float3) : Size of the object bounding box (object space)
 
 * OBJUNITTRANS (float4x4) : Transform that brings object bounding box in unit space (-0.5 to 0.5)
 * OBJSDFTRANS (float4x4) :Transform that brings object bounding box in sdf space (0 to 1)

# Layer and Geometry FX : Resource bindings

* BACKBUFFER (RWTexture1D, RWTexture1DArray, RWTexture2D, RWTexture2DArray, RWTexture3D, RWStructuredBuffer, RWByteAddressBuffer,ConsumeStructuredBuffer,AppendStructuredBuffer) : The current resource from the downstream renderer.
* TARGETSIZE (float2) : Size of the bound texture (in case of buffer, x -> element count, y = 1)
* INVTARGETSIZE (float2) : 1.0 / Size of the bound texture (x is also element count in case of buffer).
* TARGETSIZE (float4) : xy = size, zw = 1 / size
* TARGETSIZE (float3) : Size of the bound volume
* INVTARGETSIZE (float3) : 1 / Size of the bound volume
* VOLUMESIZE (float4) : Size of the bound volume
* DRAWCOUNT (int/float) : number of draws that this layer will perform
* INVDRAWCOUNT (float) : 1 / number of draws that this layer will perform
* ELEMENTCOUNT (int) : Element count of structured buffer
* VIEWPORTCOUNT (int) : number of view/projection/viewport combinations associated to the renderer
* VIEWPORTINDEX (int) : current index of view/projection/viewport combinations associated to the renderer

# Texture FX Specifics

Specific to texture FX (tfx files).

* INITIAL (Texture2D) : Texture that is attached to the Texture in pin of the node.
* PREVIOUS (Texture2D) : Result of the previous effect pass, or texture input pin in case of first pass
* PASSRESULT[n] (Texture2D) : Result of the previous [nth] effect pass

Technique annotations : 
* bool wantmips : Will perform a mip generation pass on first pass, if texture has only one level, if texture already has mips does nothing.

Pass annotations : 
* bool mips : performs a mip generation pass once complete.
* float2 scale : rescales target texture by this factor (references previous pass size by default, so scale = float2(0.5,0.5) half the size (see absolute and initial annotations too).
* bool absolute : rescales in exact pixel values
* bool initial : uses input texture reference instead of previous pass before to apply scale
* bool clear : clears target 
* bool hasstate : indicates we are binding a state, and that next pass should reset to default
* string format : forces an output texture format (use any dxgi based format)


# Input Pins annotations 

Those are extra annotations that can added to input pins in order to change pin behaviour.

Syntax :
[annotation type] [annotation name] ([list of types if applies to)]

string uiname (any type) : Sets pin name
bool visible (any type) : Set pin default visibility (if set to false, they can be seen in inspector)

float uimin (float) : minimum pin value
float uimax (float) : maximum pin value
float uistep(float) : pin step value when performing mouse drag edit

bool bang (bool) : Use a bang type pin instead of a toggle style pin.
bool color (float4 or array of float4) : Use a color pin instead of value (vector4)


bool uvspace (float4x4) : Transform the matrix to fit texture coordinates instead of position
bool invy (float4x4) : If matrix is in uv space, also invert y axis


string topology (on EffectPass) : Only for layer/geometry. Enforces a primitive topology (to force patch or point to a shader that expect those for example)

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

