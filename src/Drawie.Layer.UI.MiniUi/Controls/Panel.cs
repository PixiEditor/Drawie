using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Layer.UI.MiniUi.Exceptions;
using Drawie.Numerics;

namespace Drawie.Layer.UI.MiniUi.Controls;

public static class Panel
{
    private static readonly Stack<PanelState> States = new();

    public static void BeginColumn()
    {
        MiniUiContext? ctx = MiniUiContext.Active;

        if (ctx == null)
            throw new MiniUiMissingContextException();

        States.Push(new PanelState(
            ctx.CurrentPosition,
            LayoutDirection.Column));
    }

    public static void EndColumn()
    {
        MiniUiContext? ctx = MiniUiContext.Active;

        if (ctx == null)
            throw new MiniUiMissingContextException();

        if (States.Count == 0)
            throw new InvalidOperationException(
                "Panel.EndColumn() was called without a matching Panel.BeginColumn().");

        PanelState state = States.Pop();

        if (state.Direction != LayoutDirection.Column)
            throw new InvalidOperationException(
                "Panel.EndColumn() does not match the current panel layout.");

        Advance(state.OwnSize);
    }

    public static void BeginRow()
    {
        MiniUiContext? ctx = MiniUiContext.Active;

        if (ctx == null)
            throw new MiniUiMissingContextException();

        States.Push(new PanelState(ctx.CurrentPosition, LayoutDirection.Row));
    }

    public static void EndRow()
    {
        MiniUiContext? ctx = MiniUiContext.Active;

        if (ctx == null)
            throw new MiniUiMissingContextException();

        if (States.Count == 0)
            throw new InvalidOperationException(
                "Panel.EndRow() was called without a matching Panel.BeginRow().");

        PanelState state = States.Pop();

        if (state.Direction != LayoutDirection.Row)
            throw new InvalidOperationException(
                "Panel.EndRow() does not match the current panel layout.");

        Advance(state.OwnSize);
    }
    
    public static void Advance(RectD bounds)
    {
        MiniUiContext? ctx = MiniUiContext.Active;

        if (ctx == null)
            throw new MiniUiMissingContextException();

        if (States.Count == 0)
        {
            ctx.CurrentPosition = new VecF(
                ctx.CurrentPosition.X,
                (float)bounds.Bottom);

            return;
        }

        PanelState state = States.Peek();

        state.OwnSize = state.OwnSize.Union(bounds);

        switch (state.Direction)
        {
            case LayoutDirection.Column:
                ctx.CurrentPosition = new VecF(state.Position.X, (float)bounds.Bottom + MiniUiStyle.Active.Spacing);
                break;

            case LayoutDirection.Row:
                ctx.CurrentPosition = new VecF((float)bounds.Right + MiniUiStyle.Active.Spacing, state.Position.Y);
                break;
        }
    }

    private sealed class PanelState
    {
        public VecF Position { get; }
        public LayoutDirection Direction { get; }

        public RectD OwnSize { get; set; }

        public PanelState(
            VecF position,
            LayoutDirection direction)
        {
            Position = position;
            Direction = direction;
        }
    }

    private enum LayoutDirection
    {
        Row,
        Column
    }
}