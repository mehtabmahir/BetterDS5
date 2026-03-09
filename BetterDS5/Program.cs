using HidSharp;

const int SonyVendorId = 0x054C;
const int DualSenseProductId = 0x0CE6;

var dualSense = DeviceList.Local
    .GetHidDevices(SonyVendorId, DualSenseProductId)
    .FirstOrDefault();

if (dualSense is null)
{
    Console.WriteLine("DualSense not found.");
    return;
}

if (!dualSense.TryOpen(out var stream))
{
    Console.WriteLine("Failed to open DualSense stream.");
    return;
}

using (stream)
{
    stream.ReadTimeout = Timeout.Infinite;

    var buffer = new byte[dualSense.GetMaxInputReportLength()];

    Console.WriteLine("USB parser running. Move sticks / press buttons. Ctrl+C to stop.\n");

    while (true)
    {
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        if (bytesRead < 11 || buffer[0] != 0x01)
        {
            continue;
        }

        var state = ParseUsbReport(buffer);

        Console.Clear();
        Console.WriteLine(state);
    }
}

static DualSenseState ParseUsbReport(byte[] report)
{
    byte b8 = report[8];
    byte b9 = report[9];
    byte b10 = report[10];

    int dpad = b8 & 0x0F;

    return new DualSenseState
    {
        LeftStickX = report[1],
        LeftStickY = report[2],
        RightStickX = report[3],
        RightStickY = report[4],
        L2 = report[5],
        R2 = report[6],

        DpadUp = dpad is 0 or 1 or 7,
        DpadRight = dpad is 1 or 2 or 3,
        DpadDown = dpad is 3 or 4 or 5,
        DpadLeft = dpad is 5 or 6 or 7,

        Square = (b8 & (1 << 4)) != 0,
        Cross = (b8 & (1 << 5)) != 0,
        Circle = (b8 & (1 << 6)) != 0,
        Triangle = (b8 & (1 << 7)) != 0,

        L1 = (b9 & (1 << 0)) != 0,
        R1 = (b9 & (1 << 1)) != 0,
        L2Button = (b9 & (1 << 2)) != 0,
        R2Button = (b9 & (1 << 3)) != 0,
        Create = (b9 & (1 << 4)) != 0,
        Options = (b9 & (1 << 5)) != 0,
        L3 = (b9 & (1 << 6)) != 0,
        R3 = (b9 & (1 << 7)) != 0,

        PS = (b10 & (1 << 0)) != 0,
        TouchpadButton = (b10 & (1 << 1)) != 0,
        Mute = (b10 & (1 << 2)) != 0
    };
}

sealed class DualSenseState
{
    public byte LeftStickX { get; set; }
    public byte LeftStickY { get; set; }
    public byte RightStickX { get; set; }
    public byte RightStickY { get; set; }
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

    public override string ToString()
    {
        return
            $@"LX: {LeftStickX}  LY: {LeftStickY}
            RX: {RightStickX}  RY: {RightStickY}
            L2: {L2}  R2: {R2}

            DPad    U:{DpadUp} R:{DpadRight} D:{DpadDown} L:{DpadLeft}
            Face    □:{Square} X:{Cross} O:{Circle} △:{Triangle}
            Top     L1:{L1} R1:{R1} L2B:{L2Button} R2B:{R2Button}
            Middle  Create:{Create} Options:{Options} L3:{L3} R3:{R3}
            System  PS:{PS} Touchpad:{TouchpadButton} Mute:{Mute}";
    }
}