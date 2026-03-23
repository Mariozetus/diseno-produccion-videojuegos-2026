using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Weasel.Utils
{
    public class UIUtils
    {
        /// <summary>
        /// instantiate/remove enough prefabs to match amount
        /// </summary>
        /// <param name="prefab">The prefab to balance</param>
        /// <param name="amount">The quantity of the prefab needed in parent</param>
        /// <param name="parent">The parent the prefab should be into</param>
        public static void BalancePrefabs(GameObject prefab, int amount, Transform parent)
        {
            for (int i = parent.childCount; i < amount; ++i)
            {
                GameObject go = GameObject.Instantiate(prefab);
                go.transform.SetParent(parent, false);
            }

            for (int i = parent.childCount - 1; i >= amount; --i)
                GameObject.Destroy(parent.GetChild(i).gameObject);
        }

       /// <summary>
       /// Find out if any inputfield is currenctly active.
       /// </summary>
       /// <returns>true if any input active</returns>
        public static bool AnyInputActive()
        {
            foreach (Selectable sel in Selectable.allSelectablesArray)
                if (sel is InputField && ((InputField)sel).isFocused)
                    return true;
            return false;
        }

        /// <summary>
        /// Deselect any UI element carefully (if no chances it will be selected)
        /// </summary>
        public static void DeselectCarefully()
        {
            if (!Input.GetMouseButton(0) &&
                !Input.GetMouseButton(1) &&
                !Input.GetMouseButton(2))
                EventSystem.current.SetSelectedGameObject(null);
        }
    }
}