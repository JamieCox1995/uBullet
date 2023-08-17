using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.Effects
{
    public class ImpactEffect : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            int childCount = transform.childCount;

            if (childCount <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
