using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using VersusKiosk.UI.Pages;

namespace VersusKiosk.UI.Behaviors
{
	/*
	public class FocusHelper : Behavior<FrameworkElement>
	{
		// Using a DependencyProperty as the backing store for FocusField.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FocusFieldProperty =
			DependencyProperty.Register("FocusField", typeof(object), typeof(FocusHelper), new PropertyMetadata(null, OnFocusPropertyChanged));

		
		protected override void OnAttached()
		{
			base.OnAttached();
			this.AssociatedObject.Initialized += AssociatedObject_Initialized;
		}

		void AssociatedObject_Initialized(object sender, EventArgs e)
		{
			//FocusManager.SetFocusedElement(this.AssociatedObject.Parent, this.AssociatedObject);
			//Keyboard.Focus(this.AssociatedObject);
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
		}

		void AssociatedObject_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
		}

		protected static void OnFocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			/ *
			var focusHelper = d as FocusHelper;
			var element = focusHelper.AssociatedObject.GetValue(FocusManager.FocusedElementProperty);
			if (element != null)
				return;
			 * * /
		}
		
	}
	*/
}
