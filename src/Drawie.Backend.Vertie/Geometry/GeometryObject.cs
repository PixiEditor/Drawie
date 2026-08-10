namespace Drawie.Backend.Vertie.Geometry;

public abstract class GeometryObject
{
    public Transform Transform { get; set; } = new Transform();
    public int MaterialIndex { get; set; } = -1;
    public GeometryData GeometryData { get; }

    public abstract void Draw();
    public abstract bool IsInFrustum(Frustum frustum, Transform transform);
    
    public GeometryObject(GeometryData geometryData, int materialIndex)
    {
        GeometryData = geometryData;
        MaterialIndex = materialIndex;
    }
}