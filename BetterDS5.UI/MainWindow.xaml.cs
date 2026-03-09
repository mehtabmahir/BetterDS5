using HidSharp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BetterDS5.UI;

public partial class MainWindow : Window
{
    private const int SonyVendorId = 0x054C;
    private const int DualSenseProductId = 0x0CE6;

    private static readonly SolidColorBrush OffBrush = new(Color.FromRgb(42, 42, 42));
    private static readonly SolidColorBrush OnBrush = new(Color.FromRgb(76, 194, 255));

    public MainWindow()
    {
        InitializeComponent();
        StartReadingController();
    }

    private void StartReadingController()
    {
        Task.Run(() =>
        {
            var dualSense = DeviceList.Local
                .GetHidDevices(SonyVendorId, DualSenseProductId)
                .FirstOrDefault();

            if (dualSense is null)
            {
                Dispatcher.Invoke(() => Title = "BetterDS5 - DualSense not found");
                return;
            }

            if (!dualSense.TryOpen(out var stream))
            {
                Dispatcher.Invoke(() => Title = "BetterDS5 - Failed to open DualSense");
                return;
            }

            using (stream)
            {
                stream.ReadTimeout = Timeout.Infinite;
                var buffer = new byte[dualSense.GetMaxInputReportLength()];

                Dispatcher.Invoke(() => Title = "BetterDS5 - Connected");

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead < 11 || buffer[0] != 0x01)
                    {
                        continue;
                    }

                    var state = ParseUsbReport(buffer);
                    Dispatcher.Invoke(() => ApplyState(state));
                }
            }
        });
    }

    private static DualSenseState ParseUsbReport(byte[] report)
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

    private void ApplyState(DualSenseState state)
    {
        LXText.Text = $"LX: {state.LeftStickX}";
        LYText.Text = $"LY: {state.LeftStickY}";
        RXText.Text = $"RX: {state.RightStickX}";
        RYText.Text = $"RY: {state.RightStickY}";
        L2Text.Text = $"L2: {state.L2}";
        R2Text.Text = $"R2: {state.R2}";

        DpadText.Text = $"DPad: {BuildPressedList(("Up", state.DpadUp), ("Right", state.DpadRight), ("Down", state.DpadDown), ("Left", state.DpadLeft))}";
        FaceText.Text = $"Face: {BuildPressedList(("Square", state.Square), ("Cross", state.Cross), ("Circle", state.Circle), ("Triangle", state.Triangle))}";
        ShoulderText.Text = $"Shoulders: {BuildPressedList(("L1", state.L1), ("R1", state.R1), ("L2B", state.L2Button), ("R2B", state.R2Button))}";
        MiddleText.Text = $"Middle: {BuildPressedList(("Create", state.Create), ("Options", state.Options), ("L3", state.L3), ("R3", state.R3))}";
        SystemText.Text = $"System: {BuildPressedList(("PS", state.PS), ("Touchpad", state.TouchpadButton), ("Mute", state.Mute))}";

        Canvas.SetLeft(LeftStickDot, 150 + (state.LeftStickX / 255.0) * 110);
        Canvas.SetTop(LeftStickDot, 440 + (state.LeftStickY / 255.0) * 110);

        Canvas.SetLeft(RightStickDot, 500 + (state.RightStickX / 255.0) * 110);
        Canvas.SetTop(RightStickDot, 440 + (state.RightStickY / 255.0) * 110);

        SetIndicator(DpadUpIndicator, state.DpadUp);
        SetIndicator(DpadRightIndicator, state.DpadRight);
        SetIndicator(DpadDownIndicator, state.DpadDown);
        SetIndicator(DpadLeftIndicator, state.DpadLeft);

        SetIndicator(SquareIndicator, state.Square);
        SetIndicator(CrossIndicator, state.Cross);
        SetIndicator(CircleIndicator, state.Circle);
        SetIndicator(TriangleIndicator, state.Triangle);

        SetIndicator(L1Indicator, state.L1);
        SetIndicator(R1Indicator, state.R1);
        SetIndicator(L2ButtonIndicator, state.L2Button);
        SetIndicator(R2ButtonIndicator, state.R2Button);

        SetIndicator(CreateIndicator, state.Create);
        SetIndicator(OptionsIndicator, state.Options);
        SetIndicator(L3Indicator, state.L3);
        SetIndicator(R3Indicator, state.R3);

        SetIndicator(PSIndicator, state.PS);
        SetIndicator(TouchpadIndicator, state.TouchpadButton);
        SetIndicator(MuteIndicator, state.Mute);
    }

    private static string BuildPressedList(params (string Name, bool Pressed)[] buttons)
    {
        var pressed = buttons
            .Where(b => b.Pressed)
            .Select(b => b.Name)
            .ToArray();

        return pressed.Length == 0 ? "-" : string.Join(", ", pressed);
    }

    private static void SetIndicator(Shape shape, bool isOn)
    {
        shape.Fill = isOn ? OnBrush : OffBrush;
    }

    private static void SetIndicator(System.Windows.Controls.Border border, bool isOn)
    {
        border.Background = isOn ? OnBrush : OffBrush;
    }
}