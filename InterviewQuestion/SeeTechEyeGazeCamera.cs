using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

using Stateless;

namespace InterviewQuestion
{
    /// <summary>
    /// New file for code review
    /// </summary>
    // I'd probably have this as a shared lib rather than built into the app?
    public class SeeTechEyeGazeCamera : ICamera, IDisposable
    {
        private enum Transition
        {
            Connect,
            Disconnect,
            Success,
            Error,
            Reset,
            Dispose,
        }

        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            ConnectSuccess,
            ConnectError,
            Disconnecting,
            DisconnectError,
            Disposed,
        }

        public ConnectionState State
        {
            get
            {
                _state_lock.EnterReadLock();
                try
                {
                    return _state;
                }
                finally
                {
                    _state_lock.ExitReadLock();
                }
            }
        }
        EventHandler<IFrame> FrameChanged;

        private readonly ISeeTechDriver _driver;
        private readonly CancellationTokenSource _cancellation;

        private readonly ReaderWriterLockSlim _state_lock;
        private ConnectionState _state = ConnectionState.Disconnected;
        private StateMachine<ConnectionState, Transition> _state_machine;
        private bool disposedValue;

        public SeeTechEyeGazeCamera(ISeeTechDriver driver)
        {
            // csharp q - can this be null?
            //     not any more
            _driver = driver;
            _cancellation = new();

            _state_lock = new();
            _state = ConnectionState.Disconnected;
            _state_machine = new(() => State, s => _state = s);

            _state_machine.Configure(ConnectionState.Disconnected)
                .Permit(Transition.Connect, ConnectionState.Connecting);

            _state_machine.Configure(ConnectionState.Connecting)
                .Permit(Transition.Success, ConnectionState.ConnectSuccess)
                .Permit(Transition.Error, ConnectionState.ConnectError);

            _state_machine.Configure(ConnectionState.ConnectError)
                .Permit(Transition.Reset, ConnectionState.Disconnected);

            _state_machine.Configure(ConnectionState.ConnectSuccess)
                .Permit(Transition.Disconnect, ConnectionState.Disconnecting);

            _state_machine.Configure(ConnectionState.Disconnecting)
                .Permit(Transition.Success, ConnectionState.Disconnected)
                .Permit(Transition.Error, ConnectionState.DisconnectError);

            // This isn't actually doing anything other than suppressing a nullable warning
            // presumably if ICamera were from another library without nullable context set
            // we could avoid this and then have FrameChanged?.Invoke() at point of use
            FrameChanged = (s, e) => { };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="SeeTechStartException"></exception>
        public void Start()
        {
            _state_lock.EnterWriteLock();
            try
            {
                _state_machine.Fire(Transition.Connect);

                // no error handling
                //    is now handling failure to connect but maybe not gracefully
                // this blocks? should we handle this async and allow other tasks to progress?
                //     let caller deal with any async concern
                SeeTechReturnCode connect_rc = _driver.Connect();
                // seems there's no way to ask the compiler to enforce enum handling (e.g. -Wswitch-enum in g++)
                // just check explicitly if not success in case future error cases are added?
                if (connect_rc != SeeTechReturnCode.Success)
                {
                    _state_machine.Fire(Transition.Error);
                    // This seems like a nonideal logging approach but that seems outside scope
                    string err_msg = $"Failed to connect to SeeTech camera: {connect_rc}";
                    Console.Error.WriteLine(err_msg);
                    throw new SeeTechStartException(err_msg);
                }
                _state_machine.Fire(Transition.Success);
            } finally {
                _state_lock.ExitWriteLock();
            }

            // csharp q - is this a busy loop?
            //     see Loop method below - not part of the language itself
            // should it be waiting on an event and then checking whether to reschedule?
            Task.Run(() => Loop(_cancellation.Token));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="SeeTechStopException"></exception>
        public void Stop()
        {
            _state_lock.EnterWriteLock();
            try
            {
                // cancel the loop if it's running first (then wait on a signal to continue?)
                _cancellation.Cancel();

                _state_machine.Fire(Transition.Disconnect);
                // no error handling
                // if we failed Connect() then do we fail expectations calling Disconnect()?
                SeeTechReturnCode disconnect_rc = _driver.Disconnect();
                if (disconnect_rc != SeeTechReturnCode.Success)
                {
                    _state_machine.Fire(Transition.Error);

                    string err_msg = $"Failed to connect to SeeTech camera: {disconnect_rc}";
                    Console.Error.WriteLine(err_msg);
                    throw new SeeTechStopException(err_msg);
                }
            } finally
            {
                _state_lock.ExitWriteLock();
            }
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
        /*
        protected virtual void Dispose(bool disposing)
        {
            _state_lock.EnterWriteLock();
            try
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects)
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                    // TODO: set large fields to null
                    disposedValue = true;
                }
            }
            finally
            {
                _state_lock.ExitWriteLock();
            }
            
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SeeTechEyeGazeCamera()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        */
    }
}
