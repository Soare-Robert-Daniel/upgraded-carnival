using System;
using UnityEngine;

namespace Map.Room
{
    public enum SymbolStateV
    {
        None,
        Top,
        Bottom,
        TopAndBottom
    }

    public enum SymbolStateH
    {
        None,
        Left,
        Right,
        RightAndLeft
    }

    public class SymbolController : MonoBehaviour
    {
        [Header("Positions")]
        [SerializeField] private SpriteRenderer center;

        [SerializeField] private SpriteRenderer top;
        [SerializeField] private SpriteRenderer bottom;
        [SerializeField] private SpriteRenderer right;
        [SerializeField] private SpriteRenderer left;

        [Header("Resources")]
        [SerializeField] private Sprite rect;

        [SerializeField] private Sprite lineV;
        [SerializeField] private Sprite lineH;

        public void UpdateVerticalSymbols(SymbolStateV symbolStateV)
        {
            switch (symbolStateV)
            {
                case SymbolStateV.Top:
                    top.sprite = rect;
                    bottom.sprite = lineH;
                    break;
                case SymbolStateV.Bottom:
                    top.sprite = lineH;
                    bottom.sprite = rect;
                    break;
                case SymbolStateV.TopAndBottom:
                    top.sprite = rect;
                    bottom.sprite = rect;
                    break;
                case SymbolStateV.None:
                    top.sprite = lineH;
                    bottom.sprite = lineH;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(symbolStateV), symbolStateV, null);
            }
        }

        public void UpdateHorizontalSymbols(SymbolStateH symbolStateH)
        {
            switch (symbolStateH)
            {
                case SymbolStateH.Left:
                    left.sprite = rect;
                    right.sprite = lineV;
                    break;
                case SymbolStateH.Right:
                    left.sprite = lineV;
                    right.sprite = rect;
                    break;
                case SymbolStateH.RightAndLeft:
                    left.sprite = rect;
                    right.sprite = rect;
                    break;
                case SymbolStateH.None:
                    left.sprite = lineV;
                    right.sprite = lineV;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(symbolStateH), symbolStateH, null);
            }
        }
    }
}