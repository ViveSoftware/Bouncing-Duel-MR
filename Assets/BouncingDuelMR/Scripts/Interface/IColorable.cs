using System.Collections.Generic;
using UnityEngine;

public interface IColorable
{
    List<Color32> Colors { get; set; }
    void SetColor(Color32 color);
}
