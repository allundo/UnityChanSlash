using System.Linq;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;

public class UnityChanMotionTest
{
    // private ResourceLoader resourceLoader;
    private Camera mainCamera;
    private GameObject unityChan;
    private PlayerAnimator anim;
    private PlayerFightStyle fightStyle;
    private FightStyleHandler prefabKnuckleKnuckle;
    private RuntimeAnimatorController knuckleKnuckleController;
    private FightStyleHandler prefabSwordKnuckle;
    private RuntimeAnimatorController swordKnuckleController;
    private HandEquipment handR;

    protected class EquipTest : IEquipmentStyle
    {
        private FightStyleHandler handler;
        private RuntimeAnimatorController controller;
        public EquipTest(FightStyleHandler handler, RuntimeAnimatorController controller) { this.handler = handler; }
        public IEquipmentStyle Equip(int index, ItemIcon itemIcon) => null;
        public AttackButtonsHandler LoadAttackButtonsHandler(Transform attackInputUI) => null;
        public InputRegion LoadInputRegion(Transform fightCircle) => null;
        public FightStyleHandler LoadFightStyle(Transform player) => UnityEngine.Object.Instantiate(handler, player);
        public RuntimeAnimatorController animatorController => null;
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/MainSceneTestCamera"));

        prefabKnuckleKnuckle = Resources.Load<FightStyleHandler>("Prefabs/Character/Player/KnuckleKnuckleStyleHandler");
        knuckleKnuckleController = Object.Instantiate(Resources.Load<RuntimeAnimatorController>("AnimatorController/UnityChan_KnuckleKnuckle"));
        prefabSwordKnuckle = Resources.Load<FightStyleHandler>("Prefabs/Character/Player/SwordKnuckleStyleHandler");
        swordKnuckleController = Object.Instantiate(Resources.Load<RuntimeAnimatorController>("AnimatorController/UnityChan_SwordKnuckle"));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(mainCamera.gameObject);
        Object.Destroy(knuckleKnuckleController);
        Object.Destroy(swordKnuckleController);
    }

    [SetUp]
    public void SetUp()
    {
        unityChan = Object.Instantiate(Resources.Load<GameObject>("Prefabs/Character/Player/UnityChanFightMotionTest"));
        anim = unityChan.GetComponent<PlayerAnimator>();
        anim.lifeRatio.Float = 10f;
        fightStyle = unityChan.GetComponent<PlayerFightStyle>();
        handR = unityChan.GetComponentsInChildren<HandEquipment>()[0];
    }

    [TearDown]
    public void TearDown()
    {
        DOTween.KillAll();
        Object.Destroy(unityChan);
    }

    [UnityTest]
    public IEnumerator _001_PlayerFightMotionTest([Values("KnuckleKnuckle", "SwordKnuckle")] string type)
    {
        var handler = Object.Instantiate(Resources.Load<FightStyleHandler>($"Prefabs/Character/Player/{type}StyleHandler"), unityChan.transform);
        var controller = Object.Instantiate(Resources.Load<RuntimeAnimatorController>($"AnimatorController/UnityChan_{type}"));
        var moq = new Moq.Mock<IEquipmentStyle>();
        moq.Setup(x => x.LoadFightStyle(unityChan.transform)).Returns(handler);
        moq.Setup(x => x.animatorController).Returns(controller);

        fightStyle.SetFightStyle(moq.Object);
        anim.guard.Bool = true;
        anim.critical.Bool = false;
        anim.chargeUp.Bool = false;

        yield return null;

        for (int i = 0; i < handler.Attacks.Count(); i++)
        {
            fightStyle.Attack(i).AttackSequence(1f).Play();
            anim.Attack(i).Fire();
            yield return new WaitForSeconds(1f);
        }

        anim.critical.Bool = true;
        anim.chargeUp.Bool = true;

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < handler.Attacks.Count(); i++)
        {
            (fightStyle.Attack(i) as PlayerAttack).CriticalAttackSequence().Play();
            anim.Attack(i).Fire();
            yield return new WaitForSeconds(1f);
        }

        Object.Destroy(controller);
        yield return new WaitForSeconds(1f);
    }

    [Ignore("Test for motion visual confirmation.")]
    [UnityTest]
    public IEnumerator _002_SingleMotionTest([Values("SwordKnuckle")] string type, [Values(0, 1, 2, 3)] int attackIndex)
    {
        var handler = Object.Instantiate(Resources.Load<FightStyleHandler>($"Prefabs/Character/Player/{type}StyleHandler"), unityChan.transform);
        var controller = Object.Instantiate(Resources.Load<RuntimeAnimatorController>($"AnimatorController/UnityChan_{type}"));
        var moq = new Moq.Mock<IEquipmentStyle>();
        moq.Setup(x => x.LoadFightStyle(unityChan.transform)).Returns(handler);
        moq.Setup(x => x.animatorController).Returns(controller);

        fightStyle.SetFightStyle(moq.Object);
        anim.guard.Bool = true;

        anim.critical.Bool = true;
        anim.chargeUp.Bool = true;

        yield return null;

        for (int i = 0; i < 10; i++)
        {
            (fightStyle.Attack(attackIndex) as PlayerAttack).CriticalAttackSequence(1f).Play();
            anim.Attack(attackIndex).Fire();
            yield return new WaitForSeconds(1f);
        }

    }
}
