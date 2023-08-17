using System.Collections;
using System.Collections.Generic;
using TotalDistraction.uBullet.UI;
using UnityEngine;

namespace TotalDistraction.uBullet.WeaponSystems
{
    public class WeaponHolder : MonoBehaviour
    {
        public GameObject[] equippedWeapons = new GameObject[2];
        public GameObject[] pickupObjects;
        private int currentWeapon = 0;

        public LayerMask pickupWeaponLayer;

        [Header("UI Settings: ")]
        public WeaponUI weaponUI;

        // Use this for initialization
        void Start()
        {
            if (equippedWeapons[currentWeapon] != null)
            {
                equippedWeapons[currentWeapon].SetActive(true);

                weaponUI.UpdateEquipped(equippedWeapons[currentWeapon].GetComponent<Equippable>());

                SetupWeaponViewCamera();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SwitchWeapon();
            }

            // Checking for a pickup
            RaycastHit hit;

            if (UnityEngine.Physics.SphereCast(transform.position, 0.25f, transform.forward, out hit, 1.5f))
            {
                if (pickupWeaponLayer == (pickupWeaponLayer | (1 << hit.collider.gameObject.layer)))
                {
                    WeaponPickup pickup = hit.collider.gameObject.GetComponent<WeaponPickup>();

                    weaponUI.ShowPickupText(pickup.weaponName);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        PickupWeapon(pickup);
                    }
                }
            }
            else
            {
                weaponUI.HideWeaponText();
            }
        }

        /// <summary>
        /// When the players wants to switch weapon; we want to deactivate the current weapon,
        /// increment the currentWeapon index and ensure it is not out of range of the Array,
        /// and then activate the new weapon and update the weapon UI.
        /// </summary>
        private void SwitchWeapon()
        {
            if (equippedWeapons[currentWeapon] != null) equippedWeapons[currentWeapon].SetActive(false);

            currentWeapon++;

            if (currentWeapon >= equippedWeapons.Length)
            {
                currentWeapon = 0;
            }

            if (equippedWeapons[currentWeapon] != null)
            {
                equippedWeapons[currentWeapon].SetActive(true);

                // Update the weapon UI
                weaponUI.UpdateEquipped(equippedWeapons[currentWeapon].GetComponent<Equippable>());
            }
        }

        /// <summary>
        /// When the player wants to pickup a new weapon, we want to destroy the currently equipped
        /// weapon, spawn in the prefab attached to the pickup and set the element at the currentWeapon index
        /// to the newly spawned object.
        /// </summary>
        /// <param name="pickup"></param>
        public void PickupWeapon(WeaponPickup pickup)
        {

            if (pickupObjects[currentWeapon] != null)
            {
                pickupObjects[currentWeapon].transform.position = transform.position;
                pickupObjects[currentWeapon].SetActive(true);
            }

            pickupObjects[currentWeapon] = pickup.gameObject;

            Destroy(equippedWeapons[currentWeapon]);
            pickup.gameObject.SetActive(false);

            equippedWeapons[currentWeapon] = Instantiate(pickup.weaponPrefab, transform);

            Equippable newGun = equippedWeapons[currentWeapon].GetComponent<Equippable>();

            weaponUI.gameObject.SetActive(true);

            weaponUI.UpdateEquipped(newGun);
            newGun.viewCamera = gameObject.GetComponent<Camera>();
        }

        public Gun GetCurrentWeapon()
        {
            return equippedWeapons[currentWeapon].GetComponent<Gun>();
        }

        private void SetupWeaponViewCamera()
        {
            foreach (GameObject gun in equippedWeapons)
            {
                gun.GetComponent<Equippable>().viewCamera = gameObject.GetComponent<Camera>();
            }
        }
    }
}
