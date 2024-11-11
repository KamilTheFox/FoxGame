using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerDescription
{
    public class CharacterData : MonoBehaviour
    {
        public IDiesing GetDie { get; private set; }

        public IDamaged GetDamaged { get; private set; }

        public IExplosionDamaged GetExplodionDamaged { get; private set; }

        public CharacterCapsuleData GetCapsuleData { get; private set; }

        
        public IInputCaracter IntroducingCharacter
        {
            get
            {
                return inputCaracter;
            }
            set
            {
                if (inputCaracter != null)
                    inputCaracter.Disable();
                IInputCaracter current = value;
                if (current == null)
                {
                    if (inputPreviousCaracters.TryPeek(out IInputCaracter previous))
                    {
                        inputCaracter = previous;
                        goto goEnd;
                    }
                }
                inputCaracter = value;
goEnd:
                if (inputCaracter != null)
                    inputCaracter.Enable();
                if (current == null) return;
                if (!current.GetType().Name.ToLower().Contains("Default".ToLower()))
                    inputPreviousCaracters.Push(value);
            }
        }

        private IInputCaracter inputCaracter;

        private Stack<IInputCaracter> inputPreviousCaracters = new Stack<IInputCaracter>();

        

        private void OnEnable()
        {
            if (IntroducingCharacter != null) IntroducingCharacter.Enable();
        }
        private void OnDisable()
        {
            if (IntroducingCharacter != null) IntroducingCharacter.Disable();
        }


    }
}
