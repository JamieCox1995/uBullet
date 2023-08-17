using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.Effects
{
    public class ParticleDestroyer : MonoBehaviour
    {
        private ParticleSystem[] _particleSystems;

        // Use this for initialization
        void Start()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
        }

        // Update is called once per frame
        void Update()
        {
            CheckParticlesAlive();
        }

        private void CheckParticlesAlive()
        {
            foreach (ParticleSystem system in _particleSystems)
            {
                if (system.IsAlive())
                {
                    return;
                }
            }

            Destroy(gameObject);
        }
    }
}
