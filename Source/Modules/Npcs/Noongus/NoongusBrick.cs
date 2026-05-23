using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class NoongusBrick : ITM_NanaPeel
    {
        private new void Update()
        {
            if (ready || slipping || dying)
            {
                base.Update();
                return;
            }

            height -= gravity * Time.deltaTime * ec.EnvironmentTimeScale;
            entity.UpdateInternalMovement(Vector3.zero);
            if (height <= endHeight)
            {
                height = endHeight;
                ready = true;
                entity.SetGrounded(value: true);
                time = maxTime;
            }

            entity.SetHeight(height);
        }
    }
}
