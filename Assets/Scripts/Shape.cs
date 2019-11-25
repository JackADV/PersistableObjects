using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public int ShapeId // Get the correct shape for when we load it isnt only cubes
    {
        get
        {
            return shapeId;
        }
        set
        {
            if (shapeId == int.MinValue && value != int.MinValue)
            {
                shapeId = value;
            }
        }
    }
    public Vector3 AngularVelocity { get; set; }
    public Vector3 Velocity { get; set; }
    public void GameUpdate()
    {
        transform.Rotate(AngularVelocity * 50f * Time.deltaTime);
        transform.localPosition += Velocity * Time.deltaTime;
    }

    Color color; // Reference to the colour
    static int colorPropertyId = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock sharedPropertyBlock;
    public void SetColor(Color color) // A function to set the colour of each shape
    {
        this.color = color;
        // GetComponent<MeshRenderer>().material.color = color; // Finding the mesh renderer on the shapes to change the colour
        // meshRenderer.material.color = color;
        if (sharedPropertyBlock == null)
        {
            sharedPropertyBlock = new MaterialPropertyBlock(); 
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        meshRenderer.SetPropertyBlock(sharedPropertyBlock);
    }

    public int MaterialId { get; private set; }
    public void SetMaterial(Material material, int materialId)
    {
        // This function assings a material from the array onto the shapes
        // GetComponent<MeshRenderer>().material = material;
        meshRenderer.material = material;
        MaterialId = materialId;
    }
    int shapeId = int.MinValue;

    public override void Save(GameDataWriter writer) // Saves the colour
    {
        base.Save(writer);
        writer.Write(color);
        writer.Write(AngularVelocity);
        writer.Write(Velocity);
    }

    public override void Load(GameDataReader reader) // Loads the colour
    {
        base.Load(reader);
        SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
    }
}
