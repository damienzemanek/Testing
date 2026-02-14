using EMILtools_Private.Testing;
using UnityEngine;
using static Ledge;

public interface IAPI_Mantler : IAPI_Module
{
    public void CanMantleLedge(LedgeData data);
    public void CantMantleLedge();
}
