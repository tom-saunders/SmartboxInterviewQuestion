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
            // csharp q - can this be null?
            _driver = driver;
        }

        public void Start()
        {
            // no error handling
            // this blocks? should we handle this async and allow other tasks to progress?
            _driver.Connect();

            // starts even if we didn't connect
            // can/should this be created here or in ctor? can it fail?
            _cancellation = new CancellationTokenSource();

            // csharp q - is this a busy loop?
            //     see Loop method below - not part of the language itself
            // should it be waiting on an event and then checking whether to reschedule?
            Task.Run(() => Loop(_cancellation.Token));
        }

        public void Stop()
        {
            // no error handling
            // if we failed Connect() then do we fail expectations calling Disconnect()?
            _driver.Disconnect();
            
            // if we fail disconnect what state do we get left in? are we leaking a resource?
            _cancellation.Cancel();
        }

        public void Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // this allocates a new array for each frame?
                // stub interface implies 20/sec, this is allocating 480 bytes per second (doubled or tripled with p/r/l/parameter to FrameChanged?)
                // csharp q - presumably new allocates on the heap and will then be GCd? performance hit from fragmented short lived allocations?
                int[] frame = new int[6];

                // this will potentially block forever
                _driver.GetNextFrame(frame, -1);
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
