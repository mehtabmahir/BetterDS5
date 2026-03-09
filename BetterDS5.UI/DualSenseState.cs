namespace BetterDS5.UI;

public sealed class DualSenseState
{
    public byte LeftStickX { get; set; } = 128;
    public byte LeftStickY { get; set; } = 128;
    public byte RightStickX { get; set; } = 128;
    public byte RightStickY { get; set; } = 128;
    public byte L2 { get; set; }
    public byte R2 { get; set; }

    public bool DpadUp { get; set; }
    public bool DpadRight { get; set; }
    public bool DpadDown { get; set; }
    public bool DpadLeft { get; set; }

    public bool Square { get; set; }
    public bool Cross { get; set; }
    public bool Circle { get; set; }
    public bool Triangle { get; set; }

    public bool L1 { get; set; }
    public bool R1 { get; set; }
    public bool L2Button { get; set; }
    public bool R2Button { get; set; }

    public bool Create { get; set; }
    public bool Options { get; set; }
    public bool L3 { get; set; }
    public bool R3 { get; set; }

    public bool PS { get; set; }
    public bool TouchpadButton { get; set; }
    public bool Mute { get; set; }
}