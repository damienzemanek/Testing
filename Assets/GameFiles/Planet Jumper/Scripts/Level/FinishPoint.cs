using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class FinishPoint : MonoBehaviour
{
   public LoadSceneConnector loadAdapater;
   void OnTriggerEnter(Collider other)
   {
      if (!other.CompareTag("Player")) return;
      Win();
   }

   [Button]
   public void Win()
   {
      loadAdapater.Load(3);
   }
}
