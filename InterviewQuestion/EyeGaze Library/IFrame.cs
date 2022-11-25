using System.Drawing;

namespace InterviewQuestion
{
    /// <summary>
    /// Already part of the existing eye gaze library, implementations of this
    /// interface already exist for other camera manufacturer integrations
    /// </summary>
    public interface IFrame
    {
        Point GazePosition { get; }
        Point LeftEye { get; }
        Point RightEye { get; }
    }
}
