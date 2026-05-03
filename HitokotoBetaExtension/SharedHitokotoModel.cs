using System;

namespace HitokotoBetaExtension
{
    public static class SharedHitokotoModel
    {
        private static HitokotoItem? _current;

        public static HitokotoItem? Current
        {
            get => _current;
            set
            {
                if (_current != value)
                {
                    _current = value;
                    OnCurrentChanged?.Invoke();
                }
            }
        }

        public static event Action? OnCurrentChanged;
    }
}