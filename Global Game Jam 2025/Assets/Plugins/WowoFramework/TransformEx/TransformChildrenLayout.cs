using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace WowoFramework.TransformEx
{
    public class TransformChildrenLayout : MonoBehaviour
    {
        [SerializeField] TransformChildrenLayoutMode m_layoutMode = TransformChildrenLayoutMode.Horizontal;

        [ShowIf(nameof(m_layoutMode), TransformChildrenLayoutMode.Horizontal), SerializeField, LabelText("间距")]
        private float m_spacing = 0;

        [ShowIf(nameof(m_layoutMode), TransformChildrenLayoutMode.Horizontal), SerializeField, LabelText("对齐")]
        private TransformChildHorizonAlignment _alignment = TransformChildHorizonAlignment.Center;

        [ShowIf(nameof(m_layoutMode), TransformChildrenLayoutMode.Horizontal), SerializeField, LabelText("是否缓动")]
        private bool m_isEase = false;

        [ShowIf(nameof(m_layoutMode), TransformChildrenLayoutMode.Horizontal), ShowIf(nameof(m_isEase), true),
         SerializeField, LabelText("缓动时间")]
        private float easeTime = 0.2f;

        [ShowIf(nameof(m_layoutMode), TransformChildrenLayoutMode.Horizontal), ShowIf(nameof(m_isEase), true),
         SerializeField, LabelText("缓动类型")]
        private Ease easeType = Ease.Linear;


        int m_childCount = 0;

        private void Update()
        {
            if (m_childCount != transform.childCount)
            {
                m_childCount = transform.childCount;
                RefreshLayout();
            }
        }

        private void RefreshLayout()
        {
            foreach (Transform child in transform)
            {
                transform.DOKill();
            }            
            
            switch (m_layoutMode)
            {
                case TransformChildrenLayoutMode.Horizontal:
                    HorizontalLayout();
                    break;
                case TransformChildrenLayoutMode.Vertical:
                    VerticalLayout();
                    break;
                case TransformChildrenLayoutMode.Grid:
                    GridLayout();
                    break;
                case TransformChildrenLayoutMode.Ring:
                    RingLayout();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HorizontalLayout()
        {
            switch (_alignment)
            {
                case TransformChildHorizonAlignment.Left:
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        if (m_isEase)
                        {
                            transform.GetChild(i).DOLocalMove(new Vector3(i * m_spacing, 0, 0), easeTime).SetEase(easeType);
                        }
                        else
                        {
                            transform.GetChild(i).localPosition = new Vector3(i * m_spacing, 0, 0);
                        }
                    }

                    break;
                case TransformChildHorizonAlignment.Center:
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        if (m_isEase)
                        {
                            transform.GetChild(i)
                                .DOLocalMove(new Vector3(i * m_spacing - (transform.childCount - 1) * m_spacing / 2, 0, 0),
                                    easeTime)
                                .SetEase(easeType);
                        }
                        else
                        {
                            transform.GetChild(i).localPosition =
                                new Vector3(i * m_spacing - (transform.childCount - 1) * m_spacing / 2, 0, 0);
                        }
                    }

                    break;
                case TransformChildHorizonAlignment.Right:
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        if (m_isEase)
                        {
                            transform.GetChild(i)
                                .DOLocalMove(new Vector3(i * m_spacing - (transform.childCount - 1) * m_spacing, 0, 0),
                                    easeTime)
                                .SetEase(easeType);
                        }
                        else
                        {
                            transform.GetChild(i).localPosition =
                                new Vector3(i * m_spacing - (transform.childCount - 1) * m_spacing, 0, 0);
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void VerticalLayout()
        {
            throw new NotImplementedException();
        }

        private void GridLayout()
        {
            throw new NotImplementedException();
        }

        private void RingLayout()
        {
            throw new NotImplementedException();
        }
    }

    public enum TransformChildrenLayoutMode
    {
        Horizontal,
        Vertical,
        Grid,
        Ring,
    }

    public enum TransformChildHorizonAlignment
    {
        Left,
        Center,
        Right,
    }

    public enum TransformChildVerticalAlignment
    {
        Top,
        Center,
        Bottom,
    }
}