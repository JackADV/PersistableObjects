using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : PersistableObject
{
    [SerializeField] Slider creationSpeedSlider;
    [SerializeField] Slider destructionSpeedSlider;
    [SerializeField] bool reseedOnLoad;
    [SerializeField] ShapeFactory shapeFactory; // The object we will be spawning
    public KeyCode createKey = KeyCode.C; // Giving keys functionality
    public KeyCode newGameKey = KeyCode.N; // Giving keys functionality
    public KeyCode saveKey = KeyCode.S; // Giving keys functionality
    public KeyCode loadkey = KeyCode.L; // Giving keys functionality
    public KeyCode destroyKey = KeyCode.X; // Giving keys functionality
    public PersistantStorage storage; // Where the data is saving and loading form
    const int saveVersion = 4;
    public List<Shape> shapes; // The list of objects with the Persistableobjects script
    private string savePath; // Telling the game where to save
    public PersistableObject prefab;
    List<PersistableObject> objects;
    float creationProgress, destructionProgress;
    public int levelCount;
    int loadedLevelBuildIndex;
    public static Game Instance { get; private set; }

    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }
    Random.State mainRandomState;
    //public PersistantStorage storage;
    void Start()
    {
        objects = new List<PersistableObject>();
        if (Application.isEditor)
        {
            //Scene loadedLevel = SceneManager.GetSceneByName("Level 1");
            //if (loadedLevel.isLoaded)
            //{
            //    SceneManager.SetActiveScene(loadedLevel);
            //    return;
            //}
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.name.Contains("Level"))
                {
                    SceneManager.SetActiveScene(loadedScene);
                    loadedLevelBuildIndex = loadedScene.buildIndex;
                    return;
                }
            }
        }

        mainRandomState = Random.state;
        BeginNewGame();
        StartCoroutine(LoadLevel(1));
    }
    IEnumerator LoadLevel(int levelBuildIndex)
    {
        enabled = false;
        if (loadedLevelBuildIndex > 0)
        {
            yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
        }
        yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
        loadedLevelBuildIndex = levelBuildIndex;
        enabled = true;
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
            StartCoroutine(LoadLevel(loadedLevelBuildIndex));
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
        else
        {
            for (int i = 0; i < levelCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    BeginNewGame();
                    StartCoroutine(LoadLevel(i));
                    return;
                }
            }
        }
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            shapes[i].GameUpdate();
        }
        creationProgress += Time.deltaTime * CreationSpeed; // Allows you to speed up the creation process
        while (creationProgress >= 1f)
        {
            creationProgress -= 1f;
            CreateShape();
            destructionProgress += Time.deltaTime * DestructionSpeed;
            while (destructionProgress >= 1f)
            {
                destructionProgress -= 1f;
                DestroyShape();
            }

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
        // t.localPosition = Random.insideUnitSphere * 5f;
        GameLevel.Current.ConfigureSpawn(instance);
        shapes.Add(instance);
    }
    private void BeginNewGame()
    {
        Random.state = mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
        mainRandomState = Random.state;
        Random.InitState(seed);
        creationSpeedSlider.value = CreationSpeed = 0;
        destructionSpeedSlider.value = DestructionSpeed = 0;
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
        writer.Write(Random.state);
        writer.Write(CreationSpeed);
        writer.Write(creationProgress);
        writer.Write(DestructionSpeed);
        writer.Write(destructionProgress);
        writer.Write(loadedLevelBuildIndex);
        GameLevel.Current.Save(writer);
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
        StartCoroutine(LoadLevel(version < 2 ? 1 : reader.ReadInt()));
        if (version > saveVersion)
        {
            Debug.Log("Unsupported future save version " + version);
            return;
        }
        StartCoroutine(LoadGame(reader));
    }
    IEnumerator LoadGame(GameDataReader reader)
    {
        int version = reader.Version;
        int count = version <= 0 ? -version : reader.ReadInt();
        yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
        if (version >= 3)
        {
            GameLevel.Current.Load(reader);
            Random.State state = reader.ReadRandomState();
            if (!reseedOnLoad)
            {
                Random.state = state;
            }
            creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
            creationProgress = reader.ReadFloat();
            destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
            destructionProgress = reader.ReadFloat();
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
