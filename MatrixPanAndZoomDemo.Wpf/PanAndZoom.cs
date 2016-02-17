﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MatrixPanAndZoomDemo.Wpf
{
    public class PanAndZoom : Border
    {
        private UIElement _element;
        private double _zoomSpeed = 1.2;
        private Point _pan;
        private Point _previous;
        private Matrix _matrix = MatrixHelper.Identity;

        public PanAndZoom()
            : base()
        {
            Focusable = true;
            Background = Brushes.Transparent;
            Unloaded += UIElementZoomManager_Unloaded;
        }

        private void UIElementZoomManager_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_element != null)
            {
                Unload();
            }
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != _element && _element != null)
                {
                    Unload();
                }

                base.Child = value;

                if (value != null && value != _element)
                {
                    Initialize(value);
                }
            }
        }

        private void Initialize(UIElement element)
        {
            if (element != null)
            {
                _element = element;
                this.Focus();
                this.PreviewMouseWheel += Element_PreviewMouseWheel;
                this.PreviewMouseRightButtonDown += Element_PreviewMouseRightButtonDown;
                this.PreviewMouseRightButtonUp += Element_PreviewMouseRightButtonUp;
                this.PreviewMouseMove += Element_PreviewMouseMove;
                this.KeyDown += Element_KeyDown;
            }
        }

        private void Unload()
        {
            if (_element != null)
            {
                this.PreviewMouseWheel -= Element_PreviewMouseWheel;
                this.PreviewMouseRightButtonDown -= Element_PreviewMouseRightButtonDown;
                this.PreviewMouseRightButtonUp -= Element_PreviewMouseRightButtonUp;
                this.PreviewMouseMove -= Element_PreviewMouseMove;
                this.KeyDown -= Element_KeyDown;
                _element.RenderTransform = null;
                _element = null;
            }
        }

        private void Invalidate()
        {
            if (_element != null)
            {
                _element.RenderTransformOrigin = new Point(0, 0);
                _element.RenderTransform = new MatrixTransform(_matrix);
                _element.InvalidateVisual();
            }
        }

        private void ZoomTo(double zoom, Point point)
        {
            _matrix = MatrixHelper.ScaleAtPrepend(_matrix, zoom, zoom, point.X, point.Y);

            Invalidate();
        }

        private void ZoomDeltaTo(int delta, Point point)
        {
            ZoomTo(delta > 0 ? _zoomSpeed : 1 / _zoomSpeed, point);
        }

        private void StartPan(Point point)
        {
            _pan = new Point();
            _previous = new Point(point.X, point.Y);
        }

        private void PanTo(Point point)
        {
            Point delta = new Point(point.X - _previous.X, point.Y - _previous.Y);
            _previous = new Point(point.X, point.Y);

            _pan = new Point(_pan.X + delta.X, _pan.Y + delta.Y);
            _matrix = MatrixHelper.TranslatePrepend(_matrix, _pan.X, _pan.Y);

            Invalidate();
        }

        private void Extent()
        {
            if (_element != null)
            {
                double pw = this.RenderSize.Width;
                double ph = this.RenderSize.Height;
                double ew = _element.RenderSize.Width;
                double eh = _element.RenderSize.Height;
                double zx = pw / ew;
                double zy = ph / eh;
                double zoom = Math.Min(zx, zy);

                _matrix = MatrixHelper.ScaleAt(zoom, zoom, ew / 2.0, eh / 2.0);

                Invalidate();
            }
        }

        private void Fill()
        {
            if (_element != null)
            {
                double pw = this.RenderSize.Width;
                double ph = this.RenderSize.Height;
                double ew = _element.RenderSize.Width;
                double eh = _element.RenderSize.Height;
                double zx = pw / ew;
                double zy = ph / eh;

                _matrix = MatrixHelper.ScaleAt(zx, zy, ew / 2.0, eh / 2.0);

                Invalidate();
            }
        }

        private void Reset()
        {
            _matrix = MatrixHelper.Identity;

            Invalidate();
        }

        private void Element_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_element != null)
            {
                Point point = e.GetPosition(_element);
                ZoomDeltaTo(e.Delta, point);
            }
        }

        private void Element_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_element != null)
            {
                Point point = e.GetPosition(_element);
                StartPan(point);
                _element.CaptureMouse();
            }
        }

        private void Element_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_element != null)
            {
                _element.ReleaseMouseCapture();
            }
        }

        private void Element_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_element != null && _element.IsMouseCaptured)
            {
                Point point = e.GetPosition(_element);
                PanTo(point);
            }
        }

        private void Element_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.E)
            {
                Extent();
            }

            if (e.Key == Key.F)
            {
                Fill();
            }

            if (e.Key == Key.R)
            {
                Reset();
            }

        }
    }
}
