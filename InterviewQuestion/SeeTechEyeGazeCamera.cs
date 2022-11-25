using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace InterviewQuestion
{
    /// <summary>
    /// New file for code review
    /// </summary>
    public class SeeTechEyeGazeCamera : ICamera
    {
        public ISeeTechDriver _driver;
        public CancellationTokenSource _cancellation;
        public event EventHandler<IFrame> FrameChanged;

        public SeeTechEyeGazeCamera(ISeeTechDriver driver)
        {
            _driver = driver;
        }

        public void Start()
        {
            _driver.Connect();
            _cancellation = new CancellationTokenSource();
            Task.Run(() => Loop(_cancellation.Token));
        }

        public void Stop()
        {
            _driver.Disconnect();
            _cancellation.Cancel();
        }

        public void Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int[] frame = new int[6]; _driver.GetNextFrame(frame, -1);
                var p = new Point(frame[0], frame[1]);
                var r = new Point(frame[2], frame[3]);
                var l = new Point(frame[4], frame[5]);
                FrameChanged(this, new SeeTechFrame
                {
                    GazePosition = p,
                    LeftEye = l,
                    RightEye = r
                });
            }
        }
    }
}
