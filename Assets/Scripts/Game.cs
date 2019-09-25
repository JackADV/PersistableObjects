using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : PersistableObject
{
    public PersistableObject prefab; // The object we will be spawning
    public KeyCode createKey = KeyCode.C; // Giving keys functionality
    public KeyCode newGameKey = KeyCode.N; // Giving keys functionality
    public KeyCode saveKey = KeyCode.S; // Giving keys functionality
    public KeyCode loadkey = KeyCode.L; // Giving keys functionality
    public PersistantStorage storage; // Where the data is saving and loading form

    private List<PersistableObject> objects; // The loist of objects with the Persistableobjects script
    private string savePath; // Telling the game where to save
    void Awake()
    {
        objects = new List<PersistableObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(createKey))
        {
            CreateObject(); // Spawns cubes
        }
        else if (Input.GetKey(newGameKey))
        {
            BeginNewGame(); // Clears cubes
        }
        else if (Input.GetKeyDown(saveKey))
        {
            // Saves game
            storage.Save(this);
        }
        else if (Input.GetKeyDown(loadkey))
        {
            BeginNewGame();
            storage.Load(this);
             // Loads game
        }
    }
    
    

    private void CreateObject() // This function will spawn the prefab at a random rotation and within a sphere
    {
        PersistableObject o = Instantiate(prefab);
        o.transform.localPosition = Random.insideUnitSphere * 5f;
        o.transform.localRotation = Random.rotation;
        o.transform.localScale = Random.Range(0.1f, 1f) * Vector3.one;

        objects.Add(o); // Record new object to list
    }
    private void BeginNewGame()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            // Destroys each item
            Destroy(objects[i].gameObject);
        }
        // Clears the list
        objects.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(objects.Count);
        // Loop through all objects
        for (int i = 0; i < objects.Count; i++)
        {
            // Save each
            objects[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader) // Loads the game from where it was saved last and spawns the objects where they were before
    {
        int count = reader.Readint();
        for (int i = 0; i < count; i++)
        {
            PersistableObject o = Instantiate(prefab);
            o.Load(reader);
            objects.Add(o);
        }
    }
}
