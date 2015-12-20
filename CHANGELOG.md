ChangeLog
=========

# Upcoming

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