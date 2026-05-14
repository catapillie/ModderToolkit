using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Celeste.Mod.CelesteNet.Client.Components;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.ModderToolkit.Utility;

#nullable enable

internal static class CelesteNetHelper
{
    private static readonly EverestModuleMetadata CelesteNetDep = new() {
        Name = "CelesteNet.Client",
        Version = new Version(2, 4, 2),
    };

    /// <summary>
    ///   Retrieves CelesteNet's fake RenderTarget if possible.
    /// </summary>
    /// <remarks>
    ///   CelesteNet IL hooks <see cref="Level.Render"/> and hijacks each
    ///   <see cref="GraphicsDevice.SetRenderTarget(RenderTarget2D)"/> call, replacing the parameter with
    ///   <c>GetFakeRT</c>. If the RenderTarget being set is <c>null</c>, CelesteNet replaces it with its FakeRT.<br/>
    ///   This messes up rendering in several places, like CollabUtils2's in-game overworld rendering (chapter cards,
    ///   journals, etc. don't render) or the screenshot functionality (the screen becomes pitch black instead).<br/>
    ///   Setting the RenderTarget to CelesteNet's FakeRT seemingly fixes the issue.
    /// </remarks>
    /// <returns>
    ///   <see cref="CelesteNetRenderHelperComponent.FakeRT"/>, or <c>null</c> if CelesteNet is not loaded or enabled.
    /// </returns>
    public static RenderTarget2D? GetFakeRenderTarget()
        => Everest.Loader.DependencyLoaded(CelesteNetDep) ? _GetFakeRenderTarget() : null;

    // prevent the JIT from compiling the method because of inlining and crashing with a TypeNotFoundException
    // when cnet is not loaded
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static RenderTarget2D? _GetFakeRenderTarget()
        => Celeste.Instance.Components.OfType<CelesteNetRenderHelperComponent>().FirstOrDefault()?.FakeRT;
}
