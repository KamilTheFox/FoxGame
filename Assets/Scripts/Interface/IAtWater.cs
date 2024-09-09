using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAtWater
{
    public void EnterWater();
    public void ExitWater();

    public float VolumeObject { get; }

    public bool isSwim { get; }

}
