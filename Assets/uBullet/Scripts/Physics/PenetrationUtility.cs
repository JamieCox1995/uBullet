using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.Physics
{
    public static class PenetrationUtility
    {
        private const float boundsPadding = 0.1f;

        /// <summary>
        /// Gets the thickness of an object relative to in input vector. Often referred to as a "Line Of Sight Thickness".
        /// </summary>
        /// <param name="hitObject"> The Object which we want to check the Line Of Sight Thickness of.</param>
        /// <param name="impactLocation"> The point at which we the 'hitObject' was hit. This variable will be used to start the thickness check.</param>
        /// <param name="impactNormal"> The direction which we want to get the thickness of the object.</param>
        /// <param name="relativeHardness"> A multiplier of the calculated thickness. This is used to simulate varying object hardnesses.</param>
        /// <returns> Returns a floating point thickness of the object from the impactLocation, in the direction of impactNormal, to the other side of the object.</returns>
        public static float Get3DLineOfSightThickness(GameObject hitObject, Vector3 impactLocation, Vector3 impactNormal, LayerMask mask, float relativeHardness = 1f)
        {
            float thickness = Mathf.Infinity;

            Bounds bounds = hitObject.GetComponent<Collider>().bounds;
            float[] boundsSize = new float[] { bounds.size.x, bounds.size.y, bounds.size.z };
            float maxSize = Mathf.Max(boundsSize);
            RaycastHit hit;

            GameObject detected = null;
            float rayDistance = maxSize + boundsPadding;
            float cutoff = rayDistance / 100f;

            // We are using a while loop to ensure that the ray is raycasting against the correct object
            while (detected != hitObject && rayDistance > cutoff)
            {
                Vector3 rayStart = impactLocation + (impactNormal * rayDistance);
                Ray ray = new Ray(rayStart, -impactNormal);

                if (UnityEngine.Physics.Raycast(ray, out hit, rayDistance, mask))
                {
                    detected = hit.collider.gameObject;

                    if (detected == hitObject)
                    {
                        float distance = Vector3.Distance(impactLocation, hit.point);

                        return distance * relativeHardness;
                    }
                }

                rayDistance *= 0.9f;
            }

            return thickness;
        }

        public static float GetNominalThickness(GameObject hitObject, Vector3 impactLocation, Vector3 surfaceNormal, LayerMask mask, float relativeHardness)
        {
            float thickness = Mathf.Infinity;

            Bounds bounds = hitObject.GetComponent<Collider>().bounds;
            float[] boundsSize = new float[] { bounds.size.x, bounds.size.y, bounds.size.z };
            float maxSize = Mathf.Max(boundsSize);
            RaycastHit hit;

            GameObject detected = null;
            float rayDistance = maxSize + boundsPadding;
            float cutoff = rayDistance / 100f;

            while (detected != hitObject && rayDistance > cutoff)
            {
                Vector3 rayStart = impactLocation + (surfaceNormal * rayDistance);
                Ray ray = new Ray(rayStart, -surfaceNormal);

                if (UnityEngine.Physics.Raycast(ray, out hit, rayDistance, mask))
                {
                    detected = hit.collider.gameObject;

                    if (detected == hitObject)
                    {
                        float distance = Vector3.Distance(impactLocation, hit.point);

                        return distance * relativeHardness;
                    }
                }

                rayDistance *= 0.9f;
            }

            return thickness;
        }
    }
}
