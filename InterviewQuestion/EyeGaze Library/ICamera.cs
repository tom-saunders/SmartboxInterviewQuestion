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
        // What happens if we cannot successfully start the camera? raise exception? any existing defined?
        // our implementation is blocking - this is not async so presumably that's as expected?
        void Start();
        // ditto
        void Stop();
    }
}
