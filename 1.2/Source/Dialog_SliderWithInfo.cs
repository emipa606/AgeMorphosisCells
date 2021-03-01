using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;


namespace AMCells
{
    public class Dialog_SliderWithInfo : Dialog_Slider
    {
        private Action<int> confirmAction;

        private int curValue;

        private const float BotAreaHeight = 30f;

        private const float TopPadding = 15f;

        public Dialog_SliderWithInfo(Func<int, string> textGetter, int from, int to, Action<int> confirmAction, int startingValue = -2147483648) : base(textGetter, from, to, confirmAction, startingValue)
        {
            this.textGetter = textGetter;
            this.@from = from;
            this.to = to;
            this.confirmAction = confirmAction;
            this.forcePause = true;
            //this.closeOnEscapeKey = true;
            
            this.closeOnClickedOutside = true;
            if (startingValue != -2147483648)
            {
                this.curValue = startingValue;
            }
            else
            {
                this.curValue = from;
            }
        }

        public Dialog_SliderWithInfo(string text, int from, int to, Action<int> confirmAction, int startingValue = -2147483648) : this((int val) => string.Format(text, val), from, to, confirmAction, startingValue)
        {
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y + 15f, inRect.width, 30f);
            Text.Font = GameFont.Small;
            
            this.curValue = (int)Widgets.HorizontalSlider(rect, (float)this.curValue, (float)this.@from, (float)this.to, true, this.textGetter(this.curValue), null, null, 1f);
            Text.Font = GameFont.Small;
            Rect rect3 = new Rect(inRect.x + inRect.width / 2f + 70, inRect.y + 10f, inRect.width, 30f);
            Widgets.TextArea(rect3, curValue.ToString(), true);
            Rect rect1 = new Rect(inRect.x, inRect.yMax - 30f, inRect.width / 2f, 30f);
            if (Widgets.ButtonText(rect1, "CancelButton".Translate(), true, false, true))
            {
                this.Close(true);
            }
            Rect rect2 = new Rect(inRect.x + inRect.width / 2f, inRect.yMax - 30f, inRect.width / 2f, 30f);
            if (Widgets.ButtonText(rect2, "OK".Translate(), true, false, true))
            {
                this.Close(true);
                this.confirmAction(this.curValue);
            }
        }
    }
}
