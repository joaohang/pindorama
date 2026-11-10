using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Faíscas + poeira no ponto em que o gancho se fixa. Tudo é criado em
    /// runtime (sem prefab) para o protótipo continuar "plug and play".
    /// </summary>
    public static class HookImpactVfx
    {
        private static Texture2D softCircle;

        public static void Play(Vector2 position, Color color)
        {
            GameObject root = new GameObject("HookImpact");
            root.transform.position = new Vector3(position.x, position.y, 0f);

            CreateSparks(root.transform, color);
            CreateDust(root.transform);

            Object.Destroy(root, 1.2f);
        }

        public static Color ColorFor(HookColor color)
        {
            switch (color)
            {
                case HookColor.Red:
                    return new Color(1f, 0.35f, 0.40f, 1f);
                case HookColor.Blue:
                    return new Color(0.30f, 0.72f, 1f, 1f);
                default:
                    return new Color(1f, 0.85f, 0.40f, 1f);
            }
        }

        private static void CreateSparks(Transform parent, Color color)
        {
            GameObject go = new GameObject("Sparks");
            go.transform.SetParent(parent, false);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.duration = 0.1f;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = new ParticleSystem.MinMaxGradient(color);
            main.gravityModifier = 1.8f;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 16)
            });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.06f;

            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = MakeParticleMaterial("Legacy Shaders/Particles/Additive");
            renderer.material.mainTexture = SoftCircle();
            renderer.sortingOrder = 28;

            ps.Play();
        }

        private static void CreateDust(Transform parent)
        {
            GameObject go = new GameObject("Dust");
            go.transform.SetParent(parent, false);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.duration = 0.15f;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.30f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.65f, 0.67f, 0.70f, 0.45f));
            main.gravityModifier = 0.25f;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 8)
            });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.12f;

            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = MakeParticleMaterial("Legacy Shaders/Particles/Alpha Blended");
            renderer.material.mainTexture = SoftCircle();
            renderer.sortingOrder = 27;

            ps.Play();
        }

        private static Material MakeParticleMaterial(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            return new Material(shader);
        }

        private static Texture2D SoftCircle()
        {
            if (softCircle != null)
                return softCircle;

            const int size = 16;
            softCircle = new Texture2D(size, size, TextureFormat.RGBA32, false);
            softCircle.wrapMode = TextureWrapMode.Clamp;
            softCircle.filterMode = FilterMode.Bilinear;

            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
                    float alpha = Mathf.Clamp01(1f - distance);
                    softCircle.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            softCircle.Apply();
            return softCircle;
        }
    }
}
