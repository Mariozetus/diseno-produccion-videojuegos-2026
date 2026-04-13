using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weasel.Utils
{
    public class V3Utils
    {
        /// <summary>
        /// Get the flatten Vec3 (with Y = 0)
        /// </summary>
        /// <param name="entry">Vec3 to convert</param>
        /// <returns>Converted Vec3</returns>
        public static Vector3 GetFlatVec3(Vector3 entry)
        {
            return new Vector3(entry.x, 0, entry.z);
        }

        /// <summary>
        /// Check if a transform is close on (1,0,1) plane to a position
        /// </summary>
        /// <param name="self"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        public static bool IsFlatClose(Transform self, Vector3 goal)
        {
            return GetFlatDist(self.position, goal).magnitude < 0.1f;
        }

        /// <summary>
        /// Get flatten distance between two vectors
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        public static Vector3 GetFlatDist(Vector3 start, Vector3 goal)
        {
            return (GetFlatVec3(goal) - GetFlatVec3(start));
        }

        /// <summary>
        /// Get flatten direction between two vectors
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        public static Vector3 GetFlatDir(Vector3 start, Vector3 goal)
        {
            return GetFlatDist(start, goal).normalized;
        }

        /// <summary>
        /// Is a transform looking at a specific position on the (1,0,1) plane
        /// </summary>
        /// <param name="self"></param>
        /// <param name="forward"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        public static bool IsLookingAt(Transform self, Vector3 forward, Vector3 goal)
        {
            return Vector3.Dot(forward, GetFlatDir(self.position, goal).normalized) > 0.9f;
        }
    }

    public static class V3Extensions
    {
        /// <summary>
        /// Flatten the Vec3 (with Y = 0)
        /// </summary>
        public static void SetFlat(this Vector3 self)
        {
            self.Set(self.x, 0, self.z);
        }
    }
}
