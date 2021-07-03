using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class MapEditor : EditorWindow
{

	Vector2Int mapSize;
	public MapBlock[] tiles;
	private int paletteIndex;
	private Map _map;
	const int TILES_IN_ROW = 3;
//	string[] options = new string[] { "None", "Paint", "Erase" };
	int selectedOption = 0;
	TextAsset mapContent;
	string mapName;
	bool editMode = false;
	private Transform lastSpawnedObject;

	[MenuItem("Tools/MapEditor")]
	static void Open()
	{
		MapEditor win = GetWindow<MapEditor>();
		win.titleContent = new GUIContent("Map Editor");
		win.Show();
	}

	
	void OnEnable()
	{
		SceneView.duringSceneGui += OnSceneGUI;
		tiles = Resources.LoadAll<MapBlock>("TileSet");
		mapSize = new Vector2Int(60, 60);

		_map = GameObject.FindObjectOfType<Map>();

		if (_map != null)
		{
			mapSize = new Vector2Int(_map.Width, _map.Height);
		}
		editMode = true;
	}

	
	void OnDisable()
	{
		SceneView.duringSceneGui -= OnSceneGUI;
	}

    #region EDITOR_GUI

    void OnGUI()
	{
		EditorGUILayout.HelpBox($"{"Left mouse button click to paint a block"}" +
			$"\n{"Right mouse button click to delete a block"}" +
			$"\n{"Hold ctrl & move mouse fot batch Erase"}" +
			$"\n{"Hold shift and move mouse for batch Paint"}" +
			$"\n{"Do not remove Block Manually"}", MessageType.Info);
		
		DrawView();
	}

	void DrawView()
	{
		if (_map == null)
		{
			GUILayout.BeginVertical("box");

			if (GUILayout.Button("Create New Map", GUILayout.ExpandWidth(true)))
			{
				CreateMap();
				editMode = true;
			}

			GUILayout.Space(12);

			GUILayout.BeginHorizontal("box");

			mapContent = (TextAsset)EditorGUILayout.ObjectField(mapContent, typeof(TextAsset), GUILayout.ExpandWidth(true));

			GUI.enabled = mapContent ? true : false;

			if (GUILayout.Button("Edit Map", GUILayout.Width(150)))
			{
				// here load map to edit
				LoadMap(mapContent.text);
				editMode = true;
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

		}
		else
		{
			DrawProperties();

			switch (Event.current.type)
			{
				case EventType.KeyDown:
					if (Event.current.keyCode == KeyCode.D)
					{
						selectedOption = 1;
					}
					if (Event.current.keyCode == KeyCode.E)
					{
						selectedOption = 2;

					}
					Event.current.Use();
					Repaint();
					break;
			}

			DrawTitles();
		}
		
	}


	int rotationOffSet = 45;
	void DrawProperties() 
	{
		GUILayout.BeginVertical("BOX");

		mapSize = EditorGUILayout.Vector2IntField("Map Size", mapSize, GUILayout.ExpandWidth(true));

		//GUILayout.Space(6);
		//GUILayout.Label("Options");
		//selectedOption = GUILayout.SelectionGrid(selectedOption, options, 3, GUILayout.ExpandWidth(true));
		GUILayout.Space(24);


		GUILayout.BeginHorizontal("box");
		rotationOffSet = EditorGUILayout.IntField("Ratation Angle", rotationOffSet);
		if (lastSpawnedObject != null)
		{
			//GUILayout.Label("Rotate", GUILayout.Width(75));
			if (GUILayout.Button("<"))
			{
				lastSpawnedObject.Rotate(Vector3.up, -rotationOffSet);
			}
			if (GUILayout.Button(">"))
			{
				lastSpawnedObject.Rotate(Vector3.up, rotationOffSet);
			}
		}
		GUILayout.EndHorizontal();


		GUILayout.BeginHorizontal("box");

		GUILayout.Label("Map Name", GUILayout.Width(75));
		mapName = EditorGUILayout.TextField(mapName, GUILayout.ExpandWidth(true));
		GUI.enabled = mapName == "" ? false : true;
		if (GUILayout.Button("Save Map", GUILayout.Width(150)))
		{
			_map.Width = mapSize.x;
			_map.Height = mapSize.y;
			_map.SaveMap(mapName);
			AssetDatabase.Refresh();
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();

		GUILayout.Space(12);

		GUILayout.EndVertical();
	}
	Vector2 scrollPos;
	void DrawTitles() 
	{
		EditorGUILayout.BeginVertical("box");
		List<GUIContent> paletteIcons = new List<GUIContent>();

		for (int i = 0; i < tiles.Length; i++)
		{
			Texture2D preview = AssetPreview.GetAssetPreview(tiles[i].gameObject as Object);
			paletteIcons.Add(new GUIContent(preview));
		}

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

		paletteIndex = GUILayout.SelectionGrid(paletteIndex, paletteIcons.ToArray(), TILES_IN_ROW);

		EditorGUILayout.EndScrollView();

		EditorGUILayout.EndVertical();
	}

	private void CreateMap() 
	{
		var map = GameObject.FindObjectOfType<Map>();

		if (map == null)
		{
			var newMap = new GameObject("__MAP__");
			_map = newMap.AddComponent<Map>();
		}
		else
		{
			_map = map;
		}
	}

	private void LoadMap(string data)
	{
		var map = data.DeserializeFromXml<MapModel>();
		mapSize.x = map.Width;
		mapSize.y = map.Height;

		if (map == null)
		{
			Debug.LogError("Unable to parse Map data");
		}
		else
		{
			var newMap = new GameObject("__MAP__");
			_map = newMap.AddComponent<Map>();
			_map.MapModel = map;
			for (int i = 0; i < map.Data.Count; i++)
			{
				// TODO: load tile based on data type

				if (map.Data[i].Type != BlockType.None)
				{
					LoadBlock(map.Data[i], GetTileById(map.Data[i].Type));
				}
			}
			
		}
	}

	private void LoadBlock(BlockModel model, GameObject tile = null)
	{
		//var block = _map.GetBlock((int)model.X, (int)model.Y);
		//if (block != null)
		//{
		//	return;
		//}
		GameObject gameObject = tile;
		if (tile == null)
		{
			GameObject prefab = tiles[paletteIndex].gameObject;
			gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		}
		gameObject.transform.position = new Vector3(model.X, 0, model.Y);
		gameObject.transform.SetParent(_map.transform);
		gameObject.transform.rotation = Quaternion.Euler(0,model.RotY,0);
		_map.AddBlock(gameObject.GetComponent<MapBlock>());
		lastSpawnedObject = gameObject.transform;
		// Allow the use of Undo (Ctrl+Z, Ctrl+Y).
		//Undo.RegisterCreatedObjectUndo(gameObject, "");
	}

	#endregion

	#region SCENE_GUI

	/// <summary>
	/// Draw or do input/handle overriding in the scene view
	/// </summary>
	void OnSceneGUI(SceneView sceneView)
	{
		if (!editMode) return;

		Vector3 mouseWorldPosition = GetMousePosInSceneView(sceneView);
		DrawGrid();
		HandleSceneViewInputs(mouseWorldPosition);
		Handles.color = Color.green;
		Handles.DrawWireCube(new Vector3(Mathf.CeilToInt(mouseWorldPosition.x - 0.5f), 0, Mathf.CeilToInt(mouseWorldPosition.z - 0.5f)), Vector3.one);
	}

	void DrawGrid()
	{
		for (int i = 0; i <= mapSize.y; i++)
		{
			Handles.DrawLine(new Vector3(-.5f, 0, i - 0.5f), new Vector3(mapSize.x - 0.5f, 0, i - 0.5f));
			for (int j = 0; j <= mapSize.x; j++)
			{
				Handles.DrawLine(new Vector3(j - 0.5f, 0, -.5f), new Vector3(j - 0.5f, 0, mapSize.y - 0.5f));
			}
		}
	}

	private void HandleSceneViewInputs(Vector3 tileCenter)
	{

		if (tileCenter.x + 0.5f < 0 || tileCenter.x + 0.5f > mapSize.x || tileCenter.z + 0.5f < 0 || tileCenter.z + 0.5f > mapSize.y) return;
		// Filter the left click so that we can't select objects in the scene
		if (Event.current.type == EventType.Layout)
		{
			HandleUtility.AddDefaultControl(0); // Consume the event
		}
		
		//if (selectedOption == 1)
		//{
		//	if (paletteIndex < tiles.Length && Event.current.type == EventType.MouseDown && Event.current.button == 0)
		//	{
		//		PaintTile(tileCenter);
		//	}
		//}
		//else if (selectedOption == 2)
		//{
		//	// single tile draw
		//	if (paletteIndex < tiles.Length && Event.current.type == EventType.MouseDown && Event.current.button == 1)
		//	{
		//		EraseTile(tileCenter);
		//	}
		//}


		if (paletteIndex < tiles.Length && Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			PaintTile(tileCenter);
		}

		// single tile draw
		if (paletteIndex < tiles.Length && Event.current.type == EventType.MouseDown && Event.current.button == 1)
		{
			EraseTile(tileCenter);
		}

		// batch draw
		if (paletteIndex < tiles.Length && Event.current.type == EventType.MouseMove && Event.current.button == 0 && Event.current.shift)
		{
			PaintTile(tileCenter);
		}

		// batch erase
		if (paletteIndex < tiles.Length && Event.current.type == EventType.MouseMove && Event.current.button == 0 && Event.current.control)
		{
			EraseTile(tileCenter);
		}

	}

	private GameObject GetTileById(BlockType type) 
	{
		var tileObj = tiles.FirstOrDefault((tile) => tile.block == type);
		return tileObj == null ? null : PrefabUtility.InstantiatePrefab(tileObj.gameObject) as GameObject;
	}

	private void EraseTile(Vector3 tileCenter)
	{
		int x = Mathf.CeilToInt(tileCenter.x - 0.5f);
		int z = Mathf.CeilToInt(tileCenter.z - 0.5f);

		var block = _map.GetBlock(x, z);

		if (block != null)
		{
			//Undo.RegisterCreatedObjectUndo(block.gameObject, "");
			_map.RemoveBlock(block);
			DestroyImmediate(block.gameObject);
		}
	}
	private void PaintTile(Vector3 tileCenter, GameObject tile = null) 
	{
		int x = Mathf.CeilToInt(tileCenter.x - 0.5f);
		int z = Mathf.CeilToInt(tileCenter.z - 0.5f);

		var block = _map.GetBlock(x, z);
		if (block != null)
		{
			lastSpawnedObject = block.transform; ;
			return;
		}
		GameObject gameObject = tile;
		if (tile == null)
		{
			GameObject prefab = tiles[paletteIndex].gameObject;
			gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		}
		gameObject.transform.position = new Vector3(x, 0, z);
		gameObject.transform.SetParent(_map.transform);
		_map.AddBlock(gameObject.GetComponent<MapBlock>());
		lastSpawnedObject = gameObject.transform;
		// Allow the use of Undo (Ctrl+Z, Ctrl+Y).
		//Undo.RegisterCreatedObjectUndo(gameObject, "");
	}

	private Vector3 GetMousePosInSceneView(SceneView sceneView)
	{
		Vector3 mousePosition = Event.current.mousePosition;
		Camera sceneCamera = sceneView.camera;

		mousePosition.y = sceneCamera.pixelHeight - mousePosition.y;
		mousePosition = sceneCamera.ScreenToWorldPoint(mousePosition);
		mousePosition.y = -mousePosition.y;

		return mousePosition;
	}
	#endregion



	/// <summary>
	/// When user attempts to select an object, this sees if they selected an
	/// object with the given component. This will swallow the event and select
	/// the object if successful.
	/// </summary>
	/// <param name="e">Event from OnSceneGUI</param>
	/// <typeparam name="T">Component type</typeparam>
	/// <returns>Returns the object</returns>
	public static GameObject Select<T>(Event e) where T : UnityEngine.Component
	{
		Camera cam = Camera.current;

		if (cam != null)
		{
			RaycastHit hit;
			Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

			if (Physics.Raycast(ray, out hit))
			{
				if (hit.collider != null)
				{
					GameObject gameObj = hit.collider.gameObject;
					if (gameObj.GetComponent<T>() != null)
					{
						e.Use();
						UnityEditor.Selection.activeGameObject = gameObj;
						return gameObj;
					}
				}
			}
		}

		return null;
	}
}