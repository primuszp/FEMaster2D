using System.Windows.Forms;

namespace FEMaster.Form.Contracts
{
    /// <summary>
    /// Provides binding mouse events
    /// </summary>
    public interface IMouseListener
    {
        /// <summary>
        /// Handles the mouse-move event.
        /// </summary>
        /// <param name="args">Event data</param>
        void OnMouseMove(MouseEventArgs args);

        /// <summary>
        /// Handles the mouse-wheel event.
        /// </summary>
        /// <param name="args">Event data</param>
        void OnMouseWheel(MouseEventArgs args);

        /// <summary>
        /// Handles the mouse-up event.
        /// </summary>
        /// <param name="args">Event data</param>
        void OnMouseButtonUp(MouseEventArgs args);

        /// <summary>
        /// Handles the mouse-down event.
        /// </summary>
        /// <param name="args">Event data</param>
        void OnMouseButtonDown(MouseEventArgs args);
    }
}