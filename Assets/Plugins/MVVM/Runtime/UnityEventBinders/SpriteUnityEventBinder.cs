using UnityEngine;

namespace MVVM.Binders
{
    public class SpriteUnityEventBinder : UnityEventBinder<Sprite>
    {
        [SerializeField] private bool _disableIfNull;

        protected override void OnStart()
        {
            base.OnStart();
            _event.AddListener(OnEvent);
        }

        private void OnEvent(Sprite newValue)
        {
            if(_disableIfNull) gameObject.SetActive(newValue != null);
        }
    }
}
