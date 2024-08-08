using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Utils
{
    public static class InterfaceFinder
    {
        public static GameObject[] FindObjectsWithInterface<T>() where T : class
        {
            return Object.FindObjectsOfType<Component>()
                .Where(c => c is T)
                .Select(c => c.gameObject)
                .Distinct()
                .ToArray();
        }

        public static T[] FindComponentsWithInterface<T>() where T : class
        {
            return Object.FindObjectsOfType<Component>()
                .OfType<T>()
                .ToArray();
        }
    }
}
