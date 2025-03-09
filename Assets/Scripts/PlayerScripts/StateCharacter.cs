using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerDescription
{
    [Flags]
    public enum StateCharacter : short
    {
        None = 1 << 0,
        Movement = 1 << 1,
        MovementRun = 1 << 2,
        Climbing = 1 << 3,
        Swim = 1 << 4,
        Fly = 1 << 5,
        Crouch = 1 << 6,
        Prone = 1 << 7,
        Jump = 1 << 8,
        DoubleJump = 1 << 9,
        WallRun = 1 << 10,
        Slide = 1 << 11,
        Dash = 1 << 12,
        Roll = 1 << 13,

        Default = Movement | Climbing | Swim | Jump | Crouch | Prone
    }
    public static partial class Extensions
    {
        #region StateCharacter

        /// <summary>
        /// Добавить состояние к текущему флагу
        /// </summary>
        public static StateCharacter AddState(this StateCharacter current, StateCharacter flag)
        {
            return current | flag;
        }

        /// <summary>
        /// Добавить несколько состояний к текущему флагу
        /// </summary>
        public static StateCharacter AddStates(this StateCharacter current, params StateCharacter[] flags)
        {
            foreach (var flag in flags)
                current |= flag;
            return current;
        }

        /// <summary>
        /// Удалить состояние из текущего флага
        /// </summary>
        public static StateCharacter RemoveState(this StateCharacter current, StateCharacter flag)
        {
            return current & ~flag;
        }

        /// <summary>
        /// Удалить несколько состояний из текущего флага
        /// </summary>
        public static StateCharacter RemoveStates(this StateCharacter current, params StateCharacter[] flags)
        {
            foreach (var flag in flags)
                current &= ~flag;
            return current;
        }

        /// <summary>
        /// Проверить наличие состояния в текущем флаге
        /// </summary>
        public static bool HasState(this StateCharacter current, StateCharacter flag)
        {
            return (current & flag) == flag;
        }

        /// <summary>
        /// Проверить наличие всех указанных состояний в текущем флаге
        /// </summary>
        public static bool HasStates(this StateCharacter current, params StateCharacter[] flags)
        {
            foreach (var flag in flags)
            {
                if ((current & flag) != flag)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Проверить наличие хотя бы одного из указанных состояний в текущем флаге
        /// </summary>
        public static bool HasAnyState(this StateCharacter current, params StateCharacter[] flags)
        {
            foreach (var flag in flags)
            {
                if ((current & flag) == flag)
                    return true;
            }
            return false;
        }

        #endregion
    }
}
