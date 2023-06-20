===============================================
 Retro Game Shader
===============================================

<< Package >>
RetroGameShader/
  Documents/
    License.txt
    Readme.txt
  Samples/
  Scripts/
    RetroGameShader.cs
  Shaders/
    RetroGameShader.shader
  Textures/
    CRT_ShadowMask1.png
    CRT_ShadowMask2.png


<< Usage >>
<<<< Built-in Render Pipeline >>>>
1. Add RetroGameShader component to the Main Camera.
2. Change parameters.
3. Enjoy!

<<<< Universal Render Pipeline >>>>
1. Add RetroGameShaderRenderFeature to your renderer.
2. Add Override RetroGameShaderVolume in the volume component on your scene.
3. Change parameters.
4. Enjoy!


<< Parameters >>
* Pixel Size ... Pixel size.
* Gamma ... Postalization gamma.
* Gradation ... Postalization range.
* Dither ... Enable/Disable Dither.
* Crt Texture ... Set texture CRT_ShadowMask1 or CRT_ShadowMask2 If you use CRT.
* Crt Color Gain ... Color gain.
* Crt Curve ... Emulate CRT curve.
* Gray Scale Factor ... Gray Scale Factor.
* Gray Scale Ratio ... Gray Scale Ratio.
* Shader ... Set shader.


<< Samples >>
* OldGameMachine ... The old game machine like NES.
* OldPortableGameMachine ... The old game machine like GameBoy.
* OldComputer ... The old computer like Apple II


<< Contact >>
If you need more help, contact us.
http://tk-syoutem.com/



© tk-syouten
