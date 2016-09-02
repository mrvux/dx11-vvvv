ChangeLog
=========

# Upcoming
* [Nodes] New DynamicTexture (DX11.Texture1d Color) node.
* [Nodes] Blend (DX11.RenderState) now has a "Keep" preset.
* [Nodes] All space layers had an issue when disabled.
* [Nodes] Quad (DX11.Layer) now supports Depth only rendering.
* [Nodes] ViewportIndex (DX11.Validator) has a cyclic option (user: sebl)
* [Core] Text nodes are now built against Visual Studio 2015 (so visual c++ redist are now required)
* [Nodes] Polygon (DX11.Geometry) center has correct UV (user:sebl)
* [Nodes] Renderer (DX11) uses SlimDX color type instead of 4v native type to be consistent with rest of the pipeline (precision issue).
* [Nodes] DynamicBuffer (DX11.SpotLight) was transforming position instead of direction.
* [Core] Texture FX now make sure to ignore texture input when default size is on.
* [Nodes] Within Projection has pins to allow to preserve aspect ratio/crop (so you can do all dx9 within equivalents).
* [Nodes] new node RenderState (DX11), which sets all render state in a single node
* [Core] Better render space handling, which allows more flexible usage at layer level.
* [Nodes] Node (Assimp) now has a "Include self" pin.
* [Nodes] Text (DX11.Layer Advanced) was crashing in case of Text layout count < Spreadmax, fixed.
* [Nodes] SetSlice (DX11.TextureArray) mip level and slice is now fixed.
* [Nodes] VideoIn (DX11 DShow) new node (user:gumilastik)
* [Nodes] QR Code node color flip is now fixed.
* [Nodes] Sampler state now has Mirror presets.
* [Nodes] TextFormat (DirectWrite) now has a "Is valid" output pin.
* [Nodes] LineMetrics (DirectWrite) new node.
* [Nodes] TextLayoutMetrics (DirectWrite) has a line count output.
* [Nodes] Add ClusterMetrics (DirectWrite) node.
* [Nodes] TextFormat (DirectWrite) now has Line spacing options
* [Nodes] Text nodes now use proper enum which is from DirectWrite
* [Nodes] Add RemoveSlice (DX11.Validator)
* [Nodes] Add ViewportBillBoard (DX11.layer)
* [Nodes] IndexIndirect (DX11.Drawer) and VertexIndirect (DX11.Drawer) small memory leak fix.
* [Nodes] TextLayout (DX11.Advanced) spead size fix (prevents crash).
* [Nodes] Switch (DX11.Geometry) has spreadable input switch
* [Core] Depth stencil reference and Blend factor are now part of render state.
* [Nodes] Layer nodes for render space now just don't apply change if disabled, instead of blocking rendering.
* [Nodes] AspectRatio (DX11.Layer) fitIn/FitOut mode was swapped.
* [Nodes] Quad (DX11.Layer) was sometimes generating an error if fed a null texture.
* [Nodes] Preview (DX11.Texture) now has a sampler pin.
* [Nodes] Layer input in Group node is now Spreadable.
* [Nodes] RenderSemantic (DX11.ByteAddressBuffer) new node.
* [Core] Fix small memory leak on some specific type of buffers.
* [Nodes] PassApply (DX11.Layer) new node.
* [Nodes] GetSoftBodyBuffer (Bullet) clean resources on deletion
* [Nodes] SoftBody (Bullet Geometry) clean resources on deletion

# 0.6.1 [Release 30/12/2015]
* [Nodes] Add PairKerning (DirectWrite Styles) Node
* [Nodes] Add Spacing (DirectWrite Styles) Node
* [Nodes] Add Typography (DirectWrite Styles) Node
* [Nodes] TextLayout (and advanced version) now use sharpDX enum for text alignment, to allow Justified spacing. Please note this is windows8 +
* [Core] Fix issue  with windows 7/8 not picking right version of compiler (slimdx picks latest dx11 runtime but compiler does not offer this feature)
* [Nodes] Fix issue that could happen on DynamicTexture (Color) deletion (user:mhusinsky)

# 0.6 [Release 29/12/2015]

* [Nodes]Fix Buffer count from ISpread to IDiffSpread in Renderer (DX11), this was causing resources and depth buffer to be recreated every frame (bad performances).
* [Build] Build now properly copy all kinect dependencies
* [Build] Build properly copies Assimp dependencies
* [Build] Fix missing SlimDX 64 bits dll
* [Kinect2] Fix bug in relative mode for low depth textures (user:lemtp)
* [Core] Shader can now use uint(s) as inputs (user:gumilastik)
* [Kinect2] Short body index in Skeleton node (user:mholub)
* [Nodes] Bezier patch now has an absolute position toggle
* [Nodes] Fix Linestrip node when feeding Nil
* [Core] Add toggle to force mips generation on last pass of a Texture FX
* [Core] Default Size pin for Texture FX now spreadable
* [Core] Fix small issue in shaders that could mess up shader compilation between 9 and 11
* [Nodes] Fix error in case we feed a Nil into Preview (DX11) node
* [Core] Fix sampler not updating properly on shaders when changed
* [Nodes] Renderer (DX11) ignores fullscreen toggle on first frame (to use default patch behaviour) (user:gregsn)
* [Nodes] Fix Topology node not updating properly at times.
* [Core] Texture1dArray and TextureCubeArray are now available as shader input or semantic
* [Nodes] add Renderers for Texture1d and Texture1dArray
* [Kinect2] VGB Gesture node now spredable (user:mhusinsky)
* [Core] Some semantic rework (user:dotprodukt)
* [Core] float2[] and texture2DArrayMS now available as inputs and semantics (user:dotprodukt)
* [Core] Texture2DArray now available as semantic (user:dotprodukt)
* [Core] All layer nodes use cached version of Pin.IsConnected (to avoid COM apartment potential issues)
* [Core] Remove IPluginIO access on all nodes from build (same reason as above)
* [Core] DX11 optional interfaces are now cached (some performance gain as no type cast check necessary anymore during graph traversal)
* [Nodes] Renderers have a Pin to set clear depth value (allows to use reverse depth strategy)
* [Core] Render windows are now cached (better performances when using lot of nodes)
* [Nodes] FrustrumTest (DX11.Validator) fix (reversed bounding box)
* [General] Updated to framework 4.5
* [Nodes] Info (DX11) can report for a Feature Level 11.1 device if possible, also updates on first frame.
* [Core] Stream out (gsfx) output Nil if input is Nil
* [Core] Uav counter can also be set for pipeline writes
* [Nodes] Add Cons (DX11.layer)
* [Nodes] All Layer/Renderer nodes will render a spread of layers properly
* [Core] Add layer order interface, which allows to build a lookup table to change shader slices order
* [Nodes] Add GetSlice (DX11.Layer Order) , to allow to do getslice on layer properly
* [Nodes] Add Simple ZSort (DX11.Layer Order) to ZSort shader slices (from transform in pin and object center). Basic but useful for simple renders with alpha
* [Core] Now integrates SharpDX 3 in the build, since it's required for some features
* [Core] Use SharpDX to build DirectWrite font family enumeration
* [Nodes] TextFormat (DirectWrite) now uses DirectWrite enumeration, which fixes issue when sometimes font was not found and reverting to default
* [Core] Device is now created with Feature Level 11.1 if possible (Win8 and ATI , Nvidia 900+  only)
* [Core] Effect compiler now uses SharpDX, which allows to use latest version of D3D Compiler available on the OS
* [Nodes] Add various Text Styler nodes, to allow to set specific font size/weight or family within the same text layout
* [Nodes] Text color is now white by default
* [Core] Cache Font renderer and factory, instead of having one instance per node (save memory for spritesheets and faster load)
* [Nodes] Color is now available as a text styler.
* [Nodes] Add BC4 format for BlockWriter
* [Shaders] Default shaders now have 2 techniques, one for using texture and one without, also rearranged cbuffer layouts for better performances
* [Nodes] Add Scissor (DX11.Layer) node
* [Girlpower] Many new help patches added 
* [Girpower] Add example on how to use reverse depth, and another example to view depth error margins with different formats/techniques
