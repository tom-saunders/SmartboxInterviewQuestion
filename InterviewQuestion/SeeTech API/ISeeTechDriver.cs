namespace InterviewQuestion
{
    /// <summary>
    /// The interface to the SeeTech driver, for which we have no implementation details.
    /// All of these functions will call into unmanged code and are not thread safe
    /// </summary>
    public interface ISeeTechDriver
    {
        /// <summary>
        /// Blocks until the next frame is ready or until the given timeout expires
        /// </summary>
        /// <param name="framedata">
        /// framedata[0-1] The gaze position (x,y) screen coords
        /// framedata[2-3] The left eye position (x,y) coords
        /// framedata[4-5] The right eye position (x,y) coords
        /// </param>
        /// <param name="timeout">
        /// Timeout in ms, -1 will wait indefinitely
        /// </param>
        /// <returns>Success, Error, Timeout</returns>
        SeeTechReturnCode GetNextFrame(int[] framedata, int timeout);

        /// <summary>
        /// Blocks until connection to the camera has succeeded or failed, may take up to 10 seconds
        /// </summary>
        /// <returns>Success, Error</returns>
        SeeTechReturnCode Connect();

        /// <summary>
        /// Blocks until disconnection has succeeded or failed, may take up to 10 seconds
        /// </summary>
        /// <returns>Success, Error</returns>
        SeeTechReturnCode Disconnect();
    }
}
