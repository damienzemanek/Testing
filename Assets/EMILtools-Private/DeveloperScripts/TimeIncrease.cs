using UnityEngine;

public class TimeIncrease : MonoBehaviour
{
    float _speed = 1f;
    public float speed
    {
        get => _speed;
        set => _speed = Mathf.Clamp(value, 1, 10);
    }


    public void Incr() => speed += 1f;
    public void Decr() => speed -= 1f;

    private void Update()
    {
        UpdateTimeScale();
    }

    void UpdateTimeScale()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus)) Incr();
        if (Input.GetKey(KeyCode.KeypadMinus)) Decr();
        Time.timeScale = _speed;
    }
}
