using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.CommunalTools.UI;

public static class TextMenuHelper
{
    public static TextMenu.Item AddWarning(this TextMenu.Item option, TextMenu containingMenu, string description, bool icon)
    {
        TextMenuExt.EaseInSubHeaderExt descriptionText = new(description, false, containingMenu, icon ? "areas/new-yellow" : null)
        {
            TextColor = Color.Firebrick,
            HeightExtra = 0f
        };

        List<TextMenu.Item> items = containingMenu.GetItems();
        if (items.Contains(option))
            TextMenuExt.Insert(containingMenu, items.IndexOf(option) + 1, descriptionText);

        option.OnEnter = (Action)Delegate.Combine(option.OnEnter, () => { descriptionText.FadeVisible = true; });
        option.OnLeave = (Action)Delegate.Combine(option.OnLeave, () => { descriptionText.FadeVisible = false; });

        return option;
    }

    public static TextMenuExt.EaseInSubHeaderExt AddThenGetDescription(this TextMenu.Item option, TextMenu containingMenu, string description)
    {
        TextMenuExt.EaseInSubHeaderExt descriptionText = new(description, false, containingMenu)
        {
            TextColor = Color.DimGray,
            HeightExtra = 0f
        };

        List<TextMenu.Item> items = containingMenu.GetItems();
        if (items.Contains(option))
            TextMenuExt.Insert(containingMenu, items.IndexOf(option) + 1, descriptionText);

        option.OnEnter = (Action)Delegate.Combine(option.OnEnter, () => { descriptionText.FadeVisible = true; });
        option.OnLeave = (Action)Delegate.Combine(option.OnLeave, () => { descriptionText.FadeVisible = false; });

        return descriptionText;
    }
}
