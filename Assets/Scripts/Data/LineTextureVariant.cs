using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "LineTextureVariant", menuName = "Data/LineTextureVariant")]
public class LineTextureVariant : ScriptableObject
{
    [SerializeField]
    private Sprite[] lineTextures = new Sprite[6];
    public Sprite[] LineTextures { get { return lineTextures; } private set { lineTextures = value; } }

    public Sprite GetLineSprite(int lineIndex)
    {
        return lineTextures[lineIndex];
    }
    public Texture GetLineTexture(int lineIndex)
    {
        return lineTextures[lineIndex].texture;
    }
}
