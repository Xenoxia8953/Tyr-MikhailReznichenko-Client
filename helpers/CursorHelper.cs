﻿using System.Linq;
using System.Reflection;
using EFT.UI;
using SPT.Reflection.Utils;
using UnityEngine;

namespace Helpers.CursorHelper;
public static class CursorHelper
{
    private static readonly MethodInfo setCursorMethod;
    private static readonly MethodInfo setCursorLockMethod;

    static CursorHelper()
    {
        var cursorType = PatchConstants.EftTypes.Single(x => x.GetMethod("SetCursor") != null);

        setCursorMethod = cursorType.GetMethod("SetCursor");
        setCursorLockMethod = cursorType.GetMethod("SetCursorLockMode");
    }

    public static void SetCursor(ECursorType type)
    {
        setCursorMethod.Invoke(null, new object[] { type });
    }

    public static void SetCursorLockMode(bool visible, FullScreenMode fullscreenMode)
    {
        setCursorLockMethod.Invoke(null, new object[] { visible, fullscreenMode });
    }
}