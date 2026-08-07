using SilkNet.Geometry;
using SilkNet.Rendering;

namespace SilkNet.Optimization;

public class MaterialBatcher
{
    public Dictionary<int, Batch> Batches { get; private set; } = new Dictionary<int, Batch>();
    
    public MaterialBatcher(IList<GeometryObject> geometryObjects)
    {
        RecalculateBatches(geometryObjects);
    }

    private void RecalculateBatches(IList<GeometryObject> geometryObjects)
    {
        for (var i = 0; i < geometryObjects.Count; i++)
        {
            var geometryObject = geometryObjects[i];
            var materialIndex = geometryObject.MaterialIndex;
            if (Batches.ContainsKey(materialIndex))
            {
                Batches[materialIndex].ObjectsCount++;
            }
            else
            {
                Batches.Add(materialIndex, new Batch(1, i));
            }
        }

        Batches = Batches.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    public void AddObject(GeometryObject geometryObject, int indexOfObject)
    {
        var materialIndex = geometryObject.MaterialIndex;
        if (Batches.ContainsKey(materialIndex))
        {
            Batches[materialIndex].ObjectsCount++;
        }
        else
        {
            Batches.Add(materialIndex, new Batch(1, indexOfObject));
        }
    }
    
    public void RemoveObject(GeometryObject geometryObject)
    {
        var materialIndex = geometryObject.MaterialIndex;
        if (Batches.ContainsKey(materialIndex))
        {
            Batches[materialIndex].ObjectsCount--;
        }
    }
}

public class Batch
{
    public int ObjectsCount { get; set; }
    public int StartIndex { get; set; }
    
    public Batch(int objectsCount, int startIndex)
    {
        ObjectsCount = objectsCount;
        StartIndex = startIndex;
    }
}