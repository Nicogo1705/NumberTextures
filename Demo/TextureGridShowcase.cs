using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Graphics.GeometricPrimitives;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.ProceduralModels;

namespace Demo
{
    /// <summary>
    /// Loads the 10 digit textures shipped by the StrideNumberTextures pack and shows each one on
    /// its own quad, arranged in a grid, each quad slowly rotating around its Y axis.
    ///
    /// TEXTURE URL SCHEME
    /// ------------------
    /// The pack's package (AssetData/StrideNumberTextures/StrideNumberTextures.sdpkg) declares a single
    /// asset folder "Assets", and the digit textures live directly in it as 0.sdtex .. 9.sdtex.
    /// A Stride Content URL is the asset path relative to that asset-folder root, without extension,
    /// so the ten textures resolve to the URLs "0" .. "9". We therefore build the URL list simply as
    /// the strings "0".."9". Content.Load is wrapped in try/catch so a missing/renamed asset is skipped
    /// with a warning rather than crashing the demo.
    ///
    /// QUAD API
    /// --------
    /// The task suggested GeometricPrimitive.Plane.New(...), but in Stride 4.4 the non-generic
    /// GeometricPrimitive returned by that call exposes only raw GPU Buffers (no ToMeshDraw helper),
    /// so wiring it into a Model is verbose and version-fragile. Instead we use the higher-level
    /// PlaneProceduralModel, which produces a ready-to-render Model in one call:
    ///   - Normal = UpZ           -> the plane lies in the XY plane facing +Z (a vertical "card").
    ///   - GenerateBackFace = true -> double-sided, so the digit stays visible through the whole spin.
    ///   - Size 1x1, one texture-mapped Lambert material per quad.
    /// Generate(Services) hands back a Model we drop onto a ModelComponent.
    /// </summary>
    public class TextureGridShowcase : SyncScript
    {
        /// <summary>Number of quads per row.</summary>
        public int Columns { get; set; } = 5;

        /// <summary>Distance between quad centers, in world units.</summary>
        public float Spacing { get; set; } = 1.4f;

        /// <summary>Rotation speed of each quad around Y, in radians per second.</summary>
        public float RotationSpeed { get; set; } = 0.5f;

        private readonly List<Entity> quads = new List<Entity>();

        public override void Start()
        {
            base.Start();

            // Build the Content URLs for the ten digit textures: "0" .. "9".
            var urls = new List<string>();
            for (int i = 0; i <= 9; i++)
                urls.Add(i.ToString());

            int columns = Columns < 1 ? 1 : Columns;
            int rows = (urls.Count + columns - 1) / columns;

            for (int i = 0; i < urls.Count; i++)
            {
                var url = urls[i];

                Texture texture;
                try
                {
                    texture = Content.Load<Texture>(url);
                }
                catch (System.Exception e)
                {
                    Log.Warning($"TextureGridShowcase: could not load texture '{url}' ({e.Message}); skipping.");
                    continue;
                }

                if (texture == null)
                {
                    Log.Warning($"TextureGridShowcase: texture '{url}' resolved to null; skipping.");
                    continue;
                }

                var material = Material.New(GraphicsDevice, new MaterialDescriptor
                {
                    Attributes =
                    {
                        Diffuse = new MaterialDiffuseMapFeature(new ComputeTextureColor { Texture = texture }),
                        DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    },
                });

                var proceduralModel = new PlaneProceduralModel
                {
                    Size = new Vector2(1f, 1f),
                    Normal = NormalDirection.UpZ,   // vertical card facing +Z (the camera)
                    GenerateBackFace = true,        // double-sided so it stays visible while spinning
                    MaterialInstance = { Material = material },
                };
                var model = proceduralModel.Generate(Services);

                int col = i % columns;
                int row = i / columns;
                float x = (col - (columns - 1) * 0.5f) * Spacing;
                float y = -(row - (rows - 1) * 0.5f) * Spacing;

                var quad = new Entity($"Digit_{url}")
                {
                    new ModelComponent(model),
                };
                quad.Transform.Position = new Vector3(x, y, 0f);

                Entity.AddChild(quad);
                quads.Add(quad);
            }

            if (quads.Count == 0)
                Log.Warning("TextureGridShowcase: no digit textures were loaded. Check that the StrideNumberTextures pack is referenced and its assets compiled.");
        }

        public override void Update()
        {
            float dt = (float)Game.UpdateTime.Elapsed.TotalSeconds;
            float delta = RotationSpeed * dt;

            foreach (var quad in quads)
                quad.Transform.Rotation *= Quaternion.RotationY(delta);
        }
    }
}
