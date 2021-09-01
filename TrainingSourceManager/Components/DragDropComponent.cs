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

        private readonly Dictionary<System.Windows.Controls.Control, Action<MouseButtonEventArgs>?> _registeredControls;

        private object? _control;
        private MouseButtonEventArgs? _originalArgs;
        private Point? _startPoint;

        public IReadOnlyCollection<System.Windows.Controls.Control> RegisteredControls => _registeredControls.Keys;

        public DragDropComponent()
        {
            _registeredControls = new Dictionary<System.Windows.Controls.Control, Action<MouseButtonEventArgs>?>();
        }

        
        public void Register(System.Windows.Controls.Control control, Action<MouseButtonEventArgs>? onDrag = null)
        {
            if (_registeredControls.ContainsKey(control))
                return;

            _registeredControls.Add(control, onDrag);
            control.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDown;
            control.PreviewMouseMove += PreviewMouseMove;
        }



        private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _control = sender;
            _originalArgs = e;
        }

        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_startPoint == null || _control == null || _originalArgs == null)
                return;

            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint.Value - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (_control is System.Windows.Controls.Control control)
                        if (_registeredControls.TryGetValue(control, out Action<MouseButtonEventArgs>? callback))
                            callback?.Invoke(_originalArgs);

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
