using System;
using UnityEngine;

namespace UI
{
    [Serializable]
    public class UIState
    {
        [SerializeField] private bool isOverHUD;
        [SerializeField] private bool lockSelection;

        public bool CanSelect => !isOverHUD && !lockSelection;

        public void ActivateSelecting()
        {
            lockSelection = false;
        }

        public void DeactivateSelecting()
        {
            lockSelection = true;
        }

        public void StopSelectionOverHud()
        {
            isOverHUD = true;
        }

        public void StartSelectionOutHUD()
        {
            isOverHUD = false;
        }
    }
}