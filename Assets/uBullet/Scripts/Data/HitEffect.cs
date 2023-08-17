using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.Data
{
    /// <summary>
    /// HitEffects are used to store all of the possible effects for when a projectile impacts a surface in-game. This is set up so that we
    /// can then easily switch out all of the effects easily if we want to have special events which change some of the VFX.
    /// </summary>
    [CreateAssetMenu(fileName = "GlobalHitEffect", menuName = "ScriptableObjects/Global Hit Effect", order = 0)]
    public class HitEffect : ScriptableObject
    {
        public List<HitEffectData> hitEffects;

        public ImpactEffects GetMaterialImpactEffects(MaterialType materialType)
        {
            ImpactEffects impactEffects;

            impactEffects = hitEffects.FirstOrDefault(eff => eff._materialType == materialType)._impactEffects;

            if (impactEffects == null)
            {
                Debug.LogErrorFormat("The MaterialType; {0}, does not exist. Defaulting to the ImpactEffects for the Metal MaterialType", materialType);
                impactEffects = hitEffects[0]._impactEffects;
            }

            return impactEffects;
        }
    }

    [System.Serializable]
    public enum MaterialType { Metal, Wood, Plastic, Glass, Organic }

    [System.Serializable]
    public class HitEffectData
    {
        public MaterialType _materialType;

        public ImpactEffects _impactEffects;
    }

    [System.Serializable]
    public class ImpactEffects
    {
        [Header("Particle Effects")]
        public GameObject ricochetEffect;
        public GameObject penetrationEntranceEffect;
        public GameObject penetrationExitEffect;
        [Tooltip("This effect is what is used when a projectile hits this object but does not penetrate or ricochet.")]
        public GameObject hitEffect;

        [Header("Sound Effects")]
        public AudioEffect[] impactClips;
        public AudioEffect ricochetSFX;
    }

    [System.Serializable]
    public class AudioEffect
    {
        public AudioClip clip;
        public float volume = 1f;
    }
}
