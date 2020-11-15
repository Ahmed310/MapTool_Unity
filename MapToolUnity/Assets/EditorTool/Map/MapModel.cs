
using System;

[Flags]
[System.Serializable]
public enum BlockType : int
{
    None = 0,
    Solid = 1 << 1,
    LowSolid = 1 << 2, // like water, only player checks collision with

    Block1 = 1 << 3,
    Block2 = 1 << 4,
    Block3 = 1 << 5,        // 
    Grass = 1 << 6,
    Crate = 1 << 7,
    Barrel = 1 << 8,
    SpawnPoint = 1 << 9,
    Fence = 1 << 10,
    Tree = 1 << 11,
    Water = 1 << 12
}


[System.Serializable]
public class BlockModel
{
    public float X, Y;
    public float RotY;
    public float Width;
    public float Height;
    public BlockType Type;
}

[System.Serializable]
public class MapModel
{
    public int Width;
    public int Height;
    public BlockType[] Data;


    public BlockType GetBlock(int x, int y) => Data[x + Width * y];
    public void SetBlock(BlockType block, int x, int y) => Data[x + Width * y] = block;
}
