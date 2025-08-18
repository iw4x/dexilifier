A COD-oriented HLSL source generator that takes in compiled shaders (CSO) and generates humanized HLSL code.

The goal is first and foremost to generate **readable code** as if a human had written it - ordered, renamed, structured, minimal, factorized.
Then accuracy.
This is a tool for tinkering.

<img width="1608" height="803" alt="image" src="https://github.com/user-attachments/assets/76a5d602-8eb7-43e4-86fa-b2405000c0c7" />

TODO:
- More operation reduction and detection (uniform multiplication e.g.)
- Variant detection & master shader generation from several LIT variantes of a same technique
- Pixel Shaders with anything else than 4-dimensional output (1D noise sampling for instance)
- lp sm sun fog b0c0n0s0 nc sm3 (has a 3D sampler, not supported)
