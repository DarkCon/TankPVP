using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.UI;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[System.Serializable]
public struct UIGameOverComponent : IComponent {
    public GameObject winnerPnl;
    public GameObject gameOverPnl;
    public Image imgWinner;
    public Text txtWinnerName;
    public Button btnNext;
}