# Unity-Pooled-Scroll-List

Unity UI extension that is meant to optimize ScrollRect performance by utilizing a pool of objects and reordering objects in the scroll viewport. 
Supports horizontal, vertical and grid layout groups. An example scene is included.

* [Horizontal & Vertical layout](https://drive.google.com/open?id=1ywgmnCAIkopC6GNVNuF3OuA_d0JgUDQx)
* [Grid layout](https://drive.google.com/open?id=13ms-xuaNFKubY3w2td8vKm5T_VmKZip5)

Implementation:
* Determine data that will be populated in your views and create a data class inherited from **PooledData**
* Create a view class inherited from **PooledView** and override **SetData(PooledData data)** method that applies the data to the view
* Add your **PooledView** component to the view prefab together with **PooledElement** component
* Add **PooledScrollRectController/PooledScrollRectGridController** component to your **ScrollRect**
* Assign your view prefab in PooledScrollRectController's **Template** property and set the initial **PoolCapacity** 
* (**ExternalViewPort** and **DataProvider** properties are optional)
* Populate your scroll list at runtime by using PooledScrollRectController's **Initialize/Add** methods

```
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
```
