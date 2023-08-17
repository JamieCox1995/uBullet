using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.Effects
{
    public class DestroyableDecal : MonoBehaviour
    {
        public float aliveTime = 5f;
        public float startFadeTime = 3f;

        private SpriteRenderer[] spriteRenderers;

        // Use this for initialization
        void Start()
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

            StartCoroutine(FadeDecal());
        }

        private IEnumerator FadeDecal()
        {
            yield return new WaitForSeconds(startFadeTime);

            float timer = 0f, totalTime = aliveTime/* - startFadeTime*/;

            while (timer <= totalTime)
            {
                timer += Time.deltaTime;

                foreach (SpriteRenderer sr in spriteRenderers)
                {
                    sr.color = Color.Lerp(Color.white, Color.clear, timer / totalTime);
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        private void OnValidate()
        {
            if (startFadeTime > aliveTime) startFadeTime = aliveTime;
        }
    }
}
