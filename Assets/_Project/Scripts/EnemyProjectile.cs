using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Projétil simples de torreta. Trigger; ao encostar no player, respawna.
    /// </summary>
    public class EnemyProjectile : MonoBehaviour
    {
        private static Sprite projectileSprite;

        private LevelController level;
        private Vector2 velocity;
        private float life = 5f;

        public static EnemyProjectile Spawn(Vector2 position, Vector2 direction,
                                            float speed, LevelController level)
        {
            GameObject go = new GameObject("EnemyProjectile");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.35f;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSprite();
            sr.color = new Color(1f, 0.30f, 0.35f, 1f);
            sr.sortingOrder = 8;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            EnemyProjectile projectile = go.AddComponent<EnemyProjectile>();
            projectile.level = level;
            projectile.velocity = direction.normalized * speed;
            return projectile;
        }

        private void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
            life -= Time.deltaTime;

            if (life <= 0f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController2D>() == null)
                return;

            PlayerDamageController damage =
                other.GetComponentInParent<PlayerDamageController>();
            if (damage != null)
            {
                damage.ApplyDamage(new DamageInfo(
                    DamageType.NonLethal, transform.position, 11f, gameObject));
            }
            else if (level != null)
            {
                level.RespawnPlayer();
            }

            Destroy(gameObject);
        }

        private static Sprite GetSprite()
        {
            if (projectileSprite != null)
                return projectileSprite;

            Texture2D tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(3.5f, 3.5f);

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, d <= 4f ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }

            tex.Apply();
            projectileSprite = Sprite.Create(tex, new Rect(0f, 0f, 8f, 8f),
                new Vector2(0.5f, 0.5f), 8f);
            return projectileSprite;
        }
    }
}
