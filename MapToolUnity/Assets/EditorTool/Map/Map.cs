using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Map : MonoBehaviour
{
    [SerializeField]
    public MapModel MapModel;

    public int Width, Height;


    List<MapBlock> blocks = new List<MapBlock>();

    public MapBlock GetBlock(int x, int z)
    {
       return blocks.Find(b=>b.gameObject.transform.position.x == x && b.gameObject.transform.position.z == z);
    }

    public void AddBlock(MapBlock block) 
    {
        blocks.Add(block);
    }

    public void RemoveBlock(MapBlock block)
    {
        blocks.Remove(block);
    }
    int x, z;
    [ContextMenu("SaveMap")]
    public void SaveMap(string mapName) 
    {
        MapModel = new MapModel();
        MapModel.Width = Width;
        MapModel.Height = Height;
        
        MapModel.Data = new BlockType[Width * Height];

        var blocks = GetComponentsInChildren<MapBlock>();
        
        try
        {
            foreach (var block in blocks)
            {
                x = (int)block.transform.position.x;
                z = (int)block.transform.position.z;
                MapModel.SetBlock(block.block, (int)block.transform.position.x, (int)block.transform.position.z);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{x}|{z}");
            Debug.LogError(e);
        }
        

        Debug.Log(Utils.SerializeAsXml<MapModel>(MapModel));

        Utils.SaveRecordset<MapModel>(MapModel, mapName);
    }

    public static Map LoadMap(string data)
    {
        var map = data.DeserializeFromXml<MapModel>();
        var tiles = Resources.LoadAll<MapBlock>("TileSet");
        if (map == null)
        {
            Debug.LogError("Unable to parse Map data");
        }
        else
        {
            var newMap = new GameObject("__MAP__");
            var _map = newMap.AddComponent<Map>();
            _map.MapModel = map;
            for (int i = 0; i < map.Data.Length; i++)
            {
                if (map.Data[i] != BlockType.None)
                {
                    var tileObj = tiles.FirstOrDefault((tile) => tile.block == map.Data[i]);
                    if (tileObj != null)
                    {
                        var tileView = GameObject.Instantiate(tileObj.gameObject) as GameObject;
                        var tileCenter = new Vector3(i / map.Width, 0, i % map.Width);

                        int x = Mathf.CeilToInt(tileCenter.x - 0.5f);
                        int z = Mathf.CeilToInt(tileCenter.z - 0.5f);

                        tileView.transform.position = new Vector3(x, 0, z);
                        tileView.transform.SetParent(_map.transform);
                        _map.AddBlock(tileView.GetComponent<MapBlock>());
                       
                    }
                    
                }
            }
            return _map;
        }
        return null;
    }

}
