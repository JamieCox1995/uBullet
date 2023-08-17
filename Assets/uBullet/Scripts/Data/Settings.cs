using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.Data
{
    public class Settings : MonoBehaviour
    {
        public static Settings Instance;

        public string MunitionsDatabase = "Munitions/DefaultMunitions";
        public Munitions InstanceMunitions;

        public float PlayerFOV = 80f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            InstanceMunitions = Resources.Load(MunitionsDatabase) as Munitions;
        }

        private void OnValidate()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
