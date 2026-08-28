using Drawie.Host.Input;
using Drawie.Layer.UI.MiniUi.Exceptions;
using Drawie.Numerics;

namespace Drawie.Layer.UI.MiniUi.Controls;

public static class CollapsableGroup
{
    private const string ExpandedGlyph = "M7.47461 10.5L14 3.5L0 3.5L7.47461 10.5Z";

    private const string CollapsedGlyph = "M10.5 6.52539L3.5 0L3.5 14L10.5 6.52539ZM10.5 6.52539L3.5 0L3.5 14L10.5 6.52539Z";

    private static readonly Dictionary<string, bool> States = new();

    private static readonly Stack<GroupState> ActiveGroups = new();
    
    private const float GlyphSize = 12;

    private static VecF startPos;

    public static bool Begin(string id, string label)
    {
        MiniUiContext? ctx = MiniUiContext.Active;

        if (ctx == null)
            throw new MiniUiMissingContextException();

        bool expanded = States.GetValueOrDefault(id, true);
        
        startPos = ctx.CurrentPosition;

        Layout.BeginMeasure();
            Panel.BeginRow();
            
            Glyph.Draw(expanded ? ExpandedGlyph : CollapsedGlyph);
            Label.Show(label);

            Panel.EndRow();
            
        var measured = Layout.EndMeasure();
        var bounds = measured;
        bounds.Size += new VecD(MiniUiStyle.Active.Padding * 2);
        
        ctx.Framebuffer?.DrawRectangle((float)measured.X, (float)measured.Y, (float)bounds.Width, (float)bounds.Height, MiniUiStyle.Active.BackgroundHigh);
        
        
        Panel.BeginRow();
        
        ctx.CurrentPosition += new VecF(MiniUiStyle.Active.Padding, MiniUiStyle.Active.Padding);
        Glyph.Draw(expanded ? ExpandedGlyph : CollapsedGlyph, GlyphSize);
        ctx.CurrentPosition += new VecF(0, MiniUiStyle.Active.Padding / 2f);
        
        Label.Show(label);

        Panel.EndRow();

        bool hovered = bounds.ContainsInclusive(ctx.PointerPosition);

        bool justPressed =
            !ctx.LastState.PressedPointerButtons[PointerButton.Left] &&
            ctx.InputController.PrimaryPointer.IsButtonPressed(
                PointerButton.Left);

        if (hovered && justPressed)
        {
            expanded = !expanded;
            States[id] = expanded;
        }

        if (!expanded)
        {
            ctx.CurrentPosition = new VecF(
                (float)measured.X,
                (float)measured.Bottom);

            return false;
        }

        ActiveGroups.Push(new GroupState(bounds));

        ctx.CurrentPosition = new VecF(
            (float)bounds.X + GlyphSize / 2f,
            (float)bounds.Bottom + MiniUiStyle.Active.Spacing);

        return true;
    }

    public static void End()
    {
        MiniUiContext? ctx = MiniUiContext.Active;

        if (ctx == null)
            throw new MiniUiMissingContextException();

        if (ActiveGroups.Count == 0)
            throw new InvalidOperationException(
                "CollapsableGroup.End() was called without a matching Begin().");

        GroupState state = ActiveGroups.Pop();

        double bottom = Math.Max(
            state.HeaderBounds.Bottom,
            ctx.CurrentPosition.Y);

        ctx.CurrentPosition = new VecF(
            (float)state.HeaderBounds.X,
            (float)bottom);
        
        RectD bounds = RectD.FromTwoPoints((VecD)startPos, new VecD(state.HeaderBounds.X, ctx.CurrentPosition.Y));
        
        Panel.Advance(bounds);
    }

    private readonly record struct GroupState(RectD HeaderBounds);
}