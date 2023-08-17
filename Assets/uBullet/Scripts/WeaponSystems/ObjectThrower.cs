using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.WeaponSystems
{
    public class ObjectThrower : Equippable
    {
        [Header("Throw Settings:")]
        public float throwForce = 10f;

        private int remainingShots = 3;
        private int total = 5;

        [Header("Thrown Object Variables:"), Tooltip("Actual Gameobject to be spawned when we play the Throw animation.")]
        public GameObject throwablePrefab;
        [Tooltip("This is the visual object of the Throwable Object in the Viewmodel.")]
        public GameObject throwableVisual;
        public Transform throwableSpawnpoint;

        private Animator _Animator;
        private bool readyToThrow = true;

        // Use this for initialization
        void Start()
        {
            _Animator = GetComponent<Animator>();

            readyToThrow = true;
        }

        // Update is called once per frame
        void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (readyToThrow == true)
            {
                // Checking to see if we're holding down the mouse button to ready up a throw
                if (Input.GetMouseButtonDown(0) && remainingShots > 0)
                {
                    _Animator.SetBool("QueueThrow", true);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    _Animator.SetBool("QueueThrow", false);
                }
            }
        }

        /// <summary>
        /// Called by an AnimationEvent in 'Throw' Animations. The method spawns in the throwable
        /// prefab, and hides the visual throwable in the Viewmodel.
        /// </summary>
        public void ThrowGameObject()
        {
            // Spawning in the throwable prefab
            GameObject prefab = Instantiate(throwablePrefab, throwableSpawnpoint.position, Quaternion.identity);
            prefab.transform.forward = throwableSpawnpoint.forward;

            // Applying a force to the thrown object.
            prefab.GetComponent<Rigidbody>().AddForce(prefab.transform.forward * throwForce, ForceMode.VelocityChange);

            if (prefab.GetComponent<Explosive>())
            {
                prefab.GetComponent<Explosive>().TriggerExplosion(true);
            }

            remainingShots--;

            // Hiding the throwable viewmodel
            throwableVisual.SetActive(false);
            readyToThrow = false;
        }

        /// <summary>
        /// Called by an AnimationEvent. Shows the throwable viewmodel to indicate to the player
        /// that another throwable is ready.
        /// </summary>
        public void Reload()
        {
            if (remainingShots != 0)
            {
                throwableVisual.SetActive(true);
                readyToThrow = true;
            }
        }

        public override int RemainingShots()
        {
            return remainingShots;
        }

        public override int TotalCarriedAmmo()
        {
            return total;
        }
    }
}
