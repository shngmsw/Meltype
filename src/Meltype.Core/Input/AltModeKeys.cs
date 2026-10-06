// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 shngmsw

namespace Meltype.Input;

internal enum AltKeyAction
{
    None,
    /// <summary>Alt を押し始めた (呼び出し側は、離したときにメニューバーへ移らないよう何もしないキーを送る)。</summary>
    Pressed,
    /// <summary>左 Alt だけを押して離した。</summary>
    English,
    /// <summary>右 Alt だけを押して離した。</summary>
    Japanese,
}

/// <summary>
/// 左右の Alt の単独押しを見分ける (設定「左右の Alt で英数/日本語」。半角/全角 キーの無い US 配列向け)。
/// Alt を押している間にほかのキー・もう片方の Alt・クリックが来たら (Alt + Tab など)、単独押しにしない。
/// ほかのソフトが送ったキー (Injected) は見ない。
/// </summary>
internal sealed class AltModeKeys
{
    private const int Left = 1, Right = 2;
    private int _held;
    private bool _spoiled;

    public AltKeyAction OnKey(KeyEvent e)
    {
        if (e.Injected) return AltKeyAction.None;
        var side = e.Vk switch { VirtualKeys.LMenu => Left, VirtualKeys.RMenu => Right, _ => 0 };
        if (side == 0)
        {
            if (e.IsDown) Interrupt();
            return AltKeyAction.None;
        }
        if (e.IsDown)
        {
            if ((_held & side) != 0) return AltKeyAction.None; // キーリピート
            var first = _held == 0;
            _held |= side;
            _spoiled = !first;
            return first ? AltKeyAction.Pressed : AltKeyAction.None;
        }
        if ((_held & side) == 0) return AltKeyAction.None;
        _held &= ~side;
        if (_held != 0 || _spoiled) return AltKeyAction.None;
        return side == Right ? AltKeyAction.Japanese : AltKeyAction.English;
    }

    /// <summary>Alt を押している間のクリックなど、キー以外の操作があった。</summary>
    public void Interrupt()
    {
        if (_held != 0) _spoiled = true;
    }
}
