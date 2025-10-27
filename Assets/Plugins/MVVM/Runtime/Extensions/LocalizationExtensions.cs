using System;
using R3;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace MVVM
{
    public static class LocalizationExtensions
    {
        public static Observable<Locale> ObserveSelectedLocaleChanged(this LocalizationSettings settings)
        {
            return Observable.FromEvent<Action<Locale>, Locale>(
                handler => handler,
                h => settings.OnSelectedLocaleChanged += h,
                h => settings.OnSelectedLocaleChanged -= h
            );
        }
    }
}