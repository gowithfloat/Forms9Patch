using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Forms9Patch
{

    internal class HeaderCell<TContent> : Cell<TContent> where TContent : View, new()
    {
        public HeaderCell() : base()
        {
            BaseCellView.IsHeader = true;
        }
    }


    // the non-group header version of Cell
    internal class ItemCell<TContent> : Cell<TContent> where TContent : View, new()
    {
        public ItemCell() : base()
        {
            BaseCellView.IsHeader = false;
        }
    }

    // The purpose of this class it to:
    // - capture and manage the height of Forms9Patch.ListView cells
    // - set proper BindingContext to a cell's content view 
    // In the future, it may be also to manage cell separators.
    internal class Cell<TContent> : ViewCell, ICell_T_Height, IDisposable where TContent : View, new()
    {
        #region debug convenience
        protected bool Debug
        {
            get
            {
                //return (BindingContext is ItemWrapper<string> itemWrapper && itemWrapper.Index == 0);
                return false;
                // return (BindingContext is GroupWrapper wrapper && wrapper.Index == 0);
            }
        }

        protected void DebugMessage(string message, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null, [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            if (Debug)
                System.Diagnostics.Debug.WriteLine("[" + GetType() + "." + callerName + ":" + lineNumber + "][" + InstanceId + "] " + message);
        }
        #endregion


        #region Fields
        internal int InstanceId { get; private set; }

        static int _instances;
        internal BaseCellView BaseCellView = new BaseCellView();
        bool _freshHeight;
        double _oldHeight;
        #endregion


        #region Construction / Disposal
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Forms9Patch.DataTemplateSelector.Cell`1"/> class.
        /// </summary>
        public Cell()
        {
            InstanceId = _instances++;
            View = BaseCellView;
            BaseCellView.ContentView = new TContent();
            if (BaseCellView.ContentView is ICellContentView contentView)
                Height = contentView.CellHeight;
        }

        bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
                BaseCellView.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Property Change Handlers
        /// <summary>
        /// Triggered before a property is changed.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected override void OnPropertyChanging(string propertyName = null)
        {
            if (!P42.Utils.Environment.IsOnMainThread)
            {
                Device.BeginInvokeOnMainThread(() => OnPropertyChanging(propertyName));
                return;
            }

            base.OnPropertyChanging(propertyName);

            if (propertyName == BindingContextProperty.PropertyName && View != null)
                View.BindingContext = null;
            else if (propertyName == nameof(Height))
                _oldHeight = Height;
        }

        /// <summary>
        /// Triggered by a change in the binding context.
        /// </summary>
        protected override void OnBindingContextChanged()
        {
            if (!P42.Utils.Environment.IsOnMainThread)
            {
                Device.BeginInvokeOnMainThread(OnBindingContextChanged);
                return;
            }

            _freshHeight = true;
            _oldHeight = -1;
            if (View != null)
                View.BindingContext = BindingContext;

            base.OnBindingContextChanged();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            // don't know why the below "&& UWP" was added.  A really bone headed move.
            if (propertyName == nameof(Height))// && Device.RuntimePlatform == Device.UWP)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                UpdateSizeAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _freshHeight = false;
            }
        }

        bool _updatingSize;
        async Task UpdateSizeAsync()
        //void UpdateSize()
        {
            if (_updatingSize || _freshHeight || _oldHeight < 1)
                return;
            _updatingSize = true;
            await Task.Delay(200);
            if (this.RealParent != null)
                ForceUpdateSize();
            _updatingSize = false;
        }
        #endregion


        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BaseCellView.ContentView is Layout layout && layout.Width > 0 && layout.Height > 0)
            {
                /*
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    layout.ForceLayout();
                    return false;
                });
                */
                //var bounds = layout.Bounds;
                //layout.Layout(Rectangle.Zero);
                //layout.Layout(bounds);
            }
            if (BaseCellView.ContentView is ICellContentView contentView)
                contentView.OnAppearing();


        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (BaseCellView.ContentView is ICellContentView contentView)
                contentView.OnDisappearing();

        }
    }
}