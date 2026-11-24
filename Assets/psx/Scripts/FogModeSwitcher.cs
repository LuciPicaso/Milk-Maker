using UnityEngine;

public class FogModeSwitcher : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Objects to toggle")]
    [SerializeField] private GameObject fogDome;      // твой вывернутый купол
    [SerializeField] private GameObject fogCardsRoot; // родитель для всех quad-карт

    [Header("Behavior")]
    [SerializeField] private bool insideUsesCards = true; // внутри: cards, снаружи: dome

    private int _insideCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;
        _insideCount++;

        ApplyState(isInside: true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;
        _insideCount = Mathf.Max(0, _insideCount - 1);

        if (_insideCount == 0)
            ApplyState(isInside: false);
    }

    private void ApplyState(bool isInside)
    {
        bool useCards = insideUsesCards ? isInside : !isInside;

        if (fogCardsRoot) fogCardsRoot.SetActive(useCards);
        if (fogDome) fogDome.SetActive(!useCards);
    }
}
