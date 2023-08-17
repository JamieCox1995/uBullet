using System.Collections;
using System.Collections.Generic;
using TotalDistraction.uBullet.WeaponSystems;
using UnityEngine;
using UnityEngine.UI;

namespace TotalDistraction.uBullet.UI
{
    public class WeaponUI : MonoBehaviour
    {
        [System.Serializable]
        public enum DisplayType { MagazineCapacity, TotalRemainingShots }
        public DisplayType displayType;

        public Text currentAmmo;
        public Text magazineCapacity;

        public Text firemode;
        public Equippable equipped;

        public Text pickupText;

        // Update is called once per frame
        void Update()
        {
            if (equipped == null)
            {
                gameObject.SetActive(false);
                return;
            }
            else if (equipped != null && gameObject.activeInHierarchy == false)
            {
                gameObject.SetActive(true);
            }

            currentAmmo.text = equipped.RemainingShots().ToString();

            if (equipped is Gun)
            firemode.text = string.Format("Firemode: {0}", equipped.GetComponent<Gun>().m_GunSettings.currentFireType);

            if (displayType == DisplayType.TotalRemainingShots)
            {
                magazineCapacity.text = string.Format("/ {0}", equipped.TotalCarriedAmmo().ToString());

            }
        }

        public void UpdateEquipped(Equippable newEquipped)
        {
            this.equipped = newEquipped;

            currentAmmo.text = equipped.RemainingShots().ToString();

            if (equipped is Gun)
            firemode.text = string.Format("Firemode: {0}", equipped.GetComponent<Gun>().m_GunSettings.currentFireType);

            string toDisplay = "";

            if (displayType == DisplayType.MagazineCapacity)
            {
                if (equipped is Gun)
                {
                    toDisplay = string.Format("/ {0}", equipped.GetComponent<Gun>().m_GunSettings.magazineCapacity);
                }
            }
            else
            {
                toDisplay = string.Format("/  {0}", equipped.TotalCarriedAmmo());
            }

            magazineCapacity.text = toDisplay;
        }

        public void ShowPickupText(string weaponName)
        {
            if (pickupText.enabled == false) pickupText.enabled = true;

            pickupText.text = string.Format("Press 'E' to pick up <color=#ff0000ff>{0}</color>", weaponName);
        }

        public void HideWeaponText()
        {
            if (pickupText.enabled == true)
            {
                pickupText.enabled = false;
            }
        }
    }
}
