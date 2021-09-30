public class FadeActivate : FadeEnable
{
    protected override void Activator() => fade.Activate();
    protected override void Inactivator() => fade.Inactivate();
}
