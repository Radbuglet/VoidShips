using Godot;

namespace VoidShips.Util;

public static class MultiMeshUtil
{
    private static void ResizeCapacity(this MultiMesh mm, int instanceCount)
    {
        // Grab the old instance data
        var buffer = mm.Buffer;
        
        // Determine how many floats it allocated per instance (i.e. the buffer's stride)
        var floatsPerInstance = buffer.Length / mm.InstanceCount;
        
        // Resize our local buffer in accordance to that.
        System.Array.Resize(ref buffer, instanceCount * floatsPerInstance);
        
        // Increase the size of the instance buffer and reassign it
        mm.InstanceCount = instanceCount;
        mm.Buffer = buffer;
    }
    
    public static void ReserveCapacityForOne(this MultiMesh mm)
    {
        if (mm.VisibleInstanceCount >= mm.InstanceCount)
            mm.ResizeCapacity(mm.InstanceCount * 2);
    }

    public static void ShrinkCapacity(this MultiMesh mm)
    {
        if (mm.VisibleInstanceCount < mm.InstanceCount / 2)
            mm.ResizeCapacity(mm.InstanceCount / 2);
    }
}
