using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    internal class FoodSearch : IBehavior
    {
        public string Name => nameof(FoodSearch);

        public Action<TypeAnimationAnimal> ActivateAnimation { get; set; }
        public AI AI { get; set; }

        public void Activate(AI ai)
        {
             AI = ai;
        }

        public void Deactivate()
        {
            
        }

        public void Update()
        {
            
        }
    }
