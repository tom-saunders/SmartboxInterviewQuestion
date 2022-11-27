using System;
using System.Threading.Tasks;

namespace InterviewQuestion
{
    public class MockSeeTechDriverImplementation : ISeeTechDriver
    {
        public SeeTechReturnCode Connect() => SeeTechReturnCode.Success;
        public SeeTechReturnCode Disconnect() => SeeTechReturnCode.Success;
        public SeeTechReturnCode GetNextFrame(int[] framedata, int timeout)
        {
            // Populate a dummy frame
            framedata[0] = 10;
            framedata[1] = 10;
            framedata[2] = 10;
            framedata[3] = 10;
            framedata[4] = 10;
            framedata[5] = 10;

            Task.Delay(50).Wait(); // To simulate a camera framerate of 20 frames per second
            return SeeTechReturnCode.Success;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            // The application that consumes this library would have access to the function that 
            // creates an instance of the actual implementation of ISeeTechDriver

            var driver = new MockSeeTechDriverImplementation();
            var camera = new SeeTechEyeGazeCamera(driver);
            camera.FrameChanged += OnFrameChanged;

            Console.WriteLine("Press any key to connect/disconnect");

            Console.ReadLine();
            camera.Start();

            Console.ReadLine();
            camera.Stop();

            Console.WriteLine("Camera disconnected");
            Console.ReadLine();
        }

        private static void OnFrameChanged(object? sender, IFrame e)
        {
            Console.WriteLine("Frame received");
        }
    }
}
