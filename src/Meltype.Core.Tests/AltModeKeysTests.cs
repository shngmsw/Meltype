// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 shngmsw

using Meltype.Config;
using Meltype.Input;

namespace Meltype.Tests;

internal static class AltModeKeysTests
{
    private static KeyEvent Down(int vk, bool injected = false) => new(vk, 0, false, false, injected, 0);
    private static KeyEvent Up(int vk, bool injected = false) => new(vk, 0, false, true, injected, 0);

    [Test]
    public static void LeftAltAlone_SwitchesToEnglish_RightAltAlone_ToJapanese()
    {
        var keys = new AltModeKeys();
        Assert.Equal(AltKeyAction.Pressed, keys.OnKey(Down(VirtualKeys.LMenu)), "押し始めでメニューバー対策のキーを送る");
        Assert.Equal(AltKeyAction.English, keys.OnKey(Up(VirtualKeys.LMenu)));
        Assert.Equal(AltKeyAction.Pressed, keys.OnKey(Down(VirtualKeys.RMenu)));
        Assert.Equal(AltKeyAction.Japanese, keys.OnKey(Up(VirtualKeys.RMenu)));
    }

    [Test]
    public static void KeyRepeat_StillCountsAsAlone()
    {
        var keys = new AltModeKeys();
        keys.OnKey(Down(VirtualKeys.RMenu));
        Assert.Equal(AltKeyAction.None, keys.OnKey(Down(VirtualKeys.RMenu)), "長押しのリピートでは何もしない");
        Assert.Equal(AltKeyAction.Japanese, keys.OnKey(Up(VirtualKeys.RMenu)));
    }

    [Test]
    public static void AltWithAnotherKey_DoesNotSwitch()
    {
        var keys = new AltModeKeys();
        keys.OnKey(Down(VirtualKeys.LMenu));
        keys.OnKey(Down(VirtualKeys.Tab));
        keys.OnKey(Up(VirtualKeys.Tab));
        Assert.Equal(AltKeyAction.None, keys.OnKey(Up(VirtualKeys.LMenu)), "Alt + Tab");
        keys.OnKey(Down(VirtualKeys.LMenu));
        Assert.Equal(AltKeyAction.English, keys.OnKey(Up(VirtualKeys.LMenu)), "次の単独押しは切り替える");
    }

    [Test]
    public static void BothAlts_DoNotSwitch()
    {
        var keys = new AltModeKeys();
        keys.OnKey(Down(VirtualKeys.LMenu));
        Assert.Equal(AltKeyAction.None, keys.OnKey(Down(VirtualKeys.RMenu)));
        Assert.Equal(AltKeyAction.None, keys.OnKey(Up(VirtualKeys.LMenu)));
        Assert.Equal(AltKeyAction.None, keys.OnKey(Down(VirtualKeys.RMenu)), "押したままのリピート");
        Assert.Equal(AltKeyAction.None, keys.OnKey(Up(VirtualKeys.RMenu)));
    }

    [Test]
    public static void AltWithClick_DoesNotSwitch()
    {
        var keys = new AltModeKeys();
        keys.OnKey(Down(VirtualKeys.LMenu));
        keys.Interrupt();
        Assert.Equal(AltKeyAction.None, keys.OnKey(Up(VirtualKeys.LMenu)));
    }

    [Test]
    public static void InjectedKeys_AreIgnored()
    {
        var keys = new AltModeKeys();
        Assert.Equal(AltKeyAction.None, keys.OnKey(Down(VirtualKeys.LMenu, injected: true)));
        Assert.Equal(AltKeyAction.None, keys.OnKey(Up(VirtualKeys.LMenu, injected: true)), "ほかのソフトが送った Alt では切り替えない");
        keys.OnKey(Down(VirtualKeys.RMenu));
        keys.OnKey(Down(0xE8, injected: true));
        Assert.Equal(AltKeyAction.Japanese, keys.OnKey(Up(VirtualKeys.RMenu)), "メニューバー対策に送ったキーでは単独押しを取り消さない");
    }

    [Test]
    public static void Setting_IsOffByDefault_AndShared()
    {
        Assert.True(!new Settings().AltKeysSwitchMode, "既定は OFF");
        var settings = new Settings { AltKeysSwitchMode = true }.Normalize();
        var profile = settings.AddProfile("仕事用")!;
        profile.AltKeysSwitchMode = false;
        Assert.True(!profile.SwitchProfile(Settings.DefaultProfileName).AltKeysSwitchMode, "全プロファイル共通の設定");
    }
}
