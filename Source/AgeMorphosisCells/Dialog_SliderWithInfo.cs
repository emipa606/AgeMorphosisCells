using System;
using UnityEngine;
using Verse;

namespace AMCells;

public class Dialog_SliderWithInfo : Dialog_Slider
{
    private const float BotAreaHeight = 30f;

    private const float TopPadding = 15f;
    private readonly Action<int> confirmAction;

    private int curValue;

    public Dialog_SliderWithInfo(Func<int, string> textGetter, int from, int to, Action<int> confirmAction,
        int startingValue = -2147483648) : base(textGetter, from, to, confirmAction, startingValue)
    {
        this.textGetter = textGetter;
        this.from = from;
        this.to = to;
        this.confirmAction = confirmAction;
        forcePause = true;
        //this.closeOnEscapeKey = true;

        closeOnClickedOutside = true;
        curValue = startingValue != -2147483648 ? startingValue : from;
    }

    public Dialog_SliderWithInfo(string text, int from, int to, Action<int> confirmAction,
        int startingValue = -2147483648) : this(val => string.Format(text, val), from, to, confirmAction,
        startingValue)
    {
    }

    public override void DoWindowContents(Rect inRect)
    {
        var rect = new Rect(inRect.x, inRect.y + 15f, inRect.width, 30f);
        Text.Font = GameFont.Small;

        curValue = (int)Widgets.HorizontalSlider(rect, curValue, from, to, true, textGetter(curValue), null, null,
            1f);
        Text.Font = GameFont.Small;
        var rect3 = new Rect(inRect.x + (inRect.width / 2f) + 70, inRect.y + 10f, inRect.width, 30f);
        Widgets.TextArea(rect3, curValue.ToString(), true);
        var rect1 = new Rect(inRect.x, inRect.yMax - 30f, inRect.width / 2f, 30f);
        if (Widgets.ButtonText(rect1, "CancelButton".Translate(), true, false))
        {
            Close();
        }

        var rect2 = new Rect(inRect.x + (inRect.width / 2f), inRect.yMax - 30f, inRect.width / 2f, 30f);
        if (!Widgets.ButtonText(rect2, "OK".Translate(), true, false))
        {
            return;
        }

        Close();
        confirmAction(curValue);
    }
}