using System;
using PooledScrollList.Data;
using PooledScrollList.View;
using UnityEngine;
using UnityEngine.UI;

namespace PooledScrollList.Example
{
    [Serializable]
    public class PooledDataExample : PooledData
    {
        public Color Color;
        public int Number;
    }

    public class PooledViewExample : PooledView
    {
        public Image Image;
        public Text Number;

        public override void SetData(PooledData data)
        {
            base.SetData(data);

            var exampleData = (PooledDataExample) data;
            Image.color = exampleData.Color;
            Number.text = exampleData.Number.ToString();
        }
    }
}