using UnityEngine;
using UnityEngine.UI;

namespace Bigjam.Developer
{
    public interface IDeveloper {
        Image[] Head{get;}
        Image Body{get;}
        Image RightHand{get;}
        Image LeftHand{get;}
        GameObject All{get;}
    }
}