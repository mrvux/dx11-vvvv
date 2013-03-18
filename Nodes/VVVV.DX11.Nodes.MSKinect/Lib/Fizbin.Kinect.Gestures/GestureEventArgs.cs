using System;

namespace Fizbin.Kinect.Gestures
{
    /// <summary>
    /// The gesture event arguments
    /// </summary>
    public class GestureEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GestureEventArgs"/> class.
        /// </summary>
        /// <param name="type">The gesture type.</param>
        /// <param name="trackingID">The tracking ID.</param>
        /// <param name="userID">The user ID.</param>
        public GestureEventArgs(GestureType type, int trackingId)
        {
            this.TrackingId = trackingId;
            this.GestureType = type;
        }

        /// <summary>
        /// Gets or sets the type of the gesture.
        /// </summary>
        /// <value>
        /// The type of the gesture.
        /// </value>
        public GestureType GestureType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the tracking ID.
        /// </summary>
        /// <value>
        /// The tracking ID.
        /// </value>
        public int TrackingId
        {
            get;
            set;
        }
    }
}
