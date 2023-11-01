using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AnnaMotionTest
{
    private Camera mainCamera;
    private AnnaAnimatorTest annaAnim;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/TestCamera"), new Vector3(0, 2.5f, 5f), Quaternion.Euler(20f, 180f, 0));
        annaAnim = Object.Instantiate(Resources.Load<AnnaAnimatorTest>("Prefabs/Character/AnnaAnimatorTest"));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(annaAnim.gameObject);
        Object.Destroy(mainCamera.gameObject);
    }

    [UnityTest]
    /// <summary>
    /// Transitions from 2DFreeformDirectional start immediately after transit condition is satisfied <br />
    /// without control parameters become zero.
    /// </summary>
    public IEnumerator _001_CanTransitFrom2DFreeformDirectionalBySpeedCommandFrag()
    {
        var speed = annaAnim.speed;
        var spdCmd = annaAnim.speedCommand;

        yield return new WaitForSeconds(1f);

        annaAnim.speed.Float = 4f;
        spdCmd.Bool = true;

        yield return new WaitForSeconds(1f);

        // Current state is Move
        Assert.True(annaAnim.IsCurrentState("Move"));

        // Set the transition flag.
        spdCmd.Bool = false;

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        annaAnim.speed.Float = 0f;

        // Transition Duration is 0.05 so this transition will progress over half.
        yield return new WaitForSeconds(0.03f);

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Idle"));

        // Transition Duration is 0.05 so this transition will have finished.
        yield return new WaitForSeconds(0.02f);

        Assert.False(annaAnim.IsCurrentState("Move"));
        Assert.True(annaAnim.IsCurrentState("Idle"));

        annaAnim.speed.Float = 4f;
        spdCmd.Bool = true;

        yield return new WaitForSeconds(1f);

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Idle"));

        // Set the transition flag.
        spdCmd.Bool = false;
        annaAnim.attack.Fire();

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        annaAnim.speed.Float = 0f;

        // Transition Duration to Idle is 0.05 so this transition will progress over half.
        yield return new WaitForSeconds(0.03f);

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Idle"));
        Assert.False(annaAnim.IsCurrentState("Attack"));

        // Transition Duration to Idle is 0.05 but switched target to Attack and still Moving.
        yield return new WaitForSeconds(0.02f);

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Idle"));
        Assert.False(annaAnim.IsCurrentState("Attack"));

        // Transition Duration to Attack is 0.1 so this transition will have finished.
        yield return new WaitForSeconds(0.05f);
        Assert.False(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Idle"));
        Assert.True(annaAnim.IsCurrentState("Attack"));

        yield return new WaitForSeconds(1f);
    }
    [UnityTest]
    /// <summary>
    /// </summary>
    public IEnumerator _002_RunAndGuard()
    {
        yield return new WaitForSeconds(1f);

        annaAnim.StartMoving();

        yield return new WaitForSeconds(0.58f);

        annaAnim.guard.Bool = annaAnim.fighting.Bool = true;

        yield return null;

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Guard"));
        Assert.False(annaAnim.IsCurrentState("Idle"));

        yield return null;

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Guard"));
        Assert.False(annaAnim.IsCurrentState("Idle"));

        annaAnim.EndMoving();

        yield return null;

        // Transition Move -> Exit starts.
        Assert.True(annaAnim.IsCurrentTransition("Move", "Exit"));

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Guard"));
        Assert.False(annaAnim.IsCurrentState("Idle"));

        yield return null;

        // The transition switched to Idle -> Guard.
        Assert.True(annaAnim.IsCurrentTransition("Idle", "Guard"));

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Guard"));
        Assert.False(annaAnim.IsCurrentState("Idle"));

        yield return new WaitForSeconds(0.2f);

        // Transition Duration is 0.2 so this transition will have finished.
        Assert.False(annaAnim.IsCurrentState("Move"));
        Assert.True(annaAnim.IsCurrentState("Guard"));
        Assert.False(annaAnim.IsCurrentState("Idle"));

        yield return new WaitForSeconds(0.1f);

        annaAnim.guard.Bool = annaAnim.fighting.Bool = false;

        yield return new WaitForSeconds(0.2f);

        annaAnim.StartMoving();

        yield return new WaitForSeconds(0.1f);

        annaAnim.guard.Bool = annaAnim.fighting.Bool = true;

        yield return new WaitForSeconds(0.5f);

        annaAnim.EndMoving();

        yield return null;

        // Transition Move -> Exit starts.
        Assert.True(annaAnim.IsCurrentTransition("Move", "Exit"));
        Assert.True(annaAnim.IsCurrentState("Move"));

        yield return null;

        // The transition switched to Idle -> Guard.
        Assert.True(annaAnim.IsCurrentTransition("Idle", "Guard"));
        Assert.True(annaAnim.IsCurrentState("Move"));

        annaAnim.attack.Fire();

        yield return null;
        yield return null;

        // The transition switched to Guard -> Attack.
        Assert.True(annaAnim.IsCurrentTransition("Guard", "Attack"));

        yield return null;

        // The transition switched back to Move -> Exit.
        Assert.True(annaAnim.IsCurrentTransition("Guard", "Attack"));
        // Assert.True(annaAnim.IsCurrentTransition("Move", "Exit"));
        // Assert.True(annaAnim.IsCurrentState("Move"));

        yield return new WaitForSeconds(0.2f);

        // Transition Duration is 0.2 so this transition will have finished.
        Assert.True(annaAnim.IsCurrentState("Attack"));

        yield return null;

        annaAnim.guard.Bool = annaAnim.fighting.Bool = false;

        yield return new WaitForSeconds(1f);
    }
}