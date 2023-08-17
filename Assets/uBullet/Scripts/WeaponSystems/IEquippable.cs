using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.WeaponSystems
{
    public class Equippable : MonoBehaviour
    {
        public Camera viewCamera;

        public virtual int RemainingShots()
        {
            return 0;
        }

        public virtual int TotalCarriedAmmo()
        {
            return 0;
        }
    }
}
