using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Tests
{
    public class PooledScrollTest : MonoBehaviour
    {
        public ItemScrollRectController ScrollRectController;
        public InputField InputField;
        public int Count;
        public List<Color> Colors;

        public void Start()
        {
            Apply();
        }

        public void Apply()
        {
            if (InputField != null && !string.IsNullOrEmpty(InputField.text))
            {
                int result;
                if (int.TryParse(InputField.text, out result))
                {
                    Count = result;
                }
            }

            var data = new List<ItemData>(Count);

            for (var i = 0; i < Count; i++)
            {
                data.Add(new ItemData {Color = Colors[Random.Range(0, Colors.Count)], Number = i + 1});
            }

            ScrollRectController.Initialize(data);
        }
    }
}