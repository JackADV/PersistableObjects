using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : PersistableObject
{
    public ShapeFactory shapeFactory; // The object we will be spawning
    public KeyCode createKey = KeyCode.C; // Giving keys functionality
    public KeyCode newGameKey = KeyCode.N; // Giving keys functionality
    public KeyCode saveKey = KeyCode.S; // Giving keys functionality
    public KeyCode loadkey = KeyCode.L; // Giving keys functionality
    public KeyCode destroyKey = KeyCode.X; // Giving keys functionality
    public PersistantStorage storage; // Where the data is saving and loading form
    const int saveVersion = 1;
    public List<Shape> shapes; // The list of objects with the Persistableobjects script
    private string savePath; // Telling the game where to save
    public PersistableObject prefab;
    List<PersistableObject> objects;
    float creationProgress, destructionProgress;

    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }
    //public PersistantStorage storage;
    void Awake()
    {
        objects = new List<PersistableObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(createKey))
        {
            CreateShape(); // Spawns cubes
        }
        else if (Input.GetKeyDown(destroyKey))
        {
            DestroyShape();
        }
        else if (Input.GetKey(newGameKey))
        {
            BeginNewGame(); // Clears cubes
        }
        else if (Input.GetKeyDown(saveKey))
        {
            // Saves game
            storage.Save(this, saveVersion);
        }
        else if (Input.GetKeyDown(loadkey))
        {
            BeginNewGame();
            storage.Load(this);
            // Loads game
        }
        creationProgress += Time.deltaTime * CreationSpeed; // Allows you to speed up the creation process
        while (creationProgress >= 1f)
        {
            creationProgress -= 1f;
            CreateShape();
        }
        destructionProgress += Time.deltaTime * DestructionSpeed;
        while (destructionProgress >= 1f)
        {
            destructionProgress -= 1f;
            DestroyShape();
        }
    }
    //void CreateObject() // Function to create the shapes
    //{
    //    PersistableObject o = Instantiate(prefab);
    //    Transform t = o.transform;
    //    objects.Add(o);
    //}

    void DestroyShape() // Function to destroy the shapes as long as their is more than 0
    {
        if (shapes.Count > 0)
        {
            int index = Random.Range(0, shapes.Count);
            //  Destroy(shapes[index].gameObject);
            shapeFactory.Reclaim(shapes[index]);
            int lastIndex = shapes.Count - 1;
            shapes[index] = shapes[lastIndex];
            shapes.RemoveAt(lastIndex);
        }

    }

    private void CreateShape() // This function will spawn the prefab at a random rotation and within a sphere
                               // And assign a random colour
    {
        Shape instance = shapeFactory.GetRandom();
        Transform t = instance.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        instance.SetColor(Random.ColorHSV(hueMin: 0f, hueMax: 1f, saturationMin: 0.5f, saturationMax: 1f, valueMin: 0.25f, valueMax: 1f, alphaMin: 1f, alphaMax: 1f));
        shapes.Add(instance);
    }
    private void BeginNewGame()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            // Destroys each item
            //  Destroy(shapes[i].gameObject);
            shapeFactory.Reclaim(shapes[i]);
        }
        // Clears the list
        shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {

        writer.Write(shapes.Count);
        // Loop through all objects
        for (int i = 0; i < shapes.Count; i++)
        {
            // Save each
            writer.Write(shapes[i].ShapeId);
            writer.Write(shapes[i].MaterialId);
            shapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader) // Loads the game from where it was saved last and spawns the objects where they were before
    {
        int version = reader.Version;
        int count = version <= 0 ? -version : reader.ReadInt();
        if (version > saveVersion)
        {
            Debug.Log("Unsupported future save version " + version);
            return;
        }
        for (int i = 0; i < count; i++) // Loads the correct shapes according to their id
        {
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
            Shape instance = shapeFactory.Get(shapeId, materialId);
            instance.Load(reader);
            shapes.Add(instance);

        }
    }
}
