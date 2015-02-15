using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VersusKiosk.UI.Behaviors
{
	// courtesy http://stackoverflow.com/questions/6002046/binding-visualstatemanager-view-state-to-a-mvvm-viewmodel
	public class StateHelper
	{
		public static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
			"State",
			typeof(String),
			typeof(StateHelper),
			new UIPropertyMetadata(null, StateChanged));

		internal static void StateChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
		{
			if (args.NewValue != null)
				VisualStateManager.GoToState((FrameworkElement)target, args.NewValue.ToString(), true);
		}
	}
}
