using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TrainingSourceManager.Components
{
    public class DragDropComponent
    {
        public event EventHandler<MouseButtonEventArgs>? OnDrag;

        private Point? _startPoint;
        private object? _control;
        private MouseButtonEventArgs? _originalArgs;


        public void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _control = sender;
            _originalArgs = e;
        }

        public void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_startPoint == null || _control == null || _originalArgs == null)
                return;

            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint.Value - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    OnDrag?.Invoke(_control, _originalArgs);
                    Clear();
                }
            }
            else
                Clear();
        }

        private void Clear()
        {
            _control = null;
            _startPoint = null;
            _originalArgs = null;
        }
    }
}
