using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlowMotionBar : MonoBehaviour
{
    public Slider slider;
    
    public void SetMaxSlowMotion(int slowMotion)
    {
        slider.maxValue = slowMotion;
        slider.value = slowMotion;
    }
    
    public void SetSlowMotion(int slowMotion)
    {
        slider.value = slowMotion;
    }
}
