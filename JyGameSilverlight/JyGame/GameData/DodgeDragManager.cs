using System;
using System.Windows;
using System.Windows.Input;

namespace JyGame.UserControls
{
    public class DodgeDragManager
    {
        private bool isStartMove = true;
        private bool isDragging = false;
        private Point lastMousePosition;
        private UIElement layoutRoot;
        private UIElement elementToDrag;

        public event EventHandler<EventArgs> OnFirstTimeMove;

        public event EventHandler<EventArgs> OnDragMove;

        public DodgeDragManager(UIElement layoutRoot)
        {
            this.layoutRoot = layoutRoot;
        }

        public void EnableDragableElement(UIElement elementToDrag)
        {
            this.elementToDrag = elementToDrag;

            this.elementToDrag.MouseLeftButtonDown += element_MouseLeftButtonDown;
            this.elementToDrag.MouseMove += elementToDrag_MouseMove;
            this.elementToDrag.MouseLeftButtonUp += elementToDrag_MouseLeftButtonUp;

        }

        public void DisableDragableElement()
        {
            isDragging = false;
            elementToDrag.MouseLeftButtonDown -= element_MouseLeftButtonDown;
            elementToDrag.MouseMove -= elementToDrag_MouseMove;
            elementToDrag.MouseLeftButtonUp -= elementToDrag_MouseLeftButtonUp;
            isStartMove = true;
        }

        void elementToDrag_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DodgeMan element = (DodgeMan)sender;
            element.Cursor = null;
            element.ReleaseMouseCapture();
            isDragging = false;
        }

        void elementToDrag_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                DodgeMan element = (DodgeMan)sender;
                Point currentMousePosition = e.GetPosition(layoutRoot);
                double mouseX = currentMousePosition.X - lastMousePosition.X;
                double mouseY = currentMousePosition.Y - lastMousePosition.Y;
                element.X += mouseX;
                element.Y += mouseY;

                EventArgs moveagrs = new EventArgs();
                OnDragMove(this, moveagrs);

                lastMousePosition = currentMousePosition;

            }
        }

        void element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            if (isStartMove)
            {
                EventArgs args = new EventArgs();
                OnFirstTimeMove(this, args);
                isStartMove = false;
            }
            DodgeMan element = (DodgeMan)sender;
            element.Cursor = Cursors.Hand;

            lastMousePosition = e.GetPosition(layoutRoot);
            ((UIElement)sender).CaptureMouse();
        }


    }

}
