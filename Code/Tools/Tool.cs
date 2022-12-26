namespace Celeste.Mod.CommunalTools.Tools;

/// <summary>
/// Represents a base tool, which can execute code before and after a <see cref="Level"/>'s update and render calls.
/// </summary>
public abstract class Tool
{
    internal bool DiscardedUpdate { get; set; }
    internal bool DiscardedRender { get; set; }

    /// <summary>
    /// The reference to the current level.
    /// </summary>
    public Level Level { get; internal set; }

    /// <summary>
    /// Called after the <see cref="Level.Level"/> constructor.
    /// </summary>
    public virtual void Begin() { }

    /// <summary>
    /// Called when this tool is registered.
    /// </summary>
    public virtual void Registered() { }

    /// <summary>
    /// Called when this tool is unregistered.
    /// </summary>
    public virtual void Unregistered() { }

    /// <summary>
    /// Called before <see cref="Level.Update"/>.
    /// </summary>
    /// <returns>A <see cref="bool"/> that determines whether the current <see cref="Level"/> itself should be updated, or skipped when <c>false</c> is returned.</returns>
    public virtual bool UpdateBefore() => true;

    /// <summary>
    /// Called after <see cref="Level.Update"/>.
    /// </summary>
    public virtual void UpdateAfter() { }

    /// <summary>
    /// Called before <see cref="Level.Render"/>.
    /// </summary>
    /// <returns>A <see cref="bool"/> that determines whether the current <see cref="Level"/> itself should be rendered, or skipped when <c>false</c> is returned.</returns>
    public virtual bool RenderBefore() => true;

    /// <summary>
    /// Called after <see cref="Level.Render"/>.
    /// </summary>
    public virtual void RenderAfter() { }

    /// <summary>
    /// Marks the rest of the update step of this tool to be discarded.
    /// </summary>
    public void DiscardUpdate() => DiscardedUpdate = true;

    /// <summary>
    /// Marks the rest of the render step of this tool to be discarded.
    /// </summary>
    public void DiscardRender() => DiscardedRender = true;
}
