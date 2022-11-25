using System.Drawing;

namespace InterviewQuestion
{
    /// <summary>
    /// New file for code review
    /// </summary>
    public class SeeTechFrame : IFrame
    {
        // base interface is nonmutable - is this hiding the base or extending it?
        public Point GazePosition { get; set; }
        public Point LeftEye { get; set; }
        public Point RightEye { get; set; }
    }
}
