using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace InterviewQuestion
{
    /// <summary>
    /// New file for code review
    /// </summary>
    // I'd probably have this as a shared lib rather than built into the app?
    public class SeeTechEyeGazeCamera : ICamera
    {
        public event EventHandler<IFrame>? FrameChanged;

        private readonly ISeeTechDriver _driver;
        private readonly CancellationTokenSource _cancellation;

        public SeeTechEyeGazeCamera(ISeeTechDriver driver)
        {
            // csharp q - can this be null?
            //     not any more
            _driver = driver;
            _cancellation = new();
        }

        public void Start()
        {
            // no error handling
            //    is now handling failure to connect but maybe not gracefully
            // this blocks? should we handle this async and allow other tasks to progress?
            //     let caller deal with any async concern
            SeeTechReturnCode connect_rc = _driver.Connect();
            // seems there's no way to ask the compiler to enforce enum handling (e.g. -Wswitch-enum in g++)
            // just check explicitly if not success in case future error cases are added?
            if (connect_rc != SeeTechReturnCode.Success)
            {
                // This seems like a nonideal logging approach but that seems outside scope
                Console.Error.WriteLine("Failed to connect to SeeTech camera: " + connect_rc);
                throw new InvalidOperationException("Failed to connect to SeeTech camera: " + connect_rc);
            }

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
                FrameChanged?.Invoke(this, new SeeTechFrame
                {
                    GazePosition = p,
                    LeftEye = l,
                    RightEye = r
                });
            }
        }
    }
}
