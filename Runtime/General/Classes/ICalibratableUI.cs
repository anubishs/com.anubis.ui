public interface ICalibratableUI
{
    string StableId { get; }
    UICalibrationState CaptureState();
    void ApplyState(UICalibrationState state);
    void ResetCalibrationDefault();
    void SetCalibrationMode(bool enabled);
}
