using System.Collections.Generic;
using JetBrains.Annotations;
using PooledScrollList.Controller;
using PooledScrollList.Data;
using UnityEngine;
using UnityEngine.UI;

namespace PooledScrollList.Example
{
    public class PooledDataProviderExample : PooledDataProvider
    {
        public PooledScrollRectController ScrollRectController;
        public PooledScrollRectGridController ScrollRectGridController;
        public InputField InputField;
        public int Count;
        public List<Color> Colors;

        public override List<PooledData> GetData()
        {
            var data = new List<PooledData>(Count);

            for (var i = 0; i < Count; i++)
            {
                data.Add(new PooledDataExample { Color = Colors[Random.Range(0, Colors.Count)], Number = i + 1 });
            }

            return data;
        }

        [UsedImplicitly]
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

            var data = GetData();

            if (ScrollRectController != null)
            {
                ScrollRectController.Initialize(data);
            }

            if (ScrollRectGridController != null)
            {
                ScrollRectGridController.Initialize(data);
            }
        }
    }
}