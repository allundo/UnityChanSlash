public class BulletStatus : MobStatus
{
    public override float CalcAttack(float attack, IDirection attackDir) => attack;
}
