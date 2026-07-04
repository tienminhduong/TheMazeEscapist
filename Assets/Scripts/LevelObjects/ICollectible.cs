using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ICollectible
{
    void Collect();
    void Release(Vector3? fromPosition = null);
}