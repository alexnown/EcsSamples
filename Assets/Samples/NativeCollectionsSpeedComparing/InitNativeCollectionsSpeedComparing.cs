using Unity.Entities;
using UnityEngine;

public class InitNativeCollectionsSpeedComparing : MonoBehaviour
{
    public static int Iterations;
    public static int ElementsCount;

    public int _iterations = 100;
    public int _elementsCount = 1000000;

    void Start()
    {
        Iterations = _iterations;
        ElementsCount = _elementsCount;
        World.DisposeAllWorlds();
        var testWorld = new World("NativeCollectionsSpeed");
        testWorld.CreateManager<NativeArrayJobSystem>();
        testWorld.CreateManager<NativeIntArrayJobSystem>();
        World.Active = testWorld;
        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
    }
}
