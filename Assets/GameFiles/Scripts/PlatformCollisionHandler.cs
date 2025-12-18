using UnityEngine;
using static Extensions.ColliderEX;

public class PlatformCollisionHandler : MonoBehaviour
{
    Transform platform;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.TagIs("MovingPlatform"))
        {
            ContactPoint contact = collision.GetContact(0);

            //Common contact values
            // Flat Floor       (0,   1,    0)
            // Gentle Slope     (0.3, 0.95, 0)
            // Steep Slope      (0.7, 0.7   0)
            // Vertical Wall    (1,   0,    0)
            // Ceiling          (0,  -1,    0)
            if (contact.normal.y < 0.5f) return; //0.5 ~= 60 degree angle cutoff

            platform = collision.transform.parent;
            transform.SetParent(platform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.TagIs("MovingPlatform"))
        {
            transform.SetParent(null);
            platform = null;
        }
    }
}
