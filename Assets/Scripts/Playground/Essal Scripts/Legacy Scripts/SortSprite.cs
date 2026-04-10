using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Playground.GameScripts
{
    class SortSprite : MonoBehaviour
    {
        private SpriteRenderer sr;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        void LateUpdate()
        {
            sr.sortingOrder = -(int)(transform.position.y * 100);
        }
    }
}
