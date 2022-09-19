using UnityEngine;

public class AttackInputControllerTest : AttackInputController
{
    public void SetDummyCamera(Camera cam) => (enemyTarget as TargetTest).dummyCamera = cam;
}