namespace WeekNumberLite2.Forms;

internal class PowerBroadcastListener : NativeWindow, IDisposable
{
    private readonly Action _onResume;

    public PowerBroadcastListener(Action onResume)
    {
        _onResume = onResume ?? throw new ArgumentNullException(nameof(onResume));

        CreateHandle(new CreateParams());
    }

    protected override void WndProc(ref System.Windows.Forms.Message m)
    {
        const int WM_POWERBROADCAST = 0x0218;
        const int PBT_APMRESUMESUSPEND = 0x0007;

        if (m.Msg == WM_POWERBROADCAST && (int)m.WParam == PBT_APMRESUMESUSPEND)
        {
            _onResume();
        }

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        DestroyHandle();
    }
}
