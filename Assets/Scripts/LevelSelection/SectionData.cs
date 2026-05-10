using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SectionData", menuName = "Scriptable Objects/SectionData")]
public class SectionData : ScriptableObject
{
    public List<LevelNodeData> levels;
}