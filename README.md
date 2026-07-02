# Number Textures

**10 ready-to-use digit textures (0–9)** for [Stride](https://www.stride3d.net/) — one `Texture`
asset per glyph (512×512, black on white). Handy for prototype UI, debug counters, HUDs, or decals.

## What's in the box

| File | Role |
|------|------|
| `Assets/0.sdtex` … `9.sdtex` | 10 Stride `Texture` assets, one per digit, sourced from `Resources/<Digit>.png` (512×512). |

Every texture is a package **root asset**, so it is always compiled and loadable by URL from any
project that references the pack.

## Quick start

Reference the pack, then drop a texture onto a sprite, UI image or material in Game Studio — or load
one in code by its name:

```csharp
var zero = Content.Load<Texture>("0");   // "0" … "9"
```

## Demo

Open `StrideNumberTextures.sln`, set **Demo.Windows** as the startup project and run — every digit
is shown on a grid of textured quads.

## License

MIT. See [LICENSE.md](LICENSE.md).
