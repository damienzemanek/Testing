using EMILtools.Core;
using EMILtools.Signals;
using UnityEngine;
using static EMILtools.Signals.StatTags;

public static class StatEX 
{
   public static T Set<T, TTag>(this ReactiveIntercept<Stat<T, TTag>> ri)
      where T : struct
      where TTag :  struct, IStatTag
   {
      
   }
}
