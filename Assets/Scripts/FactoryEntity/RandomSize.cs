using UnityEngine;

namespace FactoryEntity
{
    internal struct RandomSize
    {
        public bool IsRandom;
        /// <summary>
        /// Исключение рандома
        /// </summary>
        public RandomSize(float DefaultSize)
        {
            IsRandom = false;
            min = 0;
            max = DefaultSize;
        }
        /// <summary>
        /// Рандом в пределах чисел
        /// </summary>
        public RandomSize(float _min, float _max)
        {
            IsRandom = true;
            min = _min;
            max = _max;
        }
        public float GetValue()
        {
            float value = UnityEngine.Random.Range(min, max);
            return IsRandom ? value : max;
        }
        public Vector3 GetVector3()
        {
            return GetValue() * Vector3.one;
        }

        public float min, max;
    }
}
