namespace MVVM.Binders
{
    public class OnDestroyMethodBinder : EmptyMethodBinder
    {
        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            Perform();
        }
    }
}