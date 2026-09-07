using UnityEngine;

public enum ObjectCategory
{
<<<<<<< HEAD
    StaticEnvironment,
    PushableBox
}

public abstract class GridObject : MonoBehaviour
{
    public abstract ObjectCategory category { get; }
}
=======
    public CellKind kind;
    private void Awake() { } //build de hata sinifin uyanmamasidir belki diye test için ekledim
}
>>>>>>> 4cd7b438d0bf79862be4770712e96635ef1d4ba3
