using UnityEngine;

[System.Serializable]
public class SceneTransitionData 
{
    public SceneType sceneName;
    public Vector2 fadeInDirection; //Direction to fade in from
    public Vector2 fadeOutDirection; //Direction to fade out to
}