using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.Data
{
    [CreateAssetMenu(fileName = "Munitions", menuName = "ScriptableObjects/Munition Objects", order = 1)]
    public class Munitions : ScriptableObject
    {
        public List<MunitionOptions> munitions;

        public MunitionOptions GetMunitionOptions(string type)
        {
            return munitions.FirstOrDefault(m => m.munitionType == type);
        }

        public GameObject GetMunitionPrefab(string type)
        {
            return munitions.FirstOrDefault(m => m.munitionType == type).munitionPrefab;
        }

        public HitEffect GetMunitionHitEffect(string type)
        {
            return munitions.FirstOrDefault(m => m.munitionType == type).hitEffect;
        }

        public HitEffect GetMunitionHitEffect(GameObject gameObject)
        {
            return munitions.FirstOrDefault(m => m.munitionPrefab == gameObject).hitEffect;
        }
    }

    [System.Serializable]
    public class MunitionOptions
    {
        public string munitionType;
        public GameObject munitionPrefab;
        public GameObject munitionCasing;
        public HitEffect hitEffect;
    }
}
