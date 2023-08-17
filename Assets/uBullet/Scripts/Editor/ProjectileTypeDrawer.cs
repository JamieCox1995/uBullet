using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TotalDistraction.uBullet.Data;
using TotalDistraction.uBullet.Utilities;

namespace TotalDistraction.uBullet.Editor
{
    [CustomPropertyDrawer(typeof(ProjectileTypeAttribute))]
    public class ProjectileTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ProjectileTypeAttribute projectileTypeAttribute = attribute as ProjectileTypeAttribute;

            if (Settings.Instance == null)
            {
                throw new InvalidOperationException("There is no Settings.Instance in the scene. Please create one.");
            }

            List<string> projectiles = Settings.Instance.InstanceMunitions.munitions.Select(munition => munition.munitionType).ToList();
            int selectedIndex = projectiles.IndexOf(property.stringValue);

            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, projectiles.ToArray());

            property.stringValue = projectiles[selectedIndex];
        }
    }
}
