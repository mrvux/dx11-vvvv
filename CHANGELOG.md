ChangeLog
=========

# Upcoming
* [Help] Vlc (Dx11.Texture2D) help patch

# 1.0.1
* [Nodes] Fixed last minute issue in DynamicBuffer 
* [Deployment] Sorted Kinect2 missing libraries.

# 1.0.0 (Release 06/02/2017)
* [Girlpower] Many new help patches and examples.
* [Core]  Renderer nodes now have a WriteOnly depth mode (in case depth is not needed for reading, this is the recommended mode)
* [Nodes] Dynamicbuffers now have different upload option (Dynamic/Default/Immutable)
* [Nodes] InputElement (DX11.Geometry Preset) new node (bunch of commonly used presets in an enum to avoid InputElement join boilerplate)
* [Nodes] Vlc node reports file as invalid if they are parsing late, instead of ptotential crash.
* [Nodes] TextSettings (DX11.Advanded) Allows to create a different text engine, can be provide to text nodes (this allows larger glyph texture sizes for example, to bypass the 512 limit).
* [Nodes] Readback structured buffer nodes can now read any structured buffer.
* [Nodes] new node BufferData (DX11.RawBuffer) : gets raw buffer data as a stream.
* [Nodes] DispatcherX DispatcherXY (DX11.Drawer) convenience nodes for 1d/2d dispatch, which does thread group calculations (avoid to replicate this everywhere)
* [Nodes] New node DynamicBuffer (DX11.Buffer Raw) : Dynamic raw buffer from stream
* [Nodes] New node PixelData (DX11.Texture), reads raw pixel data in a stream (does not convert to image format)
* [Nodes] Kinect2 (Devices) has Depth Camera Intrinsics pin (user:id144)
* [Nodes] Add TextFormat(DirectWrite.Advanced)
* [Nodes] new Enabled (DX11.Layer) : Blocks render and evaluate if disabled (same as group does but lightweight version)
* [Nodes] Renderer (DX11) : Now accepts key presses again (was removed due to some old beep issue in 4v)  (user: azeno)
* [Build] Build and deployments are now automated, so release cycle for alpha (on Git Release) is much more frequent
* [Nodes] Add Anisotropic (DX11 Sampler) node
* [Nodes] ViewProjection (DX11.Layer) is now spreadable, allows to fully repeat a layer with different view/projection settings.
* [Nodes] Add World (DX11.Layer). Allows to move a whole layer independently of the camera. Shader can access it via WORLDLAYER semantic (WORLDLAYERINVERSE, WORLDLAYERVIEW, WORLDLAYERVIEWPROJECTION are also added for convenience)
* [Nodes] Standard (eg: All Layer shaders) large performance improvements (better caching and runtime calls),and much more GC friendly.
* [Nodes] Standard (eg: All Layer shaders) have some optimisation if rendering a single geometry several times.
* [Nodes] VideoIn (DX11.DShow) : Fix potential error on delete.
* [Nodes] GetSlice (DX11.TextureArray) and GetSlice (DX11.DepthTextureArray) , fix potential error on delete.
* [Nodes] All platonic solid : Isocahedron, Octahedron, Tetrahedron have a radius parameter.
* [Nodes] Renderer (DX11) now has a Flip Sequential option, which should help in case of tearing.
* [Core] Better caching of interfaces, and better registration for windows (eg: improved performances)
* [Core+Nodes] All parts of the pack are now using IDX11RendererHost, IDX11LayerHost, IDX11ResourceHost instead of provider. This removes IPluginIO as parameter, which creates some issues in many scenarios (just for even being there). Old provider are still there and working alongside but marked as obsolete (and will be removed in next release). Layer version could not run alongside and has been removed. This will need a recompile if a plugin is a custom layer node.
* [Core] Effect pass can now have topology annotation, in order to force a shader to use a specific topology (This is particularily useful for point sprites (which expect points), or tesselation shaders (which expects a fix control point topology), and avoid to have user guess with topology node.
* [Nodes] new Zip (TextStyle) and bin version. (user: sebl)
* [Nodes] Text styles were not spreadable properly. (user: sebl)
* [Nodes] Fix Cylinder (DX11.Geometry) radius issue
* [Nodes] Info (Assimp.Mesh) Fix spread indexing.
* [Nodes] Add Viewport (DX11.Layer Indexed) Set a new viewport depending on camera index from renderer
* [Nodes] Layer nodes are now consistent in behavior. If not enabled they do not apply their eventual change, but still render upstream.
* [Core] Add Tag (DX11.RenderState) node. For advanced usage, allows to attach an object to a state (for custom validator usage for example)
* [Core] All Depth formats are now working (64 bits version was not (even if rarely used)
* [Nodes] Add Text (DX11.Geometry)
* [Nodes] Add Renderer (DX11 TextureSpread), allows to render to multiple textures (not texture array, real spread of texture, size/format can be different)
* [Core] Standard shaders now support multi pass, with the following 2 restrictions: Vertex Input signatur must be the same on all passes (runtime with not validate other passes nor check for equality), and runtime does not reset stages (so make sure to set optional stages to 0).
* [Core] Render states and object settings can now share a Tag node
* [Nodes] Add Tag (DX11.RenderState) node

# 0.7.1 [Release 04/09/2016]
* [Core] Small fix as Layer base node had Autovalidate option set to false.

# 0.7 [Release 02/09/2016]
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
