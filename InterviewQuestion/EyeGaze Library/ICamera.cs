using System;

namespace InterviewQuestion
{
    /// <summary>
    /// Already part of the existing eye gaze library, implementations of this
    /// interface already exist for other camera manufacturer integrations
    /// </summary>
    public interface ICamera
    {
        event EventHandler<IFrame> FrameChanged;
        void Start();
        void Stop();
    }
}
