﻿using System;

using Xamarin.Forms;
using System.Threading.Tasks;

namespace Forms9Patch
{
	/// <summary>
	/// Forms9Patch Bubble popup.
	/// </summary>
	public class BubblePopup : PopupBase 
	{

		#region Content 
		/// <summary>
		/// Gets or sets the content of the FormsPopup.Modal.
		/// </summary>
		/// <value>The content.</value>
		public View Content {
			get { return _bubbleLayout.Content; }
			set { _bubbleLayout.Content = value; }
		}
		#endregion

		#region Bubble Properties
		/// <summary>
		/// Gets or sets the target visual element.
		/// </summary>
		/// <value>The target.</value>
		public VisualElement Target { get; set; }
		#endregion

		#region Pointer Properties
		/// <summary>
		/// Backing store for pointer length property.
		/// </summary>
		public static readonly BindableProperty PointerLengthProperty = BindableProperty.Create("PointerLength",typeof(float), typeof(BubblePopup), (float)BubbleLayout.PointerLengthProperty.DefaultValue);
		/// <summary>
		/// Gets or sets the length of the bubble layout's pointer.
		/// </summary>
		/// <value>The length of the pointer.</value>
		public float PointerLength {
			get { return (float) GetValue (PointerLengthProperty); }
			set { SetValue (PointerLengthProperty, value); }
		}

		/// <summary>
		/// Backing store for pointer tip radius property.
		/// </summary>
		public static readonly BindableProperty PointerTipRadiusProperty = BindableProperty.Create("PointerTipRadius", typeof(float), typeof(BubblePopup), (float)BubbleLayout.PointerTipRadiusProperty.DefaultValue);
		/// <summary>
		/// Gets or sets the radius of the bubble's pointer tip.
		/// </summary>
		/// <value>The pointer tip radius.</value>
		public float PointerTipRadius {
			get { return (float)GetValue (PointerTipRadiusProperty); }
			set { SetValue (PointerTipRadiusProperty, value); }
		}

		/*
		/// <summary>
		/// Backing store for pointer axial position property.
		/// </summary>
		public static readonly BindableProperty PointerAxialPositionProperty = BindableProperty.Create("PointerAxialPosition", typeof(float), typeof(BubblePopup), (float)BubbleLayout.PointerAxialPositionProperty.DefaultValue);
		/// <summary>
		/// Gets or sets the position of the bubble's pointer along the face it's on.
		/// </summary>
		/// <value>The pointer axial position (left/top is zero).</value>
		public float PointerAxialPosition {
			get { return (float)GetValue (PointerAxialPositionProperty); }
			set { SetValue (PointerAxialPositionProperty, value); }
		}
		*/

		/// <summary>
		/// Backing store for pointer direction property.
		/// </summary>
		public static readonly BindableProperty PointerDirectionProperty = BindableProperty.Create("PointerDirection", typeof(PointerDirection), typeof(BubblePopup), (PointerDirection)BubbleLayout.PointerDirectionProperty.DefaultValue);
		/// <summary>
		/// Gets or sets the direction in which the pointer pointing.
		/// </summary>
		/// <value>The pointer direction.</value>
		public PointerDirection PointerDirection {
			get { return (PointerDirection)GetValue (PointerDirectionProperty); }
			set { SetValue (PointerDirectionProperty, value); }
		}

		/// <summary>
		/// The pointer corner radius property.  Defaults to OutlineCornerRadius if not set.
		/// </summary>
		public static readonly BindableProperty PointerCornerRadiusProperty = BindableProperty.Create("PointerCornerRadius", typeof(float), typeof(BubblePopup), (float)BubbleLayout.PointerCornerRadiusProperty.DefaultValue);
		/// <summary>
		/// Gets or sets the pointer corner radius.
		/// </summary>
		/// <value>The pointer corner radius.</value>
		public float PointerCornerRadius {
			get { return (float) GetValue (PointerCornerRadiusProperty); }
			set { SetValue (PointerCornerRadiusProperty, value); }
		}
		#endregion

		#region Fields
		readonly BubbleLayout _bubbleLayout;
		#endregion


		#region Constructor / Destructor
		/// <summary>
		/// Initializes a new instance of the <see cref="Forms9Patch.BubblePopup"/> class.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="host">Host.</param>
		public BubblePopup (VisualElement target=null, Page host=null) : base (host: host) {
			_bubbleLayout = new BubbleLayout {
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};
			SetRoundedBoxBindings (_bubbleLayout);
			_bubbleLayout.SetBinding (BubbleLayout.PointerLengthProperty, "PointerLength");
			_bubbleLayout.SetBinding (BubbleLayout.PointerTipRadiusProperty, "PointerTipRadius");
			//_bubbleLayout.SetBinding (BubbleLayout.PointerAxialPositionProperty, "PointerAxialPosition");
			// PointerDirection is too ambiguous here.  Need to clarify in LayoutChildren
			//_bubbleLayout.SetBinding (BubbleLayout.PointerDirectionProperty, "PointerDirection");
			_bubbleLayout.SetBinding (BubbleLayout.PointerCornerRadiusProperty, "PointerCornerRadius");
			_bubbleLayout.SetBinding (BubbleLayout.PaddingProperty, "Padding");
			_bubbleLayout.SetBinding (BubbleLayout.HasShadowProperty, "HasShadow");
			_bubbleLayout.BindingContext = this;
			ContentView = _bubbleLayout;

			Target = target;
		}



		#endregion



		#region Change management
		/// <param name="propertyName">The name of the property that changed.</param>
		/// <summary>
		/// Call this method from a child class to notify that a change happened on a property.
		/// </summary>
		protected override void OnPropertyChanged (string propertyName = null) {
			//System.Diagnostics.Debug.WriteLine ($"{this.GetType().FullName}.OnPropertyChanged property={propertyName}");
			//if (propertyName == IsPresentedProperty.PropertyName) {
			base.OnPropertyChanged (propertyName);
			if (_bubbleLayout == null)
				return;
			if (propertyName == IsVisibleProperty.PropertyName) {
				if (Host == null)
					Host = Application.Current.MainPage;			
				if (IsVisible) {
					Content.TranslationX = 0;
					Content.TranslationY = 0;
					//System.Diagnostics.Debug.WriteLine ("======================================================================");
					if (Target != null)
						Target.SizeChanged += OnTargetSizeChanged;
					Parent = Host;
					Host.SetValue (PopupProperty, this);
					Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion (this, new Rectangle (0, 0, Host.Bounds.Width, Host.Bounds.Height));
					// So, Bounds is correct but the Android draw cycle seemed to happen too soon - so only the background is rendered, not the contents.
					ForceNativeLayout?.Invoke ();
				} else {
					if (Target != null)
						Target.SizeChanged -= OnTargetSizeChanged;
					Host.SetValue (PopupProperty, null);
					_bubbleLayout.PointerDirection = PointerDirection.None;
					Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion (this, new Rectangle (0, 0, -1, -1));
				}
			}
		}

		void OnTargetSizeChanged(object sender, EventArgs e) {
			//Host = Host ?? Application.Current.MainPage;			
			if (Host != null) {
				Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion (this, new Rectangle(-1,-1,Host.Bounds.Width+1,Host.Bounds.Height+1));
				ForceNativeLayout?.Invoke ();
				Device.StartTimer (TimeSpan.FromMilliseconds(10), () => {
					Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion (this, new Rectangle(0,0,Host.Bounds.Width,Host.Bounds.Height));
					ForceNativeLayout?.Invoke ();
					return false;
				});

			}
		}

		double _pwfStart, _pwfWidth, _pwfTargetStart, _pwfTargetWidth, _pwfAvailableWidth;
		double positionWeightingFunction(double start) {
			// how far apart are the centers?
			double err=Math.Abs((start + _pwfWidth/2.0) - (_pwfTargetStart + _pwfTargetWidth / 2.0));
			// does the popup and the target overlap?
			err += (start + _pwfWidth >= _pwfTargetStart ? 0 : 100 * _pwfTargetStart - start - _pwfWidth);
			err += (start <= _pwfTargetStart + _pwfTargetWidth ? 0 : 100 * start - (_pwfTargetStart + _pwfTargetWidth));

			// are we close to the edges?
			err += (start < 20 ? 20 * (20-start) : 0);
			err += (start + _pwfWidth > _pwfAvailableWidth - 20 ? 20 * (start + _pwfWidth - _pwfAvailableWidth + 20) : 0);

			// are we off the screen?
			err += (start < 0 ? 1000 * -start : 0);
			err += (start + _pwfWidth > _pwfAvailableWidth ? 1000 * (start + _pwfWidth - _pwfAvailableWidth) : 0);
			//System.Diagnostics.Debug.WriteLine ("\t\t\tstart="+start+" err=" + err);
			return err;
		}

		double pointerWeightingFunction(double offset) {
			// how far is the offset from the center of the target?
			double err = Math.Abs((_pwfStart + offset) - (_pwfTargetStart + _pwfTargetWidth / 2.0));

			// does the pointer overlap the target?
			err += (_pwfStart + offset >= _pwfTargetStart ? 0 : 100 * _pwfTargetStart - _pwfStart - offset);
			err += (_pwfStart + offset <= _pwfTargetStart + _pwfTargetWidth ? 0 : 100 *  _pwfStart + offset - (_pwfTargetStart + _pwfTargetWidth));

			return err;
		}

		Tuple<double,float> StartAndPointerLocation(double width, double targetStart, double targetWidth, double availableWidth) {
			_pwfWidth = width;
			_pwfTargetStart = targetStart;
			_pwfTargetWidth = targetWidth;
			_pwfAvailableWidth = availableWidth;
			double optimalStart;
			NumericalMethods.Search1D.BrentMin (
				0, 
				targetStart + targetWidth / 2.0, 
				availableWidth - width, 
				positionWeightingFunction, 0.0001, out optimalStart);
		
			_pwfStart = optimalStart;

			double optimalPointerLoc;
			NumericalMethods.Search1D.BrentMin (
				0, 
				width/2.0, 
				width,
				pointerWeightingFunction, 0.0001, out optimalPointerLoc);

			var pointerOffset = (float)(optimalPointerLoc / width);
			return new Tuple<double, float> (optimalStart, pointerOffset );
		}

		/// <param name="x">A value representing the x coordinate of the child region bounding box.</param>
		/// <param name="y">A value representing the y coordinate of the child region bounding box.</param>
		/// <param name="width">A value representing the width of the child region bounding box.</param>
		/// <param name="height">A value representing the height of the child region bounding box.</param>
		/// <summary>
		/// Positions and sizes the children of a Layout.
		/// </summary>
		/// <remarks>Implementors wishing to change the default behavior of a Layout should override this method. It is suggested to
		/// still call the base method and modify its calculated results.</remarks>
		protected override void LayoutChildren (double x, double y, double width, double height)
		{
			if (width > 0 && height > 0) {
				Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion (PageOverlay, Host.Bounds);

				var shadow = BubbleLayout.ShadowPadding (_bubbleLayout);
				var request = _bubbleLayout.Content.GetSizeRequest (Host.Bounds.Width, Host.Bounds.Height);
				//var request = _bubbleLayout.Content.Measure(Host.Bounds.Width, Host.Bounds.Height);
				var rboxSize = new Size (request.Request.Width + _bubbleLayout.Padding.HorizontalThickness + shadow.HorizontalThickness, request.Request.Height + _bubbleLayout.Padding.VerticalThickness + shadow.VerticalThickness);

				//System.Diagnostics.Debug.WriteLine("\tBubblePopup.LayoutChildren _bubbleLayout size=[{0}, {1}]",rboxSize.Width, rboxSize.Height);
				PointerDirection pointerDir = PointerDirection.None;

				Rectangle bounds;
				Rectangle targetBounds;
				if (Target != null) {
					targetBounds = DependencyService.Get<IDescendentBounds> ().PageDescendentBounds (Host, Target);
					var reqSpaceToLeft = targetBounds.Left - rboxSize.Width - PointerLength;
					var reqSpaceToRight = Host.Bounds.Width - targetBounds.Right - rboxSize.Width - PointerLength;
					var reqSpaceAbove = targetBounds.Top - rboxSize.Height - PointerLength;
					var reqSpaceBelow = Host.Bounds.Height - targetBounds.Bottom - rboxSize.Height - PointerLength;
					var reqHzSpace = Host.Bounds.Width - rboxSize.Width;
					var reqVtSpace = Host.Bounds.Height - rboxSize.Height;


					double space = 0;
					if (this.PointerDirection.UpAllowed () && Math.Min (reqSpaceBelow, reqHzSpace) > space) {
						pointerDir = PointerDirection.Up;
						space = Math.Min (reqSpaceBelow, reqHzSpace);
					}
					if (this.PointerDirection.DownAllowed () && Math.Min (reqSpaceAbove, reqHzSpace) > space) {
						pointerDir = PointerDirection.Down;
						space = Math.Min (reqSpaceAbove, reqHzSpace);
					}
					if (this.PointerDirection.LeftAllowed () && Math.Min (reqSpaceToRight, reqVtSpace) > space) {
						pointerDir = PointerDirection.Left;
						space = Math.Min (reqSpaceToRight, reqVtSpace);
					}
					if (this.PointerDirection.RightAllowed () && Math.Min (reqSpaceToLeft, reqVtSpace) > space) {
						pointerDir = PointerDirection.Right;
						space = Math.Min (reqSpaceToLeft, reqVtSpace);
					}
					if (space < 0.01) {
						// it doesn't fit ... what's the closest fit?
						space = Int32.MaxValue;
						if (this.PointerDirection.UpAllowed () && Math.Abs (Math.Min (reqSpaceBelow, reqHzSpace)) < space) {
							pointerDir = PointerDirection.Up;
							space = Math.Abs (Math.Min (reqSpaceBelow, reqHzSpace));
						}
						if (this.PointerDirection.DownAllowed () && Math.Abs (Math.Min (reqSpaceAbove, reqHzSpace)) < space) {
							pointerDir = PointerDirection.Down;
							space = Math.Abs (Math.Min (reqSpaceAbove, reqHzSpace));
						}
						if (this.PointerDirection.LeftAllowed () && Math.Abs (Math.Min (reqSpaceToRight, reqVtSpace)) < space) {
							pointerDir = PointerDirection.Left;
							space = Math.Abs (Math.Min (reqSpaceToRight, reqVtSpace));
						}
						if (this.PointerDirection.RightAllowed () && Math.Abs (Math.Min (reqSpaceToLeft, reqVtSpace)) < space) {
							pointerDir = PointerDirection.Right;
						}
					}
				}
				_bubbleLayout.PointerDirection = pointerDir;
				_bubbleLayout.IsVisible = true;
				if (pointerDir == PointerDirection.None) {
					Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion (ContentView, new Rectangle (width / 2.0 - rboxSize.Width / 2.0, height / 2.0 - rboxSize.Height / 2.0, rboxSize.Width, rboxSize.Height));
				} else {
					Tuple<double,float> tuple;
					if (pointerDir.IsVertical ()) {
						tuple = StartAndPointerLocation (rboxSize.Width, targetBounds.Left, targetBounds.Width, Host.Bounds.Width);
						bounds = new Rectangle (
							new Point (
								tuple.Item1,
								(pointerDir == PointerDirection.Up ? targetBounds.Bottom : targetBounds.Top - rboxSize.Height - PointerLength)),
							new Size (rboxSize.Width, rboxSize.Height + PointerLength)
						);
					} else {
						tuple = StartAndPointerLocation (rboxSize.Height, targetBounds.Top, targetBounds.Height, Host.Bounds.Height);
						bounds = new Rectangle (
							new Point (
								(pointerDir == PointerDirection.Left ? targetBounds.Right : targetBounds.Left - rboxSize.Width - PointerLength), 
								tuple.Item1),
							new Size (rboxSize.Width + PointerLength, rboxSize.Height)
						);
					}
					//System.Diagnostics.Debug.WriteLine ("\tBubblePopup.LayoutChildren bounds=" + bounds);
					_bubbleLayout.PointerAxialPosition = tuple.Item2;	
					//_bubbleLayout.PointerDirection = pointerDir;
					Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion (_bubbleLayout, bounds);
				}
			} else
				_bubbleLayout.IsVisible = false;
		}


		#endregion
	}
}

