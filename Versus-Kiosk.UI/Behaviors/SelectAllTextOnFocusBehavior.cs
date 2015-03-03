using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace VersusKiosk.UI.Behaviors
{
	public class SelectAllTextOnFocusBehavior : Behavior<TextBox>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.GotKeyboardFocus += AssociatedObjectGotKeyboardFocus;
			AssociatedObject.GotMouseCapture += AssociatedObjectGotMouseCapture;
			AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObjectPreviewMouseLeftButtonDown;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.GotKeyboardFocus -= AssociatedObjectGotKeyboardFocus;
			AssociatedObject.GotMouseCapture -= AssociatedObjectGotMouseCapture;
			AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObjectPreviewMouseLeftButtonDown;
		}

		private void AssociatedObjectGotKeyboardFocus(object sender,
			System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			AssociatedObject.SelectAll();
		}

		private void AssociatedObjectGotMouseCapture(object sender,
			System.Windows.Input.MouseEventArgs e)
		{
			AssociatedObject.SelectAll();
		}

		private void AssociatedObjectPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!AssociatedObject.IsKeyboardFocusWithin)
			{
				AssociatedObject.Focus();
				e.Handled = true;
			}
		}
	}

	public class SelectAllTextOnFocusMultiBehavior : Behavior<UIElement>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.GotKeyboardFocus += HandleKeyboardFocus;
			AssociatedObject.GotMouseCapture += HandleMouseCapture;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.GotKeyboardFocus -= HandleKeyboardFocus;
			AssociatedObject.GotMouseCapture -= HandleMouseCapture;
		}

		private static void HandleKeyboardFocus(object sender,
			System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			var txt = e.NewFocus as TextBox;
			if (txt != null)
				txt.SelectAll();
		}

		private static void HandleMouseCapture(object sender,
			System.Windows.Input.MouseEventArgs e)
		{
			var txt = e.OriginalSource as TextBox;
			if (txt != null)
				txt.SelectAll();
		}
	}
}
