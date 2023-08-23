using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Mushroom : PlantEngine, ITakenEntity
{
    public bool IsTake { get; set; }

    public ITakenEntity Take()
    {
        IsTake = true;
        return this;
    }

    public void Throw()
    {
        IsTake = false;
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
        {
            base.GetTextUI(), "\n[", 
            LText.KeyCodeE ,"] - ",LText.Take ,"/",LText.Leave
        });
    }
}
